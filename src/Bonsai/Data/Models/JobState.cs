using System;
using System.ComponentModel.DataAnnotations;

namespace Bonsai.Data.Models;

public class JobState
{
    /// <summary>
    /// Surrogate ID.
    /// </summary>
    [Key]
    public Guid Id { get; set; }
        
    /// <summary>
    /// Full name of the job type.
    /// </summary>
    public string Type { get; set; }
        
    /// <summary>
    /// Full name of the arguments type.
    /// </summary>
    public string ArgumentsType { get; set; }
        
    /// <summary>
    /// Serialized arguments.
    /// </summary>
    public string Arguments { get; set; }
        
    /// <summary>
    /// Date of the job registration.
    /// </summary>
    public DateTime StartDate { get; set; }
        
    /// <summary>
    /// Date of the job's completion (either successful or not).
    /// </summary>
    public DateTime? FinishDate { get; set; }
        
    /// <summary>
    /// Flag indicating that the job has successfully completed.
    /// </summary>
    public bool? Success { get; set; }
}