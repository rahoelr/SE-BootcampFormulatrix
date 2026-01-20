using System.Collections.Specialized;
using System.Windows.Markup;

namespace Delegates
{
    class Program
    {
        static void Main(string[] args)
        {
            BasicDelegate1();
            DelegateWithParameter();
            DelegateInstaces();
            MultiCastDelegateRhl();
            GenericDelegateDemo();
            FuncAndActionDelegatesDemo();
            DelegateVsInterfaceDemo();
            DelegateEventHandler();
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


        // end of delegate instance method

        // start static multicast delegate
        delegate void Notifier(string message);

        static void MultiCastDelegateRhl()
        {
            Notifier notif = SendEmail;

            // add another methods
            notif += SendSMS;
            notif += SendWhatsapp;
            notif("haiii ini pesan");

            notif -= SendSMS;
            notif("pesan setelah diremove");
        }

        static void SendEmail(string message)
        {
            Console.WriteLine($"this message send from email : {message}");
        }

        static void SendSMS(string message)
        {
            Console.WriteLine($"this message from sms : {message}");
        }

        static void SendWhatsapp(string message)
        {
            Console.WriteLine($"this message from whatsapp : {message}");
        }

        // END static multicast delegate

        // START GENERIC DELEGATE
        public delegate TResult Transformer<TArg, TResult>(TArg arg);

        static void GenericDelegateDemo()
        {
            Console.WriteLine("5. GENERIC DELEGATE TYPES - ULTIMATE REUSABILITY");
            Console.WriteLine("================================================");


            Transformer<int, int> intSquarer = x => x * x;
            Transformer<string, int> stringLength = s => s.Length;

            Console.WriteLine($"Int squarer (5): {intSquarer(5)}");
            Console.WriteLine($"String length ('Hello'): {stringLength("Hello")}");

            Console.WriteLine("\nGeneric Transform method demo:");
            int[] numbers = { 1, 2, 3, 4 };
            Console.WriteLine($"Original numbers: [{string.Join(", ", numbers)}]");

            TransformGeneric(numbers, x => x * x);  // Square each number
            Console.WriteLine($"Squared numbers: [{string.Join(", ", numbers)}]");

            string[] words = { "cat", "dog", "elephant" };
            Console.WriteLine($"Original words: [{string.Join(", ", words)}]");

            TransformGeneric(words, s => s.ToUpper());  // Uppercase each word
            Console.WriteLine($"Uppercase words: [{string.Join(", ", words)}]");


        }

        public static void TransformGeneric<T>(T[] values, Transformer<T, T> transformer)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = transformer(values[i]);
            }
        }

        // END GENERIC DELEGATE

        // START GENERIC DELEGATE 

        static void FuncAndActionDelegatesDemo()
        {
            //func mengembalikan return value
            Func<int, int> Cube = x => x * x;
            Console.WriteLine($"Hallo ini adalah cube : {Cube(10)}");

            //action tidak mengembalikan return value
            Action<string> print = str => Console.WriteLine($"ini adalah action : {str}");
            print("hallo ini rahul");

            //action multiple parameter
            Action<string, int> multipleParam = (strName, Number) => Console.WriteLine($"ini adalah name = {strName}\n ini adalah number : {Number}");

            multipleParam("rahul", 23);

            int[] values = { 1, 2, 3, 4, 5, 6, 8 };
            TransformWithFunc(values, Square);
            Console.WriteLine($"Doubled values: [{string.Join(", ", values)}]");

        }

        public static void TransformWithFunc<T>(T[] values, Func<T, T> transformer)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = transformer(values[i]);
            }
        }

        // delegate interfac demo

        static void DelegateVsInterfaceDemo()
        {
            Console.WriteLine("7. DELEGATES VS INTERFACES - WHEN TO USE WHAT");
            Console.WriteLine("=============================================");

            ITransformer squareTransformer = new SquareTransformer();

            TransformWithInterface(new int[] { 2, 3, 4, 5 }, squareTransformer);

            Func<int, int> squareDelegate = x => x * x;

            TransformWithDelegate(new int[] { 1, 2, 3, 4, 5, 6 }, squareDelegate);

        }

        interface ITransformer
        {
            int Transform(int x);
        }

        class SquareTransformer : ITransformer
        {
            public int Transform(int x) => x * x;

        }

        class CubeTransform : ITransformer
        {
            public int Transform(int x) => x * x * x;
        }

        static void TransformWithInterface(int[] values, ITransformer transformer)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = transformer.Transform(values[i]);
            }
            Console.WriteLine($"  Result: [{string.Join(", ", values)}]");
        }

        static void TransformWithDelegate(int[] values, Func<int, int> transform)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = transform(values[i]);
            }
            Console.WriteLine($"  Result: [{string.Join(", ", values)}]");
        }


        static void DelegateEventHandler()
        {
            Downloader downlaoder = new Downloader();
            downlaoder.ProgressChanged += ShowProgress;
            // downlaoder.ProgressChanged += SaveProgressToFile;

            downlaoder.StartDownload();

            Console.WriteLine("download selesai");
        }

        static void ShowProgress(int progress)
        {
            Console.WriteLine($"Progress: {progress}%");
        }

        static void SaveProgressToFile(int progress)
        {
            File.AppendAllText("progress.txt", $"Progress: {progress}%{Environment.NewLine}");
        }

        class Downloader
        {
            public event Action<int> ProgressChanged;

            public void StartDownload()
            {
                for (int i = 0; i <= 100; i += 20)
                {
                    Thread.Sleep(500);
                    ProgressChanged?.Invoke(i);
                }
            }
        }


    }

}