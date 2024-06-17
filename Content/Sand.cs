﻿using Experiments.Core;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Experiments.Content;

public class Sand : ModProjectile
{
    private GrainSimulation _sand;
    public override string Texture => Graphics.TextureDirectory + "EmptyTexture";

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.timeLeft = 600;
    }

    public override void OnSpawn(IEntitySource source)
    {
        _sand = new GrainSimulation(new Rectangle((int)Projectile.Center.X - 200, (int)Projectile.Center.Y - 200, 400, 400), 7);
    }

    public override void AI()
    {
        _sand.Update();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        _sand.Draw(new Color(212, 192, 100));
        return false;
    }
}