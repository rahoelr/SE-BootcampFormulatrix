public interface ITransaction
{
    void Deposit(double amount);
    void Withdraw(double amount);
}