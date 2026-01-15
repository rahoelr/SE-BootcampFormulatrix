public class Motorcycle : Vehicle, IParkingFee
{
    public Motorcycle(string licensePlate, int timeIn, int saldo) : base(licensePlate, timeIn, saldo)
    {
    }

    public override void parkingFee(int timeOut)
    {
        int duration = countParkingDuration(timeOut);
        int fee = duration * 2000; // Assuming $3 per hour for motorcycles
    
        if (getSaldo() < fee)
        {
            Console.WriteLine("Insufficient balance to pay the parking fee.");
            return;
        }

        int sisaSaldo = getSaldo() - fee;
        setSaldo(sisaSaldo);

        Console.WriteLine($"Motorcycle parking fee is ${fee}. \nSisa Saldo: ${sisaSaldo}");
    }

    // public void DisplaySaldo()
    // {
    //     Console.WriteLine($"Current saldo: ${getSaldo()}");
    // }

}