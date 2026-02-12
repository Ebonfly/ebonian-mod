using EbonianMod.Content.NPCs.Corruption;
using EbonianMod.Content.Projectiles.Cecitior;
using System;
using System.Collections.Generic;
using System.IO;

namespace EbonianMod.Content.Projectiles.Friendly.Crimson;

public class LatcherProjectile : ModProjectile
{
    bool IsAttached;
    Vector2 PositionOffset, NeckOrigin;
    float Speed;
    int TargetIndex = -1;
    public override string Texture => Helper.Empty;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 3000;
    }
    public override void SetDefaults()
    { 
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.tileCollide = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 200;
        Projectile.hide = true;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(IsAttached);
        writer.WriteVector2(PositionOffset);
        writer.Write(TargetIndex);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        IsAttached = reader.ReadBoolean();
        PositionOffset = reader.ReadVector2();
        TargetIndex = reader.ReadInt32();
    }
    public override bool? CanDamage() => !IsAttached && Projectile.ai[2] < 27;
    public override void OnHitNPC(NPC target, NPC.HitInfo hitinfo, int damage)
    {
        PositionOffset = Projectile.Center - target.Center;
        Projectile.velocity = Vector2.Zero;
        TargetIndex = target.whoAmI;
        IsAttached = true;
        Projectile.netUpdate = true; // TEST
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindProjectiles.Add(index);
    }
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, (Main.MouseWorld - player.Center).ToRotation(), 0.02f);
        NeckOrigin = new Vector2(Projectile.ai[0], Projectile.ai[1]);

        if (IsAttached)
        {
            NPC Target = Main.npc[TargetIndex];
            if (!Target.active || Target.life <= 0 || player.dead)
                Projectile.Kill();

            Vector2 step = Projectile.Center - player.Center;
            if (Helper.Raycast(player.Center, step, step.Length()).Success || (player.whoAmI == Main.myPlayer && Main.mouseRight))
            {
                player.velocity *= 0.7f;
                IsAttached = false;
                Projectile.tileCollide = false;
                Projectile.timeLeft = 189;
                return;
            }
            Projectile.timeLeft = 190;

            Speed += 1.5f;
            player.velocity = Helper.FromAToB(player.Center, Projectile.Center, true) * Speed;
            player.SyncPlayerControls();
            Projectile.Center = Target.Center + PositionOffset;
            float velocityMagnitude = player.velocity.Length();
            if (Vector2.Distance(player.Center, Projectile.Center) < Max(velocityMagnitude, 16))
            {
                SoundEngine.PlaySound(Sounds.chomp2.WithPitchOffset(Main.rand.NextFloat(0.2f, 0.4f)), Projectile.Center);
                SoundEngine.PlaySound(Sounds.chomp1.WithPitchOffset(Main.rand.NextFloat(-0.6f, -0.2f)), Projectile.Center);
                player.velocity *= -0.2f;
                Target.SimpleStrikeNPC((int)MathF.Pow(velocityMagnitude * 0.31f, 2), -(int)player.velocity.X, false, velocityMagnitude / 3);
                for (int i = 0; i < 15; i++) Dust.NewDustPerfect(Target.Center, DustID.Blood, (Projectile.rotation + Main.rand.NextFloat(-PiOver4, PiOver4)).ToRotationVector2() * Main.rand.NextFloat(2, 6) * player.direction, Scale: 1.5f);
                Projectile.Kill();
            }
        }
        else if (Projectile.timeLeft < 190)
        {
            Projectile.ai[2] += 3;
            Projectile.Center += Helper.FromAToB(Projectile.Center, NeckOrigin) * Projectile.ai[2];
            if (Projectile.ai[2] >= 30) Projectile.tileCollide = false;
            if (Vector2.Distance(Projectile.Center, NeckOrigin) < Projectile.ai[2]) Projectile.Kill();
        }
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.tileCollide = false;
        Projectile.timeLeft = 189;
        Projectile.ai[2] = 27;
        return false;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (NeckOrigin == Vector2.Zero) return false;

        Player player = Main.player[Projectile.owner];
        Vector2 center = Projectile.Center;
        Vector2 distanceToProjectile = NeckOrigin - Projectile.Center;
        float projectileRotation = distanceToProjectile.ToRotation() - 1.57f;
        float distance = distanceToProjectile.Length();
        while (distance > 20 && !float.IsNaN(distance))
        {
            distanceToProjectile.Normalize();
            distanceToProjectile *= 20;
            center += distanceToProjectile;
            distanceToProjectile = NeckOrigin - center;
            distance = distanceToProjectile.Length();

            Main.spriteBatch.Draw(Assets.Projectiles.Friendly.Crimson.LatcherP_Chain.Value, center - Main.screenPosition,null, new Color(Lighting.GetSubLight(center)), projectileRotation, Assets.Projectiles.Friendly.Crimson.LatcherP_Chain.Value.Size() / 2, 1f, SpriteEffects.None, 0);
        }
        Main.spriteBatch.Draw(Assets.Projectiles.Friendly.Crimson.LatcherP_Chain.Value, Projectile.Center - Main.screenPosition, null, new Color(Lighting.GetSubLight(Projectile.Center)), projectileRotation, Assets.Projectiles.Friendly.Crimson.LatcherP_Chain.Value.Size() / 2, 1f, SpriteEffects.None, 0);
        return false;
    }
}
public class LatcherProjectileCecitior : ModProjectile
{
    public override string Texture => Helper.AssetPath+"Projectiles/Friendly/Crimson/LatcherP";
    public override void SetDefaults()
    {
        Projectile.width = 12;
        Projectile.height = 20;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.tileCollide = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.hide = true;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 300;
    }
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 3000;
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.velocity = Vector2.Zero;
        if (Projectile.localAI[0] == 0)
        {
            Projectile.timeLeft = 200;
            Projectile.ai[1] = 1;
        }
        return false;
    }
    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        Projectile.velocity = Vector2.Zero;
        if (Projectile.localAI[0] == 0 && Projectile.ai[1] == 0)
        {
            Projectile.localAI[0] = target.whoAmI;
            Projectile.timeLeft = 100;
            Projectile.ai[1] = 2;
        }
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(Projectile.localAI[0]);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Projectile.localAI[0] = reader.ReadSingle();
    }

    public override void AI()
    {
        if (Helper.Raycast(Projectile.Center, Vector2.UnitY, 30, true).RayLength < 20)
        {
            Projectile.velocity = Vector2.Zero;
            if (Projectile.localAI[0] == 0)
            {
                Projectile.timeLeft = 200;
                Projectile.ai[1] = 1;
            }
        }
        if (Projectile.ai[1] == 0)
            Projectile.rotation = Projectile.velocity.ToRotation();
        if (!NPC.AnyNPCs(NPCType<NPCs.Cecitior.Cecitior>()))
            Projectile.Kill();
        NPC player = Main.npc[(int)Projectile.ai[0]];
        if (player.ai[0] != 8)
            Projectile.Kill();
        if (Projectile.ai[1] == 1)
        {
            player.velocity = Helper.FromAToB(player.Center, Projectile.Center) * 25;
            if (player.Center.Distance(Projectile.Center) < 50)
            {
                Projectile.ai[2] = 1;
                MPUtils.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, 0), ProjectileType<FatSmash>(), 0, 0, 0, 0);

                for (int i = -6; i < 6; i++)
                {
                    if (i == 0) continue;
                    MPUtils.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(i * 3, Lerp(-3, -5, MathF.Abs(i) / 6)), ProjectileType<CecitiorTeeth>(), 20, 0, 0, 0);
                }
                player.velocity = Projectile.rotation.ToRotationVector2().RotatedByRandom(PiOver4) * -10f;
                Projectile.Kill();
                SoundEngine.PlaySound(Sounds.cecitiorSlam, Projectile.Center);
                Projectile.netUpdate = true;
            }
        }
        else if (Projectile.ai[1] == 2)
        {
            Player playerr = Main.player[(int)Projectile.localAI[0]];
            playerr.velocity = Helper.FromAToB(playerr.Center, player.Center, false) / 10;
            Projectile.velocity = Helper.FromAToB(Projectile.Center, player.Center) * 20;
        }
        else
        {
            if (Projectile.velocity.Length() < 24)
                Projectile.velocity *= 1.15f;
            if (Projectile.timeLeft < 100)
            {
                Projectile.Center = Vector2.Lerp(Projectile.Center, player.Center, 0.4f);

                if (player.Center.Distance(Projectile.Center) < 50)
                    Projectile.Kill();
                Projectile.netUpdate = true;
            }
        }
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCs.Add(index);
    }
    public override bool PreDraw(ref Color lightColor)
    {

        NPC player = Main.npc[(int)Projectile.ai[0]];
        Vector2 neckOrigin = Projectile.Center;
        Vector2 center = player.Center;
        Vector2 distToProj = neckOrigin - player.Center;
        float projRotation = distToProj.ToRotation() - 1.57f;
        float distance = distToProj.Length();
        while (distance > 20 && !float.IsNaN(distance))
        {
            distToProj.Normalize();
            distToProj *= 20;
            center += distToProj;
            distToProj = neckOrigin - center;
            distance = distToProj.Length();

            //Draw chain
            Main.spriteBatch.Draw(Assets.Projectiles.Friendly.Crimson.LatcherP_Chain.Value, center - Main.screenPosition,
                null, Lighting.GetColor((int)center.X / 16, (int)center.Y / 16), projRotation,
                Assets.Projectiles.Friendly.Crimson.LatcherP_Chain.Value.Size() / 2, 1f, SpriteEffects.None, 0);
        }
        return true;
    }
}
