﻿using System;
using System.Collections.Generic;
using System.Linq;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;

namespace Experiments.Core.Pathfinding;

public enum HeuristicType
{
    Manhattan,
    Euclidean,
    Chebyshev,
    Octile
}

/// <summary>
///     Implements the A* pathfinding algorithm.
///     For more information, see: <a href="https://en.wikipedia.org/wiki/A*_search_algorithm">this page</a>.
/// </summary>
public class Pathfinder
{
    private readonly HashSet<Node> _closedSet;
    private readonly bool _ignorePlatforms;
    private readonly Dictionary<Point16, Node> _nodeDictionary;

    private readonly PriorityQueue<Node, float> _openSet;
    private Node _current;
    private bool _noSolution;

    private readonly Point16 _start;
    private readonly Point16 _target;

    public bool Done;

    /// <summary>
    ///     The current path of the pathfinder. Null if no solution was found.
    /// </summary>
    public List<Node> Path;

    public Pathfinder(Point16 start, Point16 target, bool ignorePlatforms = false)
    {
        _start = start;
        _target = target;
        _ignorePlatforms = ignorePlatforms;

        _closedSet = [];
        Path = [];
        _openSet = new PriorityQueue<Node, float>();

        Node startNode = new(start, 0, GetHeuristic(start, target), ignorePlatforms);
        _openSet.Enqueue(startNode, startNode.FCost);

        _nodeDictionary = new Dictionary<Point16, Node> { [start] = startNode };
        _current = startNode;
    }

    private static float GetHeuristic(Point16 start, Point16 end, HeuristicType heuristicType = HeuristicType.Octile)
    {
        float heuristic = 0;

        switch (heuristicType)
        {
            case HeuristicType.Euclidean:
                heuristic = start.Distance(end);
                break;

            case HeuristicType.Manhattan:
                heuristic = Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y);
                break;

            case HeuristicType.Chebyshev:
                heuristic = Math.Max(Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
                break;

            case HeuristicType.Octile:
                int dx = Math.Abs(start.X - end.X);
                int dy = Math.Abs(start.Y - end.Y);
                const float sqrt2 = 1.4142135623730951f;

                heuristic = Math.Max(dx, dy) + (sqrt2 - 1) * Math.Min(dx, dy);
                break;
        }

        return heuristic;
    }

    public void Update(int subSteps = 2, HeuristicType heuristicType = HeuristicType.Octile)
    {
        if (_noSolution)
        {
            Path = null;
            return;
        }

        if (Done) return;

        for (int i = 0; i < subSteps; i++)
        {
            if (_openSet.Count > 0)
            {
                _current = _openSet.Dequeue();
                _closedSet.Add(_current);

                if (_current.Position == _target)
                {
                    if (_nodeDictionary.TryGetValue(_target, out Node node))
                        _current = node;

                    Done = true;
                    break;
                }

                _current.AddNeighbors(_nodeDictionary);

                List<Node> neighbors = _current.Neighbors;

                foreach (Node neighbor in neighbors)
                {
                    if (_closedSet.Contains(neighbor) || neighbor.IsWall) continue;

                    float gScore = _current.GCost + GetHeuristic(neighbor.Position, _current.Position, heuristicType);

                    if (gScore < neighbor.GCost)
                    {
                        neighbor.GCost = gScore;
                        neighbor.HCost = GetHeuristic(neighbor.Position, _target, heuristicType);
                        neighbor.FCost = neighbor.GCost + neighbor.HCost;
                        neighbor.Previous = _current;

                        if (_openSet.UnorderedItems.All(t => t.Element != neighbor))
                            _openSet.Enqueue(neighbor, neighbor.FCost);
                    }
                }
            }
            else
            {
                _noSolution = true;
                break;
            }
        }

        Path = [_current];
        while (_current.Previous != null)
        {
            Path.Add(_current.Previous);
            _current = _current.Previous;
        }

        Path.Reverse();
    }

    public Pathfinder SetTarget(Point16 target)
    {
        if (_target.Distance(target) > 0)
        {
            Done = false;
            return new Pathfinder(_start, target, _ignorePlatforms);
        }

        return this;
    }

    public Pathfinder SetStart(Point16 start)
    {
        if (_start.Distance(start) > 0)
        {
            Done = false;
            return new Pathfinder(start, _target, _ignorePlatforms);
        }

        return this;
    }

    /// <summary>
    ///     Draws the current path of the pathfinder.
    ///     Blue means that the pathfinder is currently searching, green means that the path has been found, and red means that
    ///     there is no solution.
    /// </summary>
    /// <param name="scale">The size of the lines drawn to indicate the path.</param>
    public void Draw(float scale = 8f)
    {
        for (int i = 0; i < Path.Count - 1; i++)
        {
            Color defaultColor = _noSolution ? Color.Red : Color.Blue;
            Graphics.DrawLine(Path[i].Position.ToWorldCoordinates(), Path[i + 1].Position.ToWorldCoordinates(), color: Done ? Color.Green : defaultColor,
                thickness: scale);
        }
    }
}