namespace Bonsai.Code.DomainModel.Facts.Models;

/// <summary>
/// The template for specifying arbitrary string-based facts.
/// </summary>
public class StringFactModel: FactModelBase
{
    /// <summary>
    /// Arbitrary value.
    /// </summary>
    public string Value { get; set; }
}