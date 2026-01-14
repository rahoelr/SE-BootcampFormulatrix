namespace Learn.Models
{
class Animal
{
    private string name;
    private int age;
    public Animal(string name, int age)
    {
        this.name = name;
        this.age = age;
    }

    public string Name
    {
        get { return name; }
        set { name = value;}
    }

    public int Age
    {
        get {return age;}
        set
        {
            if (value > 0)
            {
                age = value;
            }
        }
    }

    public override string ToString()
    {
        return $"Name: {name}, Age: {Age}";
    }
}
}