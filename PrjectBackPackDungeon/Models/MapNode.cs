using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PrjectBackPackDungeon;

public enum NodeState
{
    Locked,     // Pas encore accessible
    Reachable,  // Accessible (prochain choix)
    Visited,    // Déjà fait
    Current     // Position actuelle
}

public class MapNode
{
    public Room Room { get; set; }
    public int GridX { get; set; } // Position en X (0, 1, 2...) dans la couche
    public int GridY { get; set; } // Position en Y (Profondeur/Layer)
    
    public List<int> NextNodesIndices { get; set; } // Indices des nœuds connectés dans la couche suivante
    public NodeState State { get; set; }

    // Pour l'affichage
    public Vector2 Position { get; set; } 

    public MapNode(Room room, int x, int y)
    {
        Room = room;
        GridX = x;
        GridY = y;
        NextNodesIndices = new List<int>();
        State = NodeState.Locked;
    }
}