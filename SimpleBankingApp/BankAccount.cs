
public abstract class BankAccount {
    private string _accountNumber;
    private string _ownerName;
    private double _balance;

    public string AccountNumber
    {
        get { return _accountNumber; }
        set { _accountNumber = value; }
    }

    public string OwnerName
    {
        get { return _ownerName; }
        set { _ownerName = value; }
    }

    public double Balance
    {
        get { return _balance; }
        set { _balance = value; }
    }

    public BankAccount(string accountNumber, string ownerName, double balance)
    {
        _accountNumber = accountNumber;
        _ownerName = ownerName;
        _balance = balance;
    }

    public void Deposit(double amount)
    {
        if (amount > 0)
        {
            _balance += amount;
            Console.WriteLine($"Deposited {amount}. New balance is {_balance}.");
        } else
        {
            Console.WriteLine("Deposit amount must be positive.");
        }
    }

    public virtual void Withdraw(double amount)
    {
        if (amount > 0 && amount <= _balance)
        {
            _balance -= amount;
            Console.WriteLine($"Withdrew {amount}. New balance is {_balance}.");
        } else
        {
            Console.WriteLine("Insufficient funds or invalid amount.");
        }
    }

    public abstract void AccountInfo();
}