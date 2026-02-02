using System.Data;

public class Program
{
    static void Main(string[] args)
    {
        var RuleGen = new RuleGenerator();
        RuleGen.AddNewRule(3, "Foo");
        RuleGen.AddNewRule(4, "Baz");
        RuleGen.AddNewRule(5, "Bar");
        RuleGen.AddNewRule(7, "Jazz");
        RuleGen.AddNewRule(9, "Huzz");
        RuleGen.AddNewRule(6, "Rah");

        RuleGen.GenerateRange(20);
    }
}