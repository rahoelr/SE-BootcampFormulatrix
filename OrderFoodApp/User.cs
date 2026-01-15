public abstract class User
{
    string name;
    String phoneNumber;
    int saldo;

    public User(string name, String phoneNumber, int saldo)
    {
        this.name = name;
        this.phoneNumber = phoneNumber;
        this.saldo = saldo;
    }

    public void Login()
    {
        Console.WriteLine($"User {this.name} logged in.");
    }

    
}