namespace Application.DTOs;

public class UserQueryParameters
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = DefaultPageSize;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool? SortDescending { get; set; } = false;

    public int NormalizedPage => Page is null or < 1 ? 1 : Page.Value;

    public int NormalizedPageSize => PageSize switch
    {
        null or < 1 => DefaultPageSize,
        > MaxPageSize => MaxPageSize,
        _ => PageSize.Value
    };

    public bool NormalizedSortDescending => SortDescending ?? false;
}