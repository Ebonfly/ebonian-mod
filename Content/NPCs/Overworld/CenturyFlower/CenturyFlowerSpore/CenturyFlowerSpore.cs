using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using EbonianMod.Content.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;
using EbonianMod.Common.Graphics;

namespace EbonianMod.Content.NPCs.Overworld.CenturyFlower.CenturyFlowerSpore
{
    public class CenturyFlowerSpore : ModProjectile
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
            if (lightColor.A > 0) return false;
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frame = Projectile.frame;
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                float mult = 1 - Helper.SafeDivision(1f / Projectile.oldPos.Length) * i;
                for (float j = 0; j < 3; j++)
                {
                    Vector2 pos = Vector2.Lerp(Projectile.oldPos[i], Projectile.oldPos[i + 1], j / 3f);
                    Main.EntitySpriteDraw(tex, pos + Projectile.Size / 2 - Main.screenPosition, tex.Frame(1, 2, 0, frame), Color.MediumSlateBlue with { A = 0 } * 0.1f * MathF.Pow(mult * 2, 2) * Projectile.Opacity, Projectile.oldRot[i], Projectile.Size / 2, Projectile.scale * mult, SpriteEffects.None);
                }
            }
            return false;
        }

        const int MAX_TIMELEFT = 270;
        public override void SetDefaults()
        {
            Projectile.height = 64;
            Projectile.width = 80;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.damage = 0;
            Projectile.tileCollide = false;
            Projectile.timeLeft = MAX_TIMELEFT;
            Projectile.frame = Main.rand.Next(0, 2);
            Projectile.localNPCHitCooldown = 1;
            Projectile.usesLocalNPCImmunity = true;
        }

        void CheckCollision()
        {
            foreach (Player p in Main.player)
            {
                if (p.Hitbox.Intersects(Projectile.Hitbox))
                {
                    p.AddBuff(BuffID.Suffocation, 60);
                }
            }
        }
        public override bool ShouldUpdatePosition() => !(Helper.Raycast(Projectile.Center, Vector2.UnitY, 12).RayLength < 9 && Projectile.velocity.Y > 0);
        public override void AI()
        {
            Projectile.knockBack = 0;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, new Vector2(Main.windSpeedCurrent * 0.1f, 0.1f), 0.05f);
            if (ShouldUpdatePosition())
                Projectile.rotation += .02f * Projectile.velocity.Length();
            var currentTime = (float)(MAX_TIMELEFT - Projectile.timeLeft);
            Projectile.alpha = (int)(currentTime / MAX_TIMELEFT * 255);
            Projectile.scale = currentTime / MAX_TIMELEFT + .1f;
            CheckCollision();
        }
    }
}