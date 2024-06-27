using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Experiments.Core.Pathfinding;

/// <summary>
///     Represents a node in the pathfinding graph.
/// </summary>
/// <param name="position">The position of the node in tile coordinates.</param>
/// <seealso cref="Pathfinder" />
public class Node(Point16 position, float gCost = float.MaxValue, float fCost = float.MaxValue, bool ignorePlatforms = false)
{
    public readonly List<Node> Neighbors = [];
    public readonly Point16 Position = position;

    public float FCost = fCost;
    public float GCost = gCost;
    public float HCost;

    public Node Previous;

    private Tile Tile => Framing.GetTileSafely(Position);
    public bool IsWall => (!ignorePlatforms || !TileID.Sets.Platforms[Tile.TileType]) && Tile.HasTile && Main.tileSolid[Framing.GetTileSafely(Position).TileType];

    public void AddNeighbors(Dictionary<Point16, Node> nodeDictionary)
    {
        if (Neighbors.Count > 0) return;

        for (int di = -1; di <= 1; di++)
        {
            for (int dj = -1; dj <= 1; dj++)
            {
                // Skip the center point
                if (di == 0 && dj == 0) continue;

                Point16 point = new(Position.X + di, Position.Y + dj);

                if (WorldGen.InWorld(point.X, point.Y))
                {
                    if (!nodeDictionary.TryGetValue(point, out Node neighbor))
                    {
                        neighbor = new Node(point, ignorePlatforms: ignorePlatforms);
                        nodeDictionary[point] = neighbor;
                    }

                    if (!neighbor.IsWall)
                        Neighbors.Add(neighbor);
                }
            }
        }
    }
}