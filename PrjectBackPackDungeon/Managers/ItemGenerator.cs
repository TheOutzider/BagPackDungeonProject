using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PrjectBackPackDungeon;

public static class ItemGenerator
{
    private static Random _random = new Random();

    private class ItemBase
    {
        public string Name;
        public int Width;
        public int Height;
        public Color BaseColor;
        public DiceType BaseDice;
        public ItemType Type;
        
        public int SynStr;
        public int SynDex;
        public int SynInt;
        public ItemType SynTarget;
    }

    private static List<ItemBase> _bases = new List<ItemBase>
    {
        // Weapons
        new ItemBase { Name = "Dagger", Width = 1, Height = 2, BaseColor = Color.Silver, BaseDice = DiceType.D4_Basic, Type = ItemType.Weapon },
        new ItemBase { Name = "Shortsword", Width = 1, Height = 3, BaseColor = Color.Gray, BaseDice = DiceType.D6_Fire, Type = ItemType.Weapon },
        new ItemBase { Name = "Mace", Width = 2, Height = 2, BaseColor = Color.DarkGray, BaseDice = DiceType.D8_Basic, Type = ItemType.Weapon },
        new ItemBase { Name = "Longsword", Width = 1, Height = 4, BaseColor = Color.SteelBlue, BaseDice = DiceType.D8_Basic, Type = ItemType.Weapon },
        new ItemBase { Name = "Battleaxe", Width = 2, Height = 3, BaseColor = Color.IndianRed, BaseDice = DiceType.D10_Basic, Type = ItemType.Weapon },
        new ItemBase { Name = "Staff", Width = 1, Height = 3, BaseColor = Color.Purple, BaseDice = DiceType.D6_Ice, Type = ItemType.Weapon },
        
        // Armors
        new ItemBase { Name = "Shield", Width = 2, Height = 2, BaseColor = Color.DarkBlue, BaseDice = DiceType.D4_Basic, Type = ItemType.Armor },
        new ItemBase { Name = "Helmet", Width = 2, Height = 2, BaseColor = Color.DarkGray, BaseDice = DiceType.None, Type = ItemType.Armor },
        new ItemBase { Name = "Chestplate", Width = 2, Height = 3, BaseColor = Color.SlateGray, BaseDice = DiceType.None, Type = ItemType.Armor },
        
        // Accessories
        new ItemBase { Name = "Ring", Width = 1, Height = 1, BaseColor = Color.Gold, BaseDice = DiceType.None, Type = ItemType.Accessory },
        new ItemBase { Name = "Amulet", Width = 1, Height = 1, BaseColor = Color.Cyan, BaseDice = DiceType.None, Type = ItemType.Accessory },
        
        // Gems
        new ItemBase { Name = "Ruby", Width = 1, Height = 1, BaseColor = Color.Red, BaseDice = DiceType.None, Type = ItemType.Gem, SynStr = 2, SynTarget = ItemType.Weapon },
        new ItemBase { Name = "Sapphire", Width = 1, Height = 1, BaseColor = Color.Blue, BaseDice = DiceType.None, Type = ItemType.Gem, SynInt = 2, SynTarget = ItemType.Accessory },
        new ItemBase { Name = "Emerald", Width = 1, Height = 1, BaseColor = Color.Green, BaseDice = DiceType.None, Type = ItemType.Gem, SynDex = 2, SynTarget = ItemType.Weapon }
    };

    private static string[] _prefixes = { "Burning", "Frozen", "Swift", "Arcane", "Brutal", "Savage", "Divine", "Dark", "Radiant", "Vampiric", "Venomous" };
    private static string[] _suffixes = { "of Strength", "of Dexterity", "of Wisdom", "of Luck", "of Doom", "of the Bear", "of the Phoenix" };

    public static Item GenerateItem(int level)
    {
        var baseItem = _bases[_random.Next(_bases.Count)];
        
        double roll = _random.NextDouble();
        ItemRarity rarity = ItemRarity.Common;
        if (roll > 0.95) rarity = ItemRarity.Unique;
        else if (roll > 0.85) rarity = ItemRarity.Rare;
        else if (roll > 0.60) rarity = ItemRarity.Magic;

        Item item = new Item(baseItem.Name, baseItem.Width, baseItem.Height, baseItem.BaseColor, baseItem.BaseDice, baseItem.Type);
        item.Rarity = rarity;
        
        if (rarity != ItemRarity.Common)
        {
            string prefix = _prefixes[_random.Next(_prefixes.Length)];
            item.Name = $"{prefix} {baseItem.Name}";
            ApplyEffectFromWord(item, prefix, level);
        }

        if (rarity == ItemRarity.Rare || rarity == ItemRarity.Unique)
        {
            string suffix = _suffixes[_random.Next(_suffixes.Length)];
            item.Name += $" {suffix}";
            item.BaseStr += _random.Next(1, 4) + (level / 5);
        }

        if (item.Type == ItemType.Armor)
        {
            item.EffectType = StatusEffectType.Shield;
            item.EffectValue = 5 + level;
            item.Description = $"Provides {item.EffectValue} Shield each turn.";
        }

        item.ResetStats();
        return item;
    }

    private static void ApplyEffectFromWord(Item item, string word, int level)
    {
        switch (word)
        {
            case "Venomous":
                item.EffectType = StatusEffectType.Poison;
                item.EffectValue = 2 + (level / 3);
                item.Description = $"Applies {item.EffectValue} Poison to enemies.";
                break;
            case "Burning":
                item.EffectType = StatusEffectType.Vulnerable;
                item.EffectValue = 1;
                item.Description = "Makes enemies Vulnerable.";
                break;
            case "Frozen":
                item.EffectType = StatusEffectType.Weak;
                item.EffectValue = 1;
                item.Description = "Makes enemies Weak.";
                break;
            case "Vampiric":
                item.EffectType = StatusEffectType.Regen;
                item.EffectValue = 1 + (level / 10);
                item.Description = $"Grants {item.EffectValue} Regen to you.";
                break;
        }
    }
}
