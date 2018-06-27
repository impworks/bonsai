using System;
using System.Collections.Generic;
using System.Linq;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils.Date;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.Logic.Validation
{
    /// <summary>
    /// The class with consistency validation logic.
    /// </summary>
    public class ValidatorCore
    {
        public ValidatorCore()
        {
            _violations = new List<ConsistencyViolationInfo>();
        }

        private readonly List<ConsistencyViolationInfo> _violations;

        /// <summary>
        /// The list of found consistency violations.
        /// </summary>
        public IReadOnlyList<ConsistencyViolationInfo> Violations => _violations;

        /// <summary>
        /// Checks the context for contradictory facts.
        /// </summary>
        public void Validate(RelationContext context)
        {
            CheckWeddingConsistency(context);

            // todo: other validations
        }

        #region Checks

        /// <summary>
        /// Checks the context for inconsistent wedding information.
        /// </summary>
        private void CheckWeddingConsistency(RelationContext context)
        {
            foreach (var rel in context.Relations.Values.SelectMany(x => x))
            {
                if (rel.Type != RelationType.Spouse)
                    continue;

                var first = context.Pages[rel.SourceId];
                var second = context.Pages[rel.DestinationId];

                if(first.BirthDate >= second.DeathDate || second.BirthDate >= first.DeathDate)
                    AddViolation("Дата рождения одгого супруга не может быть раньше даты смерти другого", first.Id, rel.Id);

                if (rel.Duration is FuzzyRange dur)
                {
                    if(dur.RangeStart < first.BirthDate || dur.RangeEnd > first.DeathDate)
                        AddViolation("Брак должен быть ограничен временем жизни супруга", first.Id, rel.Id);

                    if(dur.RangeStart < second.BirthDate || dur.RangeEnd > second.DeathDate)
                        AddViolation("Брак должен быть ограничен временем жизни супруга", first.Id, rel.Id);
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Adds a new violation info.
        /// </summary>
        private void AddViolation(string msg, Guid? page, Guid? relation = null)
        {
            _violations.Add(new ConsistencyViolationInfo(msg, page, relation));
        }

        #endregion
    }
}
