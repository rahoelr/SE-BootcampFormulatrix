
public class SavingsAccount : BankAccount
{
    private double _interestRate;

    public double InterestRate
    {
        get { return _interestRate; }
        set { _interestRate = value; }
    }

    public SavingsAccount(string accountNumber, string ownerName, double balance, double interestRate)
        : base(accountNumber, ownerName, balance)
    {
        _interestRate = interestRate;
    }

    public void AddInterest()
    {
        double interest = Balance * _interestRate / 100;
        Deposit(interest);
        Console.WriteLine($"Interest of {interest} added.");
    }

    public override void AccountInfo()
    {
        Console.WriteLine($"Savings Account - Account Number: {AccountNumber}, Owner: {OwnerName}, Balance: {Balance}, Interest Rate: {_interestRate}%");
    }
}