namespace ContosoUniversity.Web.Models
{
    /// <summary>
    /// Encapsulates student search, sort, and pagination parameters.
    /// </summary>
    public class StudentSearchQuery
    {
        public string? SortOrder { get; init; }
        public string? CurrentLastNameFilter { get; init; }
        public string? CurrentFirstNameFilter { get; init; }
        public string? LastNameSearch { get; init; }
        public string? FirstNameSearch { get; init; }
        public int? PageNumber { get; init; }

        /// <summary>
        /// Returns the effective last name filter, accounting for new searches resetting pagination.
        /// </summary>
        public string? EffectiveLastNameFilter =>
            IsNewSearch ? LastNameSearch : (LastNameSearch ?? CurrentLastNameFilter);

        /// <summary>
        /// Returns the effective first name filter, accounting for new searches resetting pagination.
        /// </summary>
        public string? EffectiveFirstNameFilter =>
            IsNewSearch ? FirstNameSearch : (FirstNameSearch ?? CurrentFirstNameFilter);

        /// <summary>
        /// Returns the effective page number (resets to 1 on new searches).
        /// </summary>
        public int EffectivePageNumber => IsNewSearch ? 1 : (PageNumber ?? 1);

        /// <summary>
        /// A new search is triggered when either search field is explicitly provided.
        /// </summary>
        public bool IsNewSearch => LastNameSearch != null || FirstNameSearch != null;
    }
}
