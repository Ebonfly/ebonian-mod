using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent.Creative;
using EbonianMod.Content.Buffs;
using System;
using System.Collections.Generic;
using EbonianMod.Common.Graphics;

namespace EbonianMod.Content.Items.Weapons.Magic
{
    public class CenturySprayer : ModItem
    {
        public override string Texture => Helper.AssetPath + "Items/Weapons/Magic/CenturySprayer";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 5;
            Item.DamageType = DamageClass.Magic;
            Item.noMelee = true;
            Item.useTime = 5;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.rare = ItemRarityID.Blue;
            Item.knockBack = 5;
            Item.UseSound = SoundID.Item34;
            Item.autoReuse = true;
            Item.mana = 5;
            Item.crit = 0;
            Item.shoot = ProjectileType<CenturySpewerSpore>();
            Item.shootSpeed = 6f;
        }
        public override bool CanReforge() => false;
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            int damg = tooltips.FindIndex(x => x.Mod == "Terraria" && x.Name == "Damage");
            int knoc = tooltips.FindIndex(x => x.Mod == "Terraria" && x.Name == "Knockback");
            int crit = tooltips.FindIndex(x => x.Mod == "Terraria" && x.Name == "CritChance");
            tooltips[damg].Hide();
            tooltips[knoc].Hide();
            tooltips[crit].Hide();
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(10));
            damage = 0;
        }
    }
    public class CenturySpewerSpore : ModProjectile
    {
        public override string Texture => Helper.AssetPath+"NPCs/Overworld/CenturyFlower/CenturyFlowerSpore/CenturyFlowerSpore1";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            PixelationTarget.pixelatedProjectiles.Add(Type);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (lightColor != Color.Transparent) return false;
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frame = Projectile.frame;
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                float mult = 1 - Helper.SafeDivision(1f / Projectile.oldPos.Length) * i;
                for (float j = 0; j < 3; j++)
                {
                    Vector2 pos = Vector2.Lerp(Projectile.oldPos[i], Projectile.oldPos[i + 1], j / 3f);
                    Main.EntitySpriteDraw(tex, pos + Projectile.Size / 2 - Main.screenPosition, tex.Frame(1, 2, 0, frame), Color.MediumSlateBlue with { A = 0 } * 0.05f * MathF.Pow(mult * 2, 2) * Projectile.Opacity, Projectile.oldRot[i], Projectile.Size / 2, Projectile.scale * mult, SpriteEffects.None);
                }
            }
            return false;
        }
        const int MAX_TIMELEFT = 170;
        public override void SetDefaults()
        {
            Projectile.damage = 0;
            Projectile.height = 64;
            Projectile.penetrate = -1;
            Projectile.width = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.damage = 0;
            Projectile.tileCollide = false;
            Projectile.timeLeft = MAX_TIMELEFT;
            Projectile.frame = Main.rand.Next(0, 2);
            Projectile.localNPCHitCooldown = 1;
        }

        public override void AI()
        {
            Projectile.knockBack = 0;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, 0.05f);
            Projectile.rotation += .01f;

            var currentTime = (float)(MAX_TIMELEFT - Projectile.timeLeft);
            Projectile.alpha = (int)(currentTime / MAX_TIMELEFT * 255);
            Projectile.scale = currentTime / MAX_TIMELEFT + .1f;
            if (Projectile.owner == Main.myPlayer)
            {
                Rectangle rect1 = new((int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height);
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.friendly && !npc.dontTakeDamage && npc.lifeMax > 1)
                    {
                        Rectangle rect2 = new((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
                        if (rect1.Intersects(rect2))
                            npc.AddBuff(ModContent.BuffType<NPCSuffocating>(), 2);
                    }
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffType<NPCSuffocating>(), 2);
        }
    }
}