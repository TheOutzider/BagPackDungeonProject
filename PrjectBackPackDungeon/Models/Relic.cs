namespace PrjectBackPackDungeon;

public enum RelicEffectType
{
    StatBonus,      // Bonus passif (Str, Dex, etc.)
    StartOfCombat,  // Effet au début du combat
    EndOfTurn,      // Effet à la fin du tour
    OnHeal          // Effet quand on se soigne
}

public class Relic
{
    public string Name { get; set; }
    public string Description { get; set; }
    public RelicEffectType EffectType { get; set; }
    
    // Paramètres de l'effet
    public int Value { get; set; }
    public string StatTarget { get; set; } // "Str", "Dex", "Hp", "Mana"

    public Relic(string name, string description, RelicEffectType effectType, int value, string statTarget = "")
    {
        Name = name;
        Description = description;
        EffectType = effectType;
        Value = value;
        StatTarget = statTarget;
    }
}