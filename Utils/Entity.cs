using Terraria;
using Terraria.ModLoader;

namespace Experiments.Utils;

public static class Entity
{
    public static Player Owner(this Projectile projectile) => Main.player[projectile.owner];
}