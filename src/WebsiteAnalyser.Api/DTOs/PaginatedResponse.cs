namespace WebsiteAnalyser.Api.DTOs;

public record PaginatedResponse<T> {
    public IEnumerable<T> Items { get; set; } = [];
    public int PerPage { get; set; }
    public int TotalItems { get; set; }

    public PaginatedResponse(IEnumerable<T> items, int perPage, int totalItems) {
        Items = items;
        PerPage = perPage;
        TotalItems = totalItems;
    }
}
