using System;

namespace PrjectBackPackDungeon;

public enum SkillType
{
    Heal,
    DirectDamage,
    Reroll,
    Stun,
    ManaRestore // Pour les potions ou autre
}

public class Skill
{
    public string Name { get; set; }
    public int ManaCost { get; set; }
    public string Description { get; set; }
    public SkillType Type { get; set; }
    public int Value { get; set; } // Montant de soin, dégâts, etc.

    public Skill(string name, int manaCost, SkillType type, int value, string description)
    {
        Name = name;
        ManaCost = manaCost;
        Type = type;
        Value = value;
        Description = description;
    }
}