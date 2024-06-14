using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Experiments.Core.Boids;

public class Flock
{
    private readonly float _alignmentMult;
    private readonly Boid[] _boids;
    private readonly float _cohesionMult;

    private readonly float _separationMult;

    private readonly bool _spriteFacingUpwards;

    public Flock(int size, Vector2 basePosition, Vector2 spawnRange, Vector2 velocityRange, int perceptionRadius = 100, float maxForce = 1, float maxSpeed = 4,
        float separationMult = 1, float alignmentMult = 1, float cohesionMult = 1, Texture2D texture = null, bool spriteFacingUpwards = false)
    {
        _separationMult = separationMult;
        _alignmentMult = alignmentMult;
        _cohesionMult = cohesionMult;
        _spriteFacingUpwards = spriteFacingUpwards;
        _boids = new Boid[size];

        for (int i = 0; i < _boids.Length; i++)
        {
            Vector2 spawnPos = basePosition + spawnRange.GetRandomVector();
            _boids[i] = new Boid(spawnPos, velocityRange.GetRandomVector(), Vector2.Zero, perceptionRadius, maxForce, maxSpeed, texture);
        }
    }

    public void Update()
    {
        foreach (Boid boid in _boids)
        {
            Vector2 separation = boid.CalculateForce(_boids, BehaviorType.Separation);
            Vector2 alignment = boid.CalculateForce(_boids, BehaviorType.Alignment);
            Vector2 cohesion = boid.CalculateForce(_boids, BehaviorType.Cohesion);

            boid.Acceleration = separation * _separationMult + alignment * _alignmentMult + cohesion * _cohesionMult;
            boid.Update();
        }
    }

    public void Draw(Color? color = null)
    {
        foreach (Boid boid in _boids)
            boid.Draw(color, _spriteFacingUpwards);
    }
}