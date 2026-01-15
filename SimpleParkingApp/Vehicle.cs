public abstract class Vehicle
{
    string licensePlate;
    int timeIn;
    int saldo;

    public Vehicle(string licensePlate, int timeIn, int saldo)
    {
        this.licensePlate = licensePlate;
        this.timeIn = timeIn;
        this.saldo = saldo;
    }

    public int getSaldo()
    {
        return this.saldo;
    }

    public int setSaldo(int saldo)
    {
        return this.saldo = saldo;
    }

    public virtual void DisplaySaldo()
    {
        Console.WriteLine($"Current saldo: ${this.saldo}");
    }

    public int countParkingDuration(int timeOut)
    {
        int result = timeOut - this.timeIn;
        Console.WriteLine($"Vehicle {this.licensePlate} parked for {result} hours.");
    
        return result;
    }
    public abstract void parkingFee(int timeOut);
}