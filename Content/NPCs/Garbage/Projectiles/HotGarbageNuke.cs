using EbonianMod.Content.NPCs.ArchmageX;
using EbonianMod.Content.Tiles;
using EbonianMod.Core.Systems.Cinematic;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Graphics.CameraModifiers;

namespace EbonianMod.Content.NPCs.Garbage.Projectiles;

public class HotGarbageNuke : ModProjectile
{
    public override string Texture => Helper.AssetPath + "NPCs/Garbage/"+Name;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 10000;
        ProjectileID.Sets.TrailCacheLength[Type] = 25;
        ProjectileID.Sets.TrailingMode[Type] = 2;
        EbonianMod.projectileFinalDrawList.Add(Type);
    }

    public override void SetDefaults()
    {
        Projectile.width = 80;
        Projectile.height = 35;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 500;
    }
    public Vector2 targetPos;
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.WriteVector2(targetPos);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        targetPos = reader.ReadVector2();
    }
    float waveTimer, waveTimer2, vfxOffset;
    public override bool PreDraw(ref Color lightColor)
    {
        if (Projectile.timeLeft > 497) return false;
        List<VertexPositionColorTexture> vertices = new();

        vfxOffset -= 0.015f;
        if (vfxOffset <= 0)
            vfxOffset = 1;
        vfxOffset = Clamp(vfxOffset, float.Epsilon, 1 - float.Epsilon);

        var fadeMult = Helper.SafeDivision(1f / Projectile.oldPos.Length);
        for (int i = 1; i < Projectile.oldPos.Length; i++)
        {
            float _mult = (1f - fadeMult * i);
            float mult = Lerp(1, 0, _mult);
            Color c = Color.Lerp(Color.Red, Color.Gray * _mult, mult * mult) * _mult * alpha;

            float rot = Projectile.oldPos[i - 1].FromAToB(Projectile.oldPos[i]).ToRotation();
            float __off = vfxOffset;
            if (__off > 1) __off = -__off + 1;
            float _off = __off + mult;
            if (Projectile.oldPos[i] == Vector2.Zero) break;
            vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 + new Vector2(-14, 0).RotatedBy(Projectile.velocity.ToRotation()) + new Vector2(SmoothStep(20, 0, mult), 0).RotatedBy(rot + PiOver2) - Main.screenPosition, c, new Vector2(_off, 1)));
            vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 + new Vector2(-14, 0).RotatedBy(Projectile.velocity.ToRotation()) + new Vector2(SmoothStep(20, 0, mult), 0).RotatedBy(rot - PiOver2) - Main.screenPosition, c, new Vector2(_off, 0)));
        }
        SpritebatchParameters sbParams = Main.spriteBatch.Snapshot();
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        if (vertices.Count > 2)
        {
            Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Assets.Extras.wavyLaser2.Value, false);
        }
        Main.spriteBatch.ApplySaved(sbParams);
        Texture2D pulse = Assets.Extras.PulseCircle2.Value;
        Texture2D ring = Assets.Extras.crosslight.Value;
        Texture2D ring2 = Assets.Extras.Extras2.slash_06.Value;
        Texture2D chevron = Assets.Extras.chevron.Value;
        Texture2D hazard = Assets.Extras.hazardUnblurred.Value;
        Texture2D textGlow = Assets.Extras.textGlow.Value;
        Texture2D circle = Assets.Extras.explosion2.Value;
        Texture2D exclamation = Assets.Extras.exclamation.Value;
        float _alpha = Utils.GetLerpValue(0, 2, waveTimer);
        float alpha2 = Clamp((float)Math.Sin(_alpha * Math.PI) * 1, 0, 1f);

        float chevron_alpha = Utils.GetLerpValue(0, 1, waveTimer2);
        float chevron_alpha2 = Clamp((float)Math.Sin(chevron_alpha * Math.PI) * 1, 0, 1f);
        if (Projectile.ai[0] > 60)
            waveTimer += 0.02f * (waveTimer.SafeDivision() + (alpha2.SafeDivision()));
        if (waveTimer > 2)
            waveTimer = 0;

        waveTimer2 += 0.019f * (waveTimer2.SafeDivision());
        if (waveTimer2 > 1)
            waveTimer2 = 0;

        Color color = Color.Lerp(Color.Maroon, Color.Red, Lerp(0, 1, Clamp((Projectile.ai[0]) / 600, 0, 1)));
        Color color2 = Color.Lerp(Color.Black, Color.DarkRed, Lerp(0, 1, Clamp((Projectile.ai[0]) / 600, 0, 1)));
        if (targetPos != Vector2.Zero)
        {
            Main.spriteBatch.Draw(circle, targetPos - Main.screenPosition, null, Color.Black * Projectile.ai[2] * 0.4f, 0, circle.Size() / 2, 4.8f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(circle, targetPos - Main.screenPosition, null, color * chevron_alpha2 * 0.125f, Main.GameUpdateCount * -0.01f, circle.Size() / 2, 4.7f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(circle, targetPos - Main.screenPosition, null, color * chevron_alpha2 * 0.125f, Main.GameUpdateCount * 0.01f, circle.Size() / 2, 4.7f, SpriteEffects.None, 0);


            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Red * SmoothStep(0, 1, Clamp((Projectile.ai[0] - 600) / 60f, 0, 1)));

            Main.spriteBatch.Reload(BlendState.Additive);
            Main.spriteBatch.Draw(pulse, targetPos - Main.screenPosition, null, Color.DarkRed * Projectile.ai[2], 0, pulse.Size() / 2, 4.5f, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(ring, targetPos - Main.screenPosition, null, color2 * ((chevron_alpha2 * 0.5f) + SmoothStep(0, 1, Clamp((Projectile.ai[0] - 500) / 180f, 0, 1))), 0, ring.Size() / 2, chevron_alpha * 10 + SmoothStep(0, 4, Clamp((Projectile.ai[0] - 500) / 180f, 0, 1)), SpriteEffects.None, 0);

            //Main.spriteBatch.Draw(pulse, targetPos - Main.screenPosition, null, Color.Maroon * alpha2, 0, pulse.Size() / 2, waveTimer * 2, SpriteEffects.None, 0);
            if (chevronTimer2++ % 15 == 0)
                chevronTimer++;
            for (int j = 1; j < 10; j++)
            {
                chevronAlphas[j] = Lerp(chevronAlphas[j], 0f, 0.01f);
                if (chevronAlphas[j].InRange(0, 0.05f)) chevronAlphas[j] = 0f;
                if ((chevronTimer % 20 == j) && chevronTimer2 % 15 == 0)
                {
                    chevronAlphas[j] = 0.6f;
                }
                for (int i = 0; i < 16; i++)
                {
                    float angle = Helper.CircleDividedEqually(i, 16);
                    float scaleOff = MathF.Cos((j - Main.GlobalTimeWrappedHourly) * 20) * 0.1f;
                    Vector2 offset = Vector2.Lerp(Vector2.Zero, (pulse.Height * 2) * Vector2.One, 0.01f) + ((j + 1) * 150) * Vector2.One;
                    Vector2 scale = new Vector2(Clamp(chevron_alpha2 + (float)(j - 1) / 5, 0, 1), MathF.Pow(Clamp(chevron_alpha2 + (float)(j - 1) / 5, 0, 1), 2)) * (0.5f + scaleOff);
                    Main.spriteBatch.Draw(chevron, targetPos + offset.RotatedBy(angle) - Main.screenPosition, null, color2 * chevronAlphas[j] * Lerp(1, 0, Clamp(targetPos.Distance(targetPos + offset.RotatedBy(angle)) / (pulse.Height * 2.4f), 0, 1)), angle + PiOver4, chevron.Size() / 2, 0.75f, SpriteEffects.None, 0);
                }

                for (int i = 0; i < 16; i++)
                {
                    float angle = Helper.CircleDividedEqually(i, 16) + Helper.CircleDividedEqually(1, 32);
                    float scaleOff = MathF.Cos((j - Main.GlobalTimeWrappedHourly) * 20) * 0.1f;
                    Vector2 offset = Vector2.Lerp(Vector2.Zero, (pulse.Height * 2) * Vector2.One, waveTimer2) + ((j - 1.5f) * 150) * Vector2.One;
                    Vector2 scale = new Vector2(Clamp(chevron_alpha2 + (float)(j - 1) / 5, 0, 1), MathF.Pow(Clamp(chevron_alpha2 + (float)(j - 1) / 5, 0, 1), 2)) * (0.5f + scaleOff);
                    Main.spriteBatch.Draw(chevron, targetPos + offset.RotatedBy(angle) - Main.screenPosition, null, color2 * chevron_alpha2 * Lerp(1, 0, Clamp(targetPos.Distance(targetPos + offset.RotatedBy(angle)) / (pulse.Height * 2.4f), 0, 2)), angle + PiOver4, chevron.Size() / 2, scale, SpriteEffects.None, 0);
                }
            }
        }
        string num = MathF.Round(Projectile.ai[1] / 60, 2).ToString();
        switch (num.Length)
        {
            case 1:
                num = MathF.Round(Projectile.ai[1] / 60, 2).ToString() + ".0";
                break;
        }
        string strin = num;


        Main.spriteBatch.Reload(BlendState.AlphaBlend);

        sbParams = Main.spriteBatch.Snapshot();


        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.EffectMatrix);

        Main.spriteBatch.Draw(textGlow, new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.05f), null, Color.Black * textAlpha, 0, new Vector2(textGlow.Width / 2, textGlow.Height / 2), 10, SpriteEffects.None, 0);

        Main.spriteBatch.Draw(textGlow, new Vector2(Main.screenWidth / 2, Main.screenHeight - Main.screenHeight * 0.05f), null, Color.Black * textAlpha, 0, new Vector2(textGlow.Width / 2, textGlow.Height / 2), 10, SpriteEffects.None, 0);

        for (int i = -(int)(Main.screenWidth / hazard.Width); i < (int)(Main.screenWidth / hazard.Width); i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Main.spriteBatch.Draw(hazard, (Main.rand.NextVector2Circular(50, 50) * SmoothStep(0, 1, Clamp((Projectile.ai[0] - 500) / 180f, 0, 1))) + new Vector2(Main.screenWidth / 2 + (i * hazard.Width) - Lerp(0, waveTimer, Clamp(Projectile.ai[0] / 120, 0, 1)) * hazard.Width - Main.screenWidth * hazardDistanceMult * 2, Main.screenHeight * 0.0325f), null, color * 2 * textAlpha, 0, new Vector2(hazard.Width / 2, hazard.Height / 2), 1, SpriteEffects.None, 0);
                Main.spriteBatch.Draw(hazard, (Main.rand.NextVector2Circular(50, 50) * SmoothStep(0, 1, Clamp((Projectile.ai[0] - 500) / 180f, 0, 1))) + new Vector2(Main.screenWidth / 2 + (i * hazard.Width) + Lerp(0, waveTimer, Clamp(Projectile.ai[0] / 120, 0, 1)) * hazard.Width + Main.screenWidth * hazardDistanceMult * 2, Main.screenHeight * 0.122f), null, color * 2 * textAlpha, 0, new Vector2(hazard.Width / 2, hazard.Height / 2), 1, SpriteEffects.None, 0);

                Vector2 exPos = (Main.rand.NextVector2Circular(50, 50) * SmoothStep(0, 1, Clamp((Projectile.ai[0] - 500) / 180f, 0, 1))) + new Vector2(Main.screenWidth / 2 + (i * exclamation.Width) + waveTimer * exclamation.Width * (i < 0 ? 1 : -1), Main.screenHeight * 0.078f);
                Main.spriteBatch.Draw(exclamation, exPos, null, color * 2 * textAlpha * Lerp(0, 2, Clamp(MathF.Abs(exPos.X - Main.screenWidth / 2) / 5000, 0, 1)), 0, new Vector2(exclamation.Width / 2, exclamation.Height / 2), 0.1f, SpriteEffects.None, 0);
            }
        }

        for (int i = -(int)(Main.screenWidth / hazard.Width); i < (int)(Main.screenWidth / hazard.Width); i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Main.spriteBatch.Draw(hazard, (Main.rand.NextVector2Circular(50, 50) * SmoothStep(0, 1, Clamp((Projectile.ai[0] - 500) / 180f, 0, 1))) + new Vector2(Main.screenWidth / 2 + (i * hazard.Width) + Lerp(0, waveTimer, Clamp(Projectile.ai[0] / 120, 0, 1)) * hazard.Width + Main.screenWidth * hazardDistanceMult * 2, Main.screenHeight - Main.screenHeight * 0.0325f - 100), null, color * 2 * textAlpha, 0, new Vector2(hazard.Width / 2, hazard.Height / 2), 1, SpriteEffects.None, 0);
                Main.spriteBatch.Draw(hazard, (Main.rand.NextVector2Circular(50, 50) * SmoothStep(0, 1, Clamp((Projectile.ai[0] - 500) / 180f, 0, 1))) + new Vector2(Main.screenWidth / 2 + (i * hazard.Width) - Lerp(0, waveTimer, Clamp(Projectile.ai[0] / 120, 0, 1)) * hazard.Width - Main.screenWidth * hazardDistanceMult * 2, Main.screenHeight - Main.screenHeight * 0.122f - 100), null, color * 2 * textAlpha, 0, new Vector2(hazard.Width / 2, hazard.Height / 2), 1, SpriteEffects.None, 0);

                Vector2 exPos = (Main.rand.NextVector2Circular(50, 50) * SmoothStep(0, 1, Clamp((Projectile.ai[0] - 500) / 180f, 0, 1))) + new Vector2(Main.screenWidth / 2 + (i * exclamation.Width) + waveTimer * exclamation.Width * (i < 0 ? 1 : -1), Main.screenHeight - Main.screenHeight * 0.078f - 100);
                Main.spriteBatch.Draw(exclamation, exPos, null, color * 2 * textAlpha * Lerp(0, 2, Clamp(MathF.Abs(exPos.X - Main.screenWidth / 2) / 5000, 0, 1)), 0, new Vector2(exclamation.Width / 2, exclamation.Height / 2), 0.1f, SpriteEffects.None, 0);
            }
        }
        string warningText = Language.GetText("Mods.EbonianMod.Dialogue.HotGarbageDialogue.Evacuate").Value;
        bool run = false;
        if (Main.LocalPlayer.Center.Distance(targetPos) < 4500 / 2 - 100)
        {
            if (leftAgain)
                warningText = Language.GetText("Mods.EbonianMod.Dialogue.HotGarbageDialogue.Unfunny").Value;
            else if (left)
            {
                warningText = Language.GetText("Mods.EbonianMod.Dialogue.HotGarbageDialogue.Dumbass").Value;
            }
            else if (Projectile.ai[1] < 180)
            {
                run = true;
                warningText = Language.GetText("Mods.EbonianMod.Dialogue.HotGarbageDialogue.Run").Value;
                for (int i = 0; i < (int)((Projectile.ai[0] - 480) / 4); i++)
                {
                    warningText += " " + Language.GetText("Mods.EbonianMod.Dialogue.HotGarbageDialogue.Run").Value;
                }
            }
        }
        else
        {
            if (reentered)
                warningText = Language.GetText("Mods.EbonianMod.Dialogue.HotGarbageDialogue.Phew").Value;
            else
                warningText = Language.GetText("Mods.EbonianMod.Dialogue.HotGarbageDialogue.GG").Value;

            if (Projectile.ai[1] < 120)
                warningText = Language.GetText("Mods.EbonianMod.Dialogue.HotGarbageDialogue.HereItComes").Value;
        }

        for (int j = 0; j < 2; j++)
        {
            DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.DeathText.Value, warningText, (Main.rand.NextVector2Circular(50, 50) * SmoothStep(0, 1, Clamp((Projectile.ai[0] - 500) / 180f, 0, 1))) + new Vector2(Main.screenWidth / 2 - FontAssets.DeathText.Value.MeasureString(warningText).X / 2, Main.screenHeight - Main.screenHeight * 0.1f - 100), color * textAlpha);
            if (run)
            {
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.DeathText.Value, warningText, (Main.rand.NextVector2Circular(50, 50) * SmoothStep(0, 1, Clamp((Projectile.ai[0] - 500) / 180f, 0, 1))) + new Vector2(-Main.screenWidth / 2 - FontAssets.DeathText.Value.MeasureString(warningText).X / 2, Main.screenHeight * 0.055f), color * textAlpha);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.DeathText.Value, warningText, (Main.rand.NextVector2Circular(50, 50) * SmoothStep(0, 1, Clamp((Projectile.ai[0] - 500) / 180f, 0, 1))) + new Vector2(Main.screenWidth + Main.screenWidth / 2 - FontAssets.DeathText.Value.MeasureString(warningText).X / 2, Main.screenHeight * 0.055f), color * textAlpha);
            }
            DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.DeathText.Value, extraString + strin, (Main.rand.NextVector2Circular(15, 15) * onNumberShakeMult * (numberScaleOff * 0.2f + 1)) + (Main.rand.NextVector2Circular(50, 50) * SmoothStep(0, 1, Clamp((Projectile.ai[0] - 500) / 180f, 0, 1))) + new Vector2(Main.screenWidth / 2 - FontAssets.DeathText.Value.MeasureString((extraString + "0.00").ToString()).X / 2, Main.screenHeight * 0.055f), color * textAlpha);
        }
        Main.spriteBatch.Draw(ring2, Main.ScreenSize.ToVector2() / 2 - new Vector2(0, 100 + numberScaleOff * 30) + Main.rand.NextVector2Circular(60, 60) * numberAlpha, null, Color.Maroon with { A = 0 } * numberAlpha * 0.25f, 0, ring2.Size() / 2, 2.5f, SpriteEffects.None, 0);
        Main.spriteBatch.Draw(ring2, Main.ScreenSize.ToVector2() / 2 - new Vector2(0, 100 + numberScaleOff * 30) + Main.rand.NextVector2Circular(60, 60) * numberAlpha, null, Color.Maroon with { A = 0 } * numberAlpha * 0.25f, 0, ring2.Size() / 2, 2.5f, SpriteEffects.FlipHorizontally, 0);


        DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.DeathText.Value, number.ToString(), Main.ScreenSize.ToVector2() / 2 - new Vector2(0, 100 + numberScaleOff * 30) + Main.rand.NextVector2Circular(50, 50) * numberAlpha - new Vector2(FontAssets.DeathText.Value.MeasureString((number).ToString()).X / 2 * (15 / 2 + numberScaleOff), FontAssets.DeathText.Value.MeasureString((number).ToString()).Y / 2 * (10 / 2 + numberScaleOff)), Color.Red * numberAlpha * 0.05f, 0, new Vector2(0.5f), 15 / 2 + numberScaleOff, SpriteEffects.None, 0);

        DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.DeathText.Value, number.ToString(), Main.ScreenSize.ToVector2() / 2 - new Vector2(0, 100 + numberScaleOff * 30) + Main.rand.NextVector2Circular(30, 30) * numberAlpha - new Vector2(FontAssets.DeathText.Value.MeasureString((number).ToString()).X / 2 * (15 / 4 + numberScaleOff), FontAssets.DeathText.Value.MeasureString((number).ToString()).Y / 2 * (10 / 4 + numberScaleOff)), Color.Red * numberAlpha, 0, new Vector2(0.5f), 15 / 4 + numberScaleOff, SpriteEffects.None, 0);


        Main.spriteBatch.ApplySaved(sbParams);
        Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, Color.White * alpha, Projectile.rotation, Projectile.Size / 2, Projectile.scale, SpriteEffects.None);

        return false;
    }
    string extraString;
    public override void OnSpawn(IEntitySource source)
    {
        Projectile.ai[1] = 600;
        Projectile.rotation = -Vector2.UnitY.ToRotation();
        targetPos = Projectile.Center;
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * alpha;
    }
    public override void OnKill(int timeLeft)
    {
        MusicSystem.TemporarilySetMusicTo0(600);
        GetInstance<DownedBossSystem>().downedGarbage = true;
        SoundEngine.PlaySound(Sounds.nuke);
        SoundEngine.PlaySound(Sounds.garbageDeath);
        foreach (Player player in Main.ActivePlayers)
        {
            if (player.Center.Distance(targetPos) < 4500 / 2 - 200)
                player.KillMe(PlayerDeathReason.ByCustomReason(Language.GetText("Mods.EbonianMod.DeathMessages.NukeDeath").Format(player.name)), 999999, 0);
        }

        if (Main.zenithWorld)
        {
            int k = targetPos.ToTileCoordinates().X;
            int m = targetPos.ToTileCoordinates().Y;
            int radius = 70;
            for (int i = -(radius); i < radius; i++)
            {
                for (int j = -(radius) + (int)MathF.Abs(i / 2); j < radius - (int)MathF.Abs(i / 2); j++)
                {

                    int offX = 0;
                    int offY = 0;

                    if (MathF.Abs(i) > 60)
                    {
                        offX = Main.rand.Next((int)-(MathF.Abs(i) - 60), (int)(MathF.Abs(i) - 60));
                        offY = Main.rand.Next((int)-(MathF.Abs(i) - 60), (int)(MathF.Abs(i) - 60));
                    }

                    if (MathF.Abs(j) > 60)
                    {
                        offX = Main.rand.Next((int)-(MathF.Abs(j) - 60), (int)(MathF.Abs(j) - 60));
                        offY = Main.rand.Next((int)-(MathF.Abs(j) - 60), (int)(MathF.Abs(j) - 60));
                    }
                    if (k + i + offX <= 0 || k + i + offX >= Main.maxTilesX || m + j + offY <= 0 || m + j + offY >= Main.maxTilesY) continue;
                    int type = Main.tile[i + k, j + m].TileType;
                    if (type != TileType<ArchmageStaffTile>())
                        WorldGen.ExplodeMine(i + k + offX, m + j + offY, false);
                }
            }
        }

        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.active && npc.Center.Distance(targetPos) < 4500 / 2 - 100 && (!npc.boss || npc.type == NPCID.MoonLordCore) && !npc.dontTakeDamage && npc.type != NPCType<HotGarbage>() && npc.type != NPCType<ArchmageStaffNPC>())
            {
                npc.life = 0;
                npc.checkDead();
            }
            if (npc.active && npc.type == NPCType<HotGarbage>())
            {
                npc.immortal = false;
                npc.dontTakeDamage = false;
                npc.StrikeInstantKill();
            }
        }
        OverlaySystem.FlashAlpha = 1;
    }
    float alpha = 1, numberAlpha = 0, number, numberTimer, textAlpha, chevronTimer, chevronTimer2, numberScaleOff = -1f, hazardDistanceMult = 1, onNumberShakeMult;
    float[] chevronAlphas = new float[10];
    bool changedCam, left, reentered, leftAgain;
    public override void AI()
    {
        if (Projectile.ai[0] < 60)
            hazardDistanceMult = Lerp(1, 0, (Projectile.ai[0]) / 60);
        foreach (Player player in Main.ActivePlayers)
        {
            if ((player.HeldItem.type == ItemID.MagicMirror || player.HeldItem.type == ItemID.RecallPotion) && player.itemAnimation > 2)
            {
                player.KillMe(PlayerDeathReason.ByCustomReason(Language.GetText("Mods.EbonianMod.DeathMessages.NukeTP").Format(player.name)), 12345, 0);
                Projectile.active = false;
            }
        }
        textAlpha = Lerp(textAlpha, 1, 0.15f);
        if (--numberTimer < 0)
            numberAlpha = Lerp(numberAlpha, 0, 0.15f);

        onNumberShakeMult = Lerp(onNumberShakeMult, 0, 0.15f);
        if (Projectile.ai[1] % 60 == 0 && Projectile.ai[0] > 60)
        {
            numberScaleOff += 0.5f;
            onNumberShakeMult = 1;
            if (Projectile.ai[1] < 6 * 60)
            {
                SoundEngine.PlaySound(Sounds.buzz.WithPitchOffset((numberScaleOff + 1) * 0.04f));
                Helper.AddCameraModifier(new PunchCameraModifier(targetPos, Helper.FromAToB(targetPos, Main.LocalPlayer.Center), 23, 10, 60, 2000));
                numberAlpha = 1;
                number = Projectile.ai[1] / 60;
                numberTimer = 20;
            }
        }

        if (Projectile.ai[1] < 180 && Projectile.ai[1] > 60 && !changedCam && Main.LocalPlayer.Center.Distance(targetPos) > 4500 / 2)
        {
            CameraSystem.ChangeCameraPos(targetPos, (int)Projectile.ai[1] + 40, null, easingFunction: InOutCirc);
            changedCam = true;
        }
        if (Main.LocalPlayer.Center.Distance(targetPos) > 4500 / 2 - 100)
        {
            if (reentered) leftAgain = true;
            left = true;
        }
        else if (left)
            reentered = true;
        if (Projectile.ai[2] < 1f)
            Projectile.ai[2] += 0.05f;
        if (alpha < 0.1f)
            for (int i = 0; i < Projectile.oldPos.Length; i++)
                Projectile.oldPos[i] = Projectile.position;

        if (Projectile.ai[1] > 0 && Projectile.ai[0] > 50)
            Projectile.ai[1]--;
        if (Projectile.ai[1] <= 0 && Projectile.ai[0] > 50)
        {
            Projectile.Kill();
        }
        extraString = Language.GetText("Mods.EbonianMod.Dialogue.HotGarbageDialogue.Nuke") + ": ";
        Projectile.timeLeft = 10;
        float _alpha = Utils.GetLerpValue(0, 2, waveTimer);
        float alpha2 = Clamp((float)Math.Sin(_alpha * Math.PI) * 3, 0, 1f);

        Projectile.ai[0]++;
        if (Projectile.ai[0] < 50)
        {
            Projectile.rotation = -Vector2.UnitY.ToRotation();
            Projectile.velocity.Y -= 0.5f + Projectile.velocity.Y * 0.01f;
        }
        else if (Projectile.ai[0] > 50 && Projectile.ai[0] < 540)
        {
            alpha = Lerp(alpha, 0, 0.1f);
            Projectile.velocity *= 0.9f;
        }
        else if (Projectile.ai[0] > 540)
        {
            if (Projectile.ai[0] == 481)
                Projectile.Center = targetPos - new Vector2(0, 800);
            alpha = Lerp(alpha, 1, 0.1f);
            Projectile.velocity.Y += 0.3f;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
    }
}