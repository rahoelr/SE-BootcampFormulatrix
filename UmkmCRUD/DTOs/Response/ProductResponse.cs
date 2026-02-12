public class ProductResponse
{
    public Guid Id { get; set; }
    public string? ProductName { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
}
