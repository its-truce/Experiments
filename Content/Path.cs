using Experiments.Core.Pathfinding;
using Experiments.Core.Pixelation;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Experiments.Content;

public class Path : ModProjectile
{
    private Pathfinder _finder;
    public override string Texture => Graphics.TextureDirectory + "EmptyTexture";

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(1);
        Projectile.timeLeft = 6000;
        Projectile.tileCollide = false;
    }

    public override void OnSpawn(IEntitySource source)
    {
        _finder = new Pathfinder(Projectile.Center.ToTileCoordinates().ToPoint16(), Projectile.Owner().Center.ToTileCoordinates().ToPoint16());
    }

    public override void AI()
    {
        _finder.SetTarget(Projectile.Owner().MountedCenter.ToTileCoordinates().ToPoint16());
        _finder.Update();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        PixelationSystem.Instance.AddRenderAction(_ => { _finder.Draw(); });
        return false;
    }
}