using Experiments.Core;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Experiments.Content;

public class Chain : ModProjectile
{
    private VerletChain _chain;
    public override string Texture => Graphics.TextureDirectory + "Circle";

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(1);
        Projectile.timeLeft = 5400;
        Projectile.tileCollide = false;
    }

    public override void OnSpawn(IEntitySource source)
    {
        _chain = new VerletChain(Projectile.Center);
    }

    public override void AI()
    {
        _chain.Update(subSteps: 1);

        if (Main.mouseLeft && Main.mouseLeftRelease)
            _chain.AddPoint(Main.MouseWorld);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        _chain.Draw();
        return false;
    }
}