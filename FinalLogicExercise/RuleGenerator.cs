using System.Data;

public class RuleGenerator
{
    private Dictionary<int, string> _rules;

    public RuleGenerator()
    {
        _rules = new Dictionary<int, string>();
    }

    public void AddNewRule(int angka, string output)
    {
        _rules.Add(angka, output);
    }

    public string GenerateText(int angka)
    {
        string result = "";
        foreach (var item in _rules)
        {
            if (angka % item.Key == 0)
            {
                result += item.Value;
            }
        }

        if (result == "")
        {
            return angka.ToString();
        }
        return result;
    }

    public void GenerateRange(int input)
    {
        for (int start = 1; start <= input; start++)
        {
            string result = GenerateText(start);
            Console.WriteLine(result);
        }
    }

}
