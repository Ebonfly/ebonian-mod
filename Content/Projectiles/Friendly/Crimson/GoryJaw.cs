using System.IO;

namespace EbonianMod.Content.Projectiles.Friendly.Crimson;

public class GoryJaw : ModProjectile
{
    public override string Texture => Helper.AssetPath + "Projectiles/Friendly/Crimson/GoryJaw";
    Vector2 PositionOffset;
    Vector2 Scale = Vector2.One;
    int TargetIndex = -1;
    public override void SetDefaults()
    {
        Projectile.width = 23;
        Projectile.height = 34;
        Projectile.tileCollide = true;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2900;
        Projectile.frame = 2;
        Projectile.localNPCHitCooldown = 240;
        Projectile.ArmorPenetration = 11;
        Projectile.extraUpdates = 10;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.spriteDirection = Main.player[Projectile.owner].direction;
    }

    public override void OnKill(int timeLeft)
    {
        if (Main.dedServ) return;
        SoundEngine.PlaySound(Sounds.chomp1.WithPitchOffset(Main.rand.NextFloat(-0.4f, 0.2f)), Projectile.Center);
        for (int i = 0; i < 4; i++) Gore.NewGore(null, Projectile.Center, Main.rand.NextVector2Circular(5, 5), Find<ModGore>("EbonianMod/GoryJaw" + i).Type, 1);
        for (int i = 0; i < 20; i++) Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Main.rand.NextFloat(-Pi, Pi).ToRotationVector2() * Main.rand.NextFloat(2, 5), Scale: 1.5f);
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(TargetIndex);
        writer.WriteVector2(PositionOffset);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        TargetIndex = reader.ReadInt32();
        PositionOffset = reader.ReadVector2();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Projectile.ai[1] == 0)
        {
            PositionOffset = Projectile.Center - target.Center;
            TargetIndex = target.whoAmI;
            Projectile.ai[1] = 1;
            Projectile.netUpdate = true;
        }
    }
    public override void AI()
    {
        
        Projectile.extraUpdates = 10;
        Player player = Main.player[Projectile.owner];
        if (TargetIndex > -1 && TargetIndex < Main.npc.Length)
        {
            NPC Target = Main.npc[TargetIndex];
            if (Target.life <= 0 || !Target.active)
            {
                Projectile.ai[1] = 0;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, new Vector2(Projectile.velocity.X / 10, 1), 0.005f);
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.tileCollide = true;
                Projectile.netUpdate = true;
                return;
            }
            Projectile.localNPCHitCooldown = 180;
            Projectile.tileCollide = false;
            Projectile.Center = Target.Center + PositionOffset;
            Projectile.frameCounter++;
            if (Projectile.frameCounter == 60)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame > 2)
                {
                    for (int i = 0; i < 8; i++) Dust.NewDustPerfect(Projectile.Center + Projectile.rotation.ToRotationVector2() * 23, DustID.Blood, (Projectile.rotation + Main.rand.NextFloat(-Pi / 3f, Pi / 3f)).ToRotationVector2() * Main.rand.NextFloat(2, 6), Scale: 1.5f).noGravity = true;
                    Projectile.frame = 0;
                }
            }
        }
        else
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity.Y += 0.002f;

            if ((int)Projectile.ai[0] % 15 == 0 || Projectile.ai[0] <= 0.5f)
                Projectile.netUpdate = true;
        }
        Scale = Vector2.Lerp(Scale, new Vector2(Main.rand.NextFloat(2f - Projectile.ai[0] / 1200f, Projectile.ai[0] / 1200f), Main.rand.NextFloat(2f - Projectile.ai[0] / 1200f, Projectile.ai[0] / 1200f)), Projectile.ai[0] / 8000f);
        Projectile.ai[0]++;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frameRect = new Rectangle(0, Projectile.frame * 34, 46, 34);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frameRect, lightColor, Projectile.rotation, Projectile.Size / 2f, Vector2.Clamp(Scale, Vector2.One * 0.75f, Vector2.One * 1.25f), Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically);
        return false;
    }
}