﻿using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Vintagestory.GameContent
{
    public class ItemOre : ItemPileable
    {
        public bool IsCoal => Variant["ore"] == "lignite" || Variant["ore"] == "bituminouscoal" || Variant["ore"] == "anthracite";
        public override bool IsPileable => IsCoal;
        protected override AssetLocation PileBlockCode => new AssetLocation("coalpile");

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            if (CombustibleProps?.SmeltedStack?.ResolvedItemstack == null)
            {
                if (Attributes?["metalUnits"].Exists == true)
                {
                    float units = Attributes["metalUnits"].AsInt();

                    string orename = LastCodePart(1);
                    if (orename.Contains("_"))
                    {
                        orename = orename.Split('_')[1];
                    }
                    AssetLocation loc = new AssetLocation("nugget-" + orename);
                    Item item = api.World.GetItem(loc);

                    if (item?.CombustibleProps?.SmeltedStack?.ResolvedItemstack != null)
                    {
                        string metalname = item.CombustibleProps.SmeltedStack.ResolvedItemstack.GetName().Replace(" ingot", "");
                        dsc.AppendLine(Lang.Get("{0} units of {1}", units.ToString("0.#"), metalname));
                    }   

                    dsc.AppendLine(Lang.Get("Parent Material: {0}", Lang.Get("rock-" + LastCodePart())));
                    dsc.AppendLine();
                    dsc.AppendLine(Lang.Get("Crush with hammer to extract nuggets"));
                }
            }
            else
            {

                base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

                if (CombustibleProps.SmeltedStack.ResolvedItemstack.GetName().Contains("ingot"))
                {
                    string smelttype = CombustibleProps.SmeltingType.ToString().ToLowerInvariant();
                    int instacksize = CombustibleProps.SmeltedRatio;
                    int outstacksize = CombustibleProps.SmeltedStack.ResolvedItemstack.StackSize;
                    float units = outstacksize * 100f / instacksize;

                    string metalname = CombustibleProps.SmeltedStack.ResolvedItemstack.GetName().Replace(" ingot", "");

                    string str = Lang.Get("game:smeltdesc-" + smelttype + "ore-plural", units.ToString("0.#"), metalname);
                    dsc.AppendLine(str);
                }

                return;
            }
            

            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        }

        public override string GetHeldItemName(ItemStack itemStack)
        {
            if (Attributes?["metalUnits"].Exists == true)
            {
                string orename = LastCodePart(1);
                string rockname = LastCodePart(0);

                if (FirstCodePart() == "crystalizedore")
                {
                    return Lang.Get(LastCodePart(2) + "-crystallizedore-chunk", Lang.Get("ore-" + orename));

                }
                return Lang.Get(LastCodePart(2) + "-ore-chunk", Lang.Get("ore-" + orename));

            }

            return base.GetHeldItemName(itemStack);
        }

    }
}
