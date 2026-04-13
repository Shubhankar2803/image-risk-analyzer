namespace RiskAnalyzer.Api.DTOs;

/// <summary>
/// Pagination metadata for list responses
/// </summary>
public class PaginationMetadata
{
    /// <summary>Current page number (1-based)</summary>
    public int PageNumber { get; set; }
    
    /// <summary>Number of items per page</summary>
    public int PageSize { get; set; }
    
    /// <summary>Total number of items</summary>
    public int TotalCount { get; set; }
    
    /// <summary>Total number of pages</summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    
    /// <summary>Whether there are more pages after current page</summary>
    public bool HasNextPage => PageNumber < TotalPages;
    
    /// <summary>Whether there are pages before current page</summary>
    public bool HasPreviousPage => PageNumber > 1;
}

/// <summary>
/// Paginated API response wrapper
/// </summary>
public class PaginatedApiResponse<T>
{
    /// <summary>Whether the request was successful</summary>
    public bool Success { get; set; }
    
    /// <summary>List of items</summary>
    public List<T> Data { get; set; } = new();
    
    /// <summary>Pagination metadata</summary>
    public PaginationMetadata? Pagination { get; set; }
    
    /// <summary>Error information (null if success)</summary>
    public ApiError? Error { get; set; }
    
    /// <summary>Request timestamp in UTC</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
