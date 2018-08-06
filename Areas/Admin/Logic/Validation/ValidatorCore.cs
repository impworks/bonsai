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
        public void Validate(RelationContext context, Guid[] updatedPageIds = null)
        {
            CheckLifespans(context);
            CheckWeddings(context);
            CheckParentLifespans(context);

            if(updatedPageIds != null)
                foreach(var pageId in updatedPageIds)
                    if(pageId != Guid.Empty)
                        CheckLoops(context, pageId);
        }

        #region Checks

        /// <summary>
        /// Checks the context for inconsistent wedding information.
        /// </summary>
        private void CheckWeddings(RelationContext context)
        {
            foreach (var rel in context.Relations.Values.SelectMany(x => x))
            {
                if (rel.Type != RelationType.Spouse || rel.IsComplementary)
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

        /// <summary>
        /// Checks the lifespans consistency of each person/pet page.
        /// </summary>
        private void CheckLifespans(RelationContext context)
        {
            foreach (var page in context.Pages.Values)
            {
                if(page.BirthDate >= page.DeathDate)
                    AddViolation("Дата рождения не может быть раньше даты смерти", page.Id);
            }
        }

        /// <summary>
        /// Checks the context for inconsistencies with lifespans of parents/children.
        /// </summary>
        private void CheckParentLifespans(RelationContext context)
        {
            foreach (var rel in context.Relations.Values.SelectMany(x => x))
            {
                if (rel.Type != RelationType.Child)
                    continue;

                var parent = context.Pages[rel.SourceId];
                var child = context.Pages[rel.DestinationId];

                if(parent.BirthDate >= child.BirthDate)
                    AddViolation("Родитель не может быть старше ребенка", parent.Id, rel.Id);
            }
        }

        /// <summary>
        /// Finds loops of a particular relation in the relation graph.
        /// </summary>
        private void CheckLoops(RelationContext context, Guid pageId)
        {
            var isLoopFound = false;
            var visited = context.Pages.ToDictionary(x => x.Key, x => false);
            CheckLoopsInternal(pageId);

            void CheckLoopsInternal(Guid id)
            {
                if (isLoopFound)
                    return;

                if (visited[id])
                {
                    isLoopFound = true;
                    AddViolation("Два человека не могут быть родителями друг для друга", id);
                    return;
                }

                visited[id] = true;

                foreach(var rel in context.Relations[id])
                    if(rel.Type == RelationType.Parent)
                        CheckLoopsInternal(rel.DestinationId);
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
