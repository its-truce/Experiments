using System;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Experiments.Core.Boids;

public enum BehaviorType
{
    Separation,
    Alignment,
    Cohesion
}

//TODO: document
public class Boid(Vector2 position, Vector2 velocity, float maxForce, float maxSpeed, int perceptionRadius = 100, Texture2D texture = null)
{
    public Vector2 Acceleration;
    public Vector2 Position = position;
    public Vector2 Velocity = velocity;

    public void Update()
    {
        Position += Velocity;
        Velocity += Acceleration;
        Velocity.Limit(maxSpeed);

        Acceleration *= 0;
    }

    /// <summary>
    ///     Calculates the avoidance force for the boid based on the given range and air avoidance flag.
    /// </summary>
    /// <param name="range">The range within which tiles are checked for avoidance. Default is 15.</param>
    /// <param name="avoidAir">Flag indicating whether air tiles should be avoided. Default is false.</param>
    /// <returns>The calculated avoidance force vector.</returns>
    public Vector2 GetAvoidanceForce(int range = 15, bool avoidAir = false)
    {
        Vector2 force = Vector2.Zero;
        Point tilePos = Position.ToTileCoordinates();

        const int tileRange = 2;

        for (int i = -tileRange; i < tileRange; i++)
        for (int j = -tileRange; j < tileRange; j++)
        {
            if (WorldGen.InWorld(tilePos.X + i, tilePos.Y + j, 7))
            {
                Point currentTilePos = new(tilePos.X + i, tilePos.Y + j);
                Tile tile = Framing.GetTileSafely(currentTilePos);

                float distanceSquared = Vector2.DistanceSquared(Position, currentTilePos.ToWorldCoordinates());

                bool liquidCheck = avoidAir ? tile.LiquidAmount < 100 : tile.LiquidAmount > 0;
                if (distanceSquared < range * range && ((tile.HasTile && Main.tileSolid[tile.TileType]) || liquidCheck))
                    force += Position.DirectionFrom(currentTilePos.ToWorldCoordinates());
            }
        }

        if (force != Vector2.Zero)
        {
            force.SetMagnitude(maxSpeed);
            force -= Velocity;
            force.Limit(maxForce);
        }

        return force;
    }

    /// <summary>
    ///     Calculates the force vector for the boid based on the behavior type and the positions of neighboring boids.
    /// </summary>
    /// <param name="boids">The collection of boids to iterate through</param>
    /// <param name="behaviorType">Type of behavior to calculate force for</param>
    /// <returns></returns>
    public Vector2 CalculateForce(Boid[] boids, BehaviorType behaviorType)
    {
        Vector2 force = Vector2.Zero;
        int count = 0;

        FasterParallel.For(0, boids.Length, IterateOverBoids);

        if (count > 0)
        {
            force /= count;

            force.SetMagnitude(maxSpeed);
            force -= Velocity;
            force.Limit(maxForce);
        }

        return force;

        void IterateOverBoids(int start, int end, object context)
        {
            for (int i = start; i < end; i++)
            {
                Boid boid = boids[i];
                if (boid == this)
                    continue;

                float distanceSquared = Vector2.DistanceSquared(Position, boid.Position);

                if (distanceSquared < perceptionRadius * perceptionRadius)
                {
                    switch (behaviorType)
                    {
                        case BehaviorType.Separation:
                            Vector2 difference = Position - boid.Position;
                            difference /= distanceSquared;
                            force += difference;
                            break;

                        case BehaviorType.Alignment:
                            force += boid.Velocity;
                            break;

                        case BehaviorType.Cohesion:
                            force += boid.Position - Position;
                            break;
                    }

                    count++;
                }
            }
        }
    }

    public void Draw(Color? color = null, bool spriteFacingUpwards = false)
    {
        texture ??= ModContent.Request<Texture2D>($"{nameof(Experiments)}/Assets/Textures/Boid").Value;
        Color drawColor = color ?? Color.White;
        float offset = spriteFacingUpwards ? MathF.PI / 2 : 0;

        Main.EntitySpriteDraw(texture, Position - Main.screenPosition, null, drawColor, Velocity.ToRotation() + offset, texture.Size() / 2,
            1f, SpriteEffects.None);
    }
}