using EbonianMod.Content.Projectiles.Bases;
using EbonianMod.Content.Projectiles.Friendly.Underworld;

namespace EbonianMod.Content.Items.Weapons.Ranged;

public class Corebreaker : ModItem
{
    public override string Texture => Helper.AssetPath + "Items/Weapons/Ranged/Corebreaker";
    public override void SetDefaults()
    {
        Item.knockBack = 10f;
        Item.crit = 6;
        Item.damage = 21;
        Item.useTime = 120;
        Item.DamageType = DamageClass.Ranged;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.rare = ItemRarityID.Yellow;
        Item.shootSpeed = 1;
        Item.value = Item.buyPrice(0, 17, 0, 0);
        Item.shoot = ProjectileType<CorebreakerProjectile>();
        Item.autoReuse = false;
        Item.noUseGraphic = true;
        Item.channel = true;
        Item.noMelee = true;
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        velocity = Vector2.Zero;
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        return false;
    }
}
public class CorebreakerProjectile : HeldProjectileGun
{
    public override string Texture => Helper.AssetPath + "Items/Weapons/Ranged/Corebreaker";
    public override bool? CanDamage() => false;
    public override void SetDefaults()
    {
        base.SetDefaults();
        ItemType = ItemType<Corebreaker>();
        RotationSpeed = 0.25f;
        Projectile.Size = new Vector2(62, 38);
        HoldOffset = new Vector2(0, -2);
    }
    public override void OnSpawn(IEntitySource source)
    {
        Projectile.netUpdate = true;
        CalculateAttackSpeedParameters(120);
        Projectile.rotation = (Main.MouseWorld - Main.player[Projectile.owner].Center).ToRotation();
        Projectile.ai[1] = 40;
        Projectile.ai[0] = 50;
    }
    public override void AI()
    {
        base.AI();

        Player player = Main.player[Projectile.owner];
        HoldOffset.X = Lerp(HoldOffset.X, 24, 0.13f);
        Projectile.ai[1]++;
        if (Projectile.ai[0]++ >= 120 * AttackDelayMultiplier)
        {
            HoldOffset.X = 0;
            Vector2 shotPoint = Projectile.Center + Projectile.rotation.ToRotationVector2() * 25;
            SoundEngine.PlaySound(SoundID.Item40.WithPitchOffset(Main.rand.NextFloat(-1f, -0.5f)), player.Center);
            SoundEngine.PlaySound(SoundID.Item38.WithPitchOffset(Main.rand.NextFloat(-0.8f, -0.4f)), player.Center);
            for (int i = 0; i < 30; i++)
                Dust.NewDustPerfect(shotPoint, DustID.Torch, (Projectile.rotation + Main.rand.NextFloat(-PiOver4, PiOver4)).ToRotationVector2() * Main.rand.NextFloat(0.3f, 8), Scale: Main.rand.NextFloat(1f, 4f)).noGravity = true;
            if (player.whoAmI == Main.myPlayer)
                Projectile.NewProjectile(Projectile.InheritSource(Projectile), shotPoint, (Main.MouseWorld - Projectile.Center) / 35, ProjectileType<CoreProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            AnimationRotation = -0.4f * player.direction;
            Projectile.ai[0] = 0;
            Projectile.netUpdate = true;
        }
        if (Main.mouseRight && Projectile.ai[1] >= 80 * AttackDelayMultiplier && Main.myPlayer == Projectile.owner)
        {
            HoldOffset.X = 0;
            Vector2 shotPoint = Projectile.Center + Projectile.rotation.ToRotationVector2() * 25;
            SoundEngine.PlaySound(SoundID.Item62.WithPitchOffset(Main.rand.NextFloat(0.5f, 1.2f)), player.Center);
            SoundEngine.PlaySound(SoundID.Item98.WithPitchOffset(Main.rand.NextFloat(-1f, -0.5f)), player.Center);
            for (int i = 0; i < 30; i++)
            {
                Dust.NewDustPerfect(shotPoint, DustID.Smoke, (Projectile.rotation + Main.rand.NextFloat(PiOver2, PiOver4)).ToRotationVector2() * Main.rand.NextFloat(1, 8), Scale: Main.rand.NextFloat(0.5f, 3f)).noGravity = true;
                Dust.NewDustPerfect(shotPoint, DustID.Smoke, (Projectile.rotation + Main.rand.NextFloat(-PiOver2, -PiOver4)).ToRotationVector2() * Main.rand.NextFloat(1, 8), Scale: Main.rand.NextFloat(0.5f, 3f)).noGravity = true;
            }
            if (player.whoAmI == Main.myPlayer)
                Projectile.NewProjectile(Projectile.InheritSource(Projectile), shotPoint, Projectile.rotation.ToRotationVector2() * 4, ProjectileType<CorebreakerHitscan>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            Projectile.ai[0] = 0;
            Projectile.ai[1] = 0;
            Projectile.netUpdate = true;
        }
        if (!Main.player[Projectile.owner].channel) Projectile.Kill();
    }
}
