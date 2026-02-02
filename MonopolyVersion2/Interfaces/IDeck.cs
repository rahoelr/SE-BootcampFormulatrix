using MonopolyApp.Enums;

namespace MonopolyApp.Interfaces
{
    public interface IDeck
    {
        List<ICard> Cards {get; set;}
    }
}