using System;
using System.Collections.Generic;

namespace EbonianMod.Content.Items.Weapons.Magic;

public class ItemName : ModItem
{
	public override string Texture => Helper.AssetPath + "Items/Weapons/Magic/" + Name;

	public override void SetStaticDefaults()
	{
		ItemID.Sets.ItemNoGravity[Type] = true;
	}

	public override void SetDefaults()
	{
		Item.Size = new Vector2(52);
		Item.damage = 38;
		Item.DamageType = DamageClass.Magic;
		Item.useTime = 40;
		Item.useAnimation = 40;
		Item.value = Item.buyPrice(0, 10);
		Item.useStyle = ItemUseStyleID.HiddenAnimation;
		Item.rare = ModContent.RarityType<ItemNameRarity>();
		Item.knockBack = 2f;
		Item.shootSpeed = 1f;
		Item.mana = 10;
		Item.noMelee = true;
		Item.channel = true;
		Item.noUseGraphic = true;
		Item.shoot = ProjectileType<ItemNameProjectile>();
	}
}

public class ItemNameRarity : ModRarity
{
	public override Color RarityColor => Color.Lerp(new Color(116, 131, 250, 40), new Color(150, 170, 255, 40), MathF.Abs(MathF.Sin(Main.GameUpdateCount*0.01f)));
}

public class ItemNameProjectile : ModProjectile
{
	public override string Texture => Helper.AssetPath + "Items/Weapons/Magic/" + Name;
	
	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Type] = 10;
		ProjectileID.Sets.TrailingMode[Type] = 2;
	}
	
	public override void SetDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Type] = 25;
		Projectile.Size = new Vector2(20, 16);
		Projectile.friendly = true;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.timeLeft = 30;
		Projectile.netUpdate = true;
		Projectile.netUpdate2 = true;
		Projectile.netImportant = true;
		Projectile.aiStyle = -1;
		Projectile.Opacity = 0;
	}

	public override bool? CanDamage() => false;
	public override bool ShouldUpdatePosition() => false;
	public override bool PreDraw(ref Color lightColor)
	{
		lightColor = Color.White;
		lightColor.A = 140;
		
		List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
		List<VertexPositionColorTexture>[] verticesPlayer = [new(), new()];

		float length = 1; 
		Player player = Main.player[Projectile.owner];
		for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
		{
			if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i].Distance(Projectile.Center) > 1000f) continue;
			length++;
			
			/*Vector2 basePosition = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
			float mult = 1f - i / length;
			float rotation = (Projectile.oldPos[i] - Projectile.oldPos[i+1]).ToRotation();
			Color color = new Color(116, 131, 250, 40) * MathF.Pow(mult,2) * 3 * MathF.Sin(mult * MathF.PI) * Projectile.Opacity * MathHelper.Clamp(Projectile.position.Distance(Projectile.oldPos[i]) / 20f, 0, 1);
			for (int j = -1; j < 2; j += 2)
			{
				Vector2 position = basePosition + new Vector2(5f, 0).RotatedBy(rotation + MathHelper.PiOver2 * j);
				vertices.Add(Helper.AsVertex(position, color, new Vector2(mult + Main.GlobalTimeWrappedHourly*5, j < 0 ? 0 : j)));
			}*/	
		}

		Vector2 lastOldPos = Vector2.Lerp(Projectile.Center, Projectile.oldPos[(int)MathHelper.Clamp(length - 1, 0, 7)], Projectile.Opacity);
		if (lastOldPos.Distance(Projectile.Center) > 1000f)
			lastOldPos = Projectile.Center;
		length = 15f;
		
		for (int k = 0; k < 2; k++)
			for (float i = 0; i < length; i++) {
				float mult = 1f - i / length;
				Vector2 playerPositionOffset = new Vector2(0, MathF.Sin(mult * MathHelper.Pi + (float)Main.timeForVisualEffects * 0.1f + k * 45) 
					* MathF.Cos(mult * MathHelper.Pi * 2.5f + (float)Main.timeForVisualEffects * 0.12f+ k * 145) * (9 - k * 18) * i / length)
					.RotatedBy((player.Center - Projectile.Center).ToRotation()) - Main.screenPosition;
				
				Vector2 nextPlayerPosition = playerPositionOffset + Vector2.SmoothStep(Vector2.Lerp(Projectile.Center, lastOldPos,(i + 1f) / length), Vector2.Lerp(lastOldPos, player.MountedCenter,(i+1f) / length), (i+1f) / length);
				Vector2 basePlayerPosition = playerPositionOffset + Vector2.SmoothStep(Vector2.Lerp(Projectile.Center, lastOldPos,i / length), Vector2.Lerp(lastOldPos, player.MountedCenter,i / length), i / length);
				float rotationPlayer = (basePlayerPosition - nextPlayerPosition).ToRotation();
				float alpha = MathHelper.Clamp((Projectile.Center-Main.screenPosition).Distance(basePlayerPosition) / 20f, 0, 1) * MathF.Pow(mult, 2);
				for (int j = -1; j < 2; j += 2)
				{
					Vector2 position = basePlayerPosition + new Vector2(8f, 0).RotatedBy(rotationPlayer + MathHelper.PiOver2 * j);
					Color color = new Color(116, 131, 250, 40) * alpha;
						verticesPlayer[k].Add(Helper.AsVertex(position, color, new Vector2(mult - Main.GlobalTimeWrappedHourly, j < 0 ? 0 : j)));
				}
			}
		

		if (verticesPlayer[0].Count > 2)
		{
			Main.spriteBatch.End(out var ss);
			Main.spriteBatch.Begin(ss with {samplerState = SamplerState.PointWrap});

			/*Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Assets.Extras.Tentacle.Value);
			Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Assets.Extras.LintyTrail.Value);*/

			for (int i = 0; i < 2; i++)
			{
				Helper.DrawTexturedPrimitives(verticesPlayer[i].ToArray(), PrimitiveType.TriangleStrip,
					Assets.Extras.wavyLaser2.Value);
				Helper.DrawTexturedPrimitives(verticesPlayer[i].ToArray(), PrimitiveType.TriangleStrip,
					Assets.Extras.wavyLaser.Value);
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(ss);
		}
		
		Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value, Projectile.Center + player.GFX()  - Main.screenPosition, null, lightColor * Projectile.Opacity, Projectile.rotation, Projectile.Size / 2f, Projectile.scale, SpriteEffects.None, 0);
		
		return false;
	}

	public override void AI()
	{
		Lighting.AddLight(Projectile.Center, new Vector3(116, 131, 250) / 255 * 0.5f);
		Player player = Main.player[Projectile.owner];

		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1, 0.02f);
		
		if (Projectile.timeLeft < 28)
			Projectile.timeLeft++;

		if (player.whoAmI == Main.myPlayer)
		{
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX), 0.1f);
			player.direction = Main.MouseWorld.X >= player.Center.X ? 1 : -1;
		}

		if (Projectile.velocity != Projectile.oldVelocity)
			Projectile.netUpdate = true;
		
		float rotation = Projectile.velocity.ToRotation();
        
		player.itemTime = 2;
		player.itemAnimation = 2;
		player.heldProj = Projectile.whoAmI;
		Projectile.ModProjectile.DrawHeldProjInFrontOfHeldItemAndArms = true;
		Projectile.Center = Vector2.Lerp(Projectile.Center, player.MountedCenter + Projectile.velocity * 100, 0.25f);
		player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation - MathHelper.PiOver2);
		if (!player.channel || player.statMana <= 0 || !player.CheckMana(1)) Projectile.Kill();

		Projectile.ai[0]++;

		if ((int)Projectile.ai[0] == 1)
		{
			SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen.WithPitchOffset(0.5f), Projectile.Center);
			SoundEngine.PlaySound(SoundID.DD2_EtherianPortalIdleLoop with { IsLooped = true, Pitch = 0.5f }, Projectile.Center, (_) =>
			{
				_.Position = Projectile.Center;
				return Projectile.active && !Main.gameInactive;
			});
		}

		if (Projectile.ai[0] > 25)
		{
			float curveAngle =  MathHelper.Lerp(-0.05f, 0.05f, MathF.Sin(Projectile.ai[0] * 0.01f));
			if (Projectile.ai[0] % 2 == 0)
				MPUtils.NewProjectile(null, Projectile.Center, Main.rand.NextVector2Circular( 3, 3), ModContent.ProjectileType<ItemNameFlower>(),0, 0, Projectile.owner,Projectile.whoAmI, ai2: curveAngle);
			if (Projectile.ai[0] % 50 == 0)
			{
				SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, Projectile.Center);
				float offset = Main.rand.NextFloat(MathHelper.Pi);
				for (int i = 0; i < 5; i++) 
				{
					MPUtils.NewProjectile(null, Projectile.Center, (Helper.CircleDividedEqually(i, 5)+offset).ToRotationVector2() * 7, ModContent.ProjectileType<ItemNameFlowerDamaging>(),Projectile.damage, Projectile.knockBack, Projectile.owner, ai1: curveAngle, ai2: (i == 0 ? -1 : 0));
				}
			}
		}
	}
}

public class ItemNameFlower : ModProjectile
{
	public override string Texture => Helper.AssetPath + "Projectiles/Friendly/Generic/" + Name;
	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Type] = 25;
		ProjectileID.Sets.TrailingMode[Type] = 2;
	}

	public override void SetDefaults()
	{
		Projectile.width = 14;
		Projectile.height = 14;
		Projectile.aiStyle = -1;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.DamageType = DamageClass.Magic;

		Projectile.Opacity = 0;
		Projectile.timeLeft = 55;
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		return base.OnTileCollide(oldVelocity);
	}

	public override bool ShouldUpdatePosition() => Projectile.ai[1] <= 0;

	public override bool PreDraw(ref Color lightColor)
	{
		lightColor = Color.White;
		lightColor.A = 140;
		
		List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();

		float length = 1; 
		for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
		{
			if (Projectile.oldPos[i] == Vector2.Zero) continue;
			length++;
			
			Vector2 basePosition = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
			float mult = 1f - i / length;
			float rotation = (Projectile.oldPos[i] - Projectile.oldPos[i+1]).ToRotation();
			Color color = new Color(116, 131, 250, 40) * MathF.Pow(mult,2) * 4 * MathF.Sin(mult * MathF.PI) * Projectile.Opacity;
			for (int j = -1; j < 2; j += 2)
			{
				Vector2 position = basePosition + new Vector2(5f * Projectile.scale, 0).RotatedBy(rotation + MathHelper.PiOver2 * j);
				vertices.Add(Helper.AsVertex(position, color, new Vector2(mult + Main.GlobalTimeWrappedHourly*5, j < 0 ? 0 : j)));
			}
		}
		

		if (vertices.Count > 2)
		{
			Main.spriteBatch.End(out var ss);
			Main.spriteBatch.Begin(ss with {samplerState = SamplerState.PointWrap});

			Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Assets.Extras.Tentacle.Value);
			Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Assets.Extras.LintyTrail.Value);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(ss);
		}
		return base.PreDraw(ref lightColor);
	}

	public Vector2 PositionOffset;
	public override void AI()
	{
		int projectileOwner = (int)Projectile.ai[0];
		Projectile projectile = Main.projectile[projectileOwner];
		Projectile.ai[1] = 0;
		if (projectile.type == ModContent.ProjectileType<ItemNameProjectile>() && projectile.active && projectile.owner == Projectile.owner && Projectile.timeLeft > 30)
		{
			Projectile.Center = projectile.Center + PositionOffset;
				Projectile.ai[1] = 1;
		}
		
		Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[2]);
		PositionOffset += Projectile.velocity;
		
		Projectile.scale = MathHelper.Lerp(Projectile.scale, 0, 0.05f);
		if (Projectile.timeLeft > 20)
			Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1, 0.05f);
		else
			Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0, 0.1f);
	}
}
public class ItemNameFlowerDamaging : ModProjectile
{
	public override string Texture => Helper.AssetPath + "Projectiles/Friendly/Generic/ItemNameFlower";
	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[Type] = 25;
		ProjectileID.Sets.TrailingMode[Type] = 2;
	}

	public override void SetDefaults()
	{
		Projectile.width = 14;
		Projectile.height = 14;
		Projectile.aiStyle = -1;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.DamageType = DamageClass.Magic;

		Projectile.Opacity = 0;
		Projectile.timeLeft = 255;
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		return base.OnTileCollide(oldVelocity);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		lightColor = Color.White;
		lightColor.A = 140;
		
		List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();

		float length = 1; 
		for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
		{
			if (Projectile.oldPos[i] == Vector2.Zero) continue;
			length++;
			
			Vector2 basePosition = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
			float mult = 1f - i / length;
			float rotation = (Projectile.oldPos[i] - Projectile.oldPos[i+1]).ToRotation();
			Color color = new Color(116, 131, 250, 40) * MathF.Pow(mult, 1.5f) * 4 * MathF.Sin(mult * MathF.PI) * Projectile.Opacity;
			for (int j = -1; j < 2; j += 2)
			{
				Vector2 position = basePosition + new Vector2(5f * Projectile.scale, 0).RotatedBy(rotation + MathHelper.PiOver2 * j);
				vertices.Add(Helper.AsVertex(position, color, new Vector2(mult + Main.GlobalTimeWrappedHourly*5, j < 0 ? 0 : j)));
			}
		}
		

		if (vertices.Count > 2)
		{
			Main.spriteBatch.End(out var ss);
			Main.spriteBatch.Begin(ss with {samplerState = SamplerState.PointWrap});

			Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Assets.Extras.wavyLaser.Value);
			Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Assets.Extras.FlamesSeamless.Value);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(ss);
		}
		return base.PreDraw(ref lightColor);
	}

	public override void AI()
	{
		Projectile.ai[0]++;
		if (Projectile.ai[0] < 25)
			Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[1]) * 0.96f;

		if ((int)Projectile.ai[0] == 30 && (int)Projectile.ai[2] == -1)
			SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse, Projectile.Center);
		
		if (Projectile.timeLeft < 50)
			Projectile.ai[2] = 1;

		if (Projectile.owner == Main.myPlayer && Projectile.ai[0] > 50 && Projectile.ai[2] <= 0)
		{
			if (Projectile.velocity.Length() < 36f)
				Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * 2;

			if (Projectile.Distance(Main.MouseWorld) < 200f)
			{
				Projectile.timeLeft = 50;
				Projectile.ai[2] = 1;
			}

			Projectile.netUpdate = true;
		}

		if (Projectile.ai[2] >= 1)
		{
			Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0, 0.1f);
			if (Projectile.Opacity > 0)
				Projectile.Opacity -= 0.01f;
			Projectile.velocity *= 0.97f;
		}
		else
			Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1, 0.05f);
	}
}