using System;
using System.Collections.Generic;
using EbonianMod.Content.Dusts;
using ReLogic.Utilities;

namespace EbonianMod.Content.NPCs.Garbage;

public partial class HotGarbage : ModNPC
{
    public readonly List<Vector2> RedFrames = new List<Vector2>
    {
        new(0, 76*8), new(0, 76*10), new(0, 76*11), new(0, 76*12),

        new(80, 0), new(80, 76*1), new(80, 76*2),

        new(80*2, 0), new(80*2, 76*1), new(80*2, 76*2), new(80*2, 76*3)
    };
    public readonly List<Vector2> YellowFrames = new List<Vector2>
    {
        new(80, 76*3), new(80, 76*4), new(80, 76*5)
    };
    public readonly List<Vector2> GreenFrames = new List<Vector2>
    {
        new(80, 76*6), new(80, 76*7), new(80, 76*8), new(80, 76*9)
    };
    
    public readonly List<State> AttackPool = new List<State>() { 
        State.WarningForDash, State.WarningForBigDash,  State.SlamPreperation, State.MailBoxes, State.PipeBombAirstrike, State.MassiveLaser,
        
        State.OpenLid, State.OpenLid, State.OpenLid, State.OpenLid, State.OpenLid, State.OpenLid, 
    };
    public readonly List<State> OpenAttackPool = new List<State>()
    {
        State.SpewFire, State.SpewFire2, State.GiantFireball, State.TrashBags, State.SodaMissiles, State.SateliteLightning
    };
    
    public bool StruckDead;
    public bool PerformedFullMoveset;
    public Vector2 DisposablePosition;
    public SlotId LaserSoundSlot, LoopedBoilSound;
    public Vector2 CachedVelocityForVFX;
    
    void AmbientFX() {
        AnimationStyle = AnimationStyles.Idle;

        if (Main.GameUpdateCount % 4 == 2)
            CachedVelocityForVFX = NPC.velocity;
    
        if (NPC.velocity.Length() > 1f)
            Dust.NewDustDirect(NPC.Center + new Vector2(-NPC.width / (NPC.direction == -1 ? 2.5f : 8f), -8).RotatedBy(NPC.rotation), NPC.width / 2, 2, DustID.Poop, NPC.velocity.X, NPC.velocity.Y, Scale: 0.5f);

        if (NPC.Grounded() && MathF.Abs(NPC.velocity.X) is > 0.75f and < 5f && MathF.Abs(NPC.velocity.X) < MathF.Abs(CachedVelocityForVFX.X) && NPC.rotation == 0f)
        {
            if (Main.GameUpdateCount % 4 == 0)
                SoundEngine.PlaySound(SoundID.Item55 with { Volume = 0.3f }, NPC.Center);
            
            float[] wheelOffsets = [16, NPC.width / 2f + 6f, NPC.width - 10f];
            Vector2 position = NPC.Bottom + new Vector2(-NPC.width / 2f + Main.rand.Next(wheelOffsets), -2) * NPC.direction + Main.rand.NextVector2Circular(2, 5);
            Dust.NewDustPerfect(position, ModContent.DustType<LineDustFollowPoint>(), new Vector2(MathHelper.Clamp(-NPC.velocity.X * 0.5f, -3f, 3f), Main.rand.NextFloat(-1.5f, -0.5f)), newColor: Color.OrangeRed, Scale: 0.08f).noGravity = true;
        }

        if (!Main.dedServ && !SoundEngine.TryGetActiveSound(LoopedBoilSound, out var sound))
        {
            /*LoopedBoilSound = SoundEngine.PlaySound(??? with { Pitch = -1f, IsLooped = true, Type = SoundType.Sound }, NPC.Center,
                (_) =>
                {
                    _.Position = NPC.Center;
                    return NPC.active && NPC.type == Type && !Main.gameInactive;
                });*/
            
            // Needs a good sound
        }
        
        if (RedFrames.Contains(new(NPC.frame.X, NPC.frame.Y)))
            Lighting.AddLight(NPC.Center, TorchID.Red);
        if (YellowFrames.Contains(new(NPC.frame.X, NPC.frame.Y)))
            Lighting.AddLight(NPC.Center, TorchID.Yellow);
        if (GreenFrames.Contains(new(NPC.frame.X, NPC.frame.Y)))
            Lighting.AddLight(NPC.Center, TorchID.Green);
        if (NPC.frame.X == 80 * 2 && NPC.frame.Y > 0)
        {
            Lighting.AddLight(NPC.Center, TorchID.Torch);
        }
    }
    
    void TargetingLogic() {
        NPC.TargetClosest(false);
        if (AIState != State.Death)
        {
            bool shouldntDespawn = false;
            foreach (Player p in Main.ActivePlayers)
                if (!p.dead)
                {
                    shouldntDespawn = true;
                    break;
                }
            if (!shouldntDespawn)
            {
                if (NPC.timeLeft > 10)
                {
                    NPC.timeLeft = 10;
                }
                NPC.active = false;
                return;
            }
        }
        NPC.timeLeft = 10;
    }
    
    void FacePlayer() => NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;

    void ResetTo(State state, State? stateAfterOpenLid = null)
    {
        NextAttack = state;
        if (stateAfterOpenLid != null)
            NextAttack2 = stateAfterOpenLid.Value;
        
        AIState = State.Idle;
        AITimer = 0;
        AITimer2 = 0;
        AITimer3 = 0;
        NPC.damage = 0;
        NPC.netUpdate = true;
    }  
    
    void JumpCheck()
    {
        if (NPC.collideX || Helper.Raycast(NPC.Center, Vector2.UnitX, 1000).RayLength < NPC.width*0.5f || Helper.Raycast(NPC.Center, -Vector2.UnitX,  NPC.width).RayLength < NPC.width*0.5f)
            NPC.velocity.Y = -4f;
        if (Helper.Raycast(NPC.Center, -Vector2.UnitY, NPC.height).RayLength < NPC.height - 1 && !Collision.CanHit(NPC, player))
        {
            if (!NPC.noTileCollide)
            {
                NPC.noTileCollide = true;
                NPC.netUpdate = true;
            }

            if (player.Center.Y < NPC.Center.Y)
                NPC.Center -= Vector2.UnitY * 2;
            else
                NPC.Center += Vector2.UnitY * 2;

            NPC.Center += new Vector2(Helper.FromAToB(NPC.Center, player.Center).X * 2, 0);
        }
        else if ((!Collision.CanHit(NPC, player) || !Collision.CanHitLine(NPC.TopLeft, 10, 10, player.position, player.width, player.height) || !Collision.CanHitLine(NPC.TopRight, 10, 10, player.position, player.width, player.height)) && player.Center.X.InRange(NPC.Center.X, NPC.width))
        {
            if (!NPC.noTileCollide)
            {
                NPC.noTileCollide = true;
                NPC.netUpdate = true;
            }

            if (player.Center.Y < NPC.Center.Y)
                NPC.Center -= Vector2.UnitY * 2;
            else
                NPC.Center += Vector2.UnitY * 2;
        }
        else if (NPC.noTileCollide)
        {
            NPC.noTileCollide = false;
            NPC.netUpdate = true;
        }
    }
}