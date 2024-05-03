using System;
using System.Collections.Generic;
using System.Linq;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils.Date;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data.Models;
using Bonsai.Localization;

namespace Bonsai.Areas.Admin.Logic.Validation;

/// <summary>
/// The class with consistency validation logic.
/// </summary>
public class ValidatorCore
{
    public ValidatorCore()
    {
        _errors = new List<ConsistencyErrorInfo>();
    }

    private readonly List<ConsistencyErrorInfo> _errors;

    /// <summary>
    /// The list of found consistency violations.
    /// </summary>
    public IReadOnlyList<ConsistencyErrorInfo> Errors => _errors;

    /// <summary>
    /// Checks the context for contradictory facts.
    /// </summary>
    public void Validate(RelationContext context, Guid[] updatedPageIds = null)
    {
        CheckLifespans(context);
        CheckWeddings(context);
        CheckParents(context);
        CheckParentLifespans(context);

        if(updatedPageIds != null)
            foreach(var pageId in updatedPageIds)
                if(pageId != Guid.Empty)
                    CheckLoops(context, pageId);
    }

    /// <summary>
    /// Throws a ValidationException if there any violations found.
    /// </summary>
    public void ThrowIfInvalid(RelationContext context, string propName)
    {
        if (!Errors.Any())
            return;

        var msgs = new List<KeyValuePair<string, string>>();
        foreach (var err in Errors)
        {
            var msg = err.Message;
            if (err.PageIds?.Length > 0)
            {
                var pages = err.PageIds.Select(x => context.Pages[x].Title);
                msg += $" ({string.Join(", ", pages)})";
            }

            msgs.Add(new KeyValuePair<string, string>(propName, msg));
        }

        throw new ValidationException(msgs);
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

            if (first.BirthDate >= second.DeathDate)
                Error(string.Format(Texts.Admin_Validation_Page_SpouseLifetimesNoOverlap, first.BirthDate.Value.ReadableDate, second.DeathDate.Value.ReadableDate), first.Id, second.Id);

            if (second.BirthDate >= first.DeathDate)
                Error(string.Format(Texts.Admin_Validation_Page_SpouseLifetimesNoOverlap, second.BirthDate.Value.ReadableDate, first.DeathDate.Value.ReadableDate), first.Id, second.Id);

            if (rel.Duration is FuzzyRange dur)
            {
                if(dur.RangeStart < first.BirthDate || dur.RangeEnd > first.DeathDate)
                    Error(string.Format(Texts.Admin_Validation_Page_MarriageExceedsLifetime, new FuzzyRange(first.BirthDate, first.DeathDate).ReadableRange), first.Id);

                if(dur.RangeStart < second.BirthDate || dur.RangeEnd > second.DeathDate)
                    Error(string.Format(Texts.Admin_Validation_Page_MarriageExceedsLifetime, new FuzzyRange(second.BirthDate, second.DeathDate).ReadableRange), second.Id);
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
            if(page.BirthDate > page.DeathDate)
                Error(Texts.Admin_Validation_Page_BirthAfterDeath, page.Id);
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
                Error(Texts.Admin_Validation_Page_ParentYoungerThanChild, parent.Id, child.Id);
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
            if (isLoopFound || !context.Relations.ContainsKey(id))
                return;

            visited[id] = true;

            foreach (var rel in context.Relations[id])
            {
                if (rel.Type != RelationType.Parent)
                    continue;

                if (isLoopFound)
                    return;

                if (visited[rel.DestinationId])
                {
                    isLoopFound = true;
                    Error(Texts.Admin_Validation_Page_ParentLoop, rel.DestinationId, pageId);
                    return;
                }

                CheckLoopsInternal(rel.DestinationId);
            }
        }
    }

    /// <summary>
    /// Checks if a person has more than two parents.
    /// </summary>
    private void CheckParents(RelationContext context)
    {
        foreach (var page in context.Pages.Values)
        {
            if (!context.Relations.TryGetValue(page.Id, out var rels))
                continue;

            var parents = rels.Where(x => x.Type == RelationType.Parent)
                              .ToList();

            if(parents.Count > 2)
                Error(Texts.Admin_Validation_Page_ManyBioParents, page.Id);

            if (parents.Count == 2)
            {
                var p1 = context.Pages[parents[0].DestinationId];
                var p2 = context.Pages[parents[1].DestinationId];

                if(p1.Gender == p2.Gender && p1.Gender != null)
                    Error(Texts.Admin_Validation_Page_BioParentsSameGender, p1.Id, p2.Id, page.Id);
            }
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Adds a new violation info.
    /// </summary>
    private void Error(string msg, params Guid[] pages)
    {
        _errors.Add(new ConsistencyErrorInfo(msg, pages));
    }

    #endregion
}