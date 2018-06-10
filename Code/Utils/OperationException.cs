using System;

namespace Bonsai.Code.Utils
{
    /// <summary>
    /// Exception during an admin operation.
    /// </summary>
    public class OperationException: Exception
    {
        public OperationException(string message)
            : base(message)
        {
        }

        public OperationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
