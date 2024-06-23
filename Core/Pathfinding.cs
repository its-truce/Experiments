using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace Experiments.Core;

public enum HeuristicType
{
    Manhattan,
    Euclidean
}

public class Node(Point position)
{
    public readonly Point Position = position;

    public float GCost;
    public float HCost;
    public float FCost => GCost + HCost;

    public Node Previous;

    public readonly List<Node> Neighbours = [];

    public bool IsWall => Framing.GetTileSafely(Position).HasTile && Main.tileSolid[Framing.GetTileSafely(Position).TileType];
    public bool IsStranded => !Neighbours.Any(t => t.IsWall);

    public void AddNeighbours()
    {
        for (int di = -1; di <= 1; di++)
        {
            for (int dj = -1; dj <= 1; dj++)
            {
                // Skip the center point
                if (di == 0 && dj == 0) continue;

                Point point = new Point(Position.X + di, Position.Y + dj);

                if (WorldGen.InWorld(point.X, point.Y))
                    Neighbours.Add(new Node(point));
            }
        }
    }
}

public class Pathfinding(Point start, Point end)
{
    private readonly List<Node> _openSet = [new Node(start)];
    private readonly List<Node> _closedSet = [];
    private List<Node> _path = [];

    private static float GetHeuristic(Point start, Point end, HeuristicType heuristicType = HeuristicType.Manhattan) =>
        heuristicType == HeuristicType.Euclidean ? start.ToVector2().Distance(end.ToVector2()) : Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y);

    public void Update(HeuristicType heuristicType = HeuristicType.Manhattan)
    {
        Node current = _openSet[0];

        if (_openSet.Count > 0)
        {
            foreach (Node node in _openSet)
            {
                if (node.FCost < current.FCost)
                    current = node;
            }

            if (current.Position == end)
                Main.NewText("done :)");

            _openSet.Remove(current);
            _closedSet.Add(current);

            current.AddNeighbours();
            foreach (Node neighbour in current.Neighbours)
            {
                neighbour.AddNeighbours();

                if (_closedSet.Contains(neighbour) || neighbour.IsWall) continue;

                float tempGCost = current.GCost + GetHeuristic(neighbour.Position, current.Position, heuristicType);
                bool newPath = (_openSet.Contains(neighbour) && tempGCost < neighbour.GCost) || !_openSet.Contains(neighbour);
                Main.NewText(neighbour.GCost);

                neighbour.GCost = tempGCost;

                if (!_openSet.Contains(neighbour))
                    _openSet.Add(neighbour);

                if (newPath)
                {
                    neighbour.HCost = GetHeuristic(neighbour.Position, end, heuristicType);
                    neighbour.Previous = current;
                }
            }
        }
        else
            Main.NewText("no solution :(");

        _path = [current];
        while (current.Previous != null)
        {
            _path.Add(current.Previous);
            current = current.Previous;
        }
    }

    public void Draw(Color? color = null, float scale = 16f)
    {
        Color drawColor = color ?? Color.White;
        Texture2D texture = TextureAssets.MagicPixel.Value;

        foreach (Node node in _path)
        {
            Main.spriteBatch.Draw(texture, node.Position.ToWorldCoordinates() - Main.screenPosition, new Rectangle(0, 0, 16, 16), drawColor,
                0, new Vector2(scale / 2), scale / 16, SpriteEffects.None, 0);
        }
    }
}