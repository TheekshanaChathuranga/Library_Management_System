namespace CatalogService.Dtos;

public class SearchResultDto<T>
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public IEnumerable<T> Items { get; set; } = new List<T>();
}
