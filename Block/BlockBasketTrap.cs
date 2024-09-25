﻿using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent
{

    public class BlockBasketTrap : Block
    {
        protected float rotInterval = GameMath.PIHALF / 4;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            CanStep = false;
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);

            if (val)
            {
                var be = GetBlockEntity<BlockEntityBasketTrap>(blockSel.Position);
                if (be != null)
                {
                    BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
                    double dx = byPlayer.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
                    double dz = (float)byPlayer.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
                    float angleHor = (float)Math.Atan2(dx, dz);

                    float roundRad = ((int)Math.Round(angleHor / rotInterval)) * rotInterval;

                    be.RotationYDeg = roundRad * GameMath.RAD2DEG;
                    be.MarkDirty(true);
                }
            }

            return val;
        }


        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            var be = GetBlockEntity<BlockEntityBasketTrap>(blockSel.Position);
            if (be != null) return be.Interact(byPlayer, blockSel);

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            var be = GetBlockEntity<BlockEntityBasketTrap>(pos);
            if (be != null && be.TrapState == EnumTrapState.Destroyed)
            {
                ItemStack dropStack = null;
                if (Attributes != null && Attributes.KeyExists("drop"))
                {
                    JsonItemStack jsonDropStack = Attributes["drop"].AsObject<JsonItemStack>();
                    if (jsonDropStack != null && jsonDropStack.Resolve(world, ""))
                    {
                        ItemStack newStack = jsonDropStack.ResolvedItemstack;
                        newStack.StackSize = 6 + world.Rand.Next(8);
                        return new ItemStack[] { newStack };
                    }
                }

                return new ItemStack[] { new ItemStack(world.GetItem(AssetLocation.Create("cattailtops")), 6 + world.Rand.Next(8)) };
            }

            return base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);
        }

        public override void GetDecal(IWorldAccessor world, BlockPos pos, ITexPositionSource decalTexSource, ref MeshData decalModelData, ref MeshData blockModelData)
        {
            var be = GetBlockEntity<BlockEntityBasketTrap>(pos);
            if (be != null)
            {
                blockModelData = be.GetCurrentMesh(null).Clone().Rotate(Vec3f.Half, 0, (be.RotationYDeg-90) * GameMath.DEG2RAD, 0);
                decalModelData = be.GetCurrentMesh(decalTexSource).Clone().Rotate(Vec3f.Half, 0, (be.RotationYDeg-90) * GameMath.DEG2RAD, 0);

                return;
            }

            base.GetDecal(world, pos, decalTexSource, ref decalModelData, ref blockModelData);

        }
    }
}