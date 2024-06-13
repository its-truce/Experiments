using Terraria;

namespace Experiments.Core;

public static class Utils
{
    public static Player Owner(this Projectile proj) => Main.player[proj.owner];
}