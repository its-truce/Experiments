using Experiments.Core.Boids;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Experiments.Content;

public class Fish : Flock
{
    public Fish(int size, Vector2 basePosition, Vector2 spawnRange, Vector2 velocityRange, Texture2D[] textures, float maxForce = 0.2f, float maxSpeed = 2,
        int perceptionRadius = 100,
        float separationMult = 1, float alignmentMult = 1, float cohesionMult = 1, float avoidanceMult = 1.5f, bool spriteFacingUpwards = false, bool avoidTiles = true)
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
            Boids[i] = new Boid(spawnPos, velocityRange.GetRandomVector(), maxForce, maxSpeed, perceptionRadius,
                textures[Main.rand.Next(0, textures.Length)]);
        }
    }
}

public class FishFlock : ModProjectile
{
    private const int SimulationDist = 2500;
    private Fish _fish;

    public override string Texture => Graphics.TextureDirectory + "Fish1";

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.timeLeft = 600;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Texture2D[] textures =
        [
            Graphics.GetTexture("Fish1"),
            Graphics.GetTexture("Fish2"),
            Graphics.GetTexture("Fish3"),
            Graphics.GetTexture("Fish4")
        ];

        _fish = new Fish(30, Projectile.Center, new Vector2(100, 100), new Vector2(1, 1), textures, separationMult: 1.5f);
    }

    public override void AI()
    {
        if (_fish.AveragePosition.Distance(Projectile.Owner().Center) > SimulationDist)
            return;

        _fish.Update();
        Projectile.Center = _fish.AveragePosition;

        if (Main.rand.NextBool(40))
        {
            SoundEngine.PlaySound(SoundID.SplashWeak, _fish.AveragePosition);

            for (int i = 0; i < 20; i++) Dust.NewDust(Projectile.Center, 100, 100, DustID.BreatheBubble, Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (_fish.AveragePosition.Distance(Projectile.Owner().Center) < SimulationDist)
            _fish.Draw();

        return false;
    }
}