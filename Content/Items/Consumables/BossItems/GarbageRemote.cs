using EbonianMod.Content.Items.Consumables.Food;
using EbonianMod.Content.NPCs.Garbage;
using EbonianMod.Content.Projectiles.VFXProjectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.CameraModifiers;


namespace EbonianMod.Content.Items.Consumables.BossItems;

public class GarbageRemote : ModItem
{
    public override string Texture => Helper.AssetPath + "Items/Consumables/BossItems/GarbageRemote";
    public override void SetStaticDefaults()
    {
        ItemID.Sets.SortingPriorityBossSpawns[Item.type] = 12;
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.maxStack = 1;
        Item.value = 1000;
        Item.rare = ItemRarityID.Blue;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.noUseGraphic = true;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.shoot = ProjectileType<GarbageRemoteP>();
        Item.shootSpeed = 1;
        Item.consumable = false;
        Item.useTurn = false;
    }
    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient(ItemType<Potato>(), 5).AddRecipeGroup(RecipeGroupSystem.SilverBars, 5).AddIngredient(ItemID.Glass, 10).AddTile(TileID.Anvils).Register();
    }

    public override bool CanUseItem(Player player)
    {
        bool proj = false;
        foreach (Projectile p in Main.ActiveProjectiles)
            if (p.type == ProjectileType<GarbageRemoteP>())
                proj = true;
        return !NPC.AnyNPCs(NPCType<HotGarbage>()) && !proj;
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        position = Helper.Raycast(player.Center + new Vector2(40 * player.direction, -50), Vector2.UnitY, 1000, true).Point - new Vector2(0, 23);
        velocity = Vector2.Zero;

        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        return false;
    }
    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer != player.whoAmI)
            Projectile.NewProjectile(null, Helper.Raycast(player.Center + new Vector2(40 * player.direction, -50), Vector2.UnitY, 1000, true).Point - new Vector2(0, 23), Vector2.Zero, Item.shoot, 0, 0, player.whoAmI);

        SoundEngine.PlaySound(Sounds.garbageSignal.WithVolumeScale(3), player.position);
        return null;
    }
}
public class GarbageRemoteP : ModProjectile
{
    public override string Texture => Helper.AssetPath + "Items/Consumables/BossItems/GarbageRemote";
    public override void SetDefaults()
    {
        Projectile.width = 36;
        Projectile.height = 46;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 430;


    }
    public override bool? CanDamage()
    {
        return false;
    }
    public override void PostDraw(Color lightColor)
    {
        Texture2D tex = Request<Texture2D>(Texture + "_Overlay").Value;
        if (Projectile.timeLeft > 155)
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, Projectile.Size / 2, 1, SpriteEffects.None, 0);
    }
    public override void OnKill(int timeLeft)
    {
        Player player = Main.player[Projectile.owner];
        Helper.AddCameraModifier(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), 10, 6, 30, 1000));
        Projectile a = Projectile.NewProjectileDirect(Projectile.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ProjectileID.DaybreakExplosion, 50, 0);
        if (a is not null)
        {
            a.hostile = true;
            a.friendly = false;
            MPUtils.SyncProjectile(a);
        }
    }
    List<Vector2> points = new List<Vector2>();
    Vector2 end;
    Vector2 basePos;
    Vector2 pos;
    float rot;
    public override void AI()
    {

        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.active && npc.type == NPCType<HotGarbage>())
            {
                if (npc.Distance(Projectile.Center) < Projectile.Size.Length())
                    Projectile.Kill();
            }
        }
        Player player = Main.player[Projectile.owner];
        if (Projectile.ai[0] == 0)
        {
            Projectile.netUpdate = true;
            if (Projectile.owner == Main.myPlayer)
                basePos = Projectile.Center;

            if (Projectile.owner == Main.myPlayer)
                Projectile.netUpdate = true; // TEST
            Projectile.ai[0] = 1;
        }
        else
        {
            Projectile.netUpdate = true;
            if (Projectile.timeLeft > 155)
            {
                if (Projectile.timeLeft % 5 - (Projectile.timeLeft < 200 ? 2 : 0) == 0)
                {
                    rot = Main.rand.NextFloat(-0.5f, 0.5f);
                    if (Projectile.owner == Main.myPlayer)
                        pos = basePos + Main.rand.NextVector2Circular(5, 5);
                    if (Projectile.owner == Main.myPlayer)
                        Projectile.netUpdate = true; // TEST
                }
                if (Projectile.owner == Main.myPlayer)
                    if (pos != Vector2.Zero)
                        Projectile.Center = Vector2.Lerp(Projectile.Center, pos, 0.1f);
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, rot, 0.5f);
            }
        }
        Projectile.ai[1]++;
        if (Projectile.ai[1] % 5 == 0 && Main.rand.NextBool(4) && Projectile.timeLeft > 155)
        {
            MPUtils.NewProjectile(null, Projectile.Center, Vector2.Zero, ProjectileType<YellowShockwave>(), 0, 0);
        }
        int n = 15;
        Vector2 start = Projectile.Center;
        if (Projectile.timeLeft == 120)
        {
            if (Projectile.owner == Main.myPlayer)
                MPUtils.NewNPC(Projectile.Center + new Vector2(0, -1080), NPCType<HotGarbage>());
        }
        if (Projectile.timeLeft == 155)
        {
            Helper.AddCameraModifier(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), 5, 6, 30, 1000));
            MPUtils.NewProjectile(null, Projectile.Center, Vector2.Zero, ProjectileType<BigGrayShockwave>(), 0, 0);
            end = Projectile.Center + new Vector2(0, -Main.screenHeight);
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
        }
        if (Projectile.timeLeft <= 155)
            if (Main.rand.NextBool(5))
                Helper.DustExplosion(Projectile.Center, Vector2.One, 2, Color.Gray * 0.45f, false, false, 0.6f, 0.5f, new(Main.rand.NextFloat(-4, 4), -10));
        if (Projectile.timeLeft <= 155 && Projectile.timeLeft > 125)
        {
            Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 1, 0.1f);
            if (points.Count > 2)
            {

                Vector2 dirr = (end - start).RotatedBy(MathHelper.PiOver2);
                dirr.Normalize();
                for (int i = 0; i < points.Count; i++)
                {
                    points[i] = Vector2.SmoothStep(points[i], Vector2.SmoothStep(start, end, i / (float)n), 0.35f);
                }
                Projectile.ai[0]++;

                if (Projectile.ai[0] % 2 == 0)
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


                points[0] = start;
                points[points.Count - 1] = end;
            }
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {

        float mult = 0.55f + (float)Math.Sin(Main.GlobalTimeWrappedHourly/* * 2*/) * 0.1f;
        float scale = Projectile.scale * 2;
        Texture2D bolt = Assets.Extras.laser2.Value;
        Main.spriteBatch.Reload(BlendState.Additive);
        Main.spriteBatch.Reload(SpriteSortMode.Immediate);
        float s = 1;
        if (points.Count > 2 && Projectile.timeLeft <= 155 && Projectile.timeLeft > 125)
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

                Color color = Color.Orange * s;

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
        Main.spriteBatch.Reload(SpriteSortMode.Deferred);
        Main.spriteBatch.Reload(BlendState.AlphaBlend);
        return true;
    }
}
