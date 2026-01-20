public class CheckingAccount : BankAccount
{
    private double _overDraftLimit;

    public double OverDraftLimit
    {
        get { return _overDraftLimit; }
        set { _overDraftLimit = value; }
    }

    public CheckingAccount(string accountNumber, string ownerName, double balance, double overDraftLimit)
        : base(accountNumber, ownerName, balance)
    {
        _overDraftLimit = overDraftLimit;
    }

    public override void Withdraw(double amount)
    {
        if (amount > 0 && amount <= (Balance + _overDraftLimit))
        {
            base.Withdraw(amount);
        }
        else
        {
            Console.WriteLine("Withdrawal exceeds overdraft limit or invalid amount.");
        }
    }
    public override void AccountInfo()
    {
        Console.WriteLine($"Checking Account - Account Number: {AccountNumber}, Owner: {OwnerName}, Balance: {Balance}, Overdraft Limit: {_overDraftLimit}");
    }
}