using System.Collections.Generic;

namespace EbonianMod.Content.Projectiles.Enemy.Snow;

public class BorealSpike : ModProjectile
{
    public override string Texture => Helper.AssetPath + "Projectiles/Enemy/Snow/" + Name;
    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(12, 30);
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 80;
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        Projectile.frame = (int)Projectile.ai[2];
        if (Projectile.timeLeft > 78)
        {
            Projectile.localAI[0] = Projectile.ai[0] + 0.5f;
            SoundEngine.PlaySound(SoundID.Item28.WithPitchOffset(Main.rand.NextFloat(0.7f, 0.9f)), Projectile.Center);
            Projectile.rotation = Main.rand.NextFloat(-Pi / 12, Pi / 12);
            for (int i = 0; i < 12; i++)
            {
                Dust.NewDustPerfect(Projectile.Center - new Vector2(0, 7), 76, (Projectile.rotation - Pi / 2).ToRotationVector2() * Main.rand.NextFloat(0, 5), Scale: Main.rand.NextFloat(0.7f, 1.1f)).noGravity = true;
            }
            Projectile.scale = 0;
        }
        if (Projectile.timeLeft < 79 && Projectile.timeLeft > 76 && Projectile.ai[0] < Projectile.localAI[0] && Projectile.frame < 14)
        {
            Projectile.ai[0]++;
            MPUtils.NewProjectile(Projectile.GetSource_FromAI(), Helper.GetNearestSurface(new Vector2(Projectile.Center.X + Projectile.ai[1] * 10, Projectile.Center.Y)) + new Vector2(0, 4), Vector2.Zero, ProjectileType<BorealSpike>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai1: Projectile.ai[1], ai2: Projectile.frame + 2);
        }
        Projectile.scale = Projectile.timeLeft < 20 ? Lerp(Projectile.scale, 0, 0.25f) : Lerp(Projectile.scale, 1, 0.3f);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (Projectile.timeLeft > 78) return false;
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frameRect = new Rectangle(0, Projectile.frame / 2 * Projectile.height, Projectile.width, Projectile.height);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frameRect, lightColor, Projectile.rotation, new Vector2(Projectile.Size.X / 2, Projectile.Size.Y), new Vector2(1, Projectile.scale), Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically);
        return false;
    }
}