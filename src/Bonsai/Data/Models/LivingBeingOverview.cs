using System;
using System.ComponentModel.DataAnnotations;

namespace Bonsai.Data.Models;

public class LivingBeingOverview
{
    /// <summary>
    /// Surrogate key of the related page.
    /// </summary>
    [Key]
    public Guid PageId { get; set; }

    /// <summary>
    /// Flag indicating that the person is male.
    /// </summary>
    public bool? Gender { get; set; }

    /// <summary>
    /// Date of birth in FuzzyDate format.
    /// </summary>
    public string BirthDate { get; set; }
        
    /// <summary>
    /// Date of death in FuzzyDate format.
    /// </summary>
    public string DeathDate { get; set; }

    /// <summary>
    /// Flag indicating that the person is deceased.
    /// Must be true if DeathDate is set, but not always vice versa.
    /// </summary>
    public bool IsDead { get; set; }

    /// <summary>
    /// Short name of the person, or pet nickname.
    /// </summary>
    public string ShortName { get; set; }

    /// <summary>
    /// Maiden name of a woman.
    /// </summary>
    public string MaidenName { get; set; }
}