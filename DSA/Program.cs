

isPalindrome("level");
sumDigits(2);
twoSum([1, 2, 3, 4, 5, 6], 10);
FindMinValue([19,2,4,12,42,53,53,1]);


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

//two sum
static void twoSum(int[] array, int target)
{
    for (int i = 0; i < array.Length; i++)
    {
        for (int j = i + 1; j < array.Length; j++)
        {
            if (array[i] + array[j] == target)
            {
                Console.WriteLine($"ketemu di index {i} + index {j}");
            }
        }
    }
}

// findminvalue
static void FindMinValue(int[] arr)
{
    int minVal = arr[0];
    for (int i = 0; i < arr.Length; i++)
    {
        if (arr[i] < minVal)
        {
            minVal = arr[i];
        }
    }

    Console.WriteLine(minVal);

}