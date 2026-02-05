using System.Reflection.Metadata;
using MonopolyApp.Enums;

namespace MonopolyApp.Interfaces
{
    public interface ICard
    {
        string Name {get; set;}
        string? Description {get; set;}
        int Value {get; set;}
        CardEffect CardEffect {get; set;}
    }
}