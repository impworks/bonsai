using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.Logic.Validation;
using Bonsai.Areas.Admin.ViewModels.Relations;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;
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
        public RelationsManagerService(AppDbContext db, IMapper mapper, UserManager<AppUser> userMgr, RelationValidator validator)
        {
            _db = db;
            _mapper = mapper;
            _userMgr = userMgr;
            _validator = validator;
        }

        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userMgr;
        private readonly RelationValidator _validator;

        /// <summary>
        /// Returns the found relations.
        /// </summary>
        public async Task<RelationsListVM> GetRelationsAsync(RelationsListRequestVM request)
        {
            const int PageSize = 50;

            request = NormalizeListRequest(request);

            var query = _db.Relations.Where(x => x.IsComplementary == false);

            if (!string.IsNullOrEmpty(request.SearchQuery))
            {
                var req = request.SearchQuery.ToLower();
                query = query.Where(x => x.Destination.Title.ToLower().Contains(req)
                                         || x.Source.Title.ToLower().Contains(req)
                                         || x.Event.Title.ToLower().Contains(req));
            }

            if (request.Types?.Length > 0)
                query = query.Where(x => request.Types.Contains(x.Type));

            var totalCount = await query.CountAsync()
                                        .ConfigureAwait(false);

            if (request.OrderBy == nameof(RelationTitleVM.Destination))
                query = query.OrderBy(x => x.Destination.Title, request.OrderDescending);
            else if (request.OrderBy == nameof(RelationTitleVM.Source))
                query = query.OrderBy(x => x.Source.Title, request.OrderDescending);
            else
                query = query.OrderBy(x => x.Event.Title, request.OrderDescending);

            var items = await query.ProjectTo<RelationTitleVM>()
                                   .Skip(PageSize * request.Page)
                                   .Take(PageSize)
                                   .ToListAsync()
                                   .ConfigureAwait(false);

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
            await ValidateRequestAsync(vm).ConfigureAwait(false);

            var rel = _mapper.Map<Relation>(vm);
            rel.Id = Guid.NewGuid();

            var rels = new[] {rel, GetComplementaryRelation(vm)};

            await _validator.ValidateAsync(rels).ConfigureAwait(false);

            var changeset = await GetChangesetAsync(null, vm, rel.Id, principal).ConfigureAwait(false);
            _db.Changes.Add(changeset);

            _db.Relations.AddRange(rels);
        }

        /// <summary>
        /// Returns the form information for updating a relation.
        /// </summary>
        public async Task<RelationEditorVM> RequestUpdateAsync(Guid id)
        {
            var rel = await _db.Relations
                               .GetAsync(x => x.Id == id, "Связь не найдена")
                               .ConfigureAwait(false);

            return _mapper.Map<RelationEditorVM>(rel);
        }

        /// <summary>
        /// Updates the relation.
        /// </summary>
        public async Task UpdateAsync(RelationEditorVM vm, ClaimsPrincipal principal)
        {
            await ValidateRequestAsync(vm).ConfigureAwait(false);

            var rel = await _db.Relations
                               .GetAsync(x => x.Id == vm.Id, "Связь не найдена")
                               .ConfigureAwait(false);

            var changeset = await GetChangesetAsync(_mapper.Map<RelationEditorVM>(rel), vm, rel.Id, principal).ConfigureAwait(false);
            _db.Changes.Add(changeset);

            await RemoveComplementaryRelationAsync(rel).ConfigureAwait(false);
            _mapper.Map(vm, rel);

            var rels = new[] {rel, GetComplementaryRelation(vm)};
            await _validator.ValidateAsync(rels).ConfigureAwait(false);

            _db.Relations.AddRange(rels);
        }

        /// <summary>
        /// Removes the relation.
        /// </summary>
        public async Task RemoveAsync(Guid id, ClaimsPrincipal principal)
        {
            var rel = await _db.Relations
                               .GetAsync(x => x.Id == id, "Связь не найдена")
                               .ConfigureAwait(false);

            var changeset = await GetChangesetAsync(_mapper.Map<RelationEditorVM>(rel), null, id, principal).ConfigureAwait(false);
            _db.Changes.Add(changeset);

            await RemoveComplementaryRelationAsync(rel).ConfigureAwait(false);
            _db.Relations.Remove(rel);
        }

        #region Helpers

        /// <summary>
        /// Completes and\or corrects the search request.
        /// </summary>
        private RelationsListRequestVM NormalizeListRequest(RelationsListRequestVM vm)
        {
            if (vm == null)
                vm = new RelationsListRequestVM();

            var orderableFields = new[] {nameof(RelationTitleVM.Source), nameof(RelationTitleVM.Destination), nameof(RelationTitleVM.Event)};
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
                                 .ToDictionaryAsync(x => x.Id, x => x.Type)
                                 .ConfigureAwait(false);

            var sourceType = pages.TryGetNullableValue(vm.SourceId);
            var destType = pages.TryGetNullableValue(vm.DestinationId);
            var eventType = pages.TryGetNullableValue(vm.EventId ?? Guid.Empty);

            if (sourceType == null)
                val.Add(nameof(vm.SourceId), "Страница не найдена");

            if (destType == null)
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

            if (!string.IsNullOrEmpty(vm.Duration))
            {
                if(!RelationHelper.IsRelationDurationAllowed(vm.Type))
                    val.Add(nameof(vm.Duration), "Дату нельзя указать для данного типа связи");
                else if (FuzzyRange.TryParse(vm.Duration) == null)
                    val.Add(nameof(vm.Duration), "Введите дату в корректном формате");
            }

            var existingRelation = await _db.Relations
                                            .AnyAsync(x => x.SourceId == vm.SourceId
                                                           && x.DestinationId == vm.DestinationId
                                                           && x.Type == vm.Type
                                                           && x.Id != vm.Id)
                                            .ConfigureAwait(false);

            if (existingRelation)
                val.Add(nameof(vm.DestinationId), "Такая связь уже существует!");

            val.ThrowIfInvalid();
        }
        
        /// <summary>
        /// Gets the changeset for updates.
        /// </summary>
        private async Task<Changeset> GetChangesetAsync(RelationEditorVM prev, RelationEditorVM next, Guid id, ClaimsPrincipal principal)
        {
            if(prev == null && next == null)
                throw new ArgumentNullException();

            var userId = _userMgr.GetUserId(principal);
            var user = await _db.Users.GetAsync(x => x.Id == userId, "Пользователь не найден").ConfigureAwait(false);

            return new Changeset
            {
                Id = Guid.NewGuid(),
                Type = ChangesetEntityType.Relation,
                Date = DateTime.Now,
                EntityId = id,
                Author = user,
                OriginalState = prev == null ? null : JsonConvert.SerializeObject(prev),
                UpdatedState = next == null ? null : JsonConvert.SerializeObject(next),
            };
        }

        /// <summary>
        /// Creates a complimentary inverse relation.
        /// </summary>
        private Relation GetComplementaryRelation(RelationEditorVM vm)
        {
            return new Relation
            {
                Id = Guid.NewGuid(),
                SourceId = vm.DestinationId,
                DestinationId = vm.SourceId,
                Type = RelationHelper.ComplementaryRelations[vm.Type],
                EventId = vm.EventId,
                Duration = vm.Duration,
                IsComplementary = true
            };
        }

        /// <summary>
        /// Removes the complementary relation (it is always recreated).
        /// </summary>
        private async Task RemoveComplementaryRelationAsync(Relation rel)
        {
            var compRelType = RelationHelper.ComplementaryRelations[rel.Type];
            var compRel = await _db.Relations
                                   .FirstOrDefaultAsync(x => x.SourceId == rel.DestinationId
                                                             && x.DestinationId == rel.SourceId
                                                             && x.Type == compRelType
                                                             && x.IsComplementary)
                                   .ConfigureAwait(false);

            if (compRel != null)
                _db.Relations.Remove(compRel);
        }

        #endregion
    }
}
