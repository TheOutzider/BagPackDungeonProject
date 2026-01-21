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

    public FloorManager(int dungeonLevel = 1, int seed = -1)
    {
        _dungeonLevel = dungeonLevel;
        _roomNumber = 0;
        
        if (seed == -1)
            _seed = new Random().Next();
        else
            _seed = seed;
            
        _random = new Random(_seed);
        
        // Première salle : Combat facile
        SetNextRoom(GenerateCombatRoom(0, true));
    }
    
    public FloorManager(SaveData data)
    {
        _dungeonLevel = data.DungeonLevel;
        _roomNumber = data.FloorNumber;
        _seed = data.WorldSeed;
        _random = new Random(_seed);
        
        // On avance le RNG pour rattraper l'état (approximatif mais suffisant)
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
    }

    public List<Room> GenerateNextRoomOptions()
    {
        var options = new List<Room>();
        
        // Si on est loin dans l'étage (ex: salle 15), on propose le Boss
        if (_roomNumber >= 15)
        {
            options.Add(GenerateBossRoom(_roomNumber));
            return options; // Une seule option : le Boss
        }

        // Sinon, 3 choix aléatoires
        for (int i = 0; i < 3; i++)
        {
            double roll = _random.NextDouble();
            Room room;
            
            if (roll < 0.50) room = GenerateCombatRoom(_roomNumber);
            else if (roll < 0.60) room = new Room(RoomType.Event, "A strange encounter.");
            else if (roll < 0.70) room = new Room(RoomType.Shop, "A merchant awaits.");
            else if (roll < 0.85) room = new Room(RoomType.Rest, "A quiet campfire.");
            else if (_roomNumber > 5) room = GenerateEliteRoom(_roomNumber);
            else room = GenerateCombatRoom(_roomNumber);
            
            options.Add(room);
        }
        
        return options;
    }

    private Room GenerateCombatRoom(int level, bool isEasy = false)
    {
        string[] enemies = { "Rat", "Spider", "Bat", "Slime", "Goblin", "Wolf" };
        string[] adjectives = { "Small", "Weak", "Angry", "Hungry", "Wild" };
        
        string name = enemies[_random.Next(enemies.Length)];
        if (!isEasy) name = $"{adjectives[_random.Next(adjectives.Length)]} {name}";
        
        int hpBase = 20 + (level * 5) + ((_dungeonLevel - 1) * 20);
        int dmgBase = 2 + (level / 2) + ((_dungeonLevel - 1) * 2);
        
        if (isEasy)
        {
            hpBase = 15 + ((_dungeonLevel - 1) * 10);
            dmgBase = 1 + (_dungeonLevel - 1);
        }

        return new Room(RoomType.Combat, "A dark corridor.", name, hpBase, dmgBase, dmgBase + 3);
    }
    
    private Room GenerateEliteRoom(int level)
    {
        string[] enemies = { "Orc Captain", "Dark Knight", "Giant Spider", "Necromancer" };
        string name = enemies[_random.Next(enemies.Length)];
        
        int hpBase = 50 + (level * 8) + ((_dungeonLevel - 1) * 30);
        int dmgBase = 5 + (level / 2) + ((_dungeonLevel - 1) * 3);

        return new Room(RoomType.Elite, "A menacing aura fills the room.", name, hpBase, dmgBase, dmgBase + 5);
    }
    
    private Room GenerateBossRoom(int level)
    {
        string[] bosses = { "Dragon", "Lich King", "Demon Lord", "Hydra" };
        string name = bosses[_random.Next(bosses.Length)];
        
        int hpBase = 150 + (level * 10) + ((_dungeonLevel - 1) * 50);
        int dmgBase = 10 + (level / 2) + ((_dungeonLevel - 1) * 5);

        return new Room(RoomType.Boss, "The throne room.", name, hpBase, dmgBase, dmgBase + 10);
    }
}