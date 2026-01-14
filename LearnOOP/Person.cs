public abstract class Person : IInfo
{
    protected int id;
    protected string name;

    public Person(int id, string name)
    {
        this.id = id;
        this.name = name;
    }
    public virtual void DisplayInfo()
    {
        Console.WriteLine($"ID: {id} Name: {name}");
    }
}