using JetBrains.Annotations;

namespace Bonsai.Code.DomainModel.Facts.Models;

/// <summary>
/// The template for specifying blood type.
/// </summary>
public class BloodTypeFactModel: FactModelBase
{
    /// <summary>
    /// Blood type.
    /// </summary>
    public BloodType Type { get; set; }

    /// <summary>
    /// Rhesus factor (if known).
    /// </summary>
    public bool? RhesusFactor { get; set; }
}

public enum BloodType
{
    [UsedImplicitly]
    O,
    [UsedImplicitly]
    A,
    [UsedImplicitly]
    B,
    [UsedImplicitly]
    AB
}