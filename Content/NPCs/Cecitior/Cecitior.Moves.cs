using EbonianMod.Core.Systems.Verlets;
using EbonianMod.Content.Projectiles.Cecitior;
using EbonianMod.Content.Projectiles.Friendly.Crimson;
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
    public void DoThrowUpBlood()
    {
        NetUpdateAtSpecificTime(15, 60);
        if (AITimer < 15)
        {
            open = true;
            if (halfEyesPhase2)
                AITimer2 = 20;
            else
                AITimer2 = 10;
            NPC.velocity = Helper.FromAToB(NPC.Center, player.Center - new Vector2(0, 200), false) / 20f;
            openOffset.X++;
            openRotation += MathHelper.ToRadians(2);
            rotation -= MathHelper.ToRadians(2);
        }
        if (AITimer >= 30 && AITimer <= 60 && (int)AITimer % (phase2 ? 3 : halfEyesPhase2 ? 5 : 10) == 0)
        {
            AITimer2 -= 4;
            NPC.velocity = Vector2.Zero;
            if (halfEyesPhase2)
                MPUtils.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(AITimer2 * 0.5f, -6), ProjectileType<CIchor>(), 30, 0);
            Projectile a = MPUtils.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(AITimer2 * 5, -5), ProjectileType<CHeart>(), 30, 0);
            a.SetToHostile();
            NPC.netUpdate = true;
        }
        if (AITimer >= 60)
        {
            openOffset.X--;
            openRotation -= MathHelper.ToRadians(2);
            rotation += MathHelper.ToRadians(2);
        }
        if (AITimer >= 70)
        {
            openOffset = Vector2.Zero;
            open = false;
        }
    }

    public void DoSpitTeeth()
    {
        NetUpdateAtSpecificTime(15, 35, 50);
        if (AITimer < 15)
        {
            open = true;
            if (halfEyesPhase2)
                AITimer2 = 5;
            else
                AITimer2 = 10;
            openOffset.X++;
            openRotation -= MathHelper.ToRadians(2);
            rotation += MathHelper.ToRadians(2);
        }
        if (AITimer < 35)
            NPC.velocity = Helper.FromAToB(NPC.Center, player.Center - new Vector2(0, 200), false) / 2;
        else
            NPC.velocity = Vector2.Zero;
        if (AITimer > 50 && AITimer < 65)
        {
            AITimer2 -= 0.55f;
            NPC.velocity = Vector2.Zero;
            if (AITimer % 3 == 0)
                SoundEngine.PlaySound(Sounds.cecitiorSpit, NPC.Center);
            for (int i = -1; i < 2; i++)
            {
                if (i == 0)
                    continue;
                MPUtils.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(i * AITimer2, 5), ProjectileType<CecitiorTeeth>(), 30, 0);
            }
        }
        if (AITimer >= 65)
        {
            openOffset = Vector2.Zero;
            open = false;
            if (AITimer < 67)
                NPC.netUpdate = true;
        }
    }


    public void DoTongue()
    {
        NetUpdateAtSpecificTime(15, 20, 40);
        if (phase2)
            AITimer++;
        open = true;
        if (AITimer < 15)
        {
            AITimer2 = 10;
            openOffset.X++;
            openRotation -= MathHelper.ToRadians(2);
            rotation += MathHelper.ToRadians(2);
        }
        if (AITimer == 40)
        {
            SoundEngine.PlaySound(Sounds.cecitiorSpit, NPC.Center);
            tongue = MPUtils.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Clamp(Helper.FromAToB(NPC.Center, player.Center), new Vector2(-0.35f, 1), new Vector2(0.35f, 1)) * 1.5f, ProjectileType<LatcherProjectileCecitior>(), 15, 0, -1, NPC.whoAmI);
        }
        if (AITimer < 20)
            NPC.velocity = Helper.FromAToB(NPC.Center, player.Center - new Vector2(0, 200), false) / 10f;
        if (AITimer == 20)
        {
            NPC.velocity = Vector2.Zero;
            MPUtils.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(0, 0), ProjectileType<BloodShockwave2>(), 0, 0, -1, NPC.whoAmI);
        }
        if (tongue is not null)
        {
            if (tongue.ai[1] == 1 && tongue.active)
            {
                NPC.damage = 15;
                AITimer--;
            }
            else if (tongue.ai[1] == 0)
                AITimer++;
            else if (!tongue.active)
            {
                NPC.velocity = NPC.velocity.RotatedBy(ToRadians(3 * (NPC.velocity.X > 0 ? -1 : 1))) * 0.97f;
                rotation = 0;
                openRotation = 0;

                NPC.damage = 0;
                openOffset = Vector2.Zero;
                open = false;
                NPC.velocity *= 0.985f;
                AITimer += 2;
                NPC.netUpdate = true;
            }
        }
        if (AITimer >= 240)
        {
            openOffset = Vector2.Zero;
            open = false;
        }
    }


    public void DoTeethChomp()
    {
        NetUpdateAtSpecificTime(40, 50, 60, 75, 76);
        if (!Main.dedServ && AITimer == 2)
            openSound = SoundEngine.PlaySound(Sounds.cecitiorOpen, NPC.Center);
        if (AITimer < 20)
        {
            open = true;
            shakeVal = Lerp(shakeVal, 10, 0.1f);
            openOffset += Vector2.UnitX * 13;
            NPC.velocity = Helper.FromAToB(NPC.Center, player.Center, false) / 8f;
            NPC.netUpdate = true;
        }
        if (AITimer >= 40 && AITimer <= (40 + (phase2.ToInt() + halfEyesPhase2.ToInt()) * 10) && AITimer % 10 == 0)
        {
            NPC.velocity = Vector2.Zero;

            SoundEngine.PlaySound(Sounds.cecitiorSpit, NPC.Center);
            SoundEngine.PlaySound(Sounds.cecitiorSpit, NPC.Center);
            float offset = Lerp(0, PiOver4, (AITimer - 40) / 20f);
            for (int i = 0; i < 6; i++)
            {
                float angle = Helper.CircleDividedEqually(i, 12);
                MPUtils.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + openOffset, new Vector2(5 + (i * 0.1f)).RotatedBy(angle - offset), ProjectileType<CecitiorTeeth>(), 30, 0);
            }
            for (int i = 8; i < 12; i++)
            {
                float angle = Helper.CircleDividedEqually(i, 12);
                MPUtils.NewProjectile(NPC.GetSource_FromThis(), NPC.Center - openOffset, new Vector2(5 + (i * 0.1f)).RotatedBy(angle + offset), ProjectileType<CecitiorTeeth>(), 30, 0);
            }
        }
        if (AITimer == 60)
        {
            MPUtils.NewProjectile(NPC.GetSource_FromAI(), NPC.Center - openOffset, Vector2.Zero, ProjectileType<BloodShockwave2>(), 0, 0);
            MPUtils.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + openOffset, Vector2.Zero, ProjectileType<BloodShockwave2>(), 0, 0);

            NPC.velocity = Vector2.Zero;
        }
        if (AITimer < 75 && AITimer > 40)
            shakeVal = Lerp(shakeVal, (phase2 ? 30 : 15), 0.0f);
        if (AITimer == 75)
            open = false;
    }


    public void DoChomp()
    {
        NetUpdateAtSpecificTime(26, 50, 57, 65);
        if (!Main.dedServ && AITimer == 2)
            openSound = SoundEngine.PlaySound(Sounds.cecitiorOpen, NPC.Center);
        if (open)
        {
            if (AIByte % 2 != (phase2 ? 1 : 0))
                openOffset += Vector2.UnitY * 5;
            else
                openOffset += Vector2.UnitX * 6;
        }
        if (AITimer < 25)
        {
            open = true;
            NPC.velocity = Helper.FromAToB(NPC.Center, player.Center, false) / 10f;
        }
        if (AITimer >= 25 && AITimer < (50 + (phase2 ? 7 : 0)))
        {
            shakeVal = Lerp(shakeVal, (phase2 ? 30 : 15), 0.1f);
            if (AITimer < 53)
                savedPos = player.Center + (phase2 ? player.velocity * 5 : Vector2.Zero);
            NPC.velocity = Helper.FromAToB(NPC.Center, savedPos, false) / 5f;
        }
        if (AITimer == 50 + (phase2 ? 7 : 0))
        {
            MPUtils.NewProjectile(NPC.GetSource_FromAI(), NPC.Center - openOffset, Vector2.Zero, ProjectileType<BloodShockwave2>(), 0, 0);
            MPUtils.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + openOffset, Vector2.Zero, ProjectileType<BloodShockwave2>(), 0, 0);

            NPC.velocity = Vector2.Zero;
        }
        if (AITimer == 65)
        {
            shakeVal = 0;
            open = false;
        }
        if (AITimer > 65)
        {
            openOffset.Y = Lerp(openOffset.Y, 0, 0.3f);
        }
        if (AIByte% 2 != (phase2 ? 1 : 0))
        {
            if (MathF.Abs(openOffset.Y) < 50 && AITimer > 25)
            {
                if (openOffset != Vector2.Zero)
                {
                    SoundEngine.PlaySound(Sounds.cecitiorClose, NPC.Center);
                    CameraSystem.ScreenShakeAmount = 5;
                }
                openRotation = 0;
                rotation = 0;
                open = false;
                NPC.frame.Y = 0;
                openOffset = Vector2.Zero;
                NPC.netUpdate = true;
            }
        }
        if (AITimer >= 75)
        {
            openRotation = 0;
            rotation = 0;
            if (phase2)
                NPC.dontTakeDamage = false;
            int num = 1;
            if (halfEyesPhase2)
                num = 2;
            if (phase2)
                num = 0;
            if (AIByte < num)
            {
                AIByte++;
                AITimer = 0;
                NPC.velocity = Vector2.Zero;
            }
            else
            {
                ResetState();
            }
            NPC.netUpdate = true;
        }
    }
}
