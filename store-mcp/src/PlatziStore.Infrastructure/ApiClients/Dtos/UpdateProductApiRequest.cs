namespace PlatziStore.Infrastructure.ApiClients.Dtos;

public class UpdateProductApiRequest
{
    public string? Title { get; set; }
    public int? Price { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public List<string>? Images { get; set; }
}
