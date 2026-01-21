using System.Net.Http.Headers;

namespace DemoTryCatch
{
    class Program
    {
        static void Main(string[] args)
        {
            TestTryCatch(10, 0);
            TestParsingScenarios();

            static void TestTryCatch(int a, int b)
            {
                try
                {
                    int result = a / b;
                    Console.WriteLine(result);
                }
                catch (DivideByZeroException err)
                {
                    Console.WriteLine($"error bagi nol: {err}");
                }
                finally
                {
                    Console.WriteLine("program selesai");
                }

            }

            static void TestParsingScenarios()
            {
                string[] testCases = { "100", "abc", "500", "" };

                foreach (string testCase in testCases)
                {
                    Console.WriteLine($"  Testing '{testCase}':");
                    try
                    {
                        byte result = byte.Parse(testCase);
                        Console.WriteLine($"    Success: {result}");
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("    Error: Invalid format");
                    }
                    catch (OverflowException)
                    {
                        Console.WriteLine("    Error: Number too large for byte");
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("    Error: Empty string");
                    }
                }
            }

        }
    }
}