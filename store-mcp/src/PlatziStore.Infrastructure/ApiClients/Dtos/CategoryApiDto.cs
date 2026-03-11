namespace PlatziStore.Infrastructure.ApiClients.Dtos;

public class CategoryApiDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string? CreationAt { get; set; }
    public string? UpdatedAt { get; set; }
}
