public class Category
{
    public Guid Id {get; set;} = Guid.NewGuid();
    public string CategoryName {get; set;}
    public string Description {get; set;}

    public ICollection<Product> products {get; set;}
}