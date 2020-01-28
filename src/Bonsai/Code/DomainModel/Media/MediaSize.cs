namespace Bonsai.Code.DomainModel.Media
{
    public enum MediaSize
    {
        /// <summary>
        /// Original dimensions of the uploaded photo.
        /// </summary>
        Original,

        /// <summary>
        /// Photo for viewing in the embedded viewer.
        /// </summary>
        Large,

        /// <summary>
        /// Photo for info block.
        /// </summary>
        Medium,

        /// <summary>
        /// Photo for media tumbnail.
        /// </summary>
        Small,
    }
}
