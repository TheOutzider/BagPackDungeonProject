namespace PrjectBackPackDungeon;

public enum RoomType
{
    Combat,
    Elite,
    Rest,
    Shop,
    Event, // Nouveau type
    Boss
}

public class Room
{
    public RoomType Type { get; private set; }
    public string Description { get; private set; }
    
    // Pour les salles de combat
    public string EnemyName { get; private set; }
    public int EnemyHp { get; private set; }
    public int EnemyMinDmg { get; private set; }
    public int EnemyMaxDmg { get; private set; }

    public Room(RoomType type, string description, string enemyName = null, int enemyHp = 0, int minDmg = 0, int maxDmg = 0)
    {
        Type = type;
        Description = description;
        EnemyName = enemyName;
        EnemyHp = enemyHp;
        EnemyMinDmg = minDmg;
        EnemyMaxDmg = maxDmg;
    }
}