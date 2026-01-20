namespace Delegates
{
    class Program
    {
        static void Main(string[] args)
        {
            BasicDelegate1();
        }

        delegate int Transformer(int x, int y);

        static void BasicDelegate1()
        {
            Transformer t = Add;
            int result = t(3, 10);
            Console.WriteLine($"Hasilnya add adalah {result}");

            t = Substract;
            result = t(10, 3);
            Console.WriteLine($"Hasilnya substract adalah : {result}");

            t = Multiply;
            result = t(10, 20);
            Console.WriteLine($"Hasilnya multiply adalah {result}");

            t = Add;
            result = t.Invoke(10, 20);
            Console.WriteLine($"hasil add : {result}");
            
        }
        static int Add(int x, int y)
        {
            return x + y;
        }

        static int Substract(int x, int y)
        {
            return x - y;
        }

        static int Multiply(int x, int y)
        {
            return x * y;
        }
    }
}