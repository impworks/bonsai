using System;
using System.Collections.Generic;
using Impworks.Utils.Format;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bonsai.Code.Utils.Helpers
{
    /// <summary>
    /// Helper methods for using forms.
    /// </summary>
    public class FormHelper
    {
        /// <summary>
        /// Returns the select list with current element selected.
        /// </summary>
        public static IReadOnlyList<SelectListItem> GetEnumSelectList<T>(T? value, bool addEmpty = true)
            where T : struct, IConvertible, IComparable
        {
            var list = new List<SelectListItem>();
            if(value == null && addEmpty)
                list.Add(new SelectListItem { Text = "Не выбрано", Selected = true });

            foreach (var entry in EnumHelper.GetEnumDescriptions<T>())
            {
                list.Add(new SelectListItem
                {
                    Text = entry.Value,
                    Value = Convert.ToInt32(entry.Key).ToString(),
                    Selected = entry.Key.CompareTo(value) == 0
                });
            }

            return list;
        }
    }
}
