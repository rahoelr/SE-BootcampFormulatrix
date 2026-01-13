// See https://aka.ms/new-console-template for more information

FooBar(15);

static void FooBar(int n)
{
    for (int i = 1; i < n+1; i++)
    {
        if (i%3 == 0 && i%5 == 0)
        {
            Console.WriteLine("FooBar");
        } else if (i%3==0)
        {
            Console.WriteLine("Foo");
        } else if (i % 5 == 0)
        {
            Console.WriteLine("Bar");
        } else
        {
            Console.WriteLine(i);
        }
    }
}
