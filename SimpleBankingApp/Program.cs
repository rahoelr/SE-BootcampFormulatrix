Bank bank = new Bank();

// Buat akun tabungan (Savings Account)
SavingsAccount savingsAcc = new SavingsAccount("SA001", "Budi", 1000000, 5);
bank.AddAccount(savingsAcc);

// Buat akun giro (Checking Account)
CheckingAccount checkingAcc = new CheckingAccount("CA001", "Ani", 500000, 200000);
bank.AddAccount(checkingAcc);

Console.WriteLine("=== Daftar Semua Akun ===");
bank.ShowListAccounts();

Console.WriteLine("\n=== Test Deposit ===");
bank.DepositToAccount("SA001", 500000);

Console.WriteLine("\n=== Test Withdraw ===");
bank.WithdrawFromAccount("CA001", 300000);

Console.WriteLine("\n=== Test Overdraft (Checking Account) ===");
bank.WithdrawFromAccount("CA001", 500000); // Melebihi saldo tapi masih dalam limit overdraft

Console.WriteLine("\n=== Add Interest (Savings Account) ===");
savingsAcc.AddInterest();

Console.WriteLine("\n=== Info Akun Setelah Transaksi ===");
bank.ShowListAccounts();

Console.WriteLine("\n=== Cari Akun ===");
bank.FindAccount("SA001");
bank.FindAccount("XX999"); // Akun tidak ditemukan
