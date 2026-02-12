using EbonianMod.Core.Systems.Verlets;
using EbonianMod.Content.Projectiles.VFXProjectiles;
using System;
using System.IO;
using Terraria.GameContent.Bestiary;

namespace EbonianMod.Content.NPCs.Crimson.Jellyfish;

public class JelleyeFish : ModNPC
{
    public override string Texture => Helper.AssetPath + "NPCs/Crimson/Jellyfish/JelleyeFish";
    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.lifeMax = 350;
        NPC.damage = 20;
        NPC.noTileCollide = true;
        NPC.defense = 10;
        NPC.knockBackResist = 0;
        NPC.width = 82;
        NPC.height = 78;
        NPC.lavaImmune = true;
        NPC.noGravity = true;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.netAlways = true;
        NPC.value = Item.buyPrice(0, 0, 20);
    }
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 7;
    }
    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (Main.invasionType > 0) return 0;
        return (spawnInfo.Player.ZoneCrimson && spawnInfo.Player.ZoneOverworldHeight) ? 0.08f : 0;
    }
    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ItemID.Lens, 1, 1, 8));
    }
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
            new FlavorTextBestiaryInfoElement("Mods.EbonianMod.Misc.Types.Evil"),
            new FlavorTextBestiaryInfoElement("Mods.EbonianMod.NPCs.JelleyeFish.Bestiary"),
        });
    }
    Verlet[] eyeVerlets = new Verlet[4];
    Verlet[] tentVerlets = new Verlet[4];
    public float AIState
    {
        get => NPC.ai[0];
        set => NPC.ai[0] = value;
    }
    public float AITimer
    {
        get => NPC.ai[1];
        set => NPC.ai[1] = value;
    }
    public float AITimer2
    {
        get => NPC.ai[2];
        set => NPC.ai[2] = value;
    }
    Vector2 scaleVector = new Vector2(1, 1);
    Vector2 _velocity;
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.WriteVector2(_velocity);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        _velocity = reader.ReadVector2();
    }
    public override void AI()
    {
        NPC.TargetClosest(false);
        Player player = Main.player[NPC.target];
        AITimer++;
        AITimer2++;
        if (AITimer2 < 100)
        {
            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Clamp(NPC.Center.FromAToB(player.Center, true, true), new Vector2(-1, -1), new Vector2(1, -0.01f)) * 0.1f + new Vector2(0, 3), 0.025f);
            NPC.rotation = Utils.AngleLerp(NPC.rotation, 0, 0.01f);
        }
        else
        {
            MPUtils.NewProjectile(null, NPC.Center, -NPC.velocity.RotatedByRandom(PiOver2) * 0.5f, ModContent.ProjectileType<HostileGibs>(), 10, 0);
            if (AITimer2 == 100)
            {
                NPC.netUpdate = true;
                SoundEngine.PlaySound(SoundID.NPCHit20, NPC.Center);
                _velocity = Vector2.Clamp(NPC.Center.FromAToB(player.Center - new Vector2(0, 100)) * 10, new Vector2(-1, -1), new Vector2(1, -.5f));
            }
            Vector2 oldVel = _velocity;
            if (_velocity.X.InRange(0, 0.05f)) _velocity.X = 1 * (oldVel.X > 0 ? 1 : -1);
            NPC.velocity = Vector2.Lerp(NPC.velocity, _velocity * 10, 0.1f);
            NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.ToRotation() + PiOver2, 0.1f);
            scaleVector = new Vector2(0.8f, 1.2f);

        }
        if (AITimer2 > 110) AITimer2 = Main.rand.Next(-50, 80);
        NPC.velocity *= 0.999f;
        scaleVector = Vector2.Lerp(scaleVector, Vector2.One, 0.1f);

        for (int i = 0; i < eyeVerlets.Length; i++)
        {
            if (eyeVerlets[i] is not null)
            {
                eyeVerlets[i].gravity = Lerp(eyeVerlets[i].gravity, MathF.Sin(i + AITimer * 0.025f) * 2 + 4, 0.1f);
                if (AITimer % 2 == 0)
                {
                    Vector2 direction = -NPC.rotation.ToRotationVector2().RotatedBy(-PiOver2).RotatedBy((i - eyeVerlets.Length / 2f) * 0.5f * MathF.Sin(i + AITimer * 0.1f));
                    if (direction != Vector2.Zero)
                        direction.Normalize();
                    else direction = Vector2.UnitY;
                    eyeVerlets[i].gravityDirection = direction;
                }
                eyeVerlets[i].Update(NPC.Center + new Vector2(MathF.Abs(i - 1) * 10, -4).RotatedBy(NPC.rotation - PiOver2), eyeVerlets[i].lastP.position);
            }
        }
        for (int i = 0; i < tentVerlets.Length; i++)
        {
            if (tentVerlets[i] is not null)
            {
                tentVerlets[i].gravity = Lerp(tentVerlets[i].gravity, MathF.Sin(i + AITimer * 0.05f) + 3, 0.1f);
                if (AITimer % 2 == 0)
                {
                    Vector2 direction = -NPC.rotation.ToRotationVector2().RotatedBy(-PiOver2).RotatedBy((i - tentVerlets.Length / 2f) * 0.5f * MathF.Sin(i + AITimer * 0.1f));
                    if (direction != Vector2.Zero)
                        direction.Normalize();
                    else direction = Vector2.UnitY;
                    tentVerlets[i].gravityDirection = direction;
                }
                tentVerlets[i].Update(NPC.Center + new Vector2((i - 2) * 8, -4).RotatedBy(NPC.rotation - PiOver2), tentVerlets[i].lastP.position);
            }
        }
    }
    public override bool CheckDead()
    {
        if (Main.dedServ) return true;

        for (int i = 0; i < 55; i++) Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, Main.rand.NextFloatDirection() * 5 + NPC.velocity.X, Main.rand.NextFloatDirection() * 5 + + NPC.velocity.Y, Scale: 2);

        Gore.NewGore(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(5, 5), Find<ModGore>("EbonianMod/JelleyeFishGore0").Type, NPC.scale);
        for (int i = 0; i < tentVerlets.Length; i++)
        {
            if (tentVerlets[i] is not null)
            {
                for (int j = 0; j < tentVerlets[i].points.Count; j++)
                {
                    if (j % 4 == 0 || j == tentVerlets[i].points.Count - 1)
                    {
                        if (j < tentVerlets[i].points.Count - 1) Gore.NewGore(NPC.GetSource_Death(), tentVerlets[i].points[j].position, Main.rand.NextVector2Circular(5, 5), Find<ModGore>("EbonianMod/JelleyeFishGore1").Type, NPC.scale);
                        else Gore.NewGore(NPC.GetSource_Death(), tentVerlets[i].points[j].position, NPC.velocity, Find<ModGore>("EbonianMod/JelleyeFishGore2").Type, NPC.scale);
                    }
                }
            }
        }

        for (int i = 0; i < eyeVerlets.Length; i++)
        {
            if (eyeVerlets[i] is not null)
            {
                for (int j = 0; j < eyeVerlets[i].points.Count; j++)
                {
                    if (j % 4 == 0 || j == eyeVerlets[i].points.Count - 1)
                    {
                        if (j < eyeVerlets[i].points.Count - 1) Gore.NewGore(NPC.GetSource_Death(), eyeVerlets[i].points[j].position, NPC.velocity, Find<ModGore>("EbonianMod/JelleyeFishGore1").Type, NPC.scale);
                        else Gore.NewGore(NPC.GetSource_Death(), eyeVerlets[i].points[j].position, NPC.velocity, Find<ModGore>("EbonianMod/JelleyeFishGore0").Type, NPC.scale);
                    }
                }
            }
        }
        return true;
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy) return true;

        for (int i = 0; i < eyeVerlets.Length; i++)
        {
            if (eyeVerlets[i] is null)
                eyeVerlets[i] = new Verlet(NPC.Center + new Vector2((i - 1) * 3, 0), 6, 2 + i * 6 + Main.rand.Next(5, 12), 5, lastPointLocked: false, stiffness: 30);
            else
                eyeVerlets[i].Draw(spriteBatch, new VerletDrawData(new VerletTextureData(Texture + "_Chain", null, Texture + "_Eye")));
        }
        for (int i = 0; i < tentVerlets.Length; i++)
        {
            if (tentVerlets[i] is null)
                tentVerlets[i] = new Verlet(NPC.Center + new Vector2((i - 3) * 4, 0), 6, 10 + i * 3, 7, lastPointLocked: false, stiffness: 15);
            else
                tentVerlets[i].Draw(spriteBatch, new VerletDrawData(new VerletTextureData(Texture + "_Chain", null, Texture + "_Tip")));
        }

        Texture2D tex = TextureAssets.Npc[Type].Value;
        spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, NPC.frame, drawColor, NPC.rotation, NPC.Size / 2, scaleVector * NPC.scale, SpriteEffects.None, 0);

        if (Main.LocalPlayer.HasBuff(BuffID.Hunter) && !NPC.IsABestiaryIconDummy)
            spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, NPC.frame, NPC.HunterPotionColor(), NPC.rotation, NPC.Size / 2, scaleVector * NPC.scale, SpriteEffects.None, 0);

        return false;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        if (NPC.frameCounter > 4)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y += frameHeight;
            if (NPC.frame.Y > frameHeight * 6)
            {
                NPC.frame.Y = 0;
            }
        }
    }
}
