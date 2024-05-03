namespace Bonsai.Areas.Admin.ViewModels.Common;

/// <summary>
/// DTO for displaying a removal confirmation page for any entity.
/// </summary>
public class RemoveEntryInfoVM<T>
{
    /// <summary>
    /// Details of the entry to remove.
    /// </summary>
    public T Entry { get; set; }
        
    /// <summary>
    /// Flag indicating that the current user is allowed to irreversibly remove the entry.
    /// </summary>
    public bool CanRemoveCompletely { get; set; }
}