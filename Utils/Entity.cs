using Terraria;

namespace Experiments.Utils;

public static class Entity
{
    public static Player Owner(this Projectile projectile) => Main.player[projectile.owner];

    public static Player Target(this NPC npc) => npc.HasPlayerTarget ? Main.player[npc.target] : null;
}