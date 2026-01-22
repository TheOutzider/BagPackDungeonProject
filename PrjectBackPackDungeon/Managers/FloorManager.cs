using System;
using System.Collections.Generic;

namespace PrjectBackPackDungeon;

public class FloorManager
{
    private Random _random;
    private int _seed;
    private int _dungeonLevel;
    private int _roomNumber;
    
    public Room CurrentRoom { get; private set; }
    public int DungeonLevel => _dungeonLevel;
    public int RoomNumber => _roomNumber;
    public int Seed => _seed;

    private Dictionary<int, string[]> _biomes = new Dictionary<int, string[]>
    {
        { 1, new[] { "The Damp Caves", "A dark, mossy cavern.", "Water drips from the ceiling." } },
        { 2, new[] { "The Ancient Ruins", "Crumbling stone walls surround you.", "Dusty corridors of a lost age." } },
        { 3, new[] { "The Infernal Depths", "The air is scorching hot.", "Lava flows through the cracks." } }
    };

    public FloorManager(int dungeonLevel = 1, int seed = -1)
    {
        _dungeonLevel = dungeonLevel;
        _roomNumber = 0;
        
        if (seed == -1)
            _seed = new Random().Next();
        else
            _seed = seed;
            
        _random = new Random(_seed);
        
        SetNextRoom(GenerateCombatRoom(0, true));
    }
    
    public FloorManager(SaveData data)
    {
        _dungeonLevel = data.DungeonLevel;
        _roomNumber = data.FloorNumber;
        _seed = data.WorldSeed;
        _random = new Random(_seed);
        
        for(int i=0; i<_roomNumber * 5; i++) _random.Next();

        if (data.CurrentRoom != null)
        {
            CurrentRoom = new Room(data.CurrentRoom.Type, data.CurrentRoom.Description, 
                data.CurrentRoom.EnemyName, data.CurrentRoom.EnemyHp, 
                data.CurrentRoom.EnemyMinDmg, data.CurrentRoom.EnemyMaxDmg);
        }
        else
        {
            SetNextRoom(GenerateCombatRoom(_roomNumber));
        }
    }

    public void SetNextRoom(Room room)
    {
        CurrentRoom = room;
        _roomNumber++;
        
        // Si on a battu le boss, on prépare le passage au niveau suivant
        if (room.Type == RoomType.Boss)
        {
            // Le passage effectif se fera via une salle spéciale ou un bouton dans CoreGame
        }
    }

    public void AdvanceToNextFloor()
    {
        _dungeonLevel++;
        _roomNumber = 0;
        SetNextRoom(GenerateCombatRoom(0, true));
    }

    public List<Room> GenerateNextRoomOptions()
    {
        var options = new List<Room>();
        
        // Boss à la salle 10
        if (_roomNumber == 9) 
        {
            options.Add(GenerateBossRoom(_roomNumber));
            return options;
        }

        int count = 3;
        for (int i = 0; i < count; i++)
        {
            double roll = _random.NextDouble();
            Room room;
            
            if (roll < 0.40) room = GenerateCombatRoom(_roomNumber);
            else if (roll < 0.55) room = new Room(RoomType.Event, "A mysterious event awaits.");
            else if (roll < 0.70) room = new Room(RoomType.Shop, "A traveling merchant has set up camp.");
            else if (roll < 0.85) room = new Room(RoomType.Rest, "A safe place to catch your breath.");
            else if (roll < 0.95) room = GenerateEliteRoom(_roomNumber);
            else room = new Room(RoomType.Rest, "You found a hidden Treasure Room!", "Treasure", 0, 0, 0); // On détourne Rest pour le loot
            
            options.Add(room);
        }
        
        return options;
    }

    private string GetBiomeDesc()
    {
        int biomeKey = ((_dungeonLevel - 1) % 3) + 1;
        var desc = _biomes[biomeKey];
        return desc[_random.Next(1, desc.Length)];
    }

    private Room GenerateCombatRoom(int level, bool isEasy = false)
    {
        var enemy = EnemyGenerator.GenerateEnemy(_dungeonLevel, false, false);
        return new Room(RoomType.Combat, GetBiomeDesc(), enemy.Name, enemy.Hp, enemy.MinDamage, enemy.MaxDamage);
    }
    
    private Room GenerateEliteRoom(int level)
    {
        var enemy = EnemyGenerator.GenerateEnemy(_dungeonLevel, true, false);
        return new Room(RoomType.Elite, "A menacing aura fills the air...", enemy.Name, enemy.Hp, enemy.MinDamage, enemy.MaxDamage);
    }
    
    private Room GenerateBossRoom(int level)
    {
        var enemy = EnemyGenerator.GenerateEnemy(_dungeonLevel, false, true);
        return new Room(RoomType.Boss, "The master of this floor awaits you.", enemy.Name, enemy.Hp, enemy.MinDamage, enemy.MaxDamage);
    }
}
