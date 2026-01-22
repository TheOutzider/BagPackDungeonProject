using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PrjectBackPackDungeon;

public static class EnemyGenerator
{
    private static Random _random = new Random();

    private class EnemyBase
    {
        public string Name;
        public int BaseHp;
        public int BaseMinDmg;
        public int BaseMaxDmg;
        public Color Color;
        public string Category;
    }

    private static List<EnemyBase> _bases = new List<EnemyBase>
    {
        new EnemyBase { Name = "Rat", BaseHp = 15, BaseMinDmg = 2, BaseMaxDmg = 4, Color = Color.Gray, Category = "Beast" },
        new EnemyBase { Name = "Slime", BaseHp = 25, BaseMinDmg = 1, BaseMaxDmg = 3, Color = Color.LimeGreen, Category = "Ooze" },
        new EnemyBase { Name = "Bat", BaseHp = 12, BaseMinDmg = 3, BaseMaxDmg = 5, Color = Color.MediumPurple, Category = "Beast" },
        new EnemyBase { Name = "Skeleton", BaseHp = 30, BaseMinDmg = 4, BaseMaxDmg = 6, Color = Color.WhiteSmoke, Category = "Undead" },
        new EnemyBase { Name = "Orc", BaseHp = 45, BaseMinDmg = 6, BaseMaxDmg = 10, Color = Color.DarkOliveGreen, Category = "Humanoid" },
        new EnemyBase { Name = "Ghost", BaseHp = 35, BaseMinDmg = 5, BaseMaxDmg = 8, Color = Color.LightCyan, Category = "Undead" },
        new EnemyBase { Name = "Spider", BaseHp = 40, BaseMinDmg = 7, BaseMaxDmg = 9, Color = Color.DarkSlateGray, Category = "Beast" },
        new EnemyBase { Name = "Golem", BaseHp = 70, BaseMinDmg = 4, BaseMaxDmg = 12, Color = Color.SaddleBrown, Category = "Construct" },
        new EnemyBase { Name = "Demon", BaseHp = 80, BaseMinDmg = 10, BaseMaxDmg = 15, Color = Color.DarkRed, Category = "Demon" },
        new EnemyBase { Name = "Drake", BaseHp = 100, BaseMinDmg = 12, BaseMaxDmg = 18, Color = Color.DarkOrange, Category = "Beast" },
        new EnemyBase { Name = "Lich", BaseHp = 90, BaseMinDmg = 15, BaseMaxDmg = 20, Color = Color.MediumSlateBlue, Category = "Undead" }
    };

    public static Enemy GenerateEnemy(int floorLevel, bool isElite = false, bool isBoss = false)
    {
        List<EnemyBase> possibleBases;
        if (floorLevel < 3) possibleBases = _bases.GetRange(0, 4);
        else if (floorLevel < 7) possibleBases = _bases.GetRange(0, 8);
        else possibleBases = _bases;

        EnemyBase b = possibleBases[_random.Next(possibleBases.Count)];
        
        int hp = b.BaseHp + (floorLevel * 10);
        int minDmg = b.BaseMinDmg + (floorLevel * 2);
        int maxDmg = b.BaseMaxDmg + (floorLevel * 2);

        if (isElite) { hp = (int)(hp * 1.8f); minDmg += 5; maxDmg += 8; }
        if (isBoss) { hp = (int)(hp * 4.0f); minDmg += 15; maxDmg += 25; }

        Enemy enemy = new Enemy(b.Name, hp, minDmg, maxDmg, null, null);
        enemy.Tint = b.Color;

        // Attribution des capacités selon la catégorie
        AddAbilitiesByCategory(enemy, b.Category, floorLevel);

        if (isElite) enemy.Name = "ELITE " + enemy.Name;
        if (isBoss) enemy.Name = "BOSS " + enemy.Name;

        return enemy;
    }

    private static void AddAbilitiesByCategory(Enemy e, string category, int level)
    {
        switch (category)
        {
            case "Beast":
                e.Abilities.Add(new EnemyAbility("Bite", AbilityType.DebuffPlayer, 1, StatusEffectType.Poison, "Applies Poison"));
                break;
            case "Ooze":
                e.Abilities.Add(new EnemyAbility("Regenerate", AbilityType.Heal, 10 + level, null, "Heals self"));
                break;
            case "Undead":
                e.Abilities.Add(new EnemyAbility("Chill", AbilityType.DebuffPlayer, 1, StatusEffectType.Weak, "Applies Weak"));
                break;
            case "Construct":
                e.Abilities.Add(new EnemyAbility("Harden", AbilityType.BuffShield, 15 + level, StatusEffectType.Shield, "Gains Shield"));
                break;
            case "Demon":
                e.Abilities.Add(new EnemyAbility("Siphon", AbilityType.StealMana, 10, null, "Steals Mana"));
                break;
            case "Humanoid":
                e.Abilities.Add(new EnemyAbility("Warcry", AbilityType.DebuffPlayer, 1, StatusEffectType.Vulnerable, "Applies Vulnerable"));
                break;
        }
    }
}
