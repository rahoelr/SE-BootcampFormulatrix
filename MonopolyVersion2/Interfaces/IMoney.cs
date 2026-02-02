namespace MonopolyApp.Interfaces
{
    public interface IMoney
    {
        int Balance {get; set;}
        void Add(int amount);
        void Subtract(int amount);
    }
}