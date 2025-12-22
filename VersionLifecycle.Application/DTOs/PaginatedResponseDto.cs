namespace VersionLifecycle.Application.DTOs;

/// <summary>
/// Generic paginated response DTO.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Gets or sets the items in the current page.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the total count of items.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the number of items skipped.
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Gets or sets the number of items taken.
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (TotalCount + Take - 1) / Take;

    /// <summary>
    /// Gets or sets a value indicating whether there are more pages.
    /// </summary>
    public bool HasNextPage => Skip + Take < TotalCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedResponse{T}"/> class.
    /// </summary>
    public PaginatedResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedResponse{T}"/> class.
    /// </summary>
    public PaginatedResponse(IEnumerable<T> items, int totalCount, int skip, int take)
    {
        Items = items;
        TotalCount = totalCount;
        Skip = skip;
        Take = take;
    }
}
