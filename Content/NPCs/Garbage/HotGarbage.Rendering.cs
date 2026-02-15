namespace EbonianMod.Content.NPCs.Garbage;

// TODO: clean this
public partial class HotGarbage : ModNPC
{
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 pos, Color lightColor)
    {
        Texture2D drawTexture = TextureAssets.Npc[Type].Value;
        Texture2D glow = Assets.NPCs.Garbage.HotGarbage_Glow.Value;
        Texture2D fire = Assets.NPCs.Garbage.HotGarbage_Fire.Value;
        Texture2D fireball = Assets.Extras.fireball.Value;
        Vector2 origin = new Vector2((drawTexture.Width / 3) * 0.5F, (drawTexture.Height / Main.npcFrameCount[NPC.type]) * 0.5F);

        Vector2 drawPos = new Vector2(
            NPC.position.X - pos.X + (NPC.width / 3) - (TextureAssets.Npc[Type].Value.Width / 3) * NPC.scale / 3f + origin.X * NPC.scale,
            NPC.position.Y - pos.Y + NPC.height - TextureAssets.Npc[Type].Value.Height * NPC.scale / Main.npcFrameCount[NPC.type] + 4f + origin.Y * NPC.scale + NPC.gfxOffY);
        drawPos.Y -= 2;
        SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        spriteBatch.Draw(drawTexture, drawPos, NPC.frame, lightColor, NPC.rotation, origin, NPC.scale, effects, 0);
        spriteBatch.Draw(glow, drawPos, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, effects, 0);
        if (AIState != State.Intro && AIState != State.Idle && AIState != State.OpenLid && AIState != State.SpewFire && AIState != State.CloseLid && AIState != State.FallOver && AIState != State.SpewFire2 && AIState != State.BouncingBarrels && NPC.frame.X == 80)
            spriteBatch.Draw(fire, drawPos + new Vector2(NPC.width * -NPC.direction + (NPC.direction == 1 ? 9 : 0), 2).RotatedBy(NPC.rotation) * NPC.scale, new Rectangle(0, NPC.frame.Y - 76 * 3, 70, 76), Color.White, NPC.rotation, origin, NPC.scale, effects, 0);

        return false;
    }
    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D flame = Assets.NPCs.Garbage.HotGarbage_FlameOverlay.Value;
        Vector2 origin = new Vector2((flame.Width / 3) * 0.5F, (flame.Height / Main.npcFrameCount[NPC.type]) * 0.5F);

        Vector2 drawPos = new Vector2(
            NPC.position.X + (NPC.width / 3) - (TextureAssets.Npc[Type].Value.Width / 3) * NPC.scale / 3f + origin.X * NPC.scale,
            NPC.position.Y + NPC.height - TextureAssets.Npc[Type].Value.Height * NPC.scale / Main.npcFrameCount[NPC.type] + 4f + origin.Y * NPC.scale + NPC.gfxOffY);

        SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        SpritebatchParameters sbParams = spriteBatch.Snapshot();
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, Effects.flame.Value, Main.GameViewMatrix.TransformationMatrix);
        Effects.flame.Value.Parameters["uTime"].SetValue(-Main.GlobalTimeWrappedHourly * .4f);
        Effects.flame.Value.Parameters["tex"].SetValue(Assets.Extras.smearNoise.Value);
        Effects.flame.Value.Parameters["scale"].SetValue(5);
        Effects.flame.Value.Parameters["wavinessMult"].SetValue(1);
        Effects.flame.Value.Parameters["intensity"].SetValue(10);
        Effects.flame.Value.Parameters["colOverride"].SetValue(new Vector4(1, 0.25f, 0, 1));
        spriteBatch.Draw(flame, NPC.Center - Main.screenPosition + new Vector2(0, 2 + NPC.gfxOffY), NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, effects, 0);
        spriteBatch.End();
        spriteBatch.ApplySaved(sbParams);
    }

    public override void FindFrame(int f)
    {
        int frameHeight = 76;
        NPC.frame.Width = 80;
        NPC.frame.Height = 76;
        NPC.frameCounter++;

        if (NPC.frameCounter % 5 == 0)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                NPC.frame.X = 80;
                if (NPC.frame.Y < 2 * frameHeight)
                {
                    NPC.frame.Y += frameHeight;
                }
                else
                {
                    NPC.frame.Y = 0;
                }
                return;
            }

            switch (AnimationStyle)
            {
                case AnimationStyles.Still:
                    NPC.frame.X = 0;
                    NPC.frame.Y = 0;
                    break;
                case AnimationStyles.Intro:
                    NPC.frame.X = 0;
                    if (NPC.frame.Y < frameHeight * 12)
                        NPC.frame.Y += frameHeight;
                    break;

                case AnimationStyles.Idle:
                    NPC.frame.X = 80;
                    if (NPC.frame.Y < 2 * frameHeight)
                        NPC.frame.Y += frameHeight;
                    else
                        NPC.frame.Y = 0;
                    break;

                case AnimationStyles.BoostWarning:
                    NPC.frame.X = 80;
                    if (NPC.frame.Y < 5 * frameHeight)
                    {
                        NPC.frame.Y += frameHeight;
                    }
                    else if (NPC.frame.Y >= 5 * frameHeight || NPC.frame.Y < 3 * frameHeight)
                    {
                        NPC.frame.Y = 3 * frameHeight;
                    }
                    break;
                
                case AnimationStyles.Boost:
                    NPC.frame.X = 80;
                    if (NPC.frame.Y < 9 * frameHeight)
                    {
                        NPC.frame.Y += frameHeight;
                    }
                    else if (NPC.frame.Y >= 9 * frameHeight || NPC.frame.Y < 6 * frameHeight)
                    {
                        NPC.frame.Y = 6 * frameHeight;
                    }
                    break;
                
                case AnimationStyles.Open:
                    NPC.frame.X = 160;
                    if (NPC.frame.Y < 3 * frameHeight)
                    {
                        NPC.frame.Y += frameHeight;
                    }
                    break;
                
                case AnimationStyles.Close:
                    NPC.frame.X = 160;
                    if (NPC.frame.Y == frameHeight)
                        SoundEngine.PlaySound(SoundID.DrumFloorTom with { Pitch = 0.5f, PitchVariance = 0.1f }, NPC.Center);
                    if (NPC.frame.Y > 0)
                    {
                        NPC.frame.Y -= frameHeight;
                    }
                    break;
            }
        }
    }
}