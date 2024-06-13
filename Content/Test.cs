using Experiments.Core;
using Experiments.Core.Pixelation;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Experiments.Content;

public class Test : ModProjectile
{
    private BezierCurve _curve;

    private Limb _limb;
    public override string Texture => Graphics.EmptyTexture;

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.timeLeft = 600;
    }

    public override void OnSpawn(IEntitySource source)
    {
        _limb = new Limb(3, Projectile.Center, true, 75, 5, 10, 2);
    }

    public override void AI()
    {
        _limb.Follow(Main.MouseWorld);
        _limb.Update();

        _curve = new BezierCurve(Projectile.Center, Main.MouseWorld, Projectile.Owner().Center);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        void DrawAction(SpriteBatch spriteBatch)
        {
            _limb.Draw(Main.DiscoColor);
            _curve.Draw(100, color: Main.DiscoColor);
        }

        ModContent.GetInstance<PixelationSystem>().AddPixelationPass(DrawAction);

        return false;
    }
}