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
        new ItemBase { Name = "Warhammer", Width = 2, Height = 3, BaseColor = Color.DarkSlateGray, BaseDice = DiceType.D12_Basic, Type = ItemType.Weapon },
        new ItemBase { Name = "Staff", Width = 1, Height = 3, BaseColor = Color.Purple, BaseDice = DiceType.D6_Ice, Type = ItemType.Weapon },
        
        // Armors
        new ItemBase { Name = "Shield", Width = 2, Height = 2, BaseColor = Color.DarkBlue, BaseDice = DiceType.D4_Basic, Type = ItemType.Armor },
        new ItemBase { Name = "Helmet", Width = 2, Height = 2, BaseColor = Color.DarkGray, BaseDice = DiceType.None, Type = ItemType.Armor },
        new ItemBase { Name = "Chestplate", Width = 2, Height = 3, BaseColor = Color.SlateGray, BaseDice = DiceType.None, Type = ItemType.Armor },
        
        // Accessories
        new ItemBase { Name = "Ring", Width = 1, Height = 1, BaseColor = Color.Gold, BaseDice = DiceType.None, Type = ItemType.Accessory },
        new ItemBase { Name = "Amulet", Width = 1, Height = 1, BaseColor = Color.Cyan, BaseDice = DiceType.None, Type = ItemType.Accessory },
        new ItemBase { Name = "Tome", Width = 2, Height = 2, BaseColor = Color.Brown, BaseDice = DiceType.D6_Ice, Type = ItemType.Accessory },
        
        // Gems
        new ItemBase { Name = "Ruby", Width = 1, Height = 1, BaseColor = Color.Red, BaseDice = DiceType.None, Type = ItemType.Gem, SynStr = 2, SynTarget = ItemType.Weapon },
        new ItemBase { Name = "Sapphire", Width = 1, Height = 1, BaseColor = Color.Blue, BaseDice = DiceType.None, Type = ItemType.Gem, SynInt = 2, SynTarget = ItemType.Accessory },
        new ItemBase { Name = "Emerald", Width = 1, Height = 1, BaseColor = Color.Green, BaseDice = DiceType.None, Type = ItemType.Gem, SynDex = 2, SynTarget = ItemType.Weapon }
    };

    private static string[] _shoddyPrefixes = { "Rusty", "Old", "Broken", "Chipped", "Dusty", "Dull" };
    private static string[] _prefixes = { "Burning", "Frozen", "Heavy", "Swift", "Arcane", "Brutal", "Savage", "Ancient", "Cursed", "Divine", "Dark" };
    private static string[] _suffixes = { "of Strength", "of Dexterity", "of Wisdom", "of Luck", "of Doom", "of the Bear", "of the Eagle", "of the Owl", "of Hell", "of Heavens" };
    private static string[] _uniqueNames = { "The World Breaker", "Thunderfury", "Soul Eater", "Heart of the Mountain", "Whisper of the Void", "Godslayer", "Excalibur's Cousin" };

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
        
        if (baseItem.Type == ItemType.Gem)
        {
            item.SynergyBonusStr = baseItem.SynStr;
            item.SynergyBonusDex = baseItem.SynDex;
            item.SynergyBonusInt = baseItem.SynInt;
            item.SynergyTarget = baseItem.SynTarget;
            item.Description = "Place adjacent to items to boost them.";
        }

        if (rarity == ItemRarity.Common)
        {
            string prefix = _shoddyPrefixes[_random.Next(_shoddyPrefixes.Length)];
            item.Name = $"{prefix} {baseItem.Name}";
        }
        else if (rarity == ItemRarity.Unique)
        {
            item.Name = _uniqueNames[_random.Next(_uniqueNames.Length)];
            item.Color = Color.OrangeRed;
            item.BaseStr += _random.Next(3, 6) + level;
            item.BaseDex += _random.Next(3, 6) + level;
            item.BaseInt += _random.Next(3, 6) + level;
            item.BaseLuck += _random.Next(3, 6) + level;
            
            // Upgrade Dice for Uniques
            if (item.DiceType == DiceType.D4_Basic) item.DiceType = DiceType.D8_Basic;
            else if (item.DiceType == DiceType.D6_Fire) item.DiceType = DiceType.D10_Basic;
            else if (item.DiceType == DiceType.D6_Ice) item.DiceType = DiceType.D10_Basic;
            else if (item.DiceType == DiceType.D8_Basic) item.DiceType = DiceType.D12_Basic;
            else if (item.DiceType == DiceType.D10_Basic) item.DiceType = DiceType.D20_Steel;
            else if (item.DiceType == DiceType.D12_Basic) item.DiceType = DiceType.D20_Steel;
            else if (item.DiceType == DiceType.None) item.DiceType = DiceType.D6_Fire;
        }
        else
        {
            int statPoints = (rarity == ItemRarity.Rare) ? 4 : 2;
            statPoints += level;

            bool hasPrefix = _random.Next(2) == 0 || rarity == ItemRarity.Rare;
            bool hasSuffix = !hasPrefix || rarity == ItemRarity.Rare;

            string prefix = "";
            string suffix = "";

            if (hasPrefix)
            {
                prefix = _prefixes[_random.Next(_prefixes.Length)];
                ApplyStatFromWord(item, prefix, statPoints);
            }

            if (hasSuffix)
            {
                suffix = _suffixes[_random.Next(_suffixes.Length)];
                ApplyStatFromWord(item, suffix, statPoints);
            }

            if (hasPrefix && hasSuffix) item.Name = $"{prefix} {baseItem.Name} {suffix}";
            else if (hasPrefix) item.Name = $"{prefix} {baseItem.Name}";
            else if (hasSuffix) item.Name = $"{baseItem.Name} {suffix}";
        }
        
        item.ResetStats();

        return item;
    }

    private static void ApplyStatFromWord(Item item, string word, int points)
    {
        if (word.Contains("Heavy") || word.Contains("Brutal") || word.Contains("Strength") || word.Contains("Bear"))
            item.BaseStr += points;
        else if (word.Contains("Swift") || word.Contains("Dexterity") || word.Contains("Eagle") || word.Contains("Speed"))
            item.BaseDex += points;
        else if (word.Contains("Arcane") || word.Contains("Wisdom") || word.Contains("Owl") || word.Contains("Frozen"))
            item.BaseInt += points;
        else if (word.Contains("Lucky") || word.Contains("Luck") || word.Contains("Divine"))
            item.BaseLuck += points;
        else if (word.Contains("Burning") || word.Contains("Hell"))
        {
            item.BaseStr += points / 2;
            item.BaseInt += points / 2;
        }
        else
        {
            item.BaseStr += 1;
            item.BaseDex += 1;
        }
    }
}