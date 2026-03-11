namespace PlatziStore.Infrastructure.ApiClients.Dtos;

public class CreateProductApiRequest
{
    public string Title { get; set; } = string.Empty;
    public int Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public List<string> Images { get; set; } = new();
}
