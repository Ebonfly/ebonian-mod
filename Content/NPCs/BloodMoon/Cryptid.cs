using System;
using EbonianMod.Common.Globals;
using EbonianMod.Common.Misc;
using EbonianMod.Content.Projectiles.Enemy.BloodMoon;
using Terraria.GameContent.Bestiary;

namespace EbonianMod.Content.NPCs.BloodMoon;

public class Cryptid : CommonNPC
{
	public override string Texture => Helper.AssetPath + "NPCs/BloodMoon/" + Name;

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 19;
	}
	public override float SpawnChance(NPCSpawnInfo spawnInfo)
	{
		if (Main.invasionType > 0) return 0;
		return Main.hardMode && spawnInfo.Player.ZoneCrimson && spawnInfo.Player.ZoneOverworldHeight ? 0.07f : 0;
	}
	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
			new FlavorTextBestiaryInfoElement("Mods.EbonianMod.Misc.Types.BigBloodMoon"),
			new FlavorTextBestiaryInfoElement(NPC.BestiaryKey()),
		});
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(76, 94);
		NPC.aiStyle = -1;
		NPC.damage = 30;
		NPC.defense = 4;
		NPC.lifeMax = 750;
		NPC.knockBackResist = 0.01f;
		NPC.value = Item.buyPrice(0, 0, 15, 0);
		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		spriteBatch.Draw(TextureAssets.Npc[Type].Value, NPC.Center + _visualOffset + gfxOff + new Vector2(0, 2) - screenPos, NPC.frame, NPC.HunterPotionColor(drawColor), NPC.rotation, NPC.Size * 0.5f, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
		spriteBatch.Draw(Assets.NPCs.BloodMoon.Cryptid_Body.Value, NPC.Center + _visualOffset + gfxOff + new Vector2(0, 4) - screenPos, BodyFrame, NPC.HunterPotionColor(drawColor), NPC.rotation, NPC.Size * 0.5f, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
		spriteBatch.Draw(Assets.NPCs.BloodMoon.Cryptid_Glow.Value, NPC.Center + _visualOffset + gfxOff + new Vector2(0, 4) - screenPos, BodyFrame, Color.White, NPC.rotation, NPC.Size * 0.5f, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

		if (BodyFrame.Y is 16 * 96 or 17 * 96 && (States)AIState == States.Land)
		{
			Rectangle ClawFrame = BodyFrame with { Width = 108, Height = 118, Y = (BodyFrame.Y - BodyFrame.Height * 16) / 96 * 118 };
			spriteBatch.Draw(Assets.NPCs.BloodMoon.Cryptid_Claw.Value, NPC.Center + _visualOffset + gfxOff + new Vector2(0, -4) - screenPos, ClawFrame, Color.White with {A = 0}, NPC.rotation, ClawFrame.Size() * 0.5f, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
		}
		return false;
	}

	enum States
	{
		Walk,
		Brace,
		Leap,
		Land,
		ChargeExplosion,
		Explosion
	}

	private Vector2 _visualOffset;
	public override void AI()
	{
		Lighting.AddLight(NPC.Center, Color.Gold.ToVector3()*0.2f);
		NPC.TargetClosest(false);
		
		Player player = Main.player[NPC.target];
		float distanceToPlayer = player.Distance(NPC.Center);

		void SwitchToLeap()
		{
			SoundEngine.PlaySound(Sounds.exolDash.WithPitchOffset(0.5f), NPC.Center);
			for (int i = 0; i < 40; i++)
			{
				Dust.NewDustDirect(NPC.BottomLeft, NPC.width, 1, i % 4 == 0 ? DustID.Enchanted_Gold : DustID.Dirt,
					(i - 20f) * Main.rand.NextFloat(0.2f, 0.6f), Main.rand.NextFloat(-10f, -2.5f) + MathF.Abs(i - 20f)*0.2f);
			}
			
			float velocityX = MathHelper.Clamp(0.05f*(player.Center.X - NPC.Center.X), -10f, 10f);
			NPC.velocity = new Vector2(velocityX, -20);
			AITimer = 0;
			NPC.netUpdate = true;
			AIState = (int)States.Leap;
		}
		
		switch ((States)AIState)
		{
			case States.Walk:
			{
				NPC.FaceTarget();
				NPC.damage = 0;

				if (MathF.Abs(player.Center.X - NPC.Center.X) > 40)
					NPC.GetGlobalNPC<FighterGlobalAI>().FighterAI(NPC, 5, 2.5f);
				else
					NPC.velocity.X *= 0;

				if (distanceToPlayer < 400)
					AITimer++;
				if (distanceToPlayer < 70)
					AITimer += 2;
				if (distanceToPlayer < 40)
					AITimer += 2;

				if (AITimer > 200 && distanceToPlayer < 300 && NPC.Grounded())
					SwitchState((int)States.Brace);
			}
				break;
			case States.Brace:
			{
				_visualOffset = MathHelper.SmoothStep(0, 2.5f,AITimer / 25f) * Main.rand.NextVector2Unit();
				if (AITimer < 5)
					NPC.FaceTarget();
				NPC.velocity.X *= 0.9f;
				AITimer++;

				if (AITimer > 25)
					SwitchToLeap();
			}
				break;

			case States.Leap:
			{
				AITimer++;
				NPC.velocity.Y ++;

				if (NPC.velocity.Y > 1)
				{
					NPC.velocity.X *= 0.98f;
				}


				if (NPC.Grounded() && AITimer > 3 && NPC.velocity.Y > 4)
				{
					Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
						NPC.Center, Vector2.Zero,
						ProjectileType<CryptidLanding>(), 20, 0);
					NPC.velocity.X *= 0;
					AITimer = 0;
					NPC.netUpdate = true;
					AIState = (int)States.Land;
				}
			}
				break;

			case States.Land:
			{
				_visualOffset = MathHelper.SmoothStep(0, 2.5f,AITimer / 25f) * Main.rand.NextVector2Unit();
				AITimer++;
				if (AITimer > 15)
					NPC.FaceTarget();

				if (AITimer > 25)
				{
					AITimer3++;
					if (AITimer3 > 5) 
						SwitchState((int)States.ChargeExplosion);
					else
						SwitchToLeap();
				}
			}
				break;

			case States.ChargeExplosion:
			{
				NPC.velocity.X *= 0f;
				_visualOffset = SmoothStep(0, 3.5f,(AITimer / 35f).Saturate()) * Main.rand.NextVector2Unit();
				AITimer++;

				float dustVelocityX = Main.rand.NextFloat(-8f, 8f);
				Dust.NewDustDirect(NPC.BottomLeft, NPC.width, 1, Main.rand.NextBool(4) ? DustID.Enchanted_Gold : DustID.Dirt,
					dustVelocityX, Main.rand.NextFloat(-10f, -2.5f) + MathF.Abs(dustVelocityX)*0.2f);
				
				if (AITimer > 50)
				{
					Vector2 dustVelocity = new Vector2(dustVelocityX, Main.rand.NextFloat(-10f, -2.5f) + MathF.Abs(dustVelocityX) * 0.2f);
					Vector2 dustPosition = NPC.Bottom + new Vector2(Main.rand.NextFloatDirection() * AITimer * 3, 0);
					dustPosition = Helper.Raycast(dustPosition - new Vector2(0, 40), Vector2.UnitY, 100).Point;
					Dust.NewDustPerfect(dustPosition, DustID.Enchanted_Gold, dustVelocity);
					
				}
				
				if (AITimer > 100)
				{
					SwitchState((int)States.Explosion);
				}
			}
				break;

			case States.Explosion:
			{
				if ((int)AITimer == 10)
					Projectile.NewProjectileDirect(NPC.GetSource_FromAI(),
						Helper.Raycast(NPC.Center, Vector2.UnitY, 100).Point + new Vector2(NPC.direction * 18, 0), Vector2.Zero,
						ProjectileType<CryptidLanding>(), 40, 0, ai2: 1);
				if (++AITimer > 25)
					SwitchState((int)States.Walk);
			}
				break;
		}
		_visualOffset = Vector2.Lerp(_visualOffset, Vector2.Zero, 0.1f);
		NPC.spriteDirection = NPC.direction;
	}

	private Rectangle BodyFrame;
	public override void FindFrame(int frameHeight)
	{
		BodyFrame.Width = NPC.frame.Width;
		BodyFrame.Height = NPC.frame.Height;
		NPC.frameCounter++;
		switch ((States)AIState)
		{
			case States.Walk:
			{
				if (MathF.Abs(NPC.velocity.Y) > 0.05f)
				{
					NPC.frame.Y = 2 * frameHeight;
					return; 
				}

				if (NPC.frameCounter % 4 != 0)
					return;

				if (MathF.Abs(NPC.velocity.X) > 0.05f)
				{
					if (NPC.frame.Y < frameHeight)
						NPC.frame.Y = frameHeight;	
			
					if ((NPC.frame.Y += frameHeight) > 8 * frameHeight)
						NPC.frame.Y = frameHeight;
				}
				else
					NPC.frame.Y = 0;
			}
				break;

			case States.Brace:
			{
				if (NPC.frame.Y < 9 * frameHeight || NPC.frame.Y > 12 * frameHeight)
					NPC.frame.Y = 9 * frameHeight;
				if (NPC.frameCounter % 5 == 0 && NPC.frame.Y < frameHeight * 12)
					NPC.frame.Y += frameHeight;
			}
				break;

			case States.Leap:
			{
				if (NPC.velocity.Y < 0)
					NPC.frame.Y = 13 * frameHeight;
				else
					NPC.frame.Y = 14 * frameHeight;
			}
				break;

			case States.Land:
			case States.ChargeExplosion:
			{
				if (NPC.frame.Y < 15 * frameHeight || NPC.frame.Y > 18 * frameHeight)
					NPC.frame.Y = 15 * frameHeight;
				if (NPC.frameCounter % 5 == 0 && NPC.frame.Y < frameHeight * 18)
					NPC.frame.Y += frameHeight;
			}
				break;
			
			case States.Explosion:
			{
				if (NPC.frame.Y < 13 * frameHeight || NPC.frame.Y > 17 * frameHeight)
					NPC.frame.Y = 17 * frameHeight;
				if (NPC.frame.Y > frameHeight * 13 && NPC.frameCounter % 5 == 0)
					NPC.frame.Y -= frameHeight;
			}
				break;
		}
		
		BodyFrame = NPC.frame;
	}
}