using Microsoft.Xna.Framework;

namespace PrjectBackPackDungeon;

public enum DiceType
{
    None,
    D4_Basic,
    D6_Fire,
    D6_Ice,
    D8_Basic,
    D10_Basic,
    D12_Basic,
    D20_Steel
}

public enum ItemRarity
{
    Common,
    Magic,
    Rare,
    Unique
}

public enum ItemType
{
    Weapon,
    Armor,
    Accessory,
    Gem,
    Consumable,
    Other
}

public class Item
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ItemRarity Rarity { get; set; }
    public ItemType Type { get; set; }
    
    // Dimensions
    public int Width { get; set; }
    public int Height { get; set; }
    
    public Color Color { get; set; }
    public DiceType DiceType { get; set; }
    
    // Stats de Base (Fixes)
    public int BaseStr { get; set; }
    public int BaseDex { get; set; }
    public int BaseInt { get; set; }
    public int BaseLuck { get; set; }
    
    // Stats Actives (Base + Synergies)
    public int ActiveStr { get; set; }
    public int ActiveDex { get; set; }
    public int ActiveInt { get; set; }
    public int ActiveLuck { get; set; }
    
    // Définition de la Synergie (Bonus donné aux voisins)
    public int SynergyBonusStr { get; set; }
    public int SynergyBonusDex { get; set; }
    public int SynergyBonusInt { get; set; }
    public ItemType SynergyTarget { get; set; } // Type d'item affecté (ex: Weapon)
    public bool HasSynergy => SynergyBonusStr > 0 || SynergyBonusDex > 0 || SynergyBonusInt > 0;

    // Position Grille
    public int GridX { get; set; }
    public int GridY { get; set; }
    
    // Drag
    public bool IsDragging { get; set; }
    public Vector2 DragOffset { get; set; }

    public Item(string name, int width, int height, Color color, DiceType diceType = DiceType.None, ItemType type = ItemType.Other)
    {
        Name = name;
        Width = width;
        Height = height;
        Color = color;
        DiceType = diceType;
        Type = type;
        Description = "";
        Rarity = ItemRarity.Common;
        
        GridX = -1;
        GridY = -1;
    }

    public void ResetStats()
    {
        ActiveStr = BaseStr;
        ActiveDex = BaseDex;
        ActiveInt = BaseInt;
        ActiveLuck = BaseLuck;
    }

    public Color GetRarityColor()
    {
        switch (Rarity)
        {
            case ItemRarity.Common: return Color.White;
            case ItemRarity.Magic: return Color.CornflowerBlue;
            case ItemRarity.Rare: return Color.Gold;
            case ItemRarity.Unique: return Color.OrangeRed;
            default: return Color.White;
        }
    }
}