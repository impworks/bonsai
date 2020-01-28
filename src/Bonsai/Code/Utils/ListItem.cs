namespace Bonsai.Code.Utils
{
    /// <summary>
    /// An arbitrary item.
    /// </summary>
    public class ListItem<T>
    {
        public ListItem(T id, string title)
        {
            Id = id;
            Title = title;
        }

        /// <summary>
        /// Unique ID of this element.
        /// </summary>
        public T Id { get; }

        /// <summary>
        /// Readable title.
        /// </summary>
        public string Title { get; }
    }
}
