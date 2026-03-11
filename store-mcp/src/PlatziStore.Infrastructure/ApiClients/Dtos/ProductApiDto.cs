namespace PlatziStore.Infrastructure.ApiClients.Dtos;

public class ProductApiDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public CategoryApiDto Category { get; set; } = new();
    public List<string> Images { get; set; } = new();
    public string? CreationAt { get; set; }
    public string? UpdatedAt { get; set; }
}
