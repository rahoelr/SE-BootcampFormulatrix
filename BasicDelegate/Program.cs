using System.Collections.Specialized;

namespace Delegates
{
    class Program
    {
        static void Main(string[] args)
        {
            BasicDelegate1();
            DelegateWithParameter();
            DelegateInstaces();
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


        // END BASIC DELEGATE

        delegate int ProcessNumber(int x);

        static void DelegateWithParameter()
        {
            int[] numbers = [1, 2, 3, 4, 5, 6];
            Console.WriteLine($"Original values: [{string.Join(", ", numbers)}]");

            Transform(numbers, Double);

            Console.WriteLine($"After values: [{string.Join(", ", numbers)}]");

            //reset array
            numbers = new int[] { 1, 2, 3, 4, 5, 6 };
            Console.WriteLine($"Original values: [{string.Join(", ", numbers)}]");

            Transform(numbers, Square);

            Console.WriteLine($"After values: [{string.Join(", ", numbers)}]");
        }

        static int Double(int x)
        {
            int result = x * 2;
            return result;
        }

        static int Square(int x)
        {
            int result = x * x;
            return result;
        }

        static void Transform(int[] data, ProcessNumber p)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = p(data[i]);
            }
        }

          // end of delegate callback

        delegate int ProcessNumberX(int x);

        static void DelegateInstaces()
        {
            Counter count = new Counter(5);
            ProcessNumberX del = count.Increase;
            Console.WriteLine(del(1));   // 1 + 5 = 6
            Console.WriteLine(del(10));  // 10 + 5 = 15
            Console.WriteLine(del(100)); // 100 + 5 = 105

        }
        class Counter
        {
            private int _step;

            public Counter(int step)
            {
                _step = step;
            }

            public int Increase(int x)
            {
                return x + _step;
            }
        }
    }

}