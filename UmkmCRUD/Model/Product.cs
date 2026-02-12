
public class Product
{
    public Guid Id {get; set;} = Guid.NewGuid();
    public string? ProductName { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
}
