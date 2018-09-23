using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.Logic.Validation;
using Bonsai.Areas.Admin.ViewModels.Relations;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Services;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Dictionary;
using Impworks.Utils.Format;
using Impworks.Utils.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// Service for managing relations.
    /// </summary>
    public class RelationsManagerService
    {
        public RelationsManagerService(AppDbContext db, IMapper mapper, UserManager<AppUser> userMgr, RelationValidator validator, CacheService cache)
        {
            _db = db;
            _mapper = mapper;
            _userMgr = userMgr;
            _validator = validator;
            _cache = cache;
        }

        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userMgr;
        private readonly RelationValidator _validator;
        private readonly CacheService _cache;

        /// <summary>
        /// Returns the found relations.
        /// </summary>
        public async Task<RelationsListVM> GetRelationsAsync(RelationsListRequestVM request)
        {
            const int PageSize = 50;

            request = NormalizeListRequest(request);

            var query = _db.Relations.Where(x => x.IsComplementary == false && x.IsDeleted == false);

            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                var req = request.SearchQuery.ToLower();
                query = query.Where(x => x.Destination.Title.ToLower().Contains(req)
                                         || x.Source.Title.ToLower().Contains(req)
                                         || x.Event.Title.ToLower().Contains(req));
            }

            if (request.Types?.Length > 0)
                query = query.Where(x => request.Types.Contains(x.Type));

            var totalCount = await query.CountAsync();

            if (request.OrderBy == nameof(RelationTitleVM.Destination))
                query = query.OrderBy(x => x.Destination.Title, request.OrderDescending);
            else if (request.OrderBy == nameof(RelationTitleVM.Source))
                query = query.OrderBy(x => x.Source.Title, request.OrderDescending);
            else
                query = query.OrderBy(x => x.Type, request.OrderDescending);

            var items = await query.ProjectTo<RelationTitleVM>()
                                   .Skip(PageSize * request.Page)
                                   .Take(PageSize)
                                   .ToListAsync();

            return new RelationsListVM
            {
                Items = items,
                PageCount = (int) Math.Ceiling((double) totalCount / PageSize),
                Request = request
            };
        }

        /// <summary>
        /// Creates a new relation.
        /// </summary>
        public async Task CreateAsync(RelationEditorVM vm, ClaimsPrincipal principal)
        {
            await ValidateRequestAsync(vm);

            var rel = _mapper.Map<Relation>(vm);
            rel.Id = Guid.NewGuid();

            var compRel = new Relation {Id = Guid.NewGuid()};
            MapComplementaryRelation(rel, compRel);

            var rels = new[] {rel, compRel};

            await _validator.ValidateAsync(rels);

            var changeset = await GetChangesetAsync(null, vm, rel.Id, principal, null);
            _db.Changes.Add(changeset);

            _db.Relations.AddRange(rels);

            _cache.Clear();
        }

        /// <summary>
        /// Returns the form information for updating a relation.
        /// </summary>
        public async Task<RelationEditorVM> RequestUpdateAsync(Guid id)
        {
            var rel = await _db.Relations
                               .GetAsync(x => x.Id == id
                                              && x.IsComplementary == false
                                              && x.IsDeleted == false, "Связь не найдена");

            return _mapper.Map<RelationEditorVM>(rel);
        }

        /// <summary>
        /// Updates the relation.
        /// </summary>
        public async Task UpdateAsync(RelationEditorVM vm, ClaimsPrincipal principal, Guid? revertedId = null)
        {
            await ValidateRequestAsync(vm);

            var rel = await _db.Relations
                               .GetAsync(x => x.Id == vm.Id
                                              && x.IsComplementary == false
                                              && (x.IsDeleted == false || revertedId != null),
                                         "Связь не найдена");

            var compRel = await FindComplementaryRelationAsync(rel);

            var changeset = await GetChangesetAsync(_mapper.Map<RelationEditorVM>(rel), vm, rel.Id, principal, revertedId);
            _db.Changes.Add(changeset);

            _mapper.Map(vm, rel);
            MapComplementaryRelation(rel, compRel);
            rel.IsDeleted = rel.IsDeleted && revertedId != null;

            await _validator.ValidateAsync(new[] {rel, compRel});

            _cache.Clear();
        }

        /// <summary>
        /// Returns the brief information about a relation for reviewing before removal.
        /// </summary>
        public async Task<RelationTitleVM> RequestRemoveAsync(Guid id)
        {
            return await _db.Relations
                            .Where(x => x.IsDeleted == false && x.IsComplementary == false)
                            .ProjectTo<RelationTitleVM>()
                            .GetAsync(x => x.Id == id, "Связь не найдена");
        }

        /// <summary>
        /// Removes the relation.
        /// </summary>
        public async Task RemoveAsync(Guid id, ClaimsPrincipal principal)
        {
            var rel = await _db.Relations
                               .GetAsync(x => x.Id == id
                                              && x.IsComplementary == false
                                              && x.IsDeleted == false, "Связь не найдена");

            var compRel = await FindComplementaryRelationAsync(rel);

            var changeset = await GetChangesetAsync(_mapper.Map<RelationEditorVM>(rel), null, id, principal, null);
            _db.Changes.Add(changeset);

            rel.IsDeleted = true;
            _db.Relations.Remove(compRel);

            _cache.Clear();
        }

        /// <summary>
        /// Returns extended properties based on the relation type.
        /// </summary>
        public RelationEditorPropertiesVM GetPropertiesForRelationType(RelationType relType)
        {
            return new RelationEditorPropertiesVM
            {
                SourceName = RelationHelper.ComplementaryRelations[relType].GetEnumDescription(),
                DestinationName = relType.GetEnumDescription(),
                SourceTypes = RelationHelper.SuggestSourcePageTypes(relType),
                DestinationTypes = RelationHelper.SuggestDestinationPageTypes(relType),
                ShowDuration = RelationHelper.IsRelationDurationAllowed(relType),
                ShowEvent = RelationHelper.IsRelationEventReferenceAllowed(relType)
            };
        }

        #region Helpers

        /// <summary>
        /// Completes and\or corrects the search request.
        /// </summary>
        private RelationsListRequestVM NormalizeListRequest(RelationsListRequestVM vm)
        {
            if (vm == null)
                vm = new RelationsListRequestVM();

            var orderableFields = new[] {nameof(RelationTitleVM.Destination), nameof(RelationTitleVM.Source), nameof(RelationTitleVM.Type)};
            if (!orderableFields.Contains(vm.OrderBy))
                vm.OrderBy = orderableFields[0];

            if (vm.Page < 0)
                vm.Page = 0;

            return vm;
        }

        /// <summary>
        /// Checks if the create/update request contains valid data.
        /// </summary>
        private async Task ValidateRequestAsync(RelationEditorVM vm)
        {
            var val = new Validator();

            var pageIds = new [] {vm.SourceId, vm.DestinationId, vm.EventId ?? Guid.Empty};
            var pages = await _db.Pages
                                 .Where(x => pageIds.Contains(x.Id))
                                 .ToDictionaryAsync(x => x.Id, x => x.Type);

            var sourceType = pages.TryGetNullableValue(vm.SourceId ?? Guid.Empty);
            var destType = pages.TryGetNullableValue(vm.DestinationId ?? Guid.Empty);
            var eventType = pages.TryGetNullableValue(vm.EventId ?? Guid.Empty);

            if(vm.SourceId == null)
                val.Add(nameof(vm.SourceId), "Выберите страницу");
            else if (sourceType == null)
                val.Add(nameof(vm.SourceId), "Страница не найдена");

            if(vm.DestinationId == null)
                val.Add(nameof(vm.DestinationId), "Выберите страницу");
            else if (destType == null)
                val.Add(nameof(vm.DestinationId), "Страница не найдена");

            if (sourceType != null && destType != null && !RelationHelper.IsRelationAllowed(sourceType.Value, destType.Value, vm.Type))
                val.Add(nameof(vm.Type), "Тип связи недопустимм для данных страниц");

            if (vm.EventId != null)
            {
                if(eventType == null)
                    val.Add(nameof(vm.EventId), "Страница не найдена");
                else if(eventType != PageType.Event)
                    val.Add(nameof(vm.EventId), "Требуется страница события");
                else if(!RelationHelper.IsRelationEventReferenceAllowed(vm.Type))
                    val.Add(nameof(vm.EventId), "Событие нельзя привязать к данному типу связи");
            }

            if (!string.IsNullOrEmpty(vm.DurationStart) || !string.IsNullOrEmpty(vm.DurationEnd))
            {
                if (!RelationHelper.IsRelationDurationAllowed(vm.Type))
                {
                    val.Add(nameof(vm.DurationStart), "Дату нельзя указать для данного типа связи");
                }
                else
                {
                    var from = FuzzyDate.TryParse(vm.DurationStart);
                    var to = FuzzyDate.TryParse(vm.DurationEnd);

                    if (from > to)
                        val.Add(nameof(vm.DurationStart), "Дата начала не может быть больше даты конца");
                    else if (FuzzyRange.TryParse(FuzzyRange.TryCombine(vm.DurationStart, vm.DurationEnd)) == null)
                        val.Add(nameof(vm.DurationStart), "Введите дату в корректном формате");

                }
            }

            var existingRelation = await _db.Relations
                                            .AnyAsync(x => x.SourceId == vm.SourceId
                                                           && x.DestinationId == vm.DestinationId
                                                           && x.Type == vm.Type
                                                           && x.Id != vm.Id);

            if (existingRelation)
                val.Add(nameof(vm.DestinationId), "Такая связь уже существует!");

            val.ThrowIfInvalid();
        }
        
        /// <summary>
        /// Gets the changeset for updates.
        /// </summary>
        private async Task<Changeset> GetChangesetAsync(RelationEditorVM prev, RelationEditorVM next, Guid id, ClaimsPrincipal principal, Guid? revertedId)
        {
            if(prev == null && next == null)
                throw new ArgumentNullException();

            var userId = _userMgr.GetUserId(principal);
            var user = await _db.Users.GetAsync(x => x.Id == userId, "Пользователь не найден");

            return new Changeset
            {
                Id = Guid.NewGuid(),
                RevertedChangesetId = revertedId,
                Type = ChangesetEntityType.Relation,
                Date = DateTime.Now,
                EditedRelationId = id,
                Author = user,
                OriginalState = prev == null ? null : JsonConvert.SerializeObject(prev),
                UpdatedState = next == null ? null : JsonConvert.SerializeObject(next),
            };
        }

        /// <summary>
        /// Creates a complimentary inverse relation.
        /// </summary>
        private void MapComplementaryRelation(Relation source, Relation target)
        {
            target.SourceId = source.DestinationId;
            target.DestinationId = source.SourceId;
            target.Type = RelationHelper.ComplementaryRelations[source.Type];
            target.EventId = source.EventId;
            target.Duration = source.Duration;
            target.IsComplementary = true;
        }

        /// <summary>
        /// Removes the complementary relation (it is always recreated).
        /// </summary>
        private async Task<Relation> FindComplementaryRelationAsync(Relation rel)
        {
            var compRelType = RelationHelper.ComplementaryRelations[rel.Type];
            return await _db.Relations
                            .FirstOrDefaultAsync(x => x.SourceId == rel.DestinationId
                                                      && x.DestinationId == rel.SourceId
                                                      && x.Type == compRelType
                                                      && x.IsComplementary);
        }

        #endregion
    }
}
