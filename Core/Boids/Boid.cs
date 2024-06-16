using System;
using System.Collections.Generic;
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
    private Vector2 _velocity = velocity;
    public Vector2 Acceleration;
    public Vector2 Position = position;

    public void Update()
    {
        Position += _velocity;
        _velocity += Acceleration;
        _velocity.Limit(maxSpeed);

        Acceleration *= 0;
    }

    public Vector2 GetAvoidanceForce(int range = 15)
    {
        Vector2 force = Vector2.Zero;
        Point tilePos = Position.ToTileCoordinates();

        const int tileRange = 2;

        for (int i = -tileRange; i < tileRange; i++)
        for (int j = -tileRange; j < tileRange; j++)
            if (WorldGen.InWorld(tilePos.X + i, tilePos.Y + j, 7))
            {
                Point currentTilePos = new(tilePos.X + i, tilePos.Y + j);
                Tile tile = Framing.GetTileSafely(currentTilePos);

                float distanceSquared = Vector2.DistanceSquared(Position, currentTilePos.ToWorldCoordinates());

                if (distanceSquared < range * range && ((tile.HasTile && Main.tileSolid[tile.TileType]) || tile.LiquidAmount < 100))
                    force += Position.DirectionFrom(currentTilePos.ToWorldCoordinates());
            }

        if (force != Vector2.Zero)
        {
            force.SetMagnitude(maxSpeed);
            force -= _velocity;
            force.Limit(maxForce);
        }

        return force;
    }

    public Vector2 CalculateForce(IEnumerable<Boid> boids, BehaviorType behaviorType)
    {
        Vector2 force = Vector2.Zero;
        int count = 0;

        foreach (Boid boid in boids)
        {
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
                        force += boid._velocity;
                        break;

                    case BehaviorType.Cohesion:
                        force += boid.Position - Position;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(behaviorType), behaviorType, null);
                }

                count++;
            }
        }

        if (count > 0)
        {
            force /= count;

            force.SetMagnitude(maxSpeed);
            force -= _velocity;
            force.Limit(maxForce);
        }

        return force;
    }

    public void Draw(Color? color = null, bool spriteFacingUpwards = false)
    {
        texture ??= ModContent.Request<Texture2D>($"{nameof(Experiments)}/Assets/Textures/Boid").Value;
        Color drawColor = color ?? Color.White;
        float offset = spriteFacingUpwards ? MathF.PI / 2 : 0;

        Main.EntitySpriteDraw(texture, Position - Main.screenPosition, null, drawColor, _velocity.ToRotation() + offset, texture.Size() / 2,
            1f, SpriteEffects.None);
    }
}