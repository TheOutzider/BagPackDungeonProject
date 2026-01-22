using System;

namespace PrjectBackPackDungeon;

public enum StatusEffectType
{
    Poison,   // Dégâts à chaque tour
    Bleed,    // Dégâts quand on attaque/est attaqué
    Weak,     // Dégâts infligés réduits
    Vulnerable, // Dégâts subis augmentés
    Shield,   // Réduit les prochains dégâts subis
    Regen     // Soigne à chaque tour
}

public class StatusEffect
{
    public StatusEffectType Type { get; set; }
    public int Duration { get; set; } // Nombre de tours restants
    public int Intensity { get; set; } // Puissance de l'effet

    public StatusEffect(StatusEffectType type, int duration, int intensity = 1)
    {
        Type = type;
        Duration = duration;
        Intensity = intensity;
    }
}
