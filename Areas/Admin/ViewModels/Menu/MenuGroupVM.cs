using System.Collections.Generic;

namespace Bonsai.Areas.Admin.ViewModels.Menu
{
    /// <summary>
    /// A collection of grouped menu items.
    /// </summary>
    public class MenuGroupVM
    {
        public MenuGroupVM(params MenuItemVM[] items)
        {
            Items = items;
        }

        /// <summary>
        /// The items in current group.
        /// </summary>
        public IReadOnlyList<MenuItemVM> Items { get; }
    }
}
