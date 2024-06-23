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
        _finder = new Pathfinding(Projectile.Center.ToTileCoordinates(), Projectile.Owner().Center.ToTileCoordinates());
    }

    public override void AI()
    {
        _finder.Update(HeuristicType.Euclidean);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        _finder.Draw();
        return false;
    }
}