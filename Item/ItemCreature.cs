﻿using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using System;
using System.Linq;
using System.Text;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace Vintagestory.GameContent
{
    public class ItemCreature : Item
    {
        public override string GetHeldTpUseAnimation(ItemSlot activeHotbarSlot, Entity byEntity)
        {
            return null;
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
        {
            if (blockSel == null) return;

            IPlayer player = byEntity.World.PlayerByUid((byEntity as EntityPlayer).PlayerUID);

            if (!byEntity.World.Claims.TryAccess(player, blockSel.Position, EnumBlockAccessFlags.BuildOrBreak))
            {
                return;
            }

            if (!(byEntity is EntityPlayer) || player.WorldData.CurrentGameMode != EnumGameMode.Creative)
            {
                slot.TakeOut(1);
                slot.MarkDirty();
            }


            AssetLocation location = new AssetLocation(Code.Domain, CodeEndWithoutParts(1));
            EntityProperties type = byEntity.World.GetEntityType(location);
            if (type == null)
            {
                byEntity.World.Logger.Error("ItemCreature: No such entity - {0}", location);
                if (api.World.Side == EnumAppSide.Client)
                {
                    (api as ICoreClientAPI).TriggerIngameError(this, "nosuchentity", string.Format("No such entity loaded - '{0}'.", location));
                }
                return;
            }

            Entity entity = byEntity.World.ClassRegistry.CreateEntity(type);

            if (entity != null)
            {
                entity.ServerPos.X = blockSel.Position.X + (blockSel.DidOffset ? 0 : blockSel.Face.Normali.X) + 0.5f;
                entity.ServerPos.Y = blockSel.Position.Y + (blockSel.DidOffset ? 0 : blockSel.Face.Normali.Y);
                entity.ServerPos.Z = blockSel.Position.Z + (blockSel.DidOffset ? 0 : blockSel.Face.Normali.Z) + 0.5f;
                entity.ServerPos.Yaw = (float)byEntity.World.Rand.NextDouble() * 2 * GameMath.PI;

                entity.Pos.SetFrom(entity.ServerPos);
                entity.PositionBeforeFalling.Set(entity.ServerPos.X, entity.ServerPos.Y, entity.ServerPos.Z);

                entity.Attributes.SetString("origin", "playerplaced");

                if (Attributes?.IsTrue("setGuardedEntityAttribute") == true)
                {
                    entity.WatchedAttributes.SetLong("guardedEntityId", byEntity.EntityId);
                    if (byEntity is EntityPlayer eplr)
                    {
                        entity.WatchedAttributes.SetString("guardedPlayerUid", eplr.PlayerUID);
                    }
                }

                byEntity.World.SpawnEntity(entity);
                handHandling = EnumHandHandling.PreventDefaultAction;
            }
        }

        public override string GetHeldTpIdleAnimation(ItemSlot activeHotbarSlot, Entity byEntity, EnumHand hand)
        {
            EntityProperties type = byEntity.World.GetEntityType(new AssetLocation(Code.Domain, CodeEndWithoutParts(1)));
            if (type == null) return base.GetHeldTpIdleAnimation(activeHotbarSlot, byEntity, hand);

            float size = Math.Max(type.CollisionBoxSize.X, type.CollisionBoxSize.Y);

            if (size > 1) return "holdunderarm";
            return "holdbothhands";
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[] {
                new WorldInteraction()
                {
                    ActionLangCode = "heldhelp-place",
                    MouseButton = EnumMouseButton.Right,
                }
            }.Append(base.GetHeldInteractionHelp(inSlot));
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            if (FirstCodePart(1) == "butterfly")
            {
                base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
                dsc.Insert(0, "<font color=\"#ccc\"><i>");
                dsc.Append("</i></font>");
                dsc.AppendLine(Lang.Get("itemdesc-creature-butterfly-all"));
                return;
            }

            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        }
    }
}