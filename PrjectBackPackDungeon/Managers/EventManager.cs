using System;
using System.Collections.Generic;

namespace PrjectBackPackDungeon;

public enum EventVisual { Fountain, Altar, Merchant, Chest, Library }

public class EventChoice
{
    public string Text;
    public string ResultMessage;
    public Action<CoreGame> Action;

    public EventChoice(string text, string resultMessage, Action<CoreGame> action)
    {
        Text = text;
        ResultMessage = resultMessage;
        Action = action;
    }
}

public class GameEvent
{
    public string Title;
    public string Description;
    public EventVisual Visual;
    public List<EventChoice> Choices;

    public GameEvent(string title, string description, EventVisual visual)
    {
        Title = title;
        Description = description;
        Visual = visual;
        Choices = new List<EventChoice>();
    }
}

public static class EventManager
{
    private static Random _random = new Random();

    public static GameEvent GenerateEvent(int floorLevel)
    {
        int roll = _random.Next(4);
        return roll switch
        {
            0 => CreateFountainEvent(),
            1 => CreateThiefEvent(),
            2 => CreateOldLibraryEvent(),
            _ => CreateMysteriousChestEvent()
        };
    }

    private static GameEvent CreateFountainEvent()
    {
        var ev = new GameEvent("The Glowing Fountain", "You find a fountain emitting a soft blue light. The water looks pure.", EventVisual.Fountain);
        ev.Choices.Add(new EventChoice("Drink the water", "You feel refreshed! (+20 HP)", g => g.HealPlayer(20)));
        ev.Choices.Add(new EventChoice("Throw a coin (-10G)", "The fountain glows brighter. (+10 Max HP)", g => {
            if (g.PlayerGold >= 10) { g.AddGold(-10); g.PlayerMaxHp += 10; g.HealPlayer(10); }
        }));
        ev.Choices.Add(new EventChoice("Leave", "You decide not to risk it.", g => { }));
        return ev;
    }

    private static GameEvent CreateThiefEvent()
    {
        var ev = new GameEvent("A Shady Merchant", "A hooded figure offers you a 'special' deal. He wants your gold for a mystery bag.", EventVisual.Merchant);
        ev.Choices.Add(new EventChoice("Buy the bag (50G)", "It's... a rusty spoon? No, wait, it's magic!", g => {
            if (g.PlayerGold >= 50) { g.AddGold(-50); }
        }));
        ev.Choices.Add(new EventChoice("Intimidate him", "He runs away, dropping some coins.", g => g.AddGold(20)));
        ev.Choices.Add(new EventChoice("Ignore", "You walk away quickly.", g => { }));
        return ev;
    }

    private static GameEvent CreateOldLibraryEvent()
    {
        var ev = new GameEvent("Forgotten Library", "Dusty shelves filled with ancient tomes surround you.", EventVisual.Library);
        ev.Choices.Add(new EventChoice("Read a red book", "Your mind burns with power. (+5 INT, -10 HP)", g => { g.DamagePlayer(10); }));
        ev.Choices.Add(new EventChoice("Read a green book", "You feel more agile. (+5 DEX)", g => { }));
        ev.Choices.Add(new EventChoice("Rest in the corner", "You have a peaceful nap. (+15 Mana)", g => g.AddMana(15)));
        return ev;
    }

    private static GameEvent CreateMysteriousChestEvent()
    {
        var ev = new GameEvent("The Mimic?", "A chest sits alone in the middle of the room. It's breathing... slightly.", EventVisual.Chest);
        ev.Choices.Add(new EventChoice("Open it carefully", "It was just a regular chest. (+30 Gold)", g => g.AddGold(30)));
        ev.Choices.Add(new EventChoice("Attack it first", "You destroyed the lock! (-5 HP)", g => g.DamagePlayer(5)));
        ev.Choices.Add(new EventChoice("Leave it alone", "Better safe than sorry.", g => { }));
        return ev;
    }
}
