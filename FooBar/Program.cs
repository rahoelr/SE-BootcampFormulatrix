// See https://aka.ms/new-console-template for more information

FooBar(20);

static void FooBar(int n)
{
    for (int i = 1; i <= n; i++)
    {
        string result = GetFooBarResult(i);
        Console.WriteLine(result);
    }
}

static string GetFooBarResult(int number)
{
    string result = "";
    
    if (number % 3 == 0) result += "Foo";
    if (number % 4 == 0) result += "Baz";
    if (number % 5 == 0) result += "Bar";
    if (number % 7 == 0) result += "Jazz";
    if (number % 9 == 0) result += "Huzz";
    
    return string.IsNullOrEmpty(result) ? number.ToString() : result;
}