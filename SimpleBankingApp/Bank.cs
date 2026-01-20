class Bank
{
    private List<BankAccount> _accounts;

    public Bank()
    {
        _accounts = new List<BankAccount>();
    }

    public void AddAccount(BankAccount account)
    {
        _accounts.Add(account);
    }

    public void ShowListAccounts()
    {
        foreach (var account in _accounts)
        {
            account.AccountInfo();
        }
    }

    public void FindAccount(string accountNumber)
    {
        var account = _accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
        if (account != null)
        {
            account.AccountInfo();
        }
        else
        {
            Console.WriteLine("Account not found.");
        }
    }

    public void DepositToAccount(string accountNumber, double amount)
    {
        var account = _accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
        if (account != null)
        {
            account.Deposit(amount);
        }
        else
        {
            Console.WriteLine("Account not found.");
        }
    }

    public void WithdrawFromAccount(string accountNumber, double amount)
    {
        var account = _accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
        if (account != null)
        {
            account.Withdraw(amount);
        }
        else
        {
            Console.WriteLine("Account not found.");
        }
    }
}