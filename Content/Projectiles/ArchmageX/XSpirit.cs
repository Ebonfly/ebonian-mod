using EbonianMod.Content.Dusts;
using System;
using System.Collections.Generic;

namespace EbonianMod.Content.Projectiles.ArchmageX;

public class XSpirit : ModProjectile
{
    public override string Texture => Helper.AssetPath + "Projectiles/ArchmageX/"+Name;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 30;
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
    }
    public override void SetDefaults()
    {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = true;
        Projectile.aiStyle = 0;
        Projectile.timeLeft = 500 * 5;
        Projectile.hide = true;
        Projectile.Size = new(34, 38);
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCs.Add(index);
    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox.Width = 30;
        hitbox.Height = 30;
    }
    public override bool PreKill(int timeLeft)
    {
        int i = 0;
        foreach (Vector2 pos in Projectile.oldPos)
        {
            var fadeMult = Helper.SafeDivision(1f / Projectile.oldPos.Length);
            float mult = (1f - fadeMult * i);
            Dust.NewDustPerfect(pos + Projectile.Size / 2, DustType<GenericAdditiveDust>(), Main.rand.NextVector2Circular(3, 3), 0, Color.Indigo, Main.rand.NextFloat(0.02f, 0.075f) * mult);
            Dust.NewDustPerfect(pos + Projectile.Size / 2, DustType<SparkleDust>(), Main.rand.NextVector2Circular(3, 3), 0, Color.Indigo, Main.rand.NextFloat(0.05f, 0.175f) * mult);
            Dust.NewDustPerfect(pos + Projectile.Size / 2, DustType<SparkleDust>(), Vector2.Zero, 0, Color.Indigo, Main.rand.NextFloat(0.05f, 0.15f) * mult);
            i++;
        }
        MPUtils.NewProjectile(null, Projectile.Center, Vector2.Zero, ProjectileType<XExplosion>(), 0, 0);
        return base.PreKill(timeLeft);
    }
    float vfxOffset;
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Texture2D glow = Helper.GetTexture(Texture + "_Glow").Value;
        Texture2D fireball = Assets.Extras.fireball.Value;
        vfxOffset -= 0.015f;
        if (vfxOffset <= 0)
            vfxOffset = 1;
        vfxOffset = MathHelper.Clamp(vfxOffset, float.Epsilon, 1 - float.Epsilon);
        var fadeMult = Helper.SafeDivision(1f / Projectile.oldPos.Length);
        List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            float mult = (1f - fadeMult * i);
            if (i > 0 && Projectile.oldPos[i] != Vector2.Zero)
            {
                Color col = Color.Lerp(Color.Indigo * 0.5f, Color.Gray, (float)(i / Projectile.oldPos.Length)) * 2;

                float __off = vfxOffset;
                if (__off > 1) __off = -__off + 1;
                float _off = __off + mult;
                vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(20 * mult, 0).RotatedBy(Helper.FromAToB(Projectile.oldPos[i - 1], Projectile.oldPos[i]).ToRotation() + MathHelper.PiOver2), col, new Vector2(_off, 0)));
                vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(20 * mult, 0).RotatedBy(Helper.FromAToB(Projectile.oldPos[i - 1], Projectile.oldPos[i]).ToRotation() - MathHelper.PiOver2), col, new Vector2(_off, 1)));
            }
        }
        SpritebatchParameters sbParams = Main.spriteBatch.Snapshot();
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        if (vertices.Count > 2)
        {
            Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Assets.Extras.wavyLaser2, false);
        }
        Main.spriteBatch.ApplySaved(sbParams);
        Main.EntitySpriteDraw(fireball, Projectile.Center - Main.screenPosition, null, Color.Indigo with { A = 0 } * 0.5f, Projectile.rotation + MathHelper.PiOver2, new Vector2(fireball.Width / 2, fireball.Height / 4), 1.2f + (((MathF.Sin(Main.GlobalTimeWrappedHourly * 5) + 1) / 2) * 0.4f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(fireball, Projectile.Center - Main.screenPosition, null, Color.Indigo with { A = 0 } * 0.5f, Projectile.rotation + MathHelper.PiOver2, new Vector2(fireball.Width / 2, fireball.Height / 4), 1.2f, SpriteEffects.None, 0);
        //Main.spriteBatch.Reload(BlendState.AlphaBlend);
        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, Projectile.Size / 2, Projectile.scale, SpriteEffects.None, 0);
        //Main.spriteBatch.Reload(BlendState.Additive);
        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * 0.5f, Projectile.rotation, glow.Size() / 2, Projectile.scale, SpriteEffects.None, 0);


        return false;
    }
    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, TorchID.Purple);
        Player player = Main.player[(int)Projectile.ai[0]];
        if (Projectile.timeLeft % 2 == 0)
            Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, Helper.FromAToB(Projectile.Center, player.Center), 0.35f).SafeNormalize(Vector2.UnitY) * 5.5f;
        Projectile.rotation = Projectile.velocity.ToRotation();
    }
}
public class XSpiritNoHome : ModProjectile
{
    public override string Texture => Helper.AssetPath+"Projectiles/ArchmageX/XSpirit";
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 30;
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
    }
    public override void SetDefaults()
    {
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = true;
        Projectile.aiStyle = 0;
        Projectile.timeLeft = 500 * 5;
        Projectile.hide = true;
        Projectile.Size = new(34, 38);
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCs.Add(index);
    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox.Width = 30;
        hitbox.Height = 30;
    }
    public override bool PreKill(int timeLeft)
    {
        int i = 0;
        foreach (Vector2 pos in Projectile.oldPos)
        {
            var fadeMult = Helper.SafeDivision(1f / Projectile.oldPos.Length);
            float mult = (1f - fadeMult * i);
            Dust.NewDustPerfect(pos + Projectile.Size / 2, DustType<GenericAdditiveDust>(), Main.rand.NextVector2Circular(3, 3), 0, Color.Indigo, Main.rand.NextFloat(0.02f, 0.075f) * mult);
            Dust.NewDustPerfect(pos + Projectile.Size / 2, DustType<SparkleDust>(), Main.rand.NextVector2Circular(3, 3), 0, Color.Indigo, Main.rand.NextFloat(0.05f, 0.175f) * mult);
            Dust.NewDustPerfect(pos + Projectile.Size / 2, DustType<SparkleDust>(), Vector2.Zero, 0, Color.Indigo, Main.rand.NextFloat(0.05f, 0.15f) * mult);
            i++;
        }
        MPUtils.NewProjectile(null, Projectile.Center, Vector2.Zero, ProjectileType<XExplosion>(), 0, 0);
        return base.PreKill(timeLeft);
    }
    float vfxOffset;
    public override bool PreDraw(ref Color lightColor)
    {
        float finalAlpha = 1 - Projectile.ai[1];
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Texture2D glow = Helper.GetTexture(Texture + "_Glow").Value;
        Texture2D fireball = Assets.Extras.fireball.Value;
        vfxOffset += 0.05f;
        if (vfxOffset >= 1)
            vfxOffset = 0;
        vfxOffset = MathHelper.Clamp(vfxOffset, float.Epsilon, 1 - float.Epsilon);
        var fadeMult = Helper.SafeDivision(1f / Projectile.oldPos.Length);
        List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            float mult = (1f - fadeMult * i);
            if (i > 0 && Projectile.oldPos[i] != Vector2.Zero)
            {
                Color col = Color.Lerp(Color.Indigo * 0.5f, Color.Gray, (float)(i / Projectile.oldPos.Length)) * 2 * finalAlpha;

                float __off = vfxOffset;
                if (__off > 1) __off = -__off + 1;
                float _off = __off + mult;
                vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(25 * mult, 0).RotatedBy(Helper.FromAToB(Projectile.oldPos[i - 1], Projectile.oldPos[i]).ToRotation() + MathHelper.PiOver2), col, new Vector2(_off, 0)));
                vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(25 * mult, 0).RotatedBy(Helper.FromAToB(Projectile.oldPos[i - 1], Projectile.oldPos[i]).ToRotation() - MathHelper.PiOver2), col, new Vector2(_off, 1)));
            }
        }
        SpritebatchParameters sbParams = Main.spriteBatch.Snapshot();
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        if (vertices.Count > 2)
        {
            Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Assets.Extras.LintyTrail.Value, false);
        }
        Main.spriteBatch.ApplySaved(sbParams);
        Main.EntitySpriteDraw(fireball, Projectile.Center - Main.screenPosition, null, Color.Indigo with { A = 0 } * 0.5f * finalAlpha, Projectile.velocity.ToRotation() + MathHelper.PiOver2, new Vector2(fireball.Width / 2, fireball.Height / 4), 1.2f + (((MathF.Sin(Main.GlobalTimeWrappedHourly * 5) + 1) / 2) * 0.4f), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(fireball, Projectile.Center - Main.screenPosition, null, Color.Indigo with { A = 0 } * 0.5f * finalAlpha, Projectile.velocity.ToRotation() + MathHelper.PiOver2, new Vector2(fireball.Width / 2, fireball.Height / 4), 1.2f, SpriteEffects.None, 0);
        //Main.spriteBatch.Reload(BlendState.AlphaBlend);
        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * finalAlpha, Projectile.rotation, Projectile.Size / 2, Projectile.scale, SpriteEffects.None, 0);
        //Main.spriteBatch.Reload(BlendState.Additive);
        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * 0.5f * finalAlpha, Projectile.rotation, glow.Size() / 2, Projectile.scale, SpriteEffects.None, 0);


        return false;
    }
    Vector2 initP, initV;
    public override bool? CanDamage() => !(Projectile.ai[2] < 0 && Projectile.ai[1] > 0.8f && Projectile.ai[0] == 0);
    public override void AI()
    {
        if (initV == Vector2.Zero)
        {
            initP = Projectile.Center;
            initV = Projectile.velocity;
        }


        if ((Projectile.ai[0] == 0 && Projectile.ai[1] == 0) || Projectile.ai[2] < 0)
        {
            Projectile.tileCollide = false;
            Projectile.ai[2] = -1;
            Projectile.ai[1] = Lerp(Projectile.ai[1], 1, 0.01f);
            if (Projectile.velocity.Length() < 20f)
                Projectile.velocity *= 1.0075f;
        }
        else
        {
            Player player = Main.player[Projectile.owner];
            Projectile.SineMovement(initP, initV, 0.25f, 40);
        }

        if (Projectile.oldPos[1] != Vector2.Zero)
            Projectile.rotation = Helper.FromAToB(Projectile.oldPos[1], Projectile.position).ToRotation();
        else
            Projectile.rotation = Projectile.velocity.ToRotation();
    }
}
