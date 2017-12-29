namespace Bonsai.Code.Tools
{
    /// <summary>
    /// A range between two fuzzy dates.
    /// </summary>
    public struct FuzzyRange
    {
        #region Constructor

        public FuzzyRange(FuzzyDate? from, FuzzyDate? to)
        {
            _from = from;
            _to = to;
        }

        #endregion

        #region Fields

        private readonly FuzzyDate? _from;
        private readonly FuzzyDate? _to;

        #endregion

        #region Properties

        /// <summary>
        /// Flag indicating that the range contains no meaningful information.
        /// </summary>
        public bool IsEmpty => _from == null && _to == null;

        #endregion


        // todo: toString
    }
}
