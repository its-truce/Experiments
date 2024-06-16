using System.Linq;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Experiments.Core.Boids;

public class Flock
{
    protected float AlignmentMult;
    protected float AvoidanceMult;
    protected bool AvoidTiles;
    protected Boid[] Boids;
    protected float CohesionMult;
    protected float SeparationMult;

    protected bool SpriteFacingUpwards;

    public Flock(int size, Vector2 basePosition, Vector2 spawnRange, Vector2 velocityRange, float maxForce = 0.2f, float maxSpeed = 2, int perceptionRadius = 100,
        float separationMult = 1, float alignmentMult = 1, float cohesionMult = 1, float avoidanceMult = 1.5f, Texture2D texture = null, bool spriteFacingUpwards = false,
        bool avoidTiles = true)
    {
        SeparationMult = separationMult;
        AlignmentMult = alignmentMult;
        CohesionMult = cohesionMult;
        AvoidanceMult = avoidanceMult;
        SpriteFacingUpwards = spriteFacingUpwards;
        AvoidTiles = avoidTiles;
        Boids = new Boid[size];

        for (int i = 0; i < Boids.Length; i++)
        {
            Vector2 spawnPos = basePosition + spawnRange.GetRandomVector();
            Boids[i] = new Boid(spawnPos, velocityRange.GetRandomVector(), maxForce, maxSpeed, perceptionRadius, texture);
        }
    }

    protected Flock()
    {
    }

    public Vector2 AveragePosition => Boids.Aggregate(Vector2.Zero, (current, boid) => current + boid.Position) / Boids.Length;

    public void Update()
    {
        foreach (Boid boid in Boids)
        {
            Vector2 separation = boid.CalculateForce(Boids, BehaviorType.Separation);
            Vector2 alignment = boid.CalculateForce(Boids, BehaviorType.Alignment);
            Vector2 cohesion = boid.CalculateForce(Boids, BehaviorType.Cohesion);
            Vector2 avoidance = AvoidTiles ? boid.GetAvoidanceForce(30) : Vector2.Zero;

            boid.Acceleration = separation * SeparationMult + alignment * AlignmentMult + cohesion * CohesionMult + avoidance * AvoidanceMult;
            boid.Update();
        }
    }

    public void Draw(Color? color = null)
    {
        foreach (Boid boid in Boids)
            boid.Draw(color, SpriteFacingUpwards);
    }
}