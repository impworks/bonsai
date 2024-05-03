namespace Bonsai.Areas.Admin.ViewModels.Media;

/// <summary>
/// Data about an uploaded media file.
/// </summary>
public class MediaUploadRequestVM
{
    /// <summary>
    /// Flag indicating that file name should be used as title (sanitized). 
    /// </summary>
    public bool UseFileNameAsTitle { get; set; }
        
    /// <summary>
    /// Optional title for the page.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Date of the media's creation.
    /// </summary>
    public string Date { get; set; }

    /// <summary>
    /// Location (title or page's GUID).
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// Event (title or page's GUID).
    /// </summary>
    public string Event { get; set; }
}