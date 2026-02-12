using EbonianMod.Content.Projectiles.VFXProjectiles;
using System.Collections.Generic;

namespace EbonianMod.Content.Projectiles.Friendly.Corruption;

public class EbonianRocket : ModProjectile
{
    public override string Texture => Helper.AssetPath + "Projectiles/Friendly/Corruption/" + Name;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 30;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(36, 30);
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.timeLeft = 200;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
    }
    float vfxOffset;
    public override bool PreDraw(ref Color lightColor)
    {
        if (Projectile.ai[0] > 40)
        {
            var fadeMult = Helper.SafeDivision(1f / Projectile.oldPos.Length);
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
                    Color col = new Color(147, 255, 36, 255);

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
                Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Assets.Extras.FlamesSeamless.Value, false);
            }
            Main.spriteBatch.ApplySaved(sbParams);

        }
        bool shouldDraw = (Projectile.ai[1] == 0);
        return shouldDraw;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.velocity = (Projectile.rotation + Main.rand.NextFloat(-PiOver2, PiOver2)).ToRotationVector2() * Main.rand.NextFloat(6, 16);
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (Projectile.ai[1] != 2)
            Projectile.ai[1] = 1;
        return false;
    }
    public override void OnKill(int timeLeft)
    {
        if (Projectile.ai[1] == 0 && Projectile.owner == Main.myPlayer)
        {
            Projectile a = Projectile.NewProjectileDirect(Projectile.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ProjectileType<GreenShockwave>(), Projectile.damage, 0, Projectile.owner);
            a.hostile = false;
            a.friendly = true;
            a.SyncProjectile();
            Helper.DustExplosion(Projectile.Center, Vector2.One, 0, new Color(0.576f, 1f, 0.141f, 0.02f), true, true, increment: 0.03f, MinMulti: 0.6f, MaxMulti: 1.5f);
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hitinfo, int damage)
    {
        if (Projectile.ai[1] != 2)
            Projectile.ai[1] = 1;
    }
    public override void AI()
    {
        if ((int)Projectile.ai[1] == 1)
        {
            Projectile.timeLeft = 45;
            Projectile.velocity *= 0;
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile a = Projectile.NewProjectileDirect(Projectile.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ProjectileType<GreenShockwave>(), Projectile.damage, 0, Projectile.owner);
                a.hostile = false;
                a.friendly = true;
                a.SyncProjectile();
            }
            Helper.DustExplosion(Projectile.Center, Vector2.One, 0, new Color(0.576f, 1f, 0.141f, 0.02f), true, true, increment: 0.03f, MinMulti: 0.6f, MaxMulti: 1.5f);
            Projectile.ai[1] = 2;
            Projectile.netUpdate = true;
        }
        Projectile.ai[0]++;
        if (Projectile.ai[0] < 2)
            Projectile.rotation = Projectile.velocity.ToRotation();
        if (Projectile.ai[0] > 40)
        {
            if ((int)Projectile.ai[0] == 41)
            {
                if (Main.myPlayer == Projectile.owner)
                    Projectile.velocity = Projectile.rotation.ToRotationVector2();
                Projectile.netUpdate = true;
            }
            if ((int)Projectile.ai[1] == 2)
            {
                Projectile.damage = 0;
                return;
            }
            else
            {
                for (int i = 0; i < 2; i++)
                    Dust.NewDustPerfect(Projectile.Center - Projectile.rotation.ToRotationVector2() * 20, DustID.CursedTorch, (Projectile.rotation + Main.rand.NextFloat(PiOver4, -PiOver4)).ToRotationVector2() * 3, 150, Scale: Main.rand.NextFloat(1, 3)).noGravity = true;
                if (Projectile.ai[0] < 80)
                    Projectile.velocity *= 1.1f;
                Projectile.netUpdate = true;
            }
            if (Projectile.timeLeft < 45)
            {
                Projectile.velocity *= 0.9f;
                Projectile.netUpdate = true;
            }
        }
        else
        {
            Projectile.velocity *= 0.86f;
            if (Main.myPlayer == Projectile.owner)
                Projectile.rotation = Helper.FromAToB(Projectile.Center, Main.MouseWorld).ToRotation();
            Projectile.netUpdate = true;
        }
    }
}