using EbonianMod.Content.Dusts;
using System;
using System.Collections.Generic;
using System.IO;

namespace EbonianMod.Content.Projectiles.ArchmageX;

public class XLargeAmethyst : ModProjectile
{
    public override string Texture => Helper.AssetPath + "Projectiles/ArchmageX/"+Name;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 90;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = true;
        Projectile.aiStyle = 0;
        Projectile.timeLeft = 250;
        Projectile.Size = new(28, 28);
    }
    public override bool? CanDamage() => false;
    public override bool PreKill(int timeLeft)
    {
        int i = 0;
        foreach (Vector2 pos in Projectile.oldPos)
        {
            var fadeMult = Helper.SafeDivision(1f / Projectile.oldPos.Length);
            float mult = (1f - fadeMult * i);
            Dust.NewDustPerfect(pos + Projectile.Size / 2 + (Projectile.Size / 4).RotatedBy((Main.GameUpdateCount + i * 4) * 0.03f), DustType<SparkleDust>(), Main.rand.NextVector2Circular(3, 3), 0, Color.Indigo, Main.rand.NextFloat(0.05f, 0.175f) * mult);
            Dust.NewDustPerfect(pos + Projectile.Size / 2 + (Projectile.Size / 4).RotatedBy((Main.GameUpdateCount + i * 4) * 0.03f), DustType<SparkleDust>(), Vector2.Zero, 0, Color.Indigo, Main.rand.NextFloat(0.05f, 0.15f) * mult);

            Dust.NewDustPerfect(pos + Projectile.Size / 2 - (Projectile.Size / 4).RotatedBy((Main.GameUpdateCount + i * 4) * 0.03f), DustType<SparkleDust>(), Main.rand.NextVector2Circular(3, 3), 0, Color.Indigo, Main.rand.NextFloat(0.05f, 0.175f) * mult);
            Dust.NewDustPerfect(pos + Projectile.Size / 2 - (Projectile.Size / 4).RotatedBy((Main.GameUpdateCount + i * 4) * 0.03f), DustType<SparkleDust>(), Vector2.Zero, 0, Color.Indigo, Main.rand.NextFloat(0.05f, 0.15f) * mult);
            i++;
        }
        SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
        for (int h = 0; h < (Projectile.ai[2] == 0 ? 5 : 3); h++)
        {
            MPUtils.NewProjectile(null, Projectile.Center, Main.rand.NextVector2Circular(5, 5), ProjectileType<XAmethystShard>(), Projectile.damage, 0);
        }

        MPUtils.NewProjectile(null, Projectile.Center, Vector2.Zero, ProjectileType<XExplosion>(), 0, 0);
        return true;
    }
    float vfxOffset;
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Texture2D glow = Helper.GetTexture(Texture + "_Glow").Value;

        var fadeMult = Helper.SafeDivision(1f / Projectile.oldPos.Length);
        vfxOffset -= 0.015f;
        if (vfxOffset <= 0)
            vfxOffset = 1;
        vfxOffset = MathHelper.Clamp(vfxOffset, float.Epsilon, 1 - float.Epsilon);
        List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
        List<VertexPositionColorTexture> vertices2 = new List<VertexPositionColorTexture>();
        float s = 0;
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            float mult = (1f - fadeMult * i);

            if (mult < 0.5f)
                s = MathHelper.Clamp(mult * 3.5f, 0, 0.5f) * 3;
            else
                s = MathHelper.Clamp((-mult + 1) * 2, 0, 0.5f) * 3;

            if (i > 0 && Projectile.oldPos[i] != Vector2.Zero && Projectile.oldPos[i] != Projectile.position)
            {
                Color col = Color.Lerp(Color.Indigo * 0.5f, Color.Gray, (float)(i / Projectile.oldPos.Length)) * 2;

                float __off = vfxOffset;
                if (__off > 1) __off = -__off + 1;
                float _off = __off + mult;
                vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 + (Projectile.Size / 4).RotatedBy((Main.GameUpdateCount + i * 4) * 0.03f) - Main.screenPosition + new Vector2(20 * mult * s, 0).RotatedBy(Projectile.velocity.ToRotation() - MathHelper.PiOver2), col, new Vector2(_off, 1)));
                vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 + (Projectile.Size / 4).RotatedBy((Main.GameUpdateCount + i * 4) * 0.03f) - Main.screenPosition + new Vector2(20 * mult * s, 0).RotatedBy(Projectile.velocity.ToRotation() + MathHelper.PiOver2), col, new Vector2(_off, 0)));

                vertices2.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 - (Projectile.Size / 4).RotatedBy((Main.GameUpdateCount - i * 4) * 0.03f) - Main.screenPosition + new Vector2(20 * mult * s, 0).RotatedBy(Projectile.velocity.ToRotation() - MathHelper.PiOver2), col, new Vector2(_off, 1)));
                vertices2.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 - (Projectile.Size / 4).RotatedBy((Main.GameUpdateCount - i * 4) * 0.03f) - Main.screenPosition + new Vector2(20 * mult * s, 0).RotatedBy(Projectile.velocity.ToRotation() + MathHelper.PiOver2), col, new Vector2(_off, 0)));
            }
        }
        SpritebatchParameters sbParams = Main.spriteBatch.Snapshot();
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        if (vertices.Count > 2 && vertices2.Count > 2)
        {
            Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Assets.Extras.wavyLaser, false);
            Helper.DrawTexturedPrimitives(vertices2.ToArray(), PrimitiveType.TriangleStrip, Assets.Extras.wavyLaser, false);
        }
        Main.spriteBatch.ApplySaved(sbParams);
        for (int i = 0; i < 2; i++)
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 }, Projectile.rotation, glow.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, Projectile.Size / 2, Projectile.scale, SpriteEffects.None, 0);

        return false;
    }
    public Vector2 p;
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.WriteVector2(p);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        p = reader.ReadVector2();
    }
    public override void AI()
    {
        if (Projectile.timeLeft > 70)
        {
            p = Projectile.Center;
            foreach (Player pl in Main.ActivePlayers)
            {
                if (Projectile.Distance(pl.Center) < 90)
                {
                    Projectile.timeLeft = 70;
                    Projectile.netUpdate = true;
                }
            }
            Player player = Main.player[(int)Projectile.ai[0]];
            float progress = Utils.GetLerpValue(0, 250, Projectile.timeLeft);
            float vel = MathHelper.Clamp((float)Math.Sin(progress * Math.PI) * 3f, 0, 2);
            Projectile.velocity = Helper.FromAToB(Projectile.Center, player.Center + Helper.FromAToB(player.Center, Projectile.Center) * 50) * 3 * vel;
        }
        else if (Projectile.timeLeft <= 70 && Projectile.timeLeft > 50)
        {
            Projectile.velocity *= 0.9f;
            if (p == Vector2.Zero)
            {
                p = Projectile.Center;
                Projectile.netUpdate = true;
            }
        }
        if (Projectile.timeLeft <= 50 && p.Distance(Projectile.Center) < 40)
        {
            Projectile.ai[1] += 0.1f;
            Projectile.Center = p + Main.rand.NextVector2Circular(Projectile.ai[1], Projectile.ai[1]);
        }
    }
}
public class XLargeAmethystAlt : XLargeAmethyst
{
    public override string Texture => Helper.AssetPath+"Projectiles/ArchmageX/XLargeAmethyst";
    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.timeLeft = 130;
    }
    public override void AI()
    {
        if (Projectile.timeLeft > 70)
        {
            p = Projectile.Center;
            Projectile.velocity = Projectile.velocity.RotatedBy(ToRadians(2)) * 1.05f;
        }
        else if (Projectile.timeLeft <= 70 && Projectile.timeLeft > 50)
        {
            Projectile.velocity *= 0.9f;
            if (p == Vector2.Zero)
            {
                p = Projectile.Center;
                Projectile.netUpdate = true;
            }
        }
        if (Projectile.timeLeft <= 50 && p.Distance(Projectile.Center) < 40)
        {
            Projectile.ai[1] += 0.1f;
            Projectile.Center = p + Main.rand.NextVector2Circular(Projectile.ai[1], Projectile.ai[1]);
        }
    }
}
