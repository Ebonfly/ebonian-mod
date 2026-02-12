using System.Collections.Generic;

namespace EbonianMod.Content.Projectiles.Cecitior;

public class CIchor : ModProjectile
{
    public override string Texture => Helper.Empty;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 30;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }
    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(BuffID.Ichor, Main.rand.Next(100, 300));
    }
    float vfxOffset;
    public override bool PreDraw(ref Color lightColor)
    {
        var fadeMult = Helper.SafeDivision(1f / Projectile.oldPos.Length);
        Main.spriteBatch.Reload(BlendState.Additive);
        float alpha = 1f;
        vfxOffset -= 0.015f;
        if (vfxOffset <= 0)
            vfxOffset = 1;
        vfxOffset = MathHelper.Clamp(vfxOffset, float.Epsilon, 1 - float.Epsilon);
        List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
        float s = 0;
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            float mult = (1f - fadeMult * i);

            if (mult < 0.5f)
                s = MathHelper.Clamp(mult * 3.5f, 0, 0.5f) * 3;
            else
                s = MathHelper.Clamp((-mult + 1) * 2, 0, 0.5f) * 3;

            if (i > 0 && Projectile.oldPos[i] != Vector2.Zero)
            {
                Color col = Color.Gold * mult * 2 * s;

                float __off = vfxOffset;
                if (__off > 1) __off = -__off + 1;
                float _off = __off + mult;
                vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(50 * mult, 0).RotatedBy(Helper.FromAToB(Projectile.oldPos[i - 1], Projectile.oldPos[i]).ToRotation() + MathHelper.PiOver2), col, new Vector2(_off, 0)));
                vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(50 * mult, 0).RotatedBy(Helper.FromAToB(Projectile.oldPos[i - 1], Projectile.oldPos[i]).ToRotation() - MathHelper.PiOver2), col, new Vector2(_off, 1)));
            }
        }
        SpritebatchParameters sbParams = Main.spriteBatch.Snapshot();
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        if (vertices.Count > 2)
        {
            Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Assets.Extras.laser2.Value, false);
        }
        Main.spriteBatch.ApplySaved(sbParams);
        Main.spriteBatch.Reload(BlendState.AlphaBlend);
        return false;
    }
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.GoldenShowerHostile);
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        AIType = ProjectileID.GoldenShowerHostile;
    }
}
