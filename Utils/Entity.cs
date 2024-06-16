using Terraria;

namespace Experiments.Utils;

public static class Entity
{
    public static Player Owner(this Projectile projectile)
    {
        return Main.player[projectile.owner];
    }
}