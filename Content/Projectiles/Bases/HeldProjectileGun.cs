using System.IO;

namespace EbonianMod.Content.Projectiles.Bases;

public abstract class HeldProjectileGun : HeldProjectile
{
    protected float RotationSpeed, AnimationRotationSpeed = 0.2f, AnimationRotation;
    protected Vector2 CursorOffset;
    public override void AI()
    {
        base.AI();
        Player player = Main.player[Projectile.owner];
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - PiOver2);

        AnimationRotation = Lerp(AnimationRotation, 0, AnimationRotationSpeed);
        Projectile.netUpdate = true;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        base.SendExtraAI(writer);
        writer.Write(Projectile.rotation);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        base.ReceiveExtraAI(reader);
        Projectile.rotation = reader.ReadSingle();
    }

    protected override void LocalBehaviour()
    {
        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, (Difference + CursorOffset.RotatedBy(Projectile.rotation) * Main.player[Projectile.owner].direction).ToRotation(), RotationSpeed) + AnimationRotation * AttackDelayMultiplier;
    }
}
