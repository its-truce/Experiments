using Experiments.Utils;
using Microsoft.Xna.Framework;

namespace Experiments.Core.Verlet;

public class VerletBody(Vector2 position, float radius = 20, Color? color = null)
{
    public Vector2 Position = position;
    private Vector2 _oldPos;
    public Vector2 Acceleration = Vector2.Zero;
    public readonly float Radius = radius;

    private Vector2 Velocity => Position - _oldPos;

    public void UpdatePosition(float dt)
    {
        _oldPos = Position;
        Position = Position + Velocity + Acceleration * (dt * dt);
        Acceleration = Vector2.Zero;
    }

    public void Register()
    {
        VerletSolver.Objects.Add(this);
    }

    public void Draw()
    {
        Graphics.DrawCircle(Position, Radius + 2, Color.White);
        Graphics.DrawCircle(Position, Radius, color);
    }
}