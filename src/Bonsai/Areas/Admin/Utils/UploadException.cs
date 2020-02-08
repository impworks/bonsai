using System;

namespace Bonsai.Areas.Admin.Utils
{
    /// <summary>
    /// Exception that occurs during a file upload.
    /// </summary>
    public class UploadException: Exception
    {
        public UploadException()
        {
        }

        public UploadException(string message) : base(message)
        {
        }

        public UploadException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
