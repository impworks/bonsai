using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Relations;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.Logic.Validation
{
    /// <summary>
    /// Validates added relations.
    /// </summary>
    public class RelationValidator
    {
        public RelationValidator(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

        /// <summary>
        /// Validates the context integrity.
        /// </summary>
        public async Task ValidateAsync(IReadOnlyList<Relation> relations)
        {
            var firstRel = relations?.FirstOrDefault();
            if(firstRel == null)
                throw new ArgumentNullException();

            var context = await RelationContext.LoadContextAsync(_db).ConfigureAwait(false);
            foreach(var rel in relations)
                context.Augment(CreateExcerpt(rel));

            var core = new ValidatorCore();
            core.Validate(context, new [] { firstRel.SourceId, firstRel.DestinationId, firstRel.EventId ?? Guid.Empty });

            if(core.Violations.Any())
            {
                throw new ValidationException(
                    nameof(RelationEditorVM.DestinationId),
                    string.Join(
                        "\n",
                        new[] { "Противоречивые факты:" }.Concat(
                            core.Violations.Select(x => x.Message)
                        )
                    )
                );
            }
        }

        /// <summary>
        /// Maps the relation to an excerpt.
        /// </summary>
        private RelationContext.RelationExcerpt CreateExcerpt(Relation rel)
        {
            return new RelationContext.RelationExcerpt
            {
                Id = rel.Id,
                SourceId = rel.SourceId,
                DestinationId = rel.DestinationId,
                Duration = FuzzyRange.TryParse(rel.Duration),
                EventId = rel.EventId,
                IsComplementary = rel.IsComplementary,
                Type = rel.Type
            };
        }
    }
}
