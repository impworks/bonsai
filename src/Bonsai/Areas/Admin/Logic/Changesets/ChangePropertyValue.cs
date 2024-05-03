namespace Bonsai.Areas.Admin.Logic.Changesets;

/// <summary>
/// Property name to value binding.
/// <param name="PropertyName">Original name of the property (as in source code).</param>
/// <param name="Title">Readable of the property.</param>
/// <param name="Value">Rendered value.</param>
/// </summary>
public record ChangePropertyValue(string PropertyName, string Title, string Value);