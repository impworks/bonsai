using Bonsai.Data.Models;

namespace Bonsai.Code.DomainModel.Relations;

/// <summary>
/// A list of allowed relation types between two types of pages.
/// <param name="SourceType">Type of the source page.</param>
/// <param name="DestinationType">Type of the destination page.</param>
/// <param name="RelationTypes">Types of the relation.</param>
/// </summary>
public record RelationBinding(PageType SourceType, PageType DestinationType, RelationType[] RelationTypes);