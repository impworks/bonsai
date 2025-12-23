using System.ComponentModel.DataAnnotations;
using Bonsai.Data.Models;

namespace Bonsai.Code.Services.Config;

/// <summary>
/// General application configuration.
/// </summary>
public class DynamicConfig
{
    /// <summary>
    /// The title of the website. Displayed in the top bar and browser title.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    /// <summary>
    /// Flag indicating that the website allows unauthorized visitors to view the contents.
    /// </summary>
    public bool AllowGuests { get; set; }

    /// <summary>
    /// Flag indicating that new registrations are accepted.
    /// </summary>
    public bool AllowRegistration { get; set; }
        
    /// <summary>
    /// Flag indicating that the black ribbon should not be displayed for deceased relatives in tree view.
    /// </summary>
    public bool HideBlackRibbon { get; set; }

    /// <summary>
    /// Tree render thoroughness coefficient.
    /// </summary>
    public int TreeRenderThoroughness { get; set; }

    /// <summary>
    /// Allowed kinds of trees.
    /// </summary>
    public TreeKind TreeKinds { get; set; }

    /// <summary>
    /// Flag indicating whether the MCP server is enabled for AI agent access.
    /// </summary>
    public bool McpEnabled { get; set; }
}