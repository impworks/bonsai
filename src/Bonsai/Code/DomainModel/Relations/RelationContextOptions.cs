namespace Bonsai.Code.DomainModel.Relations;

/// <summary>
/// Options for loading the RelationContext.
/// </summary>
public class RelationContextOptions
{
    /// <summary>
    /// Loads only pages of type "Person" and their relations.
    /// </summary>
    public bool PeopleOnly { get; init; }

    /// <summary>
    /// Omits loading relations.
    /// </summary>
    public bool PagesOnly { get; init; }

    /// <summary>
    /// Omits loading relations except for "Parent", "Child" and "Spouse" which are required for the tree.
    /// </summary>
    public bool TreeRelationsOnly { get; init; }
}