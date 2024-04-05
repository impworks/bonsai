using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Common;
using Bonsai.Areas.Admin.ViewModels.Components;
using Bonsai.Code.Services;
using Impworks.Utils.Format;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Components
{
    /// <summary>
    /// Renders the filter for an enum.
    /// </summary>
    public class ListEnumFilterComponent: ViewComponent
    { 
        /// <summary>
        /// Renders the result.
        /// </summary>
        public async Task<IViewComponentResult> InvokeAsync(ListRequestVM request, string propName)
        {
            if (request == null || propName == null)
                throw new ArgumentNullException(nameof(request), "Either request or propName must be specified!");

            var prop = request.GetType().GetProperty(propName);
            if (prop == null)
                throw new ArgumentException($"Request of type '{request.GetType().Name}' does not have a property '{propName}'.");

            if (!prop.PropertyType.IsArray)
                throw new ArgumentException($"Property {request.GetType().Name}.{propName} must be an array.");

            var elemType = prop.PropertyType.GetElementType();
            if(!elemType.IsEnum)
                throw new ArgumentException($"Type '{prop.PropertyType.Name}' is not an enum.");

            var enumValues = GetEnumValues(elemType);
            var selected = prop.GetValue(request) as int[];
            var result = new List<ListEnumFilterItemVM>();

            foreach (var enumValue in enumValues)
            {
                result.Add(new ListEnumFilterItemVM
                {
                    PropertyName = propName,
                    Title = enumValue.Value,
                    Value = enumValue.Key.ToInvariantString(),
                    IsActive = selected?.Any(x => x == enumValue.Key) == true
                });
            }

            return View("~/Areas/Admin/Views/Components/ListEnumFilter.cshtml", result);
        }

        /// <summary>
        /// Gets the raw enum values and their descriptions.
        /// </summary>
        private Dictionary<int, string> GetEnumValues(Type enumType)
        {
            var flags = BindingFlags.DeclaredOnly
                        | BindingFlags.Static
                        | BindingFlags.Public
                        | BindingFlags.GetField;

            return enumType.GetFields(flags)
                           .Where(x => x.GetCustomAttribute<ObsoleteAttribute>() == null)
                           .Select(x => new
                           {
                               Description = LocaleProvider.GetLocaleEnumDescription(enumType, x.Name),
                               Value = x.GetRawConstantValue()
                           })
                           .ToDictionary(
                               x => Convert.ToInt32(x.Value),
                               x => x.Description ?? Enum.GetName(enumType, x.Value)
                           );
        }
    }
}
