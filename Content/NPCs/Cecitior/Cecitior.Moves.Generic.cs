using EbonianMod.Core.Systems.Verlets;
using EbonianMod.Content.Projectiles.Cecitior;
using EbonianMod.Content.Projectiles.VFXProjectiles;
using EbonianMod.Core.Systems.Cinematic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbonianMod.Content.NPCs.Cecitior;
public partial class Cecitior : ModNPC
{
    public void DoIdle()
    {
        oldHP = NPC.life;
        rotation = 0;

        if (AITimer < 70 && NPC.Distance(player.Center) > 200)
            NPC.velocity = Vector2.Lerp(NPC.velocity, Helper.FromAToB(NPC.Center, player.Center - new Vector2(0, 100) + Helper.FromAToB(player.Center, NPC.Center) * 100) * 15, 0.15f);
        else NPC.velocity *= 0.97f;
        if (AITimer >= 50 * (NPC.life / (float)NPC.lifeMax) + 10)
        {
            AIState = syncedRand.Next(2, phase2 ? 15 : 9);
            NPC.velocity = Vector2.Zero;
            AITimer = 0;
            AITimer2 = 0;
            AITimer3 = 0;
            NPC.netUpdate = true;
        }
    }


    public void DoIntro()
    {
        if (AITimer == 1)
        {
            CameraSystem.ChangeCameraPos(NPC.Center, 260, new ZoomInfo(2, 1.1f, InOutElastic, InOutCirc), 1.5f, InOutQuart);
            for (int i = 0; i < 200; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, Main.rand.NextFloat(-6, 6), Main.rand.NextFloat(-6, 6), Scale: 3);
            }
            NPC.netUpdate = true;
        }
        if (AITimer == 55)
        {
            if (!Main.dedServ)
                openSound = SoundEngine.PlaySound(Sounds.cecitiorOpen, NPC.Center);
            for (int i = 0; i < 6; i++)
            {
                float angle = Helper.CircleDividedEqually(i, 6) + ToRadians(15);
                MPUtils.NewNPC(NPC.Center + new Vector2(1).RotatedBy(angle), NPCType<CecitiorEye>(), false, NPC.whoAmI, i);
            }
            NPC.netUpdate = true;
        }
        if (AITimer == 80)
        {
            CameraSystem.ScreenShakeAmount = 10f;
            SoundEngine.PlaySound(Sounds.terrortomaFlesh, NPC.Center);
            NPC.netUpdate = true;
        }
        if (AITimer >= 60 && AITimer <= 160)
        {
            open = true;
            if (AITimer >= 80 && AITimer % 10 == 0)
                MPUtils.NewProjectile(null, NPC.Center, Vector2.Zero, ProjectileType<BloodShockwave2>(), 0, 0);
            if (openOffset.X < 30)
            {
                openOffset.X += 2.5f;
            }
            if (AITimer > 80)
            {
                openRotation = ToRadians(MathF.Sin(AITimer * 1.5f) * 15);
                NPC.rotation = ToRadians(MathF.Sin(-AITimer * 1.5f) * 15);
                rotation = ToRadians(MathF.Sin(-AITimer * 1.5f) * 15);
            }
        }
        if (AITimer >= 160)
        {
            open = false;
            openOffset = Vector2.Lerp(openOffset, Vector2.Zero, 0.2f);
            rotation = Lerp(rotation, 0, 0.2f);
            openRotation = Lerp(openRotation, 0, 0.2f);
        }
        if (AITimer >= 170)
        {
            open = false;
            openOffset = Vector2.Zero;
            rotation = 0;
            openRotation = 0;
        }
        if (AITimer >= 200)
        {
            AITimer = 0;
            AIState = Idle;
            NPC.netUpdate = true;
        }
    }


    public void DoPhaseTransition()
    {
        NPC.dontTakeDamage = true;
        if (AITimer == 1)
        {
            for (int i = 0; i < 3; i++)
                claw[i].position = NPC.Center;
            savedPos = NPC.Center;
            NPC.velocity = Vector2.Zero;
            CameraSystem.ChangeCameraPos(NPC.Center, 110, null, 1.5f, InOutSine);
            NPC.netUpdate = true;
        }
        if (AITimer == 63)
            CameraSystem.ChangeZoom(50, new ZoomInfo(2, 1f, InOutSine, InOutSine, true));
        if (AITimer < 60)
        {
            NPC.dontTakeDamage = true;
            NPC.Center = savedPos + Main.rand.NextVector2Circular(AITimer * 0.25f, AITimer * 0.25f);
        }
        if (AITimer < 53 && AITimer > 40)
        {
            CameraSystem.ScreenShakeAmount = (AITimer - 40) * 0.1f;
        }
        if (AITimer >= 63 && claw[0].verlet is null && !Main.dedServ)
        {
            for (int i = 0; i < 3; i++)
            {
                claw[i].verlet = new Verlet(NPC.Center, 12, 22, 0.15f, stiffness: 50);
            }
            SoundEngine.PlaySound(Sounds.fleshHit with { Pitch = -0.3f, PitchVariance = 0.2f }, player.Center);

            for (int k = 0; k < 25; k++)
            {
                Dust.NewDustPerfect(NPC.Center, DustID.Blood, Vector2.UnitX.RotatedByRandom(1) * Main.rand.NextFloat(7, 15), 0, default, Main.rand.NextFloat(1, 2));
            }
            for (int k = 0; k < 25; k++)
            {
                Dust.NewDustPerfect(NPC.Center, DustID.Blood, -Vector2.UnitX.RotatedBy(0.25f).RotatedByRandom(0.5f) * Main.rand.NextFloat(7, 15), 0, default, Main.rand.NextFloat(1, 2));
            }
        }
        if (AITimer > 63)
        {
            claw[0].position = Vector2.Lerp(claw[0].position, NPC.Center + openOffset + new Vector2(150, -65).RotatedBy(MathF.Sin(NPC.ai[2] * 0.01f) * 0.4f), 0.2f);
            claw[1].position = Vector2.Lerp(claw[1].position, NPC.Center + openOffset + new Vector2(165, 45).RotatedBy(MathF.Sin(NPC.ai[2] * 0.01f) * 0.4f), 0.2f);
            claw[2].position = Vector2.Lerp(claw[2].position, NPC.Center - openOffset + new Vector2(-110, 55).RotatedBy(MathF.Sin(NPC.ai[2] * 0.01f) * 0.4f), 0.2f);
        }
        if (AITimer >= 95)
        {
            ResetState();
            if (NPC.life <= 1)
            {
                AIState = PrePreDeath;
            }
            else
            {
                NPC.dontTakeDamage = false;
                AIState = Phase2ClawGrab;
            }
            NPC.netUpdate = true;
        }
    }


    public void DoDeathPart1()
    {
        if (phase2)
        {
            claw[0].position = Vector2.Lerp(claw[0].position, NPC.Center + openOffset, 0.025f);
            claw[1].position = Vector2.Lerp(claw[1].position, NPC.Center + openOffset, 0.025f);
            claw[2].position = Vector2.Lerp(claw[2].position, NPC.Center - openOffset, 0.025f);
        }
        if (rotation == 0 || AITimer > 0)
            AITimer++;
        if (AITimer == 1)
            open = false;
        if (AITimer == 0 || AITimer > 150)
        {
            rotation = Lerp(rotation, 0, 0.3f);
            openRotation = Lerp(openRotation, 0, 0.3f);
            openOffset = Vector2.Lerp(openOffset, Vector2.Zero, 0.3f);
        }
        if (AITimer >= 100 && AITimer < 115)
        {
            openOffset.X++;
            openRotation += ToRadians(2);
            rotation -= ToRadians(2);
        }
        if (AITimer == 1)
        {
            SoundEngine.PlaySound(Sounds.fleshHit, NPC.Center);
            SoundEngine.PlaySound(Sounds.terrortomaFlesh, NPC.Center);
        }
        if (AITimer > 115 && AITimer < 160)
        {
            if (AITimer % 5 == 0)
            {
                MPUtils.NewProjectile(NPC.GetSource_Death(), NPC.Center, new Vector2(Main.rand.NextFloat(-1, 1), -1) * Main.rand.NextFloat(1, 15), 814, 10, 0);
            }
            if (AITimer % (AITimer < 130 ? 2 : 1) == 0)
                for (int i = 0; i < 3; i++)
                {
                    Dust.NewDustPerfect(NPC.Center - openOffset, DustID.Blood, new Vector2(Main.rand.NextFloat(0, 1), -1) * Main.rand.NextFloat(1, 15), Scale: Main.rand.NextFloat(1, 2)).noGravity = false;
                    Dust.NewDustPerfect(NPC.Center + openOffset, DustID.Blood, new Vector2(Main.rand.NextFloat(-1, 0), -1) * Main.rand.NextFloat(1, 15), Scale: Main.rand.NextFloat(1, 2)).noGravity = false;
                }
        }
        NPC.Center = savedPos;
        if (AITimer < 100 && AITimer > 0)
            NPC.Center += Main.rand.NextVector2Unit() * Main.rand.NextFloat(1, (20 - AITimer * 0.2f).SafeDivision());
        if (AITimer > 170)
        {
            AIState = PreDeath;
            AITimer = 0;
            savedPos = NPC.Center;
            NPC.netUpdate = true;
        }
    }


    public void DoDeathPart2()
    {
        if (AITimer % 16 - (AITimer3 * 2) == 0 && AITimer3 > 1)
        {
            for (int i = 0; i < 2; i++)
            {
                MPUtils.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Main.rand.NextVector2Circular(20, 20), ProjectileType<CecitiorBombThing>(), 30, 0);
                if (Main.rand.NextBool())
                    MPUtils.NewProjectile(NPC.GetSource_Death(), NPC.Center + (openOffset + new Vector2(Main.rand.NextFloat(30, 75), Main.rand.NextFloat(-45, 45))) * Main.rand.NextFloatDirection(), new Vector2(Main.rand.NextFloat(-1, 1), -1) * Main.rand.NextFloat(1, 10), ProjectileType<Gibs>(), 0, 0);
                else
                    MPUtils.NewProjectile(NPC.GetSource_Death(), NPC.Center + (openOffset + new Vector2(Main.rand.NextFloat(30, 75), Main.rand.NextFloat(-45, 45))) * Main.rand.NextFloatDirection(), new Vector2(Main.rand.NextFloat(-1, 1), -1) * Main.rand.NextFloat(1, 10), 814, 10, 0);
                MPUtils.NewProjectile(null, NPC.Center + (openOffset + new Vector2(Main.rand.NextFloat(30, 75), Main.rand.NextFloat(-45, 45))) * Main.rand.NextFloatDirection(), Vector2.Zero, ProjectileType<BloodExplosionWSprite>(), 0, 0);
            }
            MPUtils.NewProjectile(null, NPC.Center + (openOffset + new Vector2(Main.rand.NextFloat(30, 75), Main.rand.NextFloat(-45, 45))) * Main.rand.NextFloatDirection(), Vector2.Zero, ProjectileType<BloodShockwave2>(), 0, 0);
        }
        open = true;
        openOffset = Vector2.Lerp(openOffset, new Vector2(100 + MathF.Sin(NPC.ai[2] * 0.01f * Main.rand.NextFloat(2, 9)) * Main.rand.NextFloat(50, 80), MathF.Sin(NPC.ai[2] * 0.01f * Main.rand.NextFloat(2, 5)) * Main.rand.NextFloat(20, 40)), 0.15f);
        if ((int)AITimer2 == 0 || AITimer == 1)
            AITimer2 = syncedRand.NextFloat(0.01f, Pi * 2);
        if (AITimer < 20)
        {
            NPC.velocity *= 0.9f;
            claw[0].position = Vector2.Lerp(claw[0].position, NPC.Center + new Vector2(45, -185).RotatedBy(AITimer2), 0.3f);
            claw[1].position = Vector2.Lerp(claw[1].position, NPC.Center + new Vector2(0, -200).RotatedBy(AITimer2), 0.3f);
            claw[2].position = Vector2.Lerp(claw[2].position, NPC.Center + new Vector2(-45, -185).RotatedBy(AITimer2), 0.3f);
        }
        if (AITimer == 20)
        {
            for (int i = 0; i < claw.Length; i++)
            {
                MPUtils.NewProjectile(NPC.GetSource_FromAI(), claw[i].position + Vector2.UnitY.RotatedBy(AITimer2) * 110, -Vector2.UnitY.RotatedBy(AITimer2), ProjectileType<CecitiorClawSlash>(), 30, 0, ai2: 1);
            }
        }
        if (AITimer >= 20)
        {
            NPC.velocity = Vector2.Lerp(NPC.velocity, Helper.FromAToB(NPC.Center, player.Center) * 10, 0.05f);
            for (int i = 0; i < claw.Length; i++)
            {
                claw[0].position = Vector2.Lerp(claw[0].position, NPC.Center - new Vector2(45, -170).RotatedBy(AITimer2), 0.05f);
                claw[1].position = Vector2.Lerp(claw[1].position, NPC.Center - new Vector2(0, -200).RotatedBy(AITimer2), 0.05f);
                claw[2].position = Vector2.Lerp(claw[2].position, NPC.Center - new Vector2(-45, -170).RotatedBy(AITimer2), 0.05f);
            }
        }
        if (AITimer >= 26)
        {
            if (AITimer3 < 6)
            {
                if (verlet[0] is not null && AITimer3 < 4)
                {
                    verlet[(int)AITimer3].segments[10].cut = true;
                    verlet[(int)AITimer3].stiffness = 1;

                    verlet[(int)AITimer3 + 5].segments[10].cut = true;
                    verlet[(int)AITimer3 + 5].stiffness = 1;
                }
                AITimer = 0;
                AITimer3++;
                savedPos = NPC.Center;
            }
            else
            {
                if (verlet[0] is not null)
                {
                    verlet[9].segments[10].cut = true;
                    verlet[9].stiffness = 1;
                }
                AITimer3 = 0;
                AITimer2 = 0;
                AIState = Death;
                AITimer = 0;
            }
            NPC.netUpdate = true;
        }
    }


    public void DoDeathPart3()
    {
        SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
        for (int i = 0; i < 7; i++)
        {
            MPUtils.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Main.rand.NextVector2Circular(20, 20), ProjectileType<CecitiorBombThing>(), 30, 0);
            MPUtils.NewProjectile(NPC.GetSource_Death(), NPC.Center + (openOffset + new Vector2(Main.rand.NextFloat(30, 75), Main.rand.NextFloat(-45, 45))) * Main.rand.NextFloatDirection(), new Vector2(Main.rand.NextFloat(-1, 1), -1) * Main.rand.NextFloat(1, 15), 814, 10, 0);
            MPUtils.NewProjectile(NPC.GetSource_Death(), NPC.Center + (openOffset + new Vector2(Main.rand.NextFloat(30, 75), Main.rand.NextFloat(-45, 45))) * Main.rand.NextFloatDirection(), new Vector2(Main.rand.NextFloat(-1, 1), -1) * Main.rand.NextFloat(1, 15), ProjectileType<Gibs>(), 10, 0);
            MPUtils.NewProjectile(null, NPC.Center + (openOffset + new Vector2(Main.rand.NextFloat(20, 45), Main.rand.NextFloat(-45, 45))) * Main.rand.NextFloatDirection(), Vector2.Zero, ProjectileType<BloodExplosionWSprite>(), 0, 0);
            MPUtils.NewProjectile(null, NPC.Center + (openOffset + new Vector2(Main.rand.NextFloat(20, 45), Main.rand.NextFloat(-45, 45))) * Main.rand.NextFloatDirection(), Vector2.Zero, ProjectileType<BloodShockwave2>(), 0, 0);
        }
        CameraSystem.ScreenShakeAmount = 10f;
        if (!Main.dedServ)
        {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position + openOffset, Vector2.UnitX * 5, Find<ModGore>("EbonianMod/Cecitior1").Type, NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, -Vector2.UnitX * 5, Find<ModGore>("EbonianMod/Cecitior2").Type, NPC.scale);
        }
        for (int i = 0; i < 3; i++)
        {
            if (Main.dedServ)
                MPUtils.NewProjectile(NPC.GetSource_Death(), claw[i].position, Main.rand.NextVector2CircularEdge(15, 15) * Main.rand.NextFloat(0.75f, 1f), ProjectileType<ClawGore>(), 0, 0, -1, claw[i].position.X, claw[i].position.Y);
            else
                MPUtils.NewProjectile(NPC.GetSource_Death(), claw[i].position, Main.rand.NextVector2CircularEdge(15, 15) * Main.rand.NextFloat(0.75f, 1f), ProjectileType<ClawGore>(), 0, 0, -1, claw[i].verlet.firstP.position.X, claw[i].verlet.firstP.position.Y);
        }
        NPC.dontTakeDamage = false;
        NPC.life = 0;
        GetInstance<DownedBossSystem>().downedCecitior = true;
        SoundEngine.PlaySound(Sounds.evilOutro);
        NPC.checkDead();
        NPC.netUpdate = true;
    }
}
