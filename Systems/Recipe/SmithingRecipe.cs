﻿using Vintagestory.API.Common;

namespace Vintagestory.GameContent
{
    public class SmithingRecipe : LayeredVoxelRecipe<SmithingRecipe>, IByteSerializable
    {
        public override int QuantityLayers => 6;
        public override string RecipeCategoryCode => "smithing";
        protected override bool RotateRecipe => true;

        /// <summary>
        /// Creates a deep copy
        /// </summary>
        /// <returns></returns>
        public override SmithingRecipe Clone()
        {
            SmithingRecipe recipe = new SmithingRecipe();

            recipe.Pattern = (string[][])Pattern.Clone();
            recipe.Ingredient = Ingredient.Clone();
            recipe.Output = Output.Clone();
            recipe.Name = Name;
            recipe.RecipeId = RecipeId;

            return recipe;
        }
    }
}
