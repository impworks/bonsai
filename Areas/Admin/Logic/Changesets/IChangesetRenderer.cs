using System.Collections.Generic;
using System.Threading.Tasks;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.Logic.Changesets
{
    public interface IChangesetRenderer
    {
        /// <summary>
        /// Type of the entity this renderer can handle.
        /// </summary>
        ChangesetEntityType EntityType { get; }

        /// <summary>
        /// Renders JSON representation of an editor into a set of property values.
        /// </summary>
        Task<IReadOnlyList<ChangePropertyValue>> RenderValuesAsync(string json);

        /// <summary>
        /// Returns a customized diff for fields that are not supported by standard diff.
        /// </summary>
        string GetCustomDiff(string propName, string oldValue, string newValue);
    }
}
