using System.Collections.Generic;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Terraria;

namespace Experiments.Core;

public class VerletPoint(Vector2 position, bool locked = false)
{
    public readonly bool Locked = locked;
    public Vector2 Position = position;
    public Vector2 PrevPosition = position;
}

public struct VerletStick(VerletPoint pointA, VerletPoint pointB)
{
    public readonly VerletPoint PointA = pointA;
    public readonly VerletPoint PointB = pointB;
    public readonly float Length = Vector2.Distance(pointA.Position, pointB.Position);
}

public class VerletChain
{
    private readonly List<VerletPoint> _points = [];
    private readonly List<VerletStick> _sticks = [];

    public VerletChain(params Vector2[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            Vector2 position = positions[i];
            _points.Add(new VerletPoint(position, i == 0));
        }

        if (_points.Count == 1)
            _points.Add(new VerletPoint(_points[0].Position));

        for (int i = 0; i < _points.Count - 1; i++) _sticks.Add(new VerletStick(_points[i], _points[i + 1]));
    }

    public void Update(float gravity = 9.8f, float deltaTime = 1, int subSteps = 8)
    {
        foreach (VerletPoint point in _points)
        {
            if (point.Locked) continue;

            Vector2 posBeforeUpdate = point.Position;
            point.Position += point.Position - point.PrevPosition;
            point.Position += Vector2.UnitY * gravity * deltaTime * deltaTime;
            point.PrevPosition = posBeforeUpdate;
        }

        for (int i = 0; i < subSteps; i++)
        {
            foreach (VerletStick stick in _sticks)
            {
                Vector2 center = (stick.PointA.Position + stick.PointB.Position) / 2;
                Vector2 direction = stick.PointA.Position.DirectionTo(stick.PointB.Position);

                switch (stick.PointA.Locked)
                {
                    case false when !stick.PointB.Locked:
                        stick.PointA.Position = center + direction * stick.Length / 2;
                        stick.PointB.Position = center - direction * stick.Length / 2;
                        break;

                    case true when !stick.PointB.Locked:
                        stick.PointB.Position = stick.PointA.Position + direction * stick.Length;
                        break;

                    case false when stick.PointB.Locked:
                        stick.PointA.Position = stick.PointB.Position - direction * stick.Length;
                        break;
                }
            }
        }
    }

    public void Draw()
    {
        foreach (VerletPoint point in _points)
            Graphics.DrawCircle(point.Position, 10, point.Locked ? Color.Red : Color.White);

        foreach (VerletStick stick in _sticks)
            Graphics.DrawLine(stick.PointA.Position, stick.PointB.Position, thickness: 5);
    }

    public void AddPoint(Vector2 position)
    {
        _points.Add(new VerletPoint(position));
        _sticks.Add(new VerletStick(_points[^2], _points[^1]));
    }
}