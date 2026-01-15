public class Truck : Vehicle, IParkingFee
{
    public Truck(string licensePlate, int timeIn, int saldo) : base(licensePlate, timeIn, saldo)
    {
    }

    public override void parkingFee(int timeOut)
    {
        int duration = countParkingDuration(timeOut);
        int fee = duration * 4000; // Assuming $7 per hour for trucks
        Console.WriteLine($"Truck parking fee is ${fee}.");
    }
}