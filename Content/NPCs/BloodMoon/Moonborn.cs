using System;
using System.Diagnostics;
using Terraria;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Terraria.ModLoader.PlayerDrawLayer;

public class Moonborn : ModNPC
{
    public override string Texture => Helper.AssetPath + "NPCs/BloodMoon/Moonborn";

    public override void SetDefaults()
    {
        NPC.Size = new Vector2(60, 80);
        //NPC.HitSound = SoundID.Item49;
        //NPC.DeathSound = SoundID.Item27;
        NPC.damage = 1;
        NPC.defense = 0;
        NPC.lifeMax = 2000;
        NPC.noTileCollide = true;
        NPC.noGravity = true;
    }

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 8;
    }
    public class Leg
    {
        public Vector2 RawPosition, Position, TargetPosition, ReferenceOffset;
        public float Speed = 1, Range, MaxDistance, VerticalOffset;
        public bool DefaultState, IsMoving;
    }
    Leg[] Legs = new Leg[6];
    bool IsGrounded, LandingTrigger = true, IsInBlocks, HasWalls;
    Vector2 MovementDirection;
    public override void OnSpawn(IEntitySource source)
    {
        for (int i = 0, j = 1; i < 6; i++, j *= -1)
        {
            Legs[i] = new Leg();
            Legs[i].TargetPosition = NPC.Center + new Vector2((1 - (i + 2) / 20) * 20 * j, 0);
            Legs[i].RawPosition = Legs[i].TargetPosition;
            Legs[i].MaxDistance = Main.rand.NextFloat(108, 126);
        }
    }

    public override void AI()
    {
        NPC.TargetClosest(true);
        Player player = Main.player[NPC.target];

        NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;
        Helper.RaycastData bodyCast = Helper.Raycast(NPC.Center, Vector2.UnitY, 110, true, false);
        Helper.RaycastData frontCast = Helper.Raycast(NPC.Center, new Vector2(NPC.direction, 0), 76, true, IsInBlocks, true);
        Helper.RaycastData backCast =  Helper.Raycast(NPC.Center, new Vector2(-NPC.direction, 0), 76, true, IsInBlocks, true);
        MovementDirection = new Vector2(NPC.velocity.X > 0 ? 1 : -1, NPC.velocity.Y > 0 ? 1 : -1);
        HasWalls = frontCast.Success && backCast.Success;
        IsGrounded = bodyCast.Success || HasWalls;
        IsInBlocks = bodyCast.RayLength < 12;

        if (IsGrounded)
        {
            if(LandingTrigger)
            {
                NPC.velocity.Y = Min(NPC.velocity.Y, 18);
                LandingTrigger = false;
            }

            Vector2 bodyToPlayer = player.Center - NPC.Center;
            if (Helper.Raycast(NPC.Center, bodyToPlayer, bodyToPlayer.Length(), true).Success || HasWalls)
            {
                NPC.velocity.Y = Lerp(NPC.velocity.Y, Distance(player.Center.Y, NPC.Center.Y + 100) > 32 ? (player.Center.Y > NPC.Center.Y + 100 ? 4f : -4f) : 0, 0.03f);
            }
            else
            {
                NPC.Center = Vector2.Lerp(NPC.Center, new Vector2(NPC.Center.X, bodyCast.Point.Y - 100), 0.23f);
                if (NPC.velocity.Y > 0.5f) NPC.velocity.Y = Lerp(NPC.velocity.Y, 0, 0.1f);
                else NPC.velocity.Y = 0;
            }
            float xToPlayer = MathF.Abs(bodyToPlayer.X);
            if (xToPlayer > 160 || MathF.Abs(NPC.velocity.X) > 4)
            {
                NPC.velocity.X += NPC.direction * xToPlayer / (NPC.velocity.X * NPC.direction > 0 ? 7000 : 100);
                NPC.velocity.X = Clamp(NPC.velocity.X, -10, 10);
            }
            else NPC.velocity.X = Lerp(NPC.velocity.X, NPC.direction * 2, 0.03f);
            NPC.rotation = NPC.velocity.X / 40;
        }
        else
        {
            LandingTrigger = true;
            NPC.velocity.Y += NPC.ai[0];
            if (NPC.ai[0] < 0.5f) NPC.ai[0] += 0.1f;
        }

        for (int i = 0, j = 1; i < 6; i++, j *= -1)
        {
            float distanceToTargetPosition = Vector2.Distance(NPC.Center, Legs[i].TargetPosition);
            bool front = j == MovementDirection.X;
            if (Legs[i].IsMoving)
            {
                float distance = Vector2.Distance(Legs[i].RawPosition, Legs[i].TargetPosition);
                Legs[i].Speed = Max(NPC.velocity.Length() * 2, 5);
                Legs[i].RawPosition += (Legs[i].TargetPosition - Legs[i].RawPosition).SafeNormalize(Vector2.UnitY) * Max(NPC.velocity.Length() * 2, 5);
                if (Legs[i].Range != 0) Legs[i].VerticalOffset = (-MathF.Pow(distance * 2 - Legs[i].Range, 2) / (Legs[i].Range * Legs[i].Range) + 1) * 30;
                Legs[i].Position = new Vector2(Legs[i].RawPosition.X, Legs[i].RawPosition.Y - Legs[i].VerticalOffset);
                if (distance < Legs[i].Speed)
                {
                    Legs[i].Position = Legs[i].TargetPosition;
                    Legs[i].IsMoving = false;
                }
            }
            else
            {
                float multiplier = 1 - (i + 2) / 8f;
                if (IsGrounded)
                {
                    if (distanceToTargetPosition > Legs[i].MaxDistance)
                    {
                        float chosenLength = (j == 1 ? frontCast : backCast).RayLength;

                        if (IsInBlocks)
                        {
                            Legs[i].TargetPosition = Helper.Raycast(NPC.Center + new Vector2(0, j * (chosenLength - 8) * multiplier).RotatedBy(MovementDirection.ToRotation()), Vector2.UnitY, 0, true, IsInBlocks, true).Point;
                        }
                        else
                        {
                            Legs[i].TargetPosition = bodyCast.Point;
                            if (front || HasWalls)
                            {
                                for (float offset = chosenLength * multiplier; offset > 0; offset -= 16)
                                {
                                    Helper.RaycastData verticalcast = Helper.Raycast(new Vector2(NPC.Center.X + offset * j, NPC.Center.Y), -Vector2.UnitY, 200, true, IsInBlocks);
                                    if (verticalcast.Success)
                                    {
                                        Legs[i].TargetPosition = verticalcast.Point;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                float otherLength = (j == 1 ? backCast : frontCast).RayLength;
                                for (float offset = 0; offset < otherLength * multiplier; offset += 16)
                                {
                                    Helper.RaycastData verticalcast = Helper.Raycast(new Vector2(NPC.Center.X - offset * j, NPC.Center.Y), Vector2.UnitY, 110, true, IsInBlocks);
                                    if (verticalcast.Success)
                                    {
                                        Legs[i].TargetPosition = verticalcast.Point;
                                        break;
                                    }
                                }
                            }
                        }
                        Legs[i].MaxDistance = IsInBlocks ? 100 : 126;
                        Legs[i].Range = Vector2.Distance(Legs[i].RawPosition, Legs[i].TargetPosition);
                        Legs[i].IsMoving = true;
                    }
                }
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (!NPC.IsABestiaryIconDummy)
        {
            for (int i = 0, j = 1; i < 6; i++, j *= -1)
            {
                Vector2 jointPosition = NPC.Center + new Vector2(70, 0).RotatedBy((Legs[i].Position - NPC.Center).ToRotation() - j * MathF.Acos(Min((Legs[i].Position - NPC.Center).Length(), 126) / 126));
                SpriteEffects flip = j == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
                spriteBatch.Draw(Assets.NPCs.BloodMoon.MoonbornLegSegment1.Value, NPC.Center - screenPos + NPC.GFX(), null, NPC.HunterPotionColor(drawColor), (jointPosition - NPC.Center).ToRotation(), new Vector2(0, 8), NPC.scale, flip, 0);
                spriteBatch.Draw(Assets.NPCs.BloodMoon.MoonbornLegSegment2.Value, jointPosition - screenPos + NPC.GFX(), null, NPC.HunterPotionColor(drawColor), (Legs[i].Position - jointPosition).ToRotation(), new Vector2(7, 10), NPC.scale, flip, 0);
            }
        }
        return true;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        if (NPC.frameCounter > 7)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y += frameHeight;
            if (NPC.frame.Y > frameHeight * 3)
            {
                NPC.frame.Y = 0;
            }
        }
    }
}