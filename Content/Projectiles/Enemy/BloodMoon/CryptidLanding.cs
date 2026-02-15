using Terraria.Graphics.CameraModifiers;

namespace EbonianMod.Content.Projectiles.Enemy.BloodMoon;

public class CryptidLanding : ModProjectile
{
    public override string Texture => Helper.Placeholder;
    public override void SetDefaults()
    {
        Projectile.width = 150;
        Projectile.height = 20;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 30;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
    }

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => Projectile.timeLeft > 25;
    int seed;
    public override void OnSpawn(IEntitySource source)
    {
        SoundEngine.PlaySound(Sounds.bigassstar.WithPitchOffset(0.5f), Projectile.Center);
        if (Projectile.ai[2] > 0)
        {
            SoundEngine.PlaySound(Sounds.eggplosion, Projectile.Center);
            Helper.AddCameraModifier(new PunchCameraModifier(Projectile.Center, Vector2.UnitY, 5f, 10, 30, 800));
        }

        seed = Main.rand.Next(int.MaxValue);
        for (int k = 0; k < 50; k++)
        {
            Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, Main.rand.NextVector2Unit(-MathHelper.Pi, MathHelper.Pi) * Main.rand.NextFloat(1, 30), 0, default, Main.rand.NextFloat(1, 3)).noGravity = true;
            Dust.NewDustPerfect(Projectile.Center, DustID.Enchanted_Gold, Main.rand.NextVector2Unit(-MathHelper.Pi, MathHelper.Pi) * Main.rand.NextFloat(1, 30), 100, default, Main.rand.NextFloat(1, 2)).noGravity = true;
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = Assets.Extras.cone5.Value;
        UnifiedRandom rand = new UnifiedRandom(seed);
        float max = 30;
        if (Projectile.ai[2] > 0)
            max *= 0.3f;
        float alpha = MathHelper.Lerp(0.5f, 0, Projectile.ai[1]) * 2;
        for (float i = 0; i < max; i++)
        {
            int dir = rand.NextBool() ? -1 : 1;
            float angle = rand.NextFloat(0, -0.7f) * dir + (dir == -1 ? MathHelper.Pi : 0) - dir * alpha;
            float scale = rand.NextFloat(0, .8f);
            if (Projectile.ai[2] > 0)
                scale *= 2.5f;
            Vector2 offset = new Vector2(rand.NextFloat(-100, 100), 0) + new Vector2(Main.rand.NextFloat(-0, 2) * Projectile.ai[1] * scale, 0).RotatedBy(angle);
            for (float j = 0; j < 2; j++)
                Main.spriteBatch.Draw(tex, Projectile.Center + offset - Main.screenPosition, null, Color.Gold with { A = 0 } * alpha, angle, new Vector2(0, tex.Height * 0.5f), new Vector2(Projectile.ai[1], alpha + Projectile.ai[1] * 0.05f) * scale * 0.6f * 4, SpriteEffects.None, 0);
        }
        Lighting.AddLight(Projectile.Center, TorchID.UltraBright);
        return false;
    }
    public override void AI()
    {
        Projectile.ai[1] = MathHelper.Lerp(Projectile.ai[1], 1, 0.25f);
        if (Projectile.ai[1] > 1)
            Projectile.Kill();

        if (Projectile.ai[2] > 0 && Projectile.width < 300)
            Projectile.Resize(400, Projectile.height*2);
    }
}