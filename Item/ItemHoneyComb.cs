﻿using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Vintagestory.GameContent
{
    public class ItemHoneyComb : Item
    {
        public float ContainedHoneyLitres = 0.2f;

        public bool CanSqueezeInto(Block block, BlockPos pos, BlockSelection blockSel)
        {
            if (block is BlockLiquidContainerTopOpened blcto)
            {
                return pos == null || !blcto.IsFull(pos);
            }

            if (pos != null && blockSel != null)
            {
                var beg = api.World.BlockAccessor.GetBlockEntity(pos) as BlockEntityGroundStorage;
                if (beg != null)
                {
                    ItemSlot squeezeIntoSlot = beg.GetSlotAt(blockSel);

                    if (squeezeIntoSlot?.Itemstack.Block != null)
                    {
                        return squeezeIntoSlot != null && !(squeezeIntoSlot.Itemstack.Block as BlockLiquidContainerTopOpened).IsFull(squeezeIntoSlot.Itemstack);
                    }
                    
                }
            }

            return false;
        }

        WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "honeyCombInteractions", () =>
            {
                List<ItemStack> stacks = new List<ItemStack>();

                foreach (Block block in api.World.Blocks)
                {
                    if (block.Code == null) continue;

                    if (CanSqueezeInto(block, null, null))
                    {
                        stacks.Add(new ItemStack(block));
                    }
                }

                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "heldhelp-squeeze",
                        HotKeyCode = "shift",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = stacks.ToArray()
                    }
                };
            });
        }



        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel?.Block != null && CanSqueezeInto(blockSel.Block, blockSel.Position) && byEntity.Controls.ShiftKey)
            {
                handling = EnumHandHandling.PreventDefault;
                if (api.World.Side == EnumAppSide.Client)
                {
                    byEntity.World.PlaySoundAt(new AssetLocation("sounds/player/squeezehoneycomb"), byEntity, null, true, 16, 0.5f);
                }
            }
            else
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent,ref handling);
            }
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (blockSel?.Block != null && CanSqueezeInto(blockSel.Block, blockSel.Position))
            {
                if (!byEntity.Controls.ShiftKey) return false;
                if (byEntity.World is IClientWorldAccessor)
                {
                    ModelTransform tf = new ModelTransform();
                    tf.EnsureDefaultValues();

                    tf.Translation.Set(Math.Min(0.6f, secondsUsed * 2), 0,
                        0); //-Math.Min(1.1f / 3, secondsUsed * 4 / 3f)
                    tf.Rotation.Y = Math.Min(20, secondsUsed * 90 * 2f);

                    if (secondsUsed > 0.4f)
                    {
                        tf.Translation.X += (float)Math.Sin(secondsUsed * 30) / 10;
                    }

                    byEntity.Controls.UsingHeldItemTransformBefore = tf;
                }

                return secondsUsed < 2f;
            }

            return base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel);
        }

        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity,
            BlockSelection blockSel, EntitySelection entitySel)
        {
            if (blockSel != null)
            {
                Block block = byEntity.World.BlockAccessor.GetBlock(blockSel.Position);
                if (CanSqueezeInto(block, blockSel.Position))
                {
                    if (secondsUsed < 1.9f) return;

                    IWorldAccessor world = byEntity.World;

                    if (!CanSqueezeInto(block, blockSel.Position)) return;

                    ItemStack honeyStack = new ItemStack(world.GetItem(new AssetLocation("honeyportion")), 99999);

                    BlockLiquidContainerTopOpened blockCnt = block as BlockLiquidContainerTopOpened;
                    if (blockCnt != null)
                    {
                        if (blockCnt.TryPutLiquid(blockSel.Position, honeyStack, ContainedHoneyLitres) == 0) return;
                    }
                    else
                    {
                        var beg = api.World.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityGroundStorage;
                        if (beg != null)
                        {
                            ItemSlot squeezeIntoSlot = beg.GetSlotAt(blockSel);

                            if (squeezeIntoSlot != null && queezeIntoSlot?.Itemstack?.Block != null && CanSqueezeInto(squeezeIntoSlot.Itemstack.Block, null))
                            {
                                blockCnt = squeezeIntoSlot.Itemstack.Block as BlockLiquidContainerTopOpened;
                                blockCnt.TryPutLiquid(squeezeIntoSlot.Itemstack, honeyStack, ContainedHoneyLitres);
                                beg.MarkDirty(true);
                            }
                        }
                    }

                    slot.TakeOut(1);
                    slot.MarkDirty();

                    IPlayer byPlayer = null;
                    if (byEntity is EntityPlayer) byPlayer = world.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
                    ItemStack stack = new ItemStack(world.GetItem(new AssetLocation("beeswax")));
                    if (byPlayer?.InventoryManager.TryGiveItemstack(stack) == false)
                    {
                        byEntity.World.SpawnItemEntity(stack, byEntity.SidedPos.XYZ);
                    }

                    return;
                }
            }
            base.OnHeldInteractStop(secondsUsed, slot, byEntity, blockSel, entitySel);
        }


        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return interactions.Append(base.GetHeldInteractionHelp(inSlot));
        }

    }
}
