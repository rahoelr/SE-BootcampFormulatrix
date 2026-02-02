using System.Reflection.Metadata;
using MonopolyApp.Enums;

namespace MonopolyApp.Interfaces
{
    public interface ICard
    {
        string Name {get; set;}
        CardEffect CardEffect {get; set;}
        
    }
}