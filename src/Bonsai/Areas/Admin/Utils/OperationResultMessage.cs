namespace Bonsai.Areas.Admin.Utils
{
    /// <summary>
    /// Information about the previous operation passed between pages.
    /// </summary>
    public class OperationResultMessage
    {
        /// <summary>
        /// Message text.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Success flag.
        /// </summary>
        public bool IsSuccess { get; set; }
    }
}
