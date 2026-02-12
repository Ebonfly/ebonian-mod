using EbonianMod.Content.Dusts;
using EbonianMod.Content.Projectiles.VFXProjectiles;
using System;
using System.Collections.Generic;
using Terraria.Graphics.CameraModifiers;

namespace EbonianMod.Content.Projectiles.Conglomerate;

public class CGhostCeci : ModProjectile
{
    public override string Texture => Helper.AssetPath + "Projectiles/Conglomerate/"+Name;
    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 6;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.width = 68;
        Projectile.height = 68;
        Projectile.aiStyle = 0;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 150;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (Projectile.ai[2] == 1)
        {
            Texture2D tex = Helper.GetTexture(Helper.AssetPath+"Projectiles/Conglomerate/CGore" + new UnifiedRandom(Projectile.whoAmI).Next(3)).Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation + PiOver2, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
        }
        else
        {
            var fadeMult = Helper.SafeDivision(1f / Projectile.oldPos.Length);
            Texture2D tex = TextureAssets.Gore[Find<ModGore>("EbonianMod/Cecitior2").Type].Value;
            if (Projectile.frame == 1)
                tex = TextureAssets.Gore[Find<ModGore>("EbonianMod/Terrortoma5").Type].Value;

            float rotOff = Pi;
            if (Projectile.frame == 0)
                rotOff = PiOver2;
            Vector2 origin = tex.Size() / 2;
            Main.spriteBatch.Reload(BlendState.Additive);
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float mult = InOutCubic.Invoke(1f - fadeMult * i);
                if (i > 0)
                    for (float j = 0; j < 5; j++)
                    {
                        Vector2 pos = Vector2.Lerp(Projectile.oldPos[i], Projectile.oldPos[i - 1], (float)(j / 5));
                        Main.spriteBatch.Draw(tex, pos + Projectile.Size / 2 - Main.screenPosition, null, Color.White * mult * mult * 0.2f * alpha, Projectile.rotation + rotOff, origin, Projectile.scale, SpriteEffects.None, 0);
                    }
            }
            Main.spriteBatch.Reload(BlendState.AlphaBlend);

            /*if (Projectile.velocity.Length() > 10)
                EbonianMod.blurDrawCache.Add(() =>
                {
                    //for (int i = 0; i < 5; i++)
                    Main.spriteBatch.Draw(Assets.Extras.cone2.Value_blur, Projectile.Center - Main.screenPosition, null, Color.White * Lerp(0, 1, Projectile.velocity.Length() / 20), Projectile.rotation - PiOver2, new Vector2(0, Assets.Extras.cone2.Value.Height / 2), new Vector2(3, 1.5f), SpriteEffects.None, 0);

                    Main.spriteBatch.Draw(Assets.Extras.seamlessNoise3, Projectile.Center - Main.screenPosition, null, Color.White * Lerp(0, 1, Projectile.velocity.Length() / 20) * 0.2f, Main.GameUpdateCount, Assets.Extras.seamlessNoise3.Size() / 2, .3f, SpriteEffects.None, 0);
                    Main.spriteBatch.Draw(Assets.Extras.seamlessNoise3, Projectile.Center - Main.screenPosition, null, Color.White * Lerp(0, 1, Projectile.velocity.Length() / 20) * 0.2f, Main.GameUpdateCount * 0.3f, Assets.Extras.seamlessNoise3.Size() / 2, .3f, SpriteEffects.None, 0);
                });*/

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * alpha * 0.2f, Projectile.rotation + rotOff, origin, Projectile.scale, SpriteEffects.None, 0);
        }
        return false;
    }
    public override Color? GetAlpha(Color lightColor) => Color.White * alpha;
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        //overWiresUI.Add(index);
    }
    public override void OnSpawn(IEntitySource source)
    {
        Projectile.frame = Main.rand.Next(2);
    }
    float alpha;
    public override void AI()
    {
        if (Projectile.timeLeft > 40)
            alpha = Lerp(alpha, 1, 0.05f);
        if (Projectile.timeLeft < 40)
            alpha = Lerp(alpha, 0, 0.1f);

        if (Projectile.ai[0] > 0 && Projectile.ai[0] < 3)
        {
            Projectile.frame = (int)Projectile.ai[0] - 1;
        }

        switch (Projectile.ai[2])
        {
            case 0:
                {
                    if (Projectile.timeLeft < 100 && Projectile.velocity.Length() > 10)
                    {
                        for (int i = 0; i < 4; i++)
                            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<BlurDust>(), Projectile.velocity.X * Main.rand.NextFloat(0.5f, 1), Projectile.velocity.Y * Main.rand.NextFloat(0.5f, 1), newColor: Color.White * Lerp(0, 1, ((Projectile.velocity.Length() - 10f) / 20f)), Scale: Main.rand.NextFloat(0.1f, 0.2f));
                    }
                    if (Projectile.velocity.Length() > 5 && Projectile.timeLeft < 100)
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<LineDustFollowPoint>(), Projectile.velocity.X * Main.rand.NextFloat(0.5f, 1), Projectile.velocity.Y * Main.rand.NextFloat(0.5f, 1), newColor: Color.Lerp(Color.Maroon, Color.LawnGreen, Projectile.frame) * Lerp(0, 0.3f, ((Projectile.velocity.Length() - 5f) / 40f)), Scale: Main.rand.NextFloat(0.1f, 0.2f));

                    Projectile.rotation = Projectile.velocity.ToRotation() - PiOver2;
                    if (Projectile.timeLeft > 140 && Projectile.velocity.Length() > 0.05f)
                        Projectile.velocity *= 0.8f;
                    else if (Projectile.timeLeft < 100 && Projectile.velocity.Length() < 40)
                    {
                        Projectile.velocity = Projectile.velocity.RotatedBy(ToRadians(Projectile.ai[1])) * 1.1f;
                        Projectile.ai[1] = Lerp(Projectile.ai[1], 0, 0.05f);
                    }
                    else
                        Projectile.velocity = Projectile.velocity.RotatedBy(ToRadians(Projectile.ai[1]));
                }
                break;
            case 1:
                {
                    if (Projectile.timeLeft < 100 && Projectile.velocity.Length() > 10)
                    {
                        for (int i = 0; i < 4; i++)
                            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<BlurDust>(), Projectile.velocity.X * Main.rand.NextFloat(0.5f, 1), Projectile.velocity.Y * Main.rand.NextFloat(0.5f, 1), newColor: Color.White * Lerp(0, 1, ((Projectile.velocity.Length() - 10f) / 20f)), Scale: Main.rand.NextFloat(0.1f, 0.2f));
                    }
                    if (Projectile.velocity.Length() > 5 && Projectile.timeLeft < 100)
                        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<LineDustFollowPoint>(), Projectile.velocity.X * Main.rand.NextFloat(0.5f, 1), Projectile.velocity.Y * Main.rand.NextFloat(0.5f, 1), newColor: Color.Lerp(Color.Maroon, Color.LawnGreen, Projectile.frame) * Lerp(0, 0.3f, ((Projectile.velocity.Length() - 5f) / 40f)), Scale: Main.rand.NextFloat(0.1f, 0.2f));

                    Projectile.rotation = Projectile.velocity.ToRotation() - PiOver2;
                    if (Projectile.timeLeft > 140 && Projectile.velocity.Length() > 0.05f)
                        Projectile.velocity *= 0.8f;
                    else if (Projectile.timeLeft < 100 && Projectile.velocity.Length() < 40)
                        Projectile.velocity *= 1.1f;
                    if (Projectile.ai[1] != 0)
                        foreach (Projectile p in Main.ActiveProjectiles)
                        {
                            if (p.type == Type && p.whoAmI != Projectile.whoAmI && p.ai[1] == Projectile.ai[1])
                                if (p.Distance(Projectile.Center) < 40)
                                {
                                    PunchCameraModifier modifier = new PunchCameraModifier(Projectile.Center, (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2(), 10f, 5f, 100, 1000f, FullName);
                                    Main.instance.CameraModifiers.Add(modifier);
                                    Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ProjectileType<BloodExplosionWSprite>(), 0, 0);
                                    Projectile.NewProjectile(null, p.Center, Vector2.Zero, ProjectileType<BloodExplosionWSprite>(), 0, 0);
                                    p.Kill();
                                    Projectile.Kill();
                                }
                        }
                }
                break;
        }
    }

}
