using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbonianMod.Content.Projectiles.Friendly.Corruption;
public class TerrorArmorRay : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1000;
        ProjectileID.Sets.TrailCacheLength[Type] = 20;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(BuffID.CursedInferno, Main.rand.Next(20, 100));
    }

    float vfxOffset;
    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.Reload(BlendState.Additive);
        float alpha = 1f;
        var fadeMult = Helper.SafeDivision(1f / Projectile.oldPos.Length);
        vfxOffset -= 0.015f;
        if (vfxOffset <= 0)
            vfxOffset = 1;
        vfxOffset = Clamp(vfxOffset, float.Epsilon, 1 - float.Epsilon);
        List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
        float s = 0;
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            float mult = (1f - fadeMult * i);

            if (mult < 0.5f)
                s = Clamp(mult * 3.5f, 0, 0.5f) * 3;
            else
                s = Clamp((-mult + 1) * 2, 0, 0.5f) * 3;

            if (i > 0 && Projectile.oldPos[i] != Vector2.Zero)
            {
                Color col = Color.LawnGreen * MathF.Pow(mult, 2) * 2 * s * Projectile.velocity.Length().Saturate();

                float __off = vfxOffset;
                if (__off > 1) __off = -__off + 1;
                float _off = __off + mult;
                vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(50, 0).RotatedBy(Helper.FromAToB(Projectile.oldPos[i - 1], Projectile.oldPos[i]).ToRotation() + PiOver2), col, new Vector2(_off, 0)));
                vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(50, 0).RotatedBy(Helper.FromAToB(Projectile.oldPos[i - 1], Projectile.oldPos[i]).ToRotation() - PiOver2), col, new Vector2(_off, 1)));
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
    public override string Texture => Helper.Empty;
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(95);
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.tileCollide = false;
        AIType = -1;
        Projectile.extraUpdates = 1;
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 200;
    }
    public override void AI()
    {
        if (Projectile.timeLeft < 170)
            Projectile.velocity *= 0.99f;
    }
}
