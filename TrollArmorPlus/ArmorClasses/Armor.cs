using System.Collections.Generic;

namespace TrollArmor
{
	public class Armor
	{
		public string NewPrefabName { get; set; }
		public string SourcePrefabName { get; set; }
		public string DisplayName { get; set; }
		public string ArmorType { get; set; }
		public Color Color { get; set; }

		public string CraftingStation { get; set; }
		public int MaxUpgradeQuality { get; set; }
		public bool UseOriginalRecipe { get; set; }

		public int StartingArmor { get; set; }
		public int ArmorUpdgradePerLevel { get; set; }

		public int StartingDurability { get; set; }
		public int DurabilityUpgradePerLevel { get; set; }

		public List<RecipeRequirement> AdditionalRecipeRequirements { get; set; }
	}
}
