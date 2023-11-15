﻿using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace Vintagestory.GameContent
{
    public class BlockIngotMold : Block
    {
        WorldInteraction[] interactionsLeft;
        WorldInteraction[] interactionsRight;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            if (LastCodePart() == "raw") return;

            interactionsLeft = ObjectCacheUtil.GetOrCreate(api, "ingotmoldBlockInteractionsLeft", () =>
            {
                List<ItemStack> smeltedContainerStacks = new List<ItemStack>();

                foreach (CollectibleObject obj in api.World.Collectibles)
                {
                    if (obj is BlockSmeltedContainer)
                    {
                        smeltedContainerStacks.Add(new ItemStack(obj));
                    }
                }

                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-ingotmold-pour",
                        HotKeyCode = "shift",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = smeltedContainerStacks.ToArray(),
                        GetMatchingStacks = (wi, bs, es) =>
                        {
                            BlockEntityIngotMold betm = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntityIngotMold;
                            return (betm != null && !betm.IsFullLeft) ? wi.Itemstacks : null;
                        }
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-ingotmold-takeingot",
                        HotKeyCode = null,
                        MouseButton = EnumMouseButton.Right,
                        ShouldApply = (wi, bs, es) =>
                        {
                            BlockEntityIngotMold betm = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntityIngotMold;
                            return betm != null && betm.IsFullLeft && betm.IsHardenedLeft;
                        }
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-ingotmold-pickup",
                        HotKeyCode = null,
                        RequireFreeHand = true,
                        MouseButton = EnumMouseButton.Right,
                        ShouldApply = (wi, bs, es) =>
                        {
                            BlockEntityIngotMold betm = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntityIngotMold;
                            return betm != null && betm.contentsRight == null && betm.contentsLeft == null;
                        }
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-ingotmold-placemold",
                        HotKeyCode = "shift",
                        Itemstacks = new ItemStack[] { new ItemStack(this) },
                        MouseButton = EnumMouseButton.Right,
                        GetMatchingStacks = (wi, bs, es) =>
                        {
                            BlockEntityIngotMold betm = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntityIngotMold;
                            return (betm != null && betm.quantityMolds < 2) ? wi.Itemstacks : null;
                        }
                    }
                };
            });



            interactionsRight = ObjectCacheUtil.GetOrCreate(api, "ingotmoldBlockInteractionsRight", () =>
            {
                List<ItemStack> smeltedContainerStacks = new List<ItemStack>();

                foreach (CollectibleObject obj in api.World.Collectibles)
                {
                    if (obj is BlockSmeltedContainer)
                    {
                        smeltedContainerStacks.Add(new ItemStack(obj));
                    }
                }

                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-ingotmold-pour",
                        HotKeyCode = "shift",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = smeltedContainerStacks.ToArray(),
                        GetMatchingStacks = (wi, bs, es) =>
                        {
                            BlockEntityIngotMold betm = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntityIngotMold;
                            return (betm != null && betm.quantityMolds > 1 && !betm.IsFullRight) ? wi.Itemstacks : null;
                        }
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-ingotmold-takeingot",
                        HotKeyCode = null,
                        MouseButton = EnumMouseButton.Right,
                        ShouldApply = (wi, bs, es) =>
                        {
                            BlockEntityIngotMold betm = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntityIngotMold;
                            return betm != null && betm.quantityMolds > 1 && betm.IsFullRight && betm.IsHardenedRight;
                        }
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "blockhelp-ingotmold-pickup",
                        HotKeyCode = null,
                        RequireFreeHand = true,
                        MouseButton = EnumMouseButton.Right,
                        ShouldApply = (wi, bs, es) =>
                        {
                            BlockEntityIngotMold betm = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BlockEntityIngotMold;
                            return betm != null && betm.quantityMolds > 1 && betm.contentsRight == null && betm.contentsLeft == null;
                        }
                    }
                };
            });


        }

        public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos)
        {
            return true;
        }

        Cuboidf[] oneMoldBoxes = new Cuboidf[] { new Cuboidf(0, 0, 0, 1, 0.1875f, 1)  };
        Cuboidf[] twoMoldBoxes = new Cuboidf[] { new Cuboidf(0, 0, 0, 0.5f, 0.1875f, 1), new Cuboidf(0.5f, 0, 0, 1, 0.1875f, 1) };

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor world, BlockPos pos)
        {
            BlockEntityIngotMold betm = api.World.BlockAccessor.GetBlockEntity(pos) as BlockEntityIngotMold;

            if (betm == null || betm.quantityMolds == 1)
            {
                return oneMoldBoxes;
            }

            return twoMoldBoxes;
        }

        public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel == null)
            {
                base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handling);
                return;
            }

            BlockEntity be = byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position.AddCopy(blockSel.Face.Opposite));

            IPlayer byPlayer = null;
            if (byEntity is EntityPlayer) byPlayer = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);

            if (byPlayer != null && be is BlockEntityIngotMold)
            {
                BlockEntityIngotMold beim = (BlockEntityIngotMold)be;
                if (beim.OnPlayerInteract(byPlayer, blockSel.Face, blockSel.HitPosition))
                {
                    handling = EnumHandHandling.PreventDefault;
                }
                
            }
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (blockSel == null) return base.OnBlockInteractStart(world, byPlayer, blockSel);

            BlockEntity be = world.BlockAccessor.GetBlockEntity(blockSel.Position);

            if (be is BlockEntityIngotMold)
            {
                BlockEntityIngotMold beim = (BlockEntityIngotMold)be;
                return beim.OnPlayerInteract(byPlayer, blockSel.Face, blockSel.HitPosition);
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
        {
            if (world.Rand.NextDouble() > 0.05)
            {
                base.OnEntityCollide(world, entity, pos, facing, collideSpeed, isImpact);
                return;
            }

            var be = GetBlockEntity<BlockEntityIngotMold>(pos);
            if (be?.TemperatureLeft > 300 || be.TemperatureRight > 300)
            {
                entity.ReceiveDamage(new DamageSource() { Source = EnumDamageSource.Block, SourceBlock = this, Type = EnumDamageType.Fire, SourcePos = pos.ToVec3d() }, 0.5f);
            }

            base.OnEntityCollide(world, entity, pos, facing, collideSpeed, isImpact);
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (!byPlayer.Entity.Controls.ShiftKey)
            {
                failureCode = "onlywhensneaking";
                return false;
            }

            if (!world.BlockAccessor.GetBlock(blockSel.Position.DownCopy()).CanAttachBlockAt(world.BlockAccessor, this, blockSel.Position.DownCopy(), BlockFacing.UP))
            {
                failureCode = "requiresolidground";
                return false;
            }

            return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
        }

        public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
        {
            return Drops;
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            List<ItemStack> stacks = new List<ItemStack>();

            BlockEntityIngotMold bei = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityIngotMold;
            if (bei != null)
            {
                stacks.Add(new ItemStack(this, bei.quantityMolds));

                ItemStack stackl = bei.GetLeftContents();
                if (stackl != null)
                {
                    stacks.Add(stackl);
                }
                ItemStack stackr = bei.GetRightContents();
                if (stackr != null)
                {
                    stacks.Add(stackr);
                }
            } else
            {
                stacks.Add(new ItemStack(this, 1));
            }

            return stacks.ToArray();
        }
        

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return (selection.SelectionBoxIndex == 0 ? interactionsLeft : interactionsRight).Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }

    }
}
