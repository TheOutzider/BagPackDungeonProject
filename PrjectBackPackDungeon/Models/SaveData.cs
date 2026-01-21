using System;
using System.Collections.Generic;

namespace PrjectBackPackDungeon;

public class SaveData
{
    public DateTime SaveDate { get; set; }
    public int WorldSeed { get; set; }

    // Progression
    public int DungeonLevel { get; set; } = 1;
    public int FloorNumber { get; set; }
    public PlayerClass Class { get; set; }
    
    // Stats Joueur
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Mana { get; set; }
    public int MaxMana { get; set; }
    public int Gold { get; set; }
    
    // Inventaire
    public List<ItemData> InventoryItems { get; set; }
    
    // Reliques
    public List<RelicData> Relics { get; set; }
    
    // Salle actuelle (pour recharger au bon endroit)
    public RoomData CurrentRoom { get; set; }
}

public class RoomData
{
    public RoomType Type { get; set; }
    public string Description { get; set; }
    public string EnemyName { get; set; }
    public int EnemyHp { get; set; }
    public int EnemyMinDmg { get; set; }
    public int EnemyMaxDmg { get; set; }
}

public class RelicData
{
    public string Name { get; set; }
    public string Description { get; set; }
    public RelicEffectType EffectType { get; set; }
    public int Value { get; set; }
    public string StatTarget { get; set; }
}

public class ItemData
{
    public string Name { get; set; }
    public int GridX { get; set; }
    public int GridY { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    
    public int BonusStr { get; set; }
    public int BonusDex { get; set; }
    public int BonusInt { get; set; }
    public int BonusLuck { get; set; }
    
    public DiceType DiceType { get; set; }
    public ItemRarity Rarity { get; set; }
    public ItemType Type { get; set; }
    
    public int SynergyBonusStr { get; set; }
    public int SynergyBonusDex { get; set; }
    public int SynergyBonusInt { get; set; }
    public ItemType SynergyTarget { get; set; }
}