using Experiments.Core;
using Experiments.Utils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Experiments.Content;

public class Path : ModProjectile
{
    private Pathfinding _finder;
    public override string Texture => Graphics.TextureDirectory + "EmptyTexture";

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(1);
        Projectile.timeLeft = 6000;
        Projectile.tileCollide = false;
    }

    public override void OnSpawn(IEntitySource source)
    {
        _finder = new Pathfinding(Projectile.Center.ToTileCoordinates().ToPoint16(), Projectile.Owner().Center.ToTileCoordinates().ToPoint16());
    }

    public override void AI()
    {
        _finder.Update();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        _finder.Draw(Color.Red);
        return false;
    }
}