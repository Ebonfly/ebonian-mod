using System;
using EbonianMod.Content.Dusts;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Bestiary;
using EbonianMod.Common.Globals;

namespace EbonianMod.Content.NPCs.Overworld.Belladonna;
public class Belladonna : ModNPC
{
    public override string Texture => Helper.AssetPath + "NPCs/Overworld/Belladonna/" + Name;
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 17;
    }
    public override void SetDefaults()
    {
        NPC.width = 32;
        NPC.height = 56;
        NPC.lifeMax = 75;
        NPC.defense = 3;
        NPC.damage = 0;
        NPC.knockBackResist = 0.6f;
        NPC.HitSound = SoundID.DD2_KoboldHurt with { Variants = [0, 1] };
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.aiStyle = -1;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D tex = TextureAssets.Npc[Type].Value;
        Texture2D glow = Assets.NPCs.Overworld.Belladonna.Belladonna_Glow.Value;
        SpriteEffects effects = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        spriteBatch.Draw(tex, NPC.Center + NPC.GFX() + new Vector2(0, 2) - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.Size / 2, NPC.scale, effects, 0);
        spriteBatch.Draw(glow, NPC.Center + NPC.GFX() + new Vector2(0, 2) - screenPos, NPC.frame, Color.White * ((MathF.Sin(Main.GlobalTimeWrappedHourly * 0.75f) + 1) * 0.5f), NPC.rotation, NPC.Size / 2, NPC.scale, effects, 0);
        return false;
    }
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
        {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new FlavorTextBestiaryInfoElement("Mods.EbonianMod.Misc.Types.Plant"),
                new FlavorTextBestiaryInfoElement("Mods.EbonianMod.NPCs.Belladonna.Bestiary")
        });
    }
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
    public float AITimer3
    {
        get => NPC.ai[3];
        set => NPC.ai[3] = value;
    }
    const int Idle = 0, Move = 1, Plant = 2;
    public override void FindFrame(int frameHeight)
    {
        if (!NPC.IsABestiaryIconDummy)
        {
            if (NPC.velocity.Y > 0.1f || NPC.velocity.Y < -0.1f)
            {
                NPC.frameCounter = 1;
                NPC.frame.Y = NPC.height;
            }
            if (AIState == Idle)
                NPC.frame.Y = 0;
            else if (AIState == Move)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter % 5 == 0)
                {
                    if (NPC.frame.Y < 7 * NPC.height)
                        NPC.frame.Y += NPC.height;
                    else
                        NPC.frame.Y = 2 * NPC.height;
                }
            }
            else if (AIState == Plant)
            {
                if (NPC.frame.Y < 8 * NPC.height)
                    NPC.frame.Y = 8 * NPC.height;
                NPC.frameCounter++;
                if (NPC.frameCounter % 5 == 0)
                {
                    if (NPC.frame.Y < 16 * NPC.height)
                        NPC.frame.Y += NPC.height;
                    else
                    {
                        AITimer = 0;
                        AIState = Idle;
                    }
                }
            }
        }
        else
        {
            NPC.frameCounter++;
            if (NPC.frameCounter % 5 == 0)
            {
                if (NPC.frame.Y < 7 * NPC.height)
                    NPC.frame.Y += NPC.height;
                else
                    NPC.frame.Y = 2 * NPC.height;
            }
        }
    }
    public override void HitEffect(NPC.HitInfo hit)
    {
        Helper.SpawnDust(NPC.position, NPC.Size, DustID.Grass, new Vector2(2 * hit.HitDirection, -1.5f), 2);
        Helper.SpawnDust(NPC.position, NPC.Size, DustType<BelladonnaD1>(), new Vector2(2 * hit.HitDirection, -1.5f), 2);
        Helper.SpawnDust(NPC.position, NPC.Size, DustType<BelladonnaD2>(), new Vector2(2 * hit.HitDirection, -1.5f), 2);
        if (NPC.life <= 0)
        {
            Helper.SpawnDust(NPC.position, NPC.Size, DustID.Grass, new Vector2(2 * hit.HitDirection, -1.5f), 8);
            Helper.SpawnDust(NPC.position, NPC.Size, DustType<BelladonnaD1>(), new Vector2(2 * hit.HitDirection, -1.5f), 8);
            Helper.SpawnDust(NPC.position, NPC.Size, DustType<BelladonnaD2>(), new Vector2(2 * hit.HitDirection, -1.5f), 8);
            Helper.SpawnGore(NPC, "EbonianMod/Belladonna", 1, 1, Vector2.One * hit.HitDirection * 2);
            Helper.SpawnGore(NPC, "EbonianMod/Belladonna", 1, 2, Vector2.One * hit.HitDirection * 2);
        }
    }
    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (Main.invasionType > 0) return 0;
        int number = 0;
        for (int i = 0; i < Main.npc.Length; i++) 
            if (Main.npc[i].active && Main.npc[i].type == Type) number++;
        float rate = (float)Math.Max(0.05f, 1f / (number + 1));

        return spawnInfo.Player.ZonePurity ? SpawnCondition.OverworldNight.Chance * rate / 10 : 0;
    }
    public override bool? CanFallThroughPlatforms()
    {
        Player player = Main.player[NPC.target];
        return player.Center.Y > NPC.Center.Y;
    }
    public override void AI()
    {
        Lighting.AddLight(NPC.Center, (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.75f) + 1) * 0.5f * 0.2f, (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.75f) + 1) * 0.5f * 0.2f, (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.75f) + 1) * 0.5f * 0.2f);
        NPC.TargetClosest(false);
        Player player = Main.player[NPC.target];
        NPC.spriteDirection = NPC.direction;
        if (MathF.Abs(NPC.Center.X - player.Center.X) > 20f)
            NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;
        if (AIState == Idle)
        {
            if (player.Center.DistanceSQ(NPC.Center) < MathF.Pow(1200, 2))
            {
                AITimer = 0;
                AIState = Move;
                NPC.frame.Y = NPC.height * 2;
            }
        }

        else if (AIState == Move)
        {
            NPC.knockBackResist = 0.8f;
            AITimer++;
            NPC.GetGlobalNPC<FighterGlobalAI>().FighterAI(NPC, 4, 1);

            if (player.Center.DistanceSQ(NPC.Center) < MathF.Pow(300, 2)) AITimer += 2;
            if (AITimer >= 400)
            {
                AITimer = 0;
                AIState = Plant;
                NPC.frame.Y = NPC.height * 8;
                NPC.velocity = Vector2.Zero;
            }
        }
        else if (AIState == Plant)
        {
            NPC.velocity.X *= 0;
            NPC.knockBackResist = 0f;
            if (NPC.frame.Y == 15 * NPC.height && AITimer == 0)
            {
                AITimer = 1;
                for (int i = 0; i < 3; i++)
                {
                    Vector2 rawPosition = new Vector2(16 * (NPC.Center.X / 16 + Main.rand.Next(-25, 25)) + 4, NPC.Center.Y);
                    Vector2 position = Helper.GetNearestSurface(rawPosition);
                    MPUtils.NewProjectile(NPC.GetSource_FromAI(), position - Vector2.UnitY * 20, Vector2.Zero, ProjectileType<BelladonnaBush>(), 0, 0);
                }
            }
        }
    }
}
public class BelladonnaBush : ModProjectile
{
    public override string Texture => Helper.AssetPath + "NPCs/Overworld/Belladonna/"+Name;
    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 12;
    }
    public override void SetDefaults()
    {
        Projectile.Size = new(40, 48);
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 180;
    }
    public override void OnSpawn(IEntitySource source)
    {
        Projectile.ai[0] = Main.rand.Next(2);
    }
    public override void PostDraw(Color lightColor)
    {
        SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        if (Projectile.frame == 11 && Projectile.timeLeft > 60)
        {
            if (Projectile.ai[1] < 1) Projectile.ai[1] += 0.05f;
            float opacity = (1 + Projectile.ai[1]) * (Projectile.ai[0] == 0 ? (MathF.Sin(Main.GlobalTimeWrappedHourly * 2) + 1) * 0.08f : (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5) * 2) * 0.5f;

            Texture2D texture = Projectile.ai[0] == 0 ? Assets.NPCs.Overworld.Belladonna.BushOverlay_0.Value : Assets.NPCs.Overworld.Belladonna.BushOverlay_1.Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * opacity, 0, texture.Size() / 2, Projectile.scale, effects, 0);
        }
    }
    public override void AI()
    {
        if (Projectile.timeLeft == 165 && Projectile.ai[2] == 0)
        {
            SoundEngine.PlaySound(SoundID.Item45 with
            {
                Volume = 0.75f,
                Pitch = 0.4f,
                PitchVariance = 0.2f,
                MaxInstances = 1
            }, Projectile.Center);
            Projectile.ai[2] = 1;
        }
        if (Projectile.timeLeft > 45)
        {
            if (++Projectile.frameCounter % 5 == 0 && Projectile.frame < 11) Projectile.frame++;
        }
        else if (++Projectile.frameCounter % 5 == 0 && Projectile.frame > 0) Projectile.frame--;

        if (Projectile.timeLeft == 60)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (Projectile.ai[0] == 0)
                    MPUtils.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Main.rand.NextVector2Unit(), ProjectileType<Blueberry>(), 0, 0);
                else
                    MPUtils.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Main.rand.NextVector2Unit(), ProjectileType<Nightshade>(), 0, 0);
            }
        }
    }
}
public class Blueberry : ModProjectile
{
    public override string Texture => Helper.AssetPath + "NPCs/Overworld/Belladonna/"+Name;
    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One * 8;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = true;
        Projectile.aiStyle = 14;
        Projectile.timeLeft = 300;
    }
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        fallThrough = false;
        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }
    public override void PostDraw(Color lightColor)
    {
        Color color = Color.White * ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 5) * 2);
        Texture2D a = TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(a, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, a.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
    }
    public override void AI()
    {
        if (Projectile.timeLeft < 10) Projectile.scale -= 0.1f;
        foreach (NPC npc in Main.npc)
        {
            if (npc.active)
            {
                if (!npc.friendly && npc.type != NPCType<Belladonna>() && npc.Center.Distance(Projectile.Center) < 50)
                {
                    if (npc.life < npc.lifeMax)
                    {
                        CombatText.NewText(npc.getRect(), CombatText.HealLife, Math.Min(10, npc.lifeMax - npc.life));
                        npc.life += Math.Min(10, npc.lifeMax - npc.life);
                        Projectile.Kill();
                    }
                }
            }
        }
    }
}
public class Nightshade : ModProjectile
{
    public override string Texture => Helper.AssetPath + "NPCs/Overworld/Belladonna/"+Name;
    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One * 8;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = true;
        Projectile.aiStyle = 14;
        Projectile.timeLeft = 300;
    }
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        fallThrough = false;
        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }
    public override void PostDraw(Color lightColor)
    {
        Color color = Color.White * ((float)Math.Cos(Main.GlobalTimeWrappedHourly * 10) * 2);
        Texture2D a = TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(a, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, a.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
    }
    public override void OnKill(int timeLeft)
    {
        MPUtils.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ProjectileType<Nightshade2>(), 10, 0);
    }
    public override void AI()
    {
        if (Projectile.timeLeft < 10) Projectile.scale -= 0.1f;
        
        Lighting.AddLight(Projectile.Center, new Vector3((MathF.Sin(Main.GlobalTimeWrappedHourly * 2) + 1) * 0.08f));
        foreach (Player npc in Main.player)
        {
            if (npc.active)
            {
                if (npc.Center.Distance(Projectile.Center) < 50)
                {
                    Projectile.Kill();
                    MPUtils.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ProjectileType<Nightshade2>(), 10, 0);
                }
            }
        }
    }
}
public class Nightshade2 : ModProjectile
{
    public override string Texture => Helper.AssetPath + "NPCs/Overworld/Belladonna/"+Name;
    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 6;
    }
    public override void SetDefaults()
    {
        Projectile.Size = Vector2.One * 28;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = true;
        Projectile.aiStyle = 14;
    }
    public override bool ShouldUpdatePosition() => false;
    public override Color? GetAlpha(Color lightColor) => Color.White;
    int seed;
    public override void OnSpawn(IEntitySource source)
    {
        for (int i = 0; i < 10; ++i)
            Dust.NewDustPerfect(Projectile.Center, DustType<BelladonnaD2>(), Main.rand.NextVector2Unit());
        seed = Main.rand.Next(int.MaxValue);
    }
    public override void PostDraw(Color lightColor)
    {
        Texture2D tex = Assets.Extras.cone4.Value;
        Main.spriteBatch.Reload(BlendState.Additive);
        UnifiedRandom rand = new UnifiedRandom(seed);
        float max = 40;
        float alpha = Lerp(0.5f, 0, Projectile.ai[1]) * 2;
        for (float i = 0; i < max; i++)
        {
            float angle = Helper.CircleDividedEqually(i, max);
            float scale = rand.NextFloat(0.025f, .15f);
            Vector2 offset = new Vector2(Main.rand.NextFloat(20) * Projectile.ai[1] * scale, 0).RotatedBy(angle);
            for (float j = 0; j < 2; j++)
                Main.spriteBatch.Draw(tex, Projectile.Center + offset - Main.screenPosition, null, Color.DarkViolet * alpha * 0.35f, angle, new Vector2(0, tex.Height / 2), new Vector2(Projectile.ai[1], alpha) * scale, SpriteEffects.None, 0);
        }
        Main.spriteBatch.Reload(BlendState.AlphaBlend);
    }
    public override void AI()
    {
        if (++Projectile.frameCounter % 5 == 0)
        {
            if (Projectile.frame < 5)
                Projectile.frame++;
            else
                Projectile.Kill();
        }
        Projectile.ai[1] = Lerp(Projectile.ai[1], 1, 0.1f);

    }
}
