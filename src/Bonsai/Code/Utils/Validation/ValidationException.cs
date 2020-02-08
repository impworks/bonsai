using System;
using System.Collections.Generic;

namespace Bonsai.Code.Utils.Validation
{
    /// <summary>
    /// Exception that fires when invalid arguments have been provided for a web operation.
    /// </summary>
    public class ValidationException: Exception
    {
        public ValidationException(IReadOnlyList<KeyValuePair<string, string>> errors)
            : this(null, errors)
        {
        }

        public ValidationException(string message, IReadOnlyList<KeyValuePair<string, string>> errors)
            : base(message)
        {
            Errors = errors;
        }

        public ValidationException(string key, string value)
            : this(new[] {new KeyValuePair<string, string>(key, value)})
        {

        }

        /// <summary>
        /// List of validation errors.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, string>> Errors { get; }
    }
}
