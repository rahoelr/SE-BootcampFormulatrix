using System.Reflection.PortableExecutable;

namespace StringTextDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // DemoCharType();
            // DemoStringBasic();
            // DemoStringSearching();
            // DemoStringManipulation();
            DemonstrateStringComparison();
            DemonstrateAdvancedNumberFormatting();
        }


        static void DemoCharType()
        {
            char[] data = { 'C', '#', ' ', 'O', 'O', 'P' };
            string myString = new string(data);
            Console.WriteLine(myString);

            char[] ca = { 'H', 'i' };
            string s = new string(ca);
            Console.WriteLine(s);

            char letter = 'a';
            char newLine = '\n';

            char unicodeChar = '\u0041';

            Console.WriteLine($"ini letter = {letter}");
            Console.WriteLine($"unicode = {unicodeChar}");

            //char uppercase and lowercase
            Console.WriteLine($"ini adalah uppercase 'c' : {char.ToUpper('c')}");
            Console.WriteLine($"ini adalah lowercase 'B' : {char.ToLower('b')}");

            //character culter invariant critical international app
            Console.WriteLine($"culture invariant uppercase 'i' : {char.ToUpperInvariant('i')}");
            Console.WriteLine($"reguler uppercase : {char.ToUpper('i')}");

            Console.WriteLine($"Is '!' punctuation? {char.IsPunctuation('!')}");
            char testChar = 'a';
            Console.WriteLine($"unicode category of {testChar} : {char.GetUnicodeCategory(testChar)}");

        }

        static void DemoStringBasic()
        {
            Console.WriteLine("==========");
            string repeated = new string('x', 10);
            Console.WriteLine(repeated);
            char[] myCharArray = { 'r', 'a', 'h', 'u', 'l' };
            string fromSubset = new string(myCharArray, 2, 3);
            Console.WriteLine(fromSubset);

            //NULL EMPTY STRING HANDLING - CRITICAL
            string strEmpty = "";
            string alsoStrEmpty = string.Empty;
            string? nullStr = null;

            Console.WriteLine(strEmpty == "");
            Console.WriteLine(strEmpty == string.Empty);
            Console.WriteLine(strEmpty.Length);

            Console.WriteLine(nullStr == null);
            Console.WriteLine(string.IsNullOrEmpty(nullStr));
        }

        static void DemoStringSearching()
        {
            string text = "The quick brown fox jumps over the lazy dog";
            Console.WriteLine(text.StartsWith("The"));
            Console.WriteLine(text.EndsWith("dog"));
            Console.WriteLine(text.Contains("brown"));

            Console.WriteLine(text.LastIndexOf("the"));
        }

        static void DemoStringManipulation()
        {
            string original = "Hello World";

            // Substring extraction
            // string left5 = original.Substring(0, 6);
            // string right5 = original.Substring(6);
            // Console.WriteLine($"Original: '{original}'");
            // Console.WriteLine($"Left 5 characters: '{left5}'");
            // Console.WriteLine($"From index 6 to end: '{right5}'");

            // string inserted = original.Insert(5, ",");
            // string removed = inserted.Remove(5, 4);
            // Console.WriteLine($"After inserting comma: '{inserted}'");
            // Console.WriteLine($"After removing comma: '{removed}'");

            // string number = "123";
            // Console.WriteLine($"Right-padded: '{number.PadRight(10, '*')}'");
            // Console.WriteLine($"Left-padded: '{number.PadLeft(10, '0')}'");

                        // Trimming whitespace - essential for user input processing
            // string messy = "   Hello World   \t\r\n";
            // Console.WriteLine($"Original length: {messy.Length}");
            // Console.WriteLine($"Trimmed length: {messy.Trim().Length}");
            // Console.WriteLine($"Trimmed result: '{messy.Trim()}'");


            // String replacement
            // string sentence = "I like cats and cats like me";
            // string replaced = sentence.Replace("cats", "dogs");
            // Console.WriteLine($"Original: '{sentence}'");
            // Console.WriteLine($"Replaced: '{replaced}'");
        }

        // static void DemonstrateStringInterpolationAndFormatting()
        // {
        //     Console.WriteLine("6. STRING INTERPOLATION AND FORMATTING DEMONSTRATION");
        //     Console.WriteLine("====================================================");

        //     // String interpolation - modern and readable way to build strings
        //     string name = "Alice";
        //     int age = 25;
        //     DateTime today = DateTime.Now;

        //     string interpolated = $"Hello, my name is {name} and I'm {age} years old.";
        //     string withDate = $"Today is {today.DayOfWeek}, {today:yyyy-MM-dd}";
            
        //     Console.WriteLine(interpolated);
        //     Console.WriteLine(withDate);

        //     // Traditional string formatting - still useful for complex scenarios
        //     string template = "It's {0} degrees in {1} on this {2} morning";
        //     string formatted = string.Format(template, 25, "Jakarta", today.DayOfWeek);
        //     Console.WriteLine(formatted);

        //     // Format specifiers for numbers and dates
        //     double price = 19.99;
        //     Console.WriteLine($"Price: {price:C}"); // Currency format
        //     Console.WriteLine($"Percentage: {0.85:P}"); // Percentage format
        //     Console.WriteLine($"Date: {today:dddd, MMMM dd, yyyy}"); // Long date format

        //     Console.WriteLine();
        // }

        static void DemonstrateStringComparison()
        {
            Console.WriteLine("7. STRING COMPARISON DEMONSTRATION");
            Console.WriteLine("==================================");

            string str1 = "Hello";
            string str2 = "hello";
            string str3 = "Hello";

            // Default equality comparison - ordinal, case-sensitive
            Console.WriteLine("=== EQUALITY COMPARISON ===");
            Console.WriteLine($"'{str1}' == '{str3}': {str1 == str3}");
            Console.WriteLine($"'{str1}' == '{str2}': {str1 == str2}");
            Console.WriteLine($"'{str1}'.Equals('{str2}'): {str1.Equals(str2)}");

            // StringComparison enum - gives you full control over comparison behavior
            Console.WriteLine("\n=== STRING COMPARISON OPTIONS ===");
            Console.WriteLine($"Ordinal (default): {string.Equals(str1, str2, StringComparison.Ordinal)}");
            Console.WriteLine($"OrdinalIgnoreCase: {string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase)}");
            Console.WriteLine($"CurrentCulture: {string.Equals(str1, str2, StringComparison.CurrentCulture)}");
            Console.WriteLine($"CurrentCultureIgnoreCase: {string.Equals(str1, str2, StringComparison.CurrentCultureIgnoreCase)}");
            Console.WriteLine($"InvariantCulture: {string.Equals(str1, str2, StringComparison.InvariantCulture)}");
            Console.WriteLine($"InvariantCultureIgnoreCase: {string.Equals(str1, str2, StringComparison.InvariantCultureIgnoreCase)}");

            // Order comparison - for sorting and alphabetical ordering
            Console.WriteLine("\n=== ORDER COMPARISON ===");
            string[] words = { "apple", "Banana", "cherry", "Date" };
            Console.WriteLine("Original order: " + string.Join(", ", words));

            // Default culture-sensitive comparison
            Array.Sort(words, string.Compare);
            Console.WriteLine("Culture sort: " + string.Join(", ", words));

            // Reset array
            words = new[] { "apple", "Banana", "cherry", "Date" };
            
            // Ordinal comparison - treats characters as their numeric Unicode values
            Array.Sort(words, StringComparer.Ordinal);
            Console.WriteLine("Ordinal sort: " + string.Join(", ", words));

            // Case-insensitive ordinal comparison
            Array.Sort(words, StringComparer.OrdinalIgnoreCase);
            Console.WriteLine("Ordinal ignore case: " + string.Join(", ", words));

            // CompareTo examples - returns negative, zero, or positive
            Console.WriteLine("\n=== COMPARETO EXAMPLES ===");
            Console.WriteLine($"'Boston'.CompareTo('Austin'): {string.Compare("Boston", "Austin")}");
            Console.WriteLine($"'Boston'.CompareTo('Boston'): {string.Compare("Boston", "Boston")}");
            Console.WriteLine($"'Boston'.CompareTo('Chicago'): {string.Compare("Boston", "Chicago")}");
            
            // Ordinal vs Culture demonstration
            Console.WriteLine("\n=== ORDINAL VS CULTURE COMPARISON ===");
            string a = "Atom";
            string b = "atom";
            Console.WriteLine($"Ordinal: '{a}' vs '{b}' = {string.Compare(a, b, StringComparison.Ordinal)}");
            Console.WriteLine($"Culture: '{a}' vs '{b}' = {string.Compare(a, b, StringComparison.CurrentCulture)}");
            Console.WriteLine("Note: Ordinal treats 'A' (65) and 'a' (97) by Unicode values");
            Console.WriteLine("Culture comparison considers language rules for proper alphabetical ordering");

            Console.WriteLine();
        }

        static void DemonstrateAdvancedNumberFormatting()
        {
            Console.WriteLine("5. ADVANCED NUMBER FORMATTING MASTERY");
            Console.WriteLine("======================================");

            int integer = 42;
            double floating = 1234.5678;
            decimal money = 19.99m;

            // Standard numeric format strings - the building blocks
            Console.WriteLine("Standard format strings:");
            Console.WriteLine($"  Integer {integer}:");
            Console.WriteLine($"    Currency: {integer:C}");
            Console.WriteLine($"    Decimal: {integer:D5}");          // Pad with zeros
            Console.WriteLine($"    Exponential: {integer:E}");
            Console.WriteLine($"    Fixed-point: {integer:F2}");
            Console.WriteLine($"    General: {integer:G}");
            Console.WriteLine($"    Number: {integer:N}");
            Console.WriteLine($"    Percent: {integer:P}");
            Console.WriteLine($"    Hexadecimal: {integer:X}");

            Console.WriteLine($"\n  Double {floating}:");
            Console.WriteLine($"    Currency: {floating:C}");
            Console.WriteLine($"    Exponential: {floating:E2}");
            Console.WriteLine($"    Fixed-point: {floating:F2}");
            Console.WriteLine($"    Number: {floating:N2}");
            Console.WriteLine($"    Percent: {floating:P1}");

            Console.WriteLine($"\n  Decimal (money) {money}:");
            Console.WriteLine($"    Currency: {money:C}");
            Console.WriteLine($"    Fixed-point: {money:F4}");
            Console.WriteLine($"    Number: {money:N}");
            Console.WriteLine($"    Percent: {money:P2}");

            // Custom numeric format strings - unleash your creativity
            Console.WriteLine("\nCustom format strings:");
            Console.WriteLine($"  Phone number format: {1234567890:###-###-####}");
            Console.WriteLine($"  Padded number: {42:00000}");
            Console.WriteLine($"  Conditional format positive: {15:#;(#);zero}");
            Console.WriteLine($"  Conditional format negative: {-15:#;(#);zero}");
            Console.WriteLine($"  Conditional format zero: {0:#;(#);zero}");

            // NumberStyles for parsing - control what patterns are allowed
            Console.WriteLine("\nAdvanced parsing with NumberStyles:");
            
            // Parse numbers with parentheses (accounting style)
            int negativeNumber = int.Parse("(42)", NumberStyles.Integer | NumberStyles.AllowParentheses);
            Console.WriteLine($"  Parsed \"(42)\" with parentheses: {negativeNumber}");

            // Parse currency values
            decimal currencyValue = decimal.Parse("$1,234.56", NumberStyles.Currency, 
                CultureInfo.GetCultureInfo("en-US"));
            Console.WriteLine($"  Parsed \"$1,234.56\" as currency: {currencyValue}");

            // Parse with leading/trailing whitespace
            double trimmedNumber = double.Parse("  123.45  ", NumberStyles.Float | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite);
            Console.WriteLine($"  Parsed \"  123.45  \" with whitespace: {trimmedNumber}");

            Console.WriteLine();
        }

        
    }
}