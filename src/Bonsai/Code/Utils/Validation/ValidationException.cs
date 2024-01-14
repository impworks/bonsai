using System;
using System.Collections.Generic;
using System.Linq;
using Impworks.Utils.Linq;

namespace Bonsai.Code.Utils.Validation
{
    /// <summary>
    /// Exception that fires when invalid arguments have been provided for a web operation.
    /// </summary>
    public class ValidationException: Exception
    {
        public ValidationException(IReadOnlyList<KeyValuePair<string, string>> errors)
            : this(GetErrorDescriptions(errors), errors)
        {
        }

        public ValidationException(string message, IReadOnlyList<KeyValuePair<string, string>> errors)
            : base(message + "\n" + GetErrorDescriptions(errors))
        {
            Errors = errors;
        }

        public ValidationException(string key, string value)
            : this(new[] {new KeyValuePair<string, string>(key, value)})
        {

        }

        private static string GetErrorDescriptions(IReadOnlyList<KeyValuePair<string, string>> errors)
        {
            return errors.Select(x => $"{x.Key}: {x.Value}").JoinString("\n");
        }

        /// <summary>
        /// List of validation errors.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, string>> Errors { get; }
    }
}
