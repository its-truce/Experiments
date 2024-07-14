using Experiments.Core;
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
        Projectile.Size = new Vector2(100, 100);
        Projectile.timeLeft = 5400;
        Projectile.tileCollide = false;
    }

    public override void OnSpawn(IEntitySource source)
    {
        _sand = new GrainSimulation(Projectile.Hitbox);
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