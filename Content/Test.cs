using Experiments.Core;
using Experiments.Core.Boids;
using Experiments.Core.InverseKinematics;
using Experiments.Core.Pixelation;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Experiments.Content;

public class Test : ModProjectile
{
    private BezierCurve _curve;
    private Flock _flock;
    private Limb _limb;
    public override string Texture => Graphics.TextureDirectory + "EmptyTexture";

    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One;
        Projectile.timeLeft = 600;
    }

    public override void OnSpawn(IEntitySource source)
    {
        _limb = new Limb(3, Projectile.Center, true, 75, 5, 10, 2);
        _curve = new BezierCurve(Projectile.Center, Main.MouseWorld, Projectile.Owner().Center);
        _flock = new Flock(100, Projectile.Center, new Vector2(200, 200), new Vector2(2, 2));
    }

    public override void AI()
    {
        _limb.Follow(Main.MouseWorld);
        _limb.Update();

        _curve[0] = Projectile.Center;
        _curve[1] = Main.MouseWorld;
        _curve[2] = Projectile.Owner().Center;

        _flock.Update();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        PixelationSystem.Instance.AddRenderAction(DrawAction);
        return false;

        void DrawAction()
        {
            _limb.Draw(Main.DiscoColor);
            _curve.Draw(100, color: Main.DiscoColor);
            _flock.Draw(Main.DiscoColor);
        }
    }
}