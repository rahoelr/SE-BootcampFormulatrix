
using System.Runtime.InteropServices;

isPalindrome("level");
sumDigits(2);


// palindrome
static void isPalindrome(string input)
{
    string reversed = "";
    for (int i = input.Length - 1; i >= 0; i--)
    {
        reversed += input[i];
    }

    if (input == reversed)
    {
        Console.WriteLine("palindrome");
    } else
    {
        Console.WriteLine("not palindrome");
    }

    Console.WriteLine(reversed);
}

// sumdigits
static void sumDigits(int n)
{
    int total = 0;
    for (int i = 0; i <= n; i++)
    {
        total = total + i;
    }

    Console.WriteLine(total);
}
