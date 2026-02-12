using EbonianMod.Content.Projectiles.Bases;
using EbonianMod.Content.Projectiles.Friendly.Crimson;

namespace EbonianMod.Content.Items.Weapons.Magic;

public class Latcher : ModItem
{
    public override string Texture => Helper.AssetPath + "Items/Weapons/Magic/Latcher";
    public override void SetDefaults()
    {
        Item.DamageType = DamageClass.Magic;
        Item.damage = 80;
        Item.useTime = 50;
        Item.shoot = ProjectileType<LatcherGraphics>();
        Item.shootSpeed = 1f;
        Item.rare = ItemRarityID.Green;
        Item.useStyle = 5;
        Item.value = Item.buyPrice(0, 5, 0, 0);
        Item.autoReuse = false;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.channel = true;
        Item.mana = 50;
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient(ItemID.Vertebrae, 20).AddIngredient(ItemID.Hook).AddTile(TileID.Anvils).Register();
    }
    public override bool CanUseItem(Player player)
    {
        return player.ownedProjectileCounts[ProjectileType<LatcherProjectile>()] < 1;
    }
    public override bool? CanAutoReuseItem(Player player) => false;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        velocity.Normalize();
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        return false;
    }
}
public class LatcherGraphics : HeldProjectileGun
{
    Vector2 Scale = new Vector2(0, 0);
    Projectile ChildProjectile;
    public override string Texture => Helper.AssetPath + "Items/Weapons/Magic/Latcher";
    public override void OnSpawn(IEntitySource source)
    {
        CalculateAttackSpeedParameters(50);
        Player player = Main.player[Projectile.owner];
        Projectile.rotation = Helper.FromAToB(player.Center, Main.MouseWorld).ToRotation() + player.direction * Pi;
    }
    public override bool? CanDamage() => false;
    public override void SetDefaults()
    {
        base.SetDefaults();
        ItemType = ItemType<Latcher>();
        Projectile.Size = new Vector2(60, 38);
        HoldOffset = new Vector2(25, -2);
    }
    public override void AI()
    {
        base.AI();

        Player player = Main.player[Projectile.owner];

        if (player.ownedProjectileCounts[ProjectileType<LatcherProjectile>()] < 1 && Projectile.ai[1] == 1)
        {
            for (int i = 0; i < 20; i++) Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Scale: 1.5f);
            Projectile.Kill();
        }

        Projectile.ai[0]++;
        if (Projectile.ai[1] == 1)
        {
            RotationSpeed = 0.03f;
            Vector2 position = Projectile.Center + Projectile.rotation.ToRotationVector2() * 10;
            ChildProjectile.ai[0] = position.X;
            ChildProjectile.ai[1] = position.Y;
        }
        else
        {
            RotationSpeed = 0.1f * AttackSpeedMultiplier;
            if (player.whoAmI == Main.myPlayer && !player.channel && Projectile.ai[0] > 45 * AttackDelayMultiplier)
            {
                Projectile.ai[1] = 1;
                Scale = new Vector2(0.65f, 1.6f);

                if (Helper.Raycast(player.Center, Projectile.rotation.ToRotationVector2(), 96).Success)
                {
                    for(int i = 0; i < 20; i++) Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Scale: 1.5f);
                    Projectile.Kill();
                }
                else ChildProjectile = Projectile.NewProjectileDirect(Projectile.InheritSource(Projectile), Projectile.Center, Projectile.rotation.ToRotationVector2() * 27, ProjectileType<LatcherProjectile>(), 1, Projectile.knockBack, Projectile.owner, ai2: Projectile.rotation);

                SoundEngine.PlaySound(SoundID.NPCHit8.WithPitchOffset(Main.rand.NextFloat(-0.4f, 0.4f)), player.Center);
            }
        }
        Scale = Vector2.Lerp(Scale, new Vector2(1, 1), 0.14f);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frameRect = new Rectangle(0, 0, Projectile.width, Projectile.height);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frameRect, lightColor, Projectile.rotation, Projectile.Size / 2, Scale, Main.player[Projectile.owner].direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically);
        return false;
    }
}
