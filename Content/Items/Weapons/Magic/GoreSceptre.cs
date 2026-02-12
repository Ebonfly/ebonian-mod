using EbonianMod.Content.Projectiles.Friendly.Crimson;
using EbonianMod.Content.Projectiles.VFXProjectiles;
using System;
using System.Collections.Generic;
using System.IO;

namespace EbonianMod.Content.Items.Weapons.Magic;

public class GoreSceptre : ModItem
{
    public override string Texture => Helper.AssetPath + "Items/Weapons/Magic/GoreSceptre";
    public override void SetStaticDefaults()
    {

        Item.staff[Item.type] = true;
    }

    public override void SetDefaults()
    {
        Item.damage = 1;
        Item.width = 40;
        Item.height = 40;
        Item.mana = 3;
        Item.useTime = 2;
        Item.DamageType = DamageClass.Magic;
        Item.useAnimation = 2;
        Item.useStyle = 5;
        Item.value = Item.buyPrice(0, 3, 0, 0);
        Item.knockBack = 10;
        Item.value = 1000;
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item8;
        Item.channel = true;
        Item.noMelee = true;
        Item.autoReuse = false;
        Item.useTurn = false;
        Item.shoot = ProjectileType<GoreBeam>();
        Item.shootSpeed = 1;
    }
    //int uses = -2;
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Projectile.NewProjectile(source, position + velocity * 14, velocity, ProjectileType<GoreBeam>(), damage, knockback, player.whoAmI);
        return false;
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient(ItemID.CrimsonHeart).AddIngredient(ItemID.CrimtaneBar, 35).AddTile(TileID.Anvils).Register();
    }
}
public class GoreBeam : ModProjectile
{
    public override string Texture => Helper.Empty;
    int MAX_TIME = 40;
    public override void SetDefaults()
    {
        Projectile.width = 25;
        Projectile.height = 25;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = MAX_TIME;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 9;
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers hit)
    {
        Rectangle rec = new Rectangle((int)end.X - 25, (int)end.Y - 25, 50, 50);
        if (target.getRect().Intersects(rec))
            hit.SetCrit();
        else
            hit.DisableCrit();
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Rectangle rec = new Rectangle((int)end.X - 25, (int)end.Y - 25, 50, 50);
        if (target.getRect().Intersects(rec))
        {

        }
    }
    public override void OnSpawn(IEntitySource source)
    {
        end = Main.player[Projectile.owner].Center;
    }
    public override bool ShouldUpdatePosition()
    {
        return false;
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (!RunOnce || points.Count < 2) return false;
        float a = 0f;
        bool ye = false;
        for (int i = 1; i < points.Count; i++)
        {
            ye = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), points[i], points[i - 1], Projectile.width, ref a);
            if (ye) break;
        }
        return ye;
    }
    bool RunOnce;
    List<Vector2> points = new List<Vector2>();
    Vector2 end;
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.WriteVector2(end);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        end = reader.ReadVector2();
    }
    public override void AI()
    {
        if (Projectile.ai[1] > 0)
            Projectile.ai[1] -= 0.1f;
        Player player = Main.player[Projectile.owner];
        if (!player.active || player.dead || player.CCed || player.noItems || !player.channel || !player.channel || !player.CheckMana(Projectile.ai[2] >= 20 ? 1 : (int)(20 - (Projectile.ai[2] / 2))))
        {
            Projectile.Kill();
            return;
        }
        int n = 25;

        if (Main.myPlayer == Projectile.owner)
        {
            Projectile.velocity = Helper.FromAToB(player.Center, end);
            if (Projectile.velocity != Projectile.oldVelocity)
                Projectile.netUpdate = true;
        }
        
        Vector2 start = player.Center + Projectile.velocity * 40;
        if (!RunOnce)
        {
            n = 5;
            end = player.Center;
            Vector2 dir = (end - start).RotatedBy(MathHelper.PiOver2);
            dir.Normalize();
            float x = Main.rand.NextFloat(30, 40);
            for (int i = 0; i < n; i++)
            {
                if (i == n - 1)
                    x = 0;
                Vector2 point = Vector2.SmoothStep(start, end, i / (float)n) + dir * Main.rand.NextFloat(-x, x).SafeDivision(); //x being maximum magnitude
                points.Add(point);
                x -= i / (float)n;
            }
            SoundEngine.PlaySound(SoundID.Item72, player.Center);

            RunOnce = true;
        }
        else if (points.Count > 2)
        {
            Projectile.direction = end.X > Projectile.Center.X ? 1 : -1;
            player.ChangeDir(Projectile.direction);
            Projectile.timeLeft = 10;
            player.itemTime = 2;
            player.itemAnimation = 2; if (player.HeldItem.type != ItemType<GoreSceptre>()) { player.itemTime = 0; player.itemAnimation = 0; Projectile.Kill(); }
            player.itemRotation = Helper.FromAToB(player.Center, end).ToRotation() + (player.direction == -1 ? MathHelper.Pi : 0);
            if (player.gravDir != -1)
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Helper.FromAToB(player.Center, end).ToRotation() - MathHelper.PiOver2);
            
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (player.Distance(end) < 200)
                n = 7;
            else if (player.Distance(end) < 300)
                n = 10;
            else if (player.Distance(end) < 450)
                n = 15;
            else if (player.Distance(end) < 700)
                n = 20;

            Vector2 dirr = (end - start).RotatedBy(MathHelper.PiOver2);
            dirr.Normalize();
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = Vector2.SmoothStep(points[i], Vector2.SmoothStep(start, end, i / (float)n), 0.35f);
            }
            Projectile.ai[0]++;


            if (Projectile.ai[2] >= 20)
            {
                player.CheckMana(1, true);
                player.manaRegenDelay = player.maxRegenDelay;
            }

            if (Projectile.ai[0] > 90 - Projectile.ai[2] * 3 && Projectile.ai[2] < 20)
            {
                Projectile.ai[1] = 1f;

                player.CheckMana((int)(20 - (Projectile.ai[2] / 2)), true);
                player.manaRegenDelay = player.maxRegenDelay;
                SoundEngine.PlaySound(Sounds.heartbeat.WithVolumeScale(1.3f), end);
                Projectile.NewProjectileDirect(Projectile.InheritSource(Projectile), end, Vector2.Zero, ProjectileType<BloodShockwave2>(), 0, 0, Projectile.owner);
                Projectile.damage++;
                Projectile.ai[2]++;
                Projectile.ai[0] = 0;
                Projectile.netUpdate = true;
            }

            if (Projectile.ai[0] % 3 == 0)
            {
                SoundStyle s = SoundID.DD2_LightningAuraZap;
                s.Volume = 0.5f;
                SoundEngine.PlaySound(s, player.Center);
                points.Clear();
                //Vector2 start = Projectile.Center + Helper.FromAToB(player.Center, Main.MouseWorld) * 40;
                Vector2 dir = (end - start).RotatedBy(MathHelper.PiOver2);
                dir.Normalize();
                float x = Main.rand.NextFloat(30, 40) - Projectile.damage;
                for (int i = 0; i < n; i++)
                {
                    if (i == n - 1)
                        x = 0;
                    float a = Main.rand.NextFloat(-x, x).SafeDivision();
                    if (i < 3)
                        a = 0;
                    Vector2 point = Vector2.SmoothStep(start, end, i / (float)n) + dir * a; //x being maximum magnitude
                    points.Add(point);
                    x -= i / (float)n;
                }
            }


            points[0] = player.Center + Helper.FromAToB(player.Center, end) * 40 - new Vector2(0, 3).RotatedBy(dirr.ToRotation() + MathHelper.Pi / 2);
            float range = (Projectile.ai[2] + 2) * 96;
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 offset = Helper.FromAToB(player.Center, Main.MouseWorld, false);
                if (offset.Length() > range)
                {
                    offset.Normalize();
                    offset *= range;
                }
                end = Vector2.Lerp(end, player.Center + offset, 0.2f);
                if (end.Distance(Main.MouseWorld) > 10 || Projectile.ai[0] < 10)
                    Projectile.netUpdate = true; // TEST
            }
            points[points.Count - 1] = end;
            Projectile.Center = player.Center;

        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (!RunOnce || points.Count < 2) return false;
        Main.spriteBatch.Reload(SpriteSortMode.Immediate);

        float mult = 0.55f + (float)Math.Sin(Main.GlobalTimeWrappedHourly/* * 2*/) * 0.1f;
        float scale = Projectile.scale * 2;
        Texture2D texture = Assets.Extras.explosion.Value;
        Texture2D bolt = Assets.Extras.laser2.Value;
        Main.spriteBatch.Reload(BlendState.Additive);
        float s = 1;
        if (points.Count > 2)
        {
            VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[(points.Count - 1) * 6];
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 start = points[i];
                Vector2 end = points[i + 1];
                float dist = Vector2.Distance(points[i], points[i + 1]);
                Vector2 vector = (end - start) / dist;
                Vector2 vector2 = start;
                float rotation = vector.ToRotation();

                Color color = Color.Maroon * s;

                Vector2 pos1 = points[i] - Main.screenPosition;
                Vector2 pos2 = points[i + 1] - Main.screenPosition;
                Vector2 dir1 = Helper.GetRotation(points, i) * 10 * scale * s;
                Vector2 dir2 = Helper.GetRotation(points, i + 1) * 10 * scale * (s + i / (float)points.Count * 0.03f);
                Vector2 v1 = pos1 + dir1;
                Vector2 v2 = pos1 - dir1;
                Vector2 v3 = pos2 + dir2;
                Vector2 v4 = pos2 - dir2;
                float p1 = i / (float)points.Count;
                float p2 = (i + 1) / (float)points.Count;
                vertices[i * 6] = Helper.AsVertex(v1, color, new Vector2(p1, 0));
                vertices[i * 6 + 1] = Helper.AsVertex(v3, color, new Vector2(p2, 0));
                vertices[i * 6 + 2] = Helper.AsVertex(v4, color, new Vector2(p2, 1));

                vertices[i * 6 + 3] = Helper.AsVertex(v4, color, new Vector2(p2, 1));
                vertices[i * 6 + 4] = Helper.AsVertex(v2, color, new Vector2(p1, 1));
                vertices[i * 6 + 5] = Helper.AsVertex(v1, color, new Vector2(p1, 0));

                s -= i / (float)points.Count * 0.03f;
            }
            Helper.DrawTexturedPrimitives(vertices, PrimitiveType.TriangleList, bolt, true);
        }
        //for (int i = 0; i < 5; i++)
        //   Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.Maroon, 0, new Vector2(texture.Width, texture.Height) / 2, scale * 0.2f, SpriteEffects.None, 0f);
        //for (int i = 0; i < 5; i++)
        texture = Assets.Projectiles.Friendly.Crimson.HeadGoreSceptre_Extra.Value;
        Main.spriteBatch.Draw(texture, end - Main.screenPosition, null, Color.Maroon * (Projectile.ai[2] < 20 ? Projectile.ai[1] * 2 : 1), 0, new Vector2(texture.Width, texture.Height) / 2, 1 + Projectile.ai[1], SpriteEffects.None, 0f);
        Main.spriteBatch.Reload(BlendState.AlphaBlend);
        texture = TextureAssets.Projectile[ProjectileType<HeadGoreSceptre>()].Value;
        Main.spriteBatch.Draw(texture, end - Main.screenPosition, null, Color.White, 0, new Vector2(texture.Width, texture.Height) / 2, 1 + Projectile.ai[1], SpriteEffects.None, 0f);
        return false;
    }
}