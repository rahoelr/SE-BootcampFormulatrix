using System.Reflection.Metadata;
using MonopolyBackend.Enums;

namespace MonopolyBackend.Interfaces
{
    public interface ICard
    {
        string Name {get; set;}
        string? Description {get; set;}
        int Value {get; set;}
        CardEffect CardEffect {get; set;}
    }
}