using System;
using System.Collections.Generic;
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

            var result = new RelationsListVM {Request = request};
            await FillAdditionalDataAsync(request, result);

            var query = _db.Relations.Where(x => x.IsComplementary == false && x.IsDeleted == false);

            if (request.EntityId != null)
                query = query.Where(x => x.DestinationId == request.EntityId
                                         || x.SourceId == request.EntityId
                                         || x.EventId == request.EntityId);

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
            result.PageCount = (int) Math.Ceiling((double) totalCount / PageSize);

            var dir = request.OrderDescending ?? false;
            if (request.OrderBy == nameof(RelationTitleVM.Destination))
                query = query.OrderBy(x => x.Destination.Title, dir);
            else if (request.OrderBy == nameof(RelationTitleVM.Source))
                query = query.OrderBy(x => x.Source.Title, dir);
            else
                query = query.OrderBy(x => x.Type, dir);

            result.Items = await query.ProjectTo<RelationTitleVM>(_mapper.ConfigurationProvider)
                                      .Skip(PageSize * request.Page)
                                      .Take(PageSize)
                                      .ToListAsync();

            return result;
        }

        /// <summary>
        /// Creates a new relation.
        /// </summary>
        public async Task CreateAsync(RelationEditorVM vm, ClaimsPrincipal principal)
        {
            await ValidateRequestAsync(vm, isNew: true);

            var newRels = new List<Relation>();
            var updatedRels = new List<Relation>();
            var groupId = vm.SourceIds.Length > 1 ? Guid.NewGuid() : (Guid?) null;

            var removedRelations = await _db.Relations
                                            .Where(x => vm.SourceIds.Contains(x.SourceId)
                                                && x.DestinationId == vm.DestinationId
                                                && x.Type == vm.Type
                                                && x.Id != vm.Id
                                                && x.IsDeleted == true)
                                            .ToDictionaryAsync(x => x.SourceId, x => x);

            var user = await GetUserAsync(principal);
            foreach (var srcId in vm.SourceIds)
            {
                Relation rel;

                if (removedRelations.TryGetValue(srcId, out rel))
                {
                    rel.IsDeleted = false;

                    updatedRels.Add(rel);
                }
                else
                {
                    rel = _mapper.Map<Relation>(vm);
                    rel.Id = Guid.NewGuid();
                    rel.SourceId = srcId;

                    newRels.Add(rel);
                }

                var compRel = new Relation {Id = Guid.NewGuid()};
                MapComplementaryRelation(rel, compRel);
                newRels.Add(compRel);

                _db.Changes.Add(GetChangeset(null, _mapper.Map<RelationEditorVM>(rel), rel.Id, user, null, groupId));
            }

            await _validator.ValidateAsync(newRels.Concat(updatedRels).ToList());

            _db.Relations.AddRange(newRels);

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
            await ValidateRequestAsync(vm, isNew: false);

            var rel = await _db.Relations
                               .GetAsync(x => x.Id == vm.Id
                                              && x.IsComplementary == false
                                              && (x.IsDeleted == false || revertedId != null),
                                         "Связь не найдена");

            var compRel = await FindComplementaryRelationAsync(rel, revertedId != null);

            var user = await GetUserAsync(principal);
            var prevVm = rel.IsDeleted ? null : _mapper.Map<RelationEditorVM>(rel);
            var changeset = GetChangeset(prevVm, vm, rel.Id, user, revertedId);
            _db.Changes.Add(changeset);

            _mapper.Map(vm, rel);
            MapComplementaryRelation(rel, compRel);

            if(revertedId != null)
                rel.IsDeleted = false;

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
                            .ProjectTo<RelationTitleVM>(_mapper.ConfigurationProvider)
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

            var user = await GetUserAsync(principal);
            var changeset = GetChangeset(_mapper.Map<RelationEditorVM>(rel), null, id, user, null);
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

            if (vm.OrderDescending == null)
                vm.OrderDescending = false;

            return vm;
        }

        /// <summary>
        /// Checks if the create/update request contains valid data.
        /// </summary>
        private async Task ValidateRequestAsync(RelationEditorVM vm, bool isNew)
        {
            var val = new Validator();

            vm.SourceIds = vm.SourceIds ?? new Guid[0];

            var pageIds = vm.SourceIds
                            .Concat(new [] {vm.DestinationId ?? Guid.Empty, vm.EventId ?? Guid.Empty})
                            .ToList();

            var pages = await _db.Pages
                                 .Where(x => pageIds.Contains(x.Id))
                                 .ToDictionaryAsync(x => x.Id, x => x.Type);

            var sourceTypes = vm.SourceIds.Select(x => pages.TryGetNullableValue(x)).ToList();
            var destType = pages.TryGetNullableValue(vm.DestinationId ?? Guid.Empty);
            var eventType = pages.TryGetNullableValue(vm.EventId ?? Guid.Empty);

            if(vm.SourceIds == null || vm.SourceIds.Length == 0)
                val.Add(nameof(vm.SourceIds), "Выберите страницу");
            else if (isNew == false && vm.SourceIds.Length > 1)
                val.Add(nameof(vm.SourceIds), "При редактировании может быть указана только одна страница");
            else if (sourceTypes.Any(x => x == null))
                val.Add(nameof(vm.SourceIds), "Страница не найдена");

            if(vm.DestinationId == null)
                val.Add(nameof(vm.DestinationId), "Выберите страницу");
            else if (destType == null)
                val.Add(nameof(vm.DestinationId), "Страница не найдена");

            if (destType != null && sourceTypes.Any(x => x != null && !RelationHelper.IsRelationAllowed(x.Value, destType.Value, vm.Type)))
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
                                            .AnyAsync(x => vm.SourceIds.Contains(x.SourceId)
                                                           && x.DestinationId == vm.DestinationId
                                                           && x.Type == vm.Type
                                                           && x.Id != vm.Id
                                                           && x.IsDeleted == false);

            if (existingRelation)
                val.Add(nameof(vm.DestinationId), "Такая связь уже существует!");

            val.ThrowIfInvalid();
        }
        
        /// <summary>
        /// Gets the changeset for updates.
        /// </summary>
        private Changeset GetChangeset(RelationEditorVM prev, RelationEditorVM next, Guid id, AppUser user, Guid? revertedId, Guid? groupId = null)
        {
            if(prev == null && next == null)
                throw new ArgumentNullException();

            return new Changeset
            {
                Id = Guid.NewGuid(),
                RevertedChangesetId = revertedId,
                GroupId = groupId,
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
        private async Task<Relation> FindComplementaryRelationAsync(Relation rel, bool includeDeleted = false)
        {
            if (includeDeleted)
            {
                var compRel = new Relation {Id = Guid.NewGuid()};
                _db.Relations.Add(compRel);
                return compRel;
            }

            var compRelType = RelationHelper.ComplementaryRelations[rel.Type];
            return await _db.Relations
                            .FirstOrDefaultAsync(x => x.SourceId == rel.DestinationId
                                                      && x.DestinationId == rel.SourceId
                                                      && x.Type == compRelType
                                                      && x.IsComplementary);
        }
                
        /// <summary>
        /// Loads extra data for the filter.
        /// </summary>
        private async Task FillAdditionalDataAsync(RelationsListRequestVM request, RelationsListVM data)
        {
            if (request.EntityId != null)
            {
                var title = await _db.Pages
                                     .Where(x => x.Id == request.EntityId)
                                     .Select(x => x.Title)
                                     .FirstOrDefaultAsync();

                if (title != null)
                    data.EntityTitle = title;
                else
                    request.EntityId = null;
            }
        }

        /// <summary>
        /// Returns the user corresponding to this principal.
        /// </summary>
        private async Task<AppUser> GetUserAsync(ClaimsPrincipal principal)
        {
            var userId = _userMgr.GetUserId(principal);
            return await _db.Users.GetAsync(x => x.Id == userId, "Пользователь не найден");
        }

        #endregion
    }
}
