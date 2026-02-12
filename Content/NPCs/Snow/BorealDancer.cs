
using EbonianMod.Content.Projectiles.Enemy.Snow;
using System;
using Terraria.GameContent.Bestiary;

namespace EbonianMod.Content.NPCs.Snow;

public class BorealDancer : ModNPC
{
    public override string Texture => Helper.AssetPath + "NPCs/Snow/"+Name;
    public override void SetDefaults()
    {
        NPC.width = 20;
        NPC.height = 50;
        NPC.HitSound = SoundID.Item49;
        NPC.DeathSound = SoundID.Item27;
        NPC.damage = 1;
        NPC.defense = 0;
        NPC.lifeMax = 20;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (Main.invasionType > 0) return 0;
        return (spawnInfo.Player.ZoneSnow && (spawnInfo.Player.ZoneNormalUnderground || spawnInfo.Player.ZoneNormalCaverns)) || (spawnInfo.Player.ZoneSnow && spawnInfo.Player.ZoneRain) ? 0.14f : 0;
    }
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
        {
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundSnow,
            new FlavorTextBestiaryInfoElement("Mods.EbonianMod.Misc.Types.Construct"),
            new FlavorTextBestiaryInfoElement(NPC.BestiaryKey())
        });
    }
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 14;
    }

    public override void AI()
    {
        Lighting.AddLight(NPC.Center, new Vector3(0f, .09f, .07f));
        NPC.TargetClosest(true);
        Player player = Main.player[NPC.target];

        Vector2 difference = player.Center - NPC.Center;
        if (!Helper.Raycast(NPC.Center - Vector2.UnitY * 6, difference, difference.Length(), false).Success)
            NPC.ai[2] = 180;
        NPC.ai[2]--;

        NPC.ai[1] = 2;

        if ((int)NPC.ai[0] == -1 && NPC.frame.Y is > 13 * 52 or < 7 * 52)
        {
            NPC.ai[0] = 1;
            NPC.netUpdate = true;
        }

        if (NPC.ai[2] > 0)
        {
            if ((int)NPC.ai[0] == 0)
            {
                SoundEngine.PlaySound(SoundID.Item1.WithPitchOffset(Main.rand.NextFloat(0f, 1f)), NPC.Center);
                NPC.ai[0] = -1;
                NPC.frame.Y = 8 * 52;
                NPC.netUpdate = true;
            }
            if ((int)NPC.ai[0] == 1)
            {
                NPC.velocity.X += NPC.direction * 0.05f;
                NPC.velocity.X = Clamp(NPC.velocity.X, -7, 7);
                if (MathF.Abs(player.Center.X - NPC.Center.X) < 62 && MathF.Abs(player.Center.Y - NPC.Center.Y) < 30)
                {
                    MPUtils.NewProjectile(NPC.GetSource_FromThis(), Helper.GetNearestSurface(new Vector2(NPC.Center.X + NPC.direction * 15, NPC.Center.Y)) + new Vector2(0, 4), Vector2.Zero, ProjectileType<BorealSpike>(), NPC.damage, 0, ai0: 2, ai1: NPC.direction);

                    SoundEngine.PlaySound(SoundID.Item1.WithPitchOffset(Main.rand.NextFloat(0f, 1f)), NPC.Center);
                    NPC.ai[0] = -1;
                    NPC.frame.Y = 8 * 52;

                    NPC.velocity.X = -NPC.direction * 2.4f;
                    NPC.netUpdate = true;
                }
            }
            else
            {
                NPC.velocity.X *= 0.9f;
                NPC.spriteDirection = player.Center.X > NPC.Center.X ? 1 : -1;
            }
        }
        float xVelocityAbs = MathF.Abs(NPC.velocity.X);
        if (NPC.ai[0] != 1)
        {
            NPC.ai[1] = 2;
            NPC.netUpdate = true;
        }
        else
        {
            NPC.ai[1] = Clamp((int)(xVelocityAbs * 0.8f), 1, 3);
        }
        if (Helper.Raycast(NPC.Center - Vector2.UnitY * 12, new Vector2(NPC.velocity.X, 0), 45, false).RayLength < 12 && xVelocityAbs > 0.4f)
        {
            for (int u = 0; u < 15; u++)
            {
                Dust.NewDustPerfect(NPC.Center, DustID.Snow, -NPC.velocity.X / 4 * Main.rand.NextFloat(-Pi / 3, Pi / 3).ToRotationVector2() * Main.rand.NextFloat(2, 7), Scale: Main.rand.NextFloat(0.7f, 1.3f)).noGravity = true;
            }
            NPC.velocity.X *= -0.6f;
        }
        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY, 1, false, 0);
    }

    public override bool CheckDead()
    {
        if (Main.dedServ)
            return true;
        for (int i = 1; i < 4; i++)
        {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position + Main.rand.NextVector2Circular(7, 7), NPC.velocity, Find<ModGore>("EbonianMod/BorealDancer" + i).Type, NPC.scale);
            for (int u = 0; u < 15; u++)
            {
                Dust.NewDustPerfect(NPC.Center, DustID.Snow, Main.rand.NextFloat(-Pi, Pi).ToRotationVector2() * Main.rand.NextFloat(2, 10), Scale: Main.rand.NextFloat(0.7f, 1.3f)).noGravity = true;
            }
        }
        return true;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        if (NPC.frameCounter * NPC.ai[1] > 10)
        {
            NPC.frameCounter = 0;
            if (NPC.ai[0] != 0)
                NPC.frame.Y += frameHeight;
            if ((int)NPC.ai[0] == 1 && NPC.frame.Y > 7 * frameHeight)
            {
                NPC.frame.Y = frameHeight;
            }
            if (NPC.frame.Y > 13 * frameHeight)
            {
                NPC.frame.Y = frameHeight;
            }
        }
    }
}