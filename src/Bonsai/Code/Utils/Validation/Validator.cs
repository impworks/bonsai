using System.Collections.Generic;

namespace Bonsai.Code.Utils.Validation
{
    /// <summary>
    /// Helper class for validation.
    /// </summary>
    public class Validator
    {
        public Validator()
        {
            _errors = new List<KeyValuePair<string, string>>();
        }

        private readonly List<KeyValuePair<string, string>> _errors;

        /// <summary>
        /// Indicates that there are currently no errors.
        /// </summary>
        public bool IsValid => _errors.Count == 0;

        /// <summary>
        /// Adds a new error to the list.
        /// </summary>
        public Validator Add(string key, string error)
        {
            _errors.Add(new KeyValuePair<string, string>(key, error));
            return this;
        }

        /// <summary>
        /// Returns the exception.
        /// </summary>
        public ValidationException Exception(string message = null)
        {
            return new ValidationException(message, _errors);
        }

        /// <summary>
        /// Throws the exception if there have been any validation warnings.
        /// </summary>
        public void ThrowIfInvalid(string message = null)
        {
            if (!IsValid)
                throw Exception(message);
        }
    }
}
