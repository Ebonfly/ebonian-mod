
using System;
using System.Collections.Generic;
using System.Threading;
using Terraria;

namespace EbonianMod.Content.Projectiles.Friendly.Generic;
public class Ball : ModProjectile
{
    public override string Texture => Helper.AssetPath + "Projectiles/Friendly/Generic/" + Name;
    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(20);
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.timeLeft = 400;
        Projectile.usesLocalNPCImmunity = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Player player = Main.player[Projectile.owner];
        Projectile.ai[0] = 0;
        Projectile.ai[2] = player.direction;
    }
    public override void OnKill(int timeLeft)
    {

    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (oldVelocity.Length() > 3)
        {
            Vector2 normal = Vector2.UnitY;
            if (Projectile.velocity.X != oldVelocity.X)
                normal = Vector2.UnitX;

            if (normal.X == 0)
            {
                Projectile.velocity.Y = oldVelocity.Y * -0.5f;
                Projectile.velocity.X = oldVelocity.X * 0.9f;
            }
            else
            {
                Projectile.velocity.Y = oldVelocity.Y * 0.9f;
                Projectile.velocity.X = oldVelocity.X * -0.6f;
            }
        }
        else Projectile.velocity *= 0.94f;
        if (Projectile.timeLeft > 20) Projectile.timeLeft -= 8;

        return false;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hitinfo, int damage)
    {
        Projectile.velocity *= 0.945f;
    }
    public override void AI()
    {
        Projectile.damage = (int)MathF.Pow(Projectile.velocity.Length(), 2) / 9;
        if (Projectile.damage > 90)
        {
            Projectile.CritChance = 100;
            Projectile.penetrate = -1;
        }
        else
        {
            Projectile.penetrate = 1;
            Projectile.CritChance = 0;
        }

        if (Projectile.ai[1] > 0) Projectile.ai[1]--;

        if (Projectile.timeLeft > 365)
        {
            Projectile.velocity.X = Main.player[Projectile.owner].velocity.X;
            Projectile.Center = new Vector2(Main.player[Projectile.owner].MountedCenter.X, Projectile.Center.Y);
        }
        float horizontalVelocityAbs = MathF.Abs(Projectile.velocity.X);
        if (horizontalVelocityAbs > 0.02f) Projectile.ai[2] = horizontalVelocityAbs / Projectile.velocity.X;
        Projectile.rotation += ToRadians(Projectile.velocity.Length() * 3 * Projectile.ai[2]);
        if (Projectile.ai[0] > -0.5f) Projectile.ai[0] -= 0.01f;
        Projectile.velocity.Y -= Projectile.ai[0];

        if (Projectile.timeLeft < 20)
            Projectile.Opacity *= 0.8f;
    }
}
