using Learn.Models;
User user1 = new User("Alice", 30);
Console.WriteLine(user1.GetUserData());

int x = 8;
Console.WriteLine($"Ini adalah angka{x}");

string A = "Rahul";
string B = "rahul";


bool result = A.Equals(B);
Console.WriteLine(result);

//null
string myNewName = null;
string resultNew = myNewName ??= "bintang";
Console.WriteLine("in adalah null test:" + resultNew);
Console.WriteLine(myNewName);


// array
char[] myChar = new char[5];
myChar[0] = 'R';

Console.WriteLine(myChar);

char[] myVowels = ['A','I','U','E','O'];
Console.WriteLine(myVowels);


List<String> myName = new List<string>();
myName.Add("Rahul");
myName.Add("Rahmatullah");

Console.WriteLine(myName);
Animal myAnimal = new Animal("Jhon",20);
// Console.WriteLine(myAnimal.ToString;
Console.WriteLine(myAnimal.Name);
Console.WriteLine(myAnimal.Age);
Console.WriteLine(myAnimal.ToString());

Learn.Service.UserService myService = new ("Rahul");
Console.WriteLine(myService.Name);


public class User
{
    string name;
    int age;
    public User(string Name, int age)
    {
        name = Name;

    }

    public string GetUserData()
    {
        return $"Name: {this.name}, Age: {this.age}";
    }
}
