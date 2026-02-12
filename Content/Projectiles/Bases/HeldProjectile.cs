using System.IO;

namespace EbonianMod.Content.Projectiles.Bases;

public abstract class HeldProjectile : ModProjectile
{
    protected int ItemType;
    protected float AttackDelayMultiplier, AttackSpeedMultiplier;
    protected Vector2 Difference, HoldOffset;
    public override void SetDefaults()
    {
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(AttackDelayMultiplier);
        writer.Write(AttackSpeedMultiplier);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        AttackDelayMultiplier = reader.ReadSingle();
        AttackSpeedMultiplier = reader.ReadSingle();
    }
    protected void CalculateAttackSpeedParameters(float baseValue)
    {
        float itemTime = Main.player[Projectile.owner].itemTime;
        AttackDelayMultiplier = itemTime / baseValue;
        AttackSpeedMultiplier = baseValue / itemTime;
        Projectile.netUpdate = true;
    }
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];

        player.itemTime = 2;
        player.itemAnimation = 2;
        Projectile.timeLeft = 10;
        if (!player.active || player.dead || player.CCed || player.HeldItem.type != ItemType) Projectile.Kill();

        if (player.whoAmI == Main.myPlayer)
        {
            Difference = Main.MouseWorld - player.Center;
            player.ChangeDir(Difference.X > 0 ? 1 : -1);
            Projectile.direction = player.direction;
            LocalBehaviour();
        }

        Projectile.Center = player.MountedCenter + new Vector2(HoldOffset.X, HoldOffset.Y * player.direction).RotatedBy(Projectile.rotation);
    }
    protected virtual void LocalBehaviour() { }
}
