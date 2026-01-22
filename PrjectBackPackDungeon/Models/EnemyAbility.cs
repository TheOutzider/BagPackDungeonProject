using System;

namespace PrjectBackPackDungeon;

public enum AbilityType
{
    Attack,
    Heal,
    BuffShield,
    DebuffPlayer,
    StealGold,
    StealMana,
    CurseDice // Pourrait réduire les dégâts des dés au prochain tour
}

public class EnemyAbility
{
    public string Name { get; set; }
    public AbilityType Type { get; set; }
    public int Value { get; set; }
    public StatusEffectType? EffectType { get; set; }
    public string Description { get; set; }

    public EnemyAbility(string name, AbilityType type, int value, StatusEffectType? effect = null, string desc = "")
    {
        Name = name;
        Type = type;
        Value = value;
        EffectType = effect;
        Description = desc;
    }
}
