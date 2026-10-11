namespace WebsiteAnalyzer.Core.Domain;

public record PagedResponse<T> {
    public IEnumerable<T> Items { get; set; } = [];
    public int PerPage { get; set; }
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PerPage);

    public PagedResponse() {

    }

    public PagedResponse(IEnumerable<T> items, int page, int perPage, int totalItems) {
        Items = items;
        Page = page;
        PerPage = perPage;
        TotalItems = totalItems;
    }

    public PagedResponse<TDestination> Map<TDestination>(Func<T, TDestination> mapper) {
        return new PagedResponse<TDestination> {
            Items = Items.Select(mapper),
            PerPage = PerPage,
            TotalItems = TotalItems,
            Page = Page
        };
    }

}
