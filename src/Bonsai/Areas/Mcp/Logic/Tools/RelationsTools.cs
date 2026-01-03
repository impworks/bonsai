using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.Logic.Tree;
using Bonsai.Areas.Admin.ViewModels.Relations;
using Bonsai.Areas.Mcp.Logic.Auth;
using Bonsai.Areas.Mcp.Logic.Services;
using Bonsai.Code.Services.Jobs;
using Bonsai.Data;
using Bonsai.Data.Models;
using ModelContextProtocol.Server;

namespace Bonsai.Areas.Mcp.Logic.Tools;

/// <summary>
/// MCP tools for managing relations between pages.
/// </summary>
[McpServerToolType]
public class RelationsTools(
    RelationsManagerService relationsManagerService,
    McpToolAuthorizationService authService,
    McpUserContext userContext,
    AppDbContext db,
    IBackgroundJobService jobs)
{
    /// <summary>
    /// Lists relations with optional filters.
    /// </summary>
    [McpServerTool(Name = "list_relations")]
    [Description("List relations between pages. Requires Editor role. Relations define family connections, friendships, ownership, and other links between people, pets, locations, and events.")]
    public async Task<ListRelationsResult> ListRelations(
        [Description("Filter by entity ID - show only relations involving this page")] string entityId = null,
        [Description("Filter by relation types (comma-separated: Parent, Child, Spouse, StepParent, StepChild, Friend, Colleague, Owner, Pet, Location, LocationInhabitant, Event, EventVisitor, Other)")] string types = null,
        [Description("Search query to filter by page titles")] string searchQuery = null,
        [Description("Field to order by (Destination, Source, Type)")] string orderBy = "Destination",
        [Description("Order descending (default: false)")] bool orderDescending = false,
        [Description("Page number for pagination (0-based, default: 0)")] int page = 0)
    {
        await authService.RequireRoleAsync(UserRole.Editor);

        var typesList = string.IsNullOrEmpty(types)
            ? null
            : types.Split(',')
                   .Select(t => Enum.TryParse<RelationType>(t.Trim(), true, out var rt) ? rt : (RelationType?)null)
                   .Where(t => t.HasValue)
                   .Select(t => t.Value)
                   .ToArray();

        var request = new RelationsListRequestVM
        {
            EntityId = string.IsNullOrEmpty(entityId) ? null : Guid.Parse(entityId),
            Types = typesList,
            SearchQuery = searchQuery,
            OrderBy = orderBy,
            OrderDescending = orderDescending,
            Page = page
        };

        var result = await relationsManagerService.GetRelationsAsync(request);

        return new ListRelationsResult
        {
            Relations = result.Items.Select(r => new RelationListItem
            {
                Id = r.Id,
                Type = r.Type.ToString(),
                SourceId = r.Source?.Id ?? Guid.Empty,
                SourceKey = r.Source?.Key,
                SourceTitle = r.Source?.Title,
                SourceType = r.Source?.Type.ToString(),
                DestinationId = r.Destination?.Id ?? Guid.Empty,
                DestinationKey = r.Destination?.Key,
                DestinationTitle = r.Destination?.Title,
                DestinationType = r.Destination?.Type.ToString(),
                Duration = r.Duration?.ReadableRange
            }).ToList(),
            TotalPages = result.PageCount,
            CurrentPage = request.Page,
            EntityTitle = result.EntityTitle
        };
    }

    /// <summary>
    /// Reads relation details.
    /// </summary>
    [McpServerTool(Name = "read_relation")]
    [Description("Read details of a specific relation by its ID. Requires Editor role.")]
    public async Task<ReadRelationResult> ReadRelation(
        [Description("Relation ID (GUID)")] string relationId)
    {
        await authService.RequireRoleAsync(UserRole.Editor);

        var id = Guid.Parse(relationId);
        var rel = await relationsManagerService.RequestUpdateAsync(id);
        var props = relationsManagerService.GetPropertiesForRelationType(rel.Type);

        return new ReadRelationResult
        {
            Id = rel.Id,
            Type = rel.Type.ToString(),
            TypeDescription = props.DestinationName,
            SourceIds = rel.SourceIds,
            DestinationId = rel.DestinationId,
            EventId = rel.EventId,
            DurationStart = rel.DurationStart,
            DurationEnd = rel.DurationEnd,
            SourceName = props.SourceName,
            DestinationName = props.DestinationName,
            AllowsDuration = props.ShowDuration,
            AllowsEvent = props.ShowEvent
        };
    }

    /// <summary>
    /// Creates a new relation.
    /// </summary>
    [McpServerTool(Name = "create_relation")]
    [Description("Create a new relation between pages. Requires Editor role. Common relation types include: Parent/Child (family), Spouse (marriage), Owner/Pet (pet ownership), Friend, Colleague, Location/LocationInhabitant.")]
    public async Task<CreateRelationResult> CreateRelation(
        [Description("Relation type that describes the destination page (Parent, Child, Spouse, StepParent, StepChild, Friend, Colleague, Owner, Pet, Location, LocationInhabitant, Event, EventVisitor, Other)")] string type,
        [Description("Source page ID (GUID) - the person/entity on one side of the relation. Example: for a 'Child' relation type, this needs the parent's page ID.")] string sourceId,
        [Description("Destination page ID (GUID) - the person/entity on the other side of the relation. Example: for a 'Child' relation type, this needs the child's page ID.")] string destinationId,
        [Description("Event page ID (GUID) for relations like marriage (optional)")] string eventId = null,
        [Description("Duration start date FuzzyDate format (optional)")] string durationStart = null,
        [Description("Duration end date in FuzzyDate format (optional)")] string durationEnd = null)
    {
        await authService.RequireRoleAsync(UserRole.Editor);

        var relType = Enum.Parse<RelationType>(type, ignoreCase: true);

        var vm = new RelationEditorVM
        {
            Type = relType,
            SourceIds = [Guid.Parse(sourceId)],
            DestinationId = Guid.Parse(destinationId),
            EventId = string.IsNullOrEmpty(eventId) ? null : Guid.Parse(eventId),
            DurationStart = durationStart,
            DurationEnd = durationEnd
        };

        await relationsManagerService.CreateAsync(vm, userContext.Principal);
        await db.SaveChangesAsync();
        await jobs.RunAsync(JobBuilder.For<TreeLayoutJob>().SupersedeAll());

        return new CreateRelationResult
        {
            Success = true,
            Type = relType.ToString(),
            Message = $"Relation of type '{relType}' created successfully"
        };
    }

    /// <summary>
    /// Updates an existing relation.
    /// </summary>
    [McpServerTool(Name = "update_relation")]
    [Description("Update an existing relation. Requires Editor role.")]
    public async Task<UpdateRelationResult> UpdateRelation(
        [Description("Relation ID (GUID)")] string relationId,
        [Description("New relation type (optional)")] string type = null,
        [Description("New destination page ID (optional)")] string destinationId = null,
        [Description("New event page ID (optional)")] string eventId = null,
        [Description("New duration start date in FuzzyDate format (optional)")] string durationStart = null,
        [Description("New duration end date in FuzzyDate format (optional)")] string durationEnd = null)
    {
        await authService.RequireRoleAsync(UserRole.Editor);

        var id = Guid.Parse(relationId);
        var current = await relationsManagerService.RequestUpdateAsync(id);

        if (type != null)
            current.Type = Enum.Parse<RelationType>(type, ignoreCase: true);

        if (destinationId != null)
            current.DestinationId = Guid.Parse(destinationId);

        if (eventId != null)
            current.EventId = string.IsNullOrEmpty(eventId) ? null : Guid.Parse(eventId);

        if (durationStart != null)
            current.DurationStart = durationStart;

        if (durationEnd != null)
            current.DurationEnd = durationEnd;

        await relationsManagerService.UpdateAsync(current, userContext.Principal);
        await db.SaveChangesAsync();
        await jobs.RunAsync(JobBuilder.For<TreeLayoutJob>().SupersedeAll());

        return new UpdateRelationResult
        {
            Id = id,
            Success = true
        };
    }

    /// <summary>
    /// Deletes a relation (soft delete).
    /// </summary>
    [McpServerTool(Name = "delete_relation")]
    [Description("Delete a relation (soft delete, can be restored). Requires Editor role.")]
    public async Task<DeleteRelationResult> DeleteRelation(
        [Description("Relation ID (GUID)")] string relationId)
    {
        await authService.RequireRoleAsync(UserRole.Editor);

        var id = Guid.Parse(relationId);
        await relationsManagerService.RemoveAsync(id, userContext.Principal);
        await db.SaveChangesAsync();
        await jobs.RunAsync(JobBuilder.For<TreeLayoutJob>().SupersedeAll());

        return new DeleteRelationResult
        {
            Id = id,
            Success = true
        };
    }

    /// <summary>
    /// Gets valid relation types and their properties.
    /// </summary>
    [McpServerTool(Name = "get_relation_types")]
    [Description("Get information about all available relation types and their properties (allowed page types, duration support, etc).")]
    public async Task<RelationTypesResult> GetRelationTypes()
    {
        await authService.RequireRoleAsync(UserRole.User);

        var types = Enum.GetValues<RelationType>()
            .Select(t =>
            {
                var props = relationsManagerService.GetPropertiesForRelationType(t);
                return new RelationTypeInfo
                {
                    Type = t.ToString(),
                    SourceName = props.SourceName,
                    DestinationName = props.DestinationName,
                    SourcePageTypes = props.SourceTypes.Select(pt => pt.ToString()).ToList(),
                    DestinationPageTypes = props.DestinationTypes.Select(pt => pt.ToString()).ToList(),
                    AllowsDuration = props.ShowDuration,
                    AllowsEvent = props.ShowEvent
                };
            })
            .ToList();

        return new RelationTypesResult
        {
            Types = types
        };
    }
}

#region Result Types

public class ListRelationsResult
{
    public List<RelationListItem> Relations { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public string EntityTitle { get; set; }
}

public class RelationListItem
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public Guid SourceId { get; set; }
    public string SourceKey { get; set; }
    public string SourceTitle { get; set; }
    public string SourceType { get; set; }
    public Guid DestinationId { get; set; }
    public string DestinationKey { get; set; }
    public string DestinationTitle { get; set; }
    public string DestinationType { get; set; }
    public string Duration { get; set; }
}

public class ReadRelationResult
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string TypeDescription { get; set; }
    public Guid[] SourceIds { get; set; }
    public Guid? DestinationId { get; set; }
    public Guid? EventId { get; set; }
    public string DurationStart { get; set; }
    public string DurationEnd { get; set; }
    public string SourceName { get; set; }
    public string DestinationName { get; set; }
    public bool AllowsDuration { get; set; }
    public bool AllowsEvent { get; set; }
}

public class CreateRelationResult
{
    public bool Success { get; set; }
    public string Type { get; set; }
    public string Message { get; set; }
}

public class UpdateRelationResult
{
    public Guid Id { get; set; }
    public bool Success { get; set; }
}

public class DeleteRelationResult
{
    public Guid Id { get; set; }
    public bool Success { get; set; }
}

public class RelationTypesResult
{
    public List<RelationTypeInfo> Types { get; set; }
}

public class RelationTypeInfo
{
    public string Type { get; set; }
    public string SourceName { get; set; }
    public string DestinationName { get; set; }
    public List<string> SourcePageTypes { get; set; }
    public List<string> DestinationPageTypes { get; set; }
    public bool AllowsDuration { get; set; }
    public bool AllowsEvent { get; set; }
}

#endregion
