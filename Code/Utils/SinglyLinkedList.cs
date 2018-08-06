using System;

namespace Bonsai.Code.Utils
{
    /// <summary>
    /// Linked list that supports appending to head.
    /// </summary>
    public class SinglyLinkedList<T>
        where T: IEquatable<T>
    {
        public SinglyLinkedList(T value, SinglyLinkedList<T> tail = null)
        {
            Value = value;
            Tail = tail;
        }

        /// <summary>
        /// Value of the current node.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Remaining elements of the list.
        /// </summary>
        public SinglyLinkedList<T> Tail { get; }

        /// <summary>
        /// Appends a new value to the head of the list.
        /// </summary>
        public SinglyLinkedList<T> Append(T value)
        {
            return new SinglyLinkedList<T>(value, this);
        }

        /// <summary>
        /// Checks if the list contains the value.
        /// </summary>
        public bool Contains(T value)
        {
            var curr = this;
            while (curr != null)
            {
                if (curr.Value.Equals(value))
                    return true;

                curr = curr.Tail;
            }

            return false;
        }
    }
}
