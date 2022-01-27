using System.Collections.Generic;


namespace LoremipsumSharp.Common
{
   /// <summary>
    /// enumerable to diff two collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DiffEnumerable<T>
    {

        public IEnumerable<T> LeftOnly { get; }

        public IEnumerable<T> Intersect { get; }

        public IEnumerable<T> RightOnly { get; }

        public DiffEnumerable(IEnumerable<T> leftOnly, IEnumerable<T> intersect, IEnumerable<T> rightOnly)
        {
            LeftOnly = leftOnly;
            Intersect = intersect;
            RightOnly = rightOnly;
        }
    }

    /// <summary>
    /// enumerable to diff two collection
    /// </summary>
    public class DiffEnumerable<TLeft, TRight>
    {

        public IEnumerable<TLeft> LeftOnly { get; }

        public IEnumerable<DiffValue> Intersect { get; }

        public IEnumerable<TRight> RightOnly { get; }


        public DiffEnumerable(IEnumerable<TLeft> leftOnly, IEnumerable<DiffValue> intersect, IEnumerable<TRight> rightOnly)
        {
            LeftOnly = leftOnly;
            Intersect = intersect;
            RightOnly = rightOnly;
        }

        /// <summary>
        ///  diff value
        /// </summary>
        public class DiffValue
        {

            public TLeft Left { get; }

            public TRight Right { get; }


            public DiffValue(TLeft left, TRight right)
            {
                Left = left;
                Right = right;
            }
        }
    }
}