public class Car : Vehicle
{
    public Car(string licensePlate, int timeIn, int saldo) : base(licensePlate, timeIn, saldo )
    {
    }

    public override void parkingFee(int timeOut)
    {
        int duration = countParkingDuration(timeOut);
        int fee = duration * 3000; // Assuming $5 per hour for cars
        Console.WriteLine($"Car parking fee is ${fee}.");
    } 
}