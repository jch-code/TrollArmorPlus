using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Piece;
using static SimpleJson.SimpleJson;

namespace TrollArmor
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
	[BepInDependency(Jotunn.Main.ModGuid)]
	internal class TrollArmorPlus : BaseUnityPlugin
	{
		public const string PluginGUID = "troll_armor_plus";
		public const string PluginName = "Troll Armor Plus";
		public const string PluginVersion = "1.0.0";

		private static List<Armor> armorConfig;

		private static Dictionary<string, Texture2D> armorTextureDictionary = new Dictionary<string, Texture2D>();
		private static Dictionary<string, Texture2D> iconDictionary = new Dictionary<string, Texture2D>();

		private void Awake()
		{
			// Custom asset bundle created using original game assets
			var assets = AssetUtils.LoadAssetBundleFromResources("troll-armor", typeof(TrollArmorPlus).Assembly);

			// Armor textures
			armorTextureDictionary.Add("tunic", assets.LoadAsset<Texture2D>("Assets/tunic-texture.png"));
			armorTextureDictionary.Add("legs", assets.LoadAsset<Texture2D>("Assets/legs-texture.png"));

			// Armor inventory icons
			iconDictionary.Add("hood", assets.LoadAsset<Texture2D>("Assets/hood.png"));
			iconDictionary.Add("cape", assets.LoadAsset<Texture2D>("Assets/cape.png"));
			iconDictionary.Add("tunic", assets.LoadAsset<Texture2D>("Assets/tunic.png"));
			iconDictionary.Add("legs", assets.LoadAsset<Texture2D>("Assets/legs.png"));

			armorConfig = DeserializeObject<List<Armor>>(File.ReadAllText(Directory.GetCurrentDirectory() + "/BepInEx/plugins/TrollArmorPlus/armor-config.json"));

			ItemManager.OnVanillaItemsAvailable += AddClonedItems;
		}

		private void AddClonedItems()
		{
			try
			{
				foreach (var armor in armorConfig)
				{
					// Create CustomItem and set some values
					var item = new CustomItem(armor.NewPrefabName, armor.SourcePrefabName);
					item.ItemDrop.m_itemData.m_shared.m_name = armor.DisplayName;

					item.ItemDrop.m_itemData.m_shared.m_maxQuality = armor.MaxUpgradeQuality;

					item.ItemDrop.m_itemData.m_shared.m_armor = armor.StartingArmor;
					item.ItemDrop.m_itemData.m_shared.m_armorPerLevel = armor.ArmorUpdgradePerLevel;
					item.ItemDrop.m_itemData.m_durability = armor.StartingDurability;
					item.ItemDrop.m_itemData.m_shared.m_durabilityPerLevel = armor.DurabilityUpgradePerLevel;

					// Recolor icon texture, create a new Sprite, set Icon
					// This is done on copies of the original assets that were repackaged into an asset bundle
					#region Armor Icon
					var color = new UnityEngine.Color(armor.Color.Red / 255f, armor.Color.Blue / 255f, armor.Color.Green / 255f, 1f);

					var width = iconDictionary[armor.ArmorType].width;
					var height = iconDictionary[armor.ArmorType].height;

					var icon = Sprite.Create(RecolorTexture(iconDictionary[armor.ArmorType], color, .25f), new Rect(0f, 0f, width, height), Vector2.zero);

					item.ItemDrop.m_itemData.m_shared.m_icons[0] = icon;
					#endregion

					// Need to manually recolor the tunic and legs because the custom shader being used does not have a standard _Color property that can be set
					// Hood and Cape can be recolored by changing the _Color property
					#region Tunic and Legs Material

					var material = item.ItemDrop.m_itemData.m_shared.m_armorMaterial != null ? new Material(item.ItemDrop.m_itemData.m_shared.m_armorMaterial) : null;
					var armorToRetexture = new string[] { "tunic", "legs" };

					if (armorToRetexture.Contains(armor.ArmorType))
					{
						var textureName = armor.ArmorType == "tunic" ? "_ChestTex" : "_LegsTex";

						var texture = RecolorTexture(armorTextureDictionary[armor.ArmorType], color, .5f);
						texture.name = armor.NewPrefabName + textureName;

						material = new Material(item.ItemDrop.m_itemData.m_shared.m_armorMaterial.shader);
						material.CopyPropertiesFromMaterial(item.ItemDrop.m_itemData.m_shared.m_armorMaterial);
						material.name = armor.NewPrefabName;
						
						material.SetTexture(textureName, texture);

						item.ItemDrop.m_itemData.m_shared.m_armorMaterial = material;
					}
                    #endregion

					// Assign and recolor materials
                    SetupMaterial(armor, item.ItemDrop, material, color);

					// Maximum number of recipe items is 4 
					// Only add requirements to recipe if requirements.count is < 4
					#region Recipe
					var originalRecipe = PrefabManager.Cache.GetPrefab<Recipe>("Recipe_" + armor.SourcePrefabName);
					var requirements = new List<RequirementConfig>();

					if (armor.UseOriginalRecipe)
                    {
						foreach (Requirement req in originalRecipe.m_resources)
							if (requirements.Count < 4)
								requirements.Add(new RequirementConfig
								{
									Item = req.m_resItem.name,
									Amount = req.m_amount,
									AmountPerLevel = req.m_amountPerLevel,
									Recover = req.m_recover
								});
					}

					armor.AdditionalRecipeRequirements.ForEach(additionalReq =>
					{
						if (requirements.Count < 4)
							requirements.Add(new RequirementConfig
							{
								Item = additionalReq.RequirementPrefabName,
								Amount = additionalReq.RequirementAmount,
								AmountPerLevel = additionalReq.RequirementAmountMorePerLevel,
								Recover = true
							});
					});

					var recipe = new CustomRecipe(new RecipeConfig()
					{
						Name = "Recipe_" + armor.NewPrefabName,
						Item = item.ItemDrop.name,
						CraftingStation = armor.CraftingStation,
						RepairStation = armor.CraftingStation,
						MinStationLevel = originalRecipe.m_minStationLevel,
						Enabled = true,
						Requirements = requirements.ToArray()
					});				
					#endregion

					// Finish item creation and add custom recipe
					ItemManager.Instance.AddItem(item);
					ItemManager.Instance.AddRecipe(recipe);
				}
            }
			catch (Exception ex)
			{
				Jotunn.Logger.LogError("Error while adding cloned item: " + ex.Message + ex.StackTrace);
			}
			finally
			{
				ItemManager.OnVanillaItemsAvailable -= AddClonedItems;
			}
		}

		// Hood, Cape, Tunic, and Legs all have child GameObjects for the dropped version of the item and the equipped version
		// Hood, Cape, and Tunic all have a child named "attach_skin", this holds the child objects for the items when they are equipped
		// The equipped object has another child specific to each item
		//
		// DROPPED OBJECT CHILD NAME
		//	Hood => hood
		//	Cape, Tunic, Legs => log
		// 
		// EQUIPPED OBJECT CHILD NAME
		//	Hood => hood
		//	Cape => cape2
		//	Tunic => shorts
		//
		public void SetupMaterial(Armor armor, ItemDrop itemDrop, Material material, UnityEngine.Color color)
		{
			Transform skinTransform = itemDrop.gameObject.transform.Find("attach_skin");

			switch (armor.ArmorType)
			{
				case "hood":
					var droppedHoodMaterial = itemDrop.gameObject.transform.Find("hood").GetComponent<MeshRenderer>().material;
					droppedHoodMaterial.SetColor("_Color", UnityEngine.Color.Lerp(droppedHoodMaterial.color, color, .75f));

					skinTransform.gameObject.SetActive(true);

					var equippedHoodMaterial = skinTransform.Find("hood").GetComponent<SkinnedMeshRenderer>().material;
					equippedHoodMaterial.SetColor("_Color", UnityEngine.Color.Lerp(equippedHoodMaterial.color, color, .75f));

					skinTransform.gameObject.SetActive(false);
					break;

				case "cape":
					var droppedCapeMaterial = itemDrop.gameObject.transform.Find("log").GetComponent<MeshRenderer>().material;
					droppedCapeMaterial.SetColor("_Color", UnityEngine.Color.Lerp(droppedCapeMaterial.color, color, .75f));

					skinTransform.gameObject.SetActive(true);

					var equippedCapeMaterial = skinTransform.Find("cape2").GetComponent<SkinnedMeshRenderer>().material;
					equippedCapeMaterial.SetColor("_Color", UnityEngine.Color.Lerp(equippedCapeMaterial.color, color, .75f));

					skinTransform.gameObject.SetActive(false);
					break;

				case "tunic":
					var droppedTunicMaterial = itemDrop.gameObject.transform.Find("log").GetComponent<MeshRenderer>().material;
					droppedTunicMaterial.SetColor("_Color", UnityEngine.Color.Lerp(droppedTunicMaterial.color, color, .75f));

					skinTransform.gameObject.SetActive(true);

					skinTransform.Find("shorts").GetComponent<SkinnedMeshRenderer>().material = material;

					skinTransform.gameObject.SetActive(false);
					break;

				case "legs":
					var droppedLegsMaterial = itemDrop.gameObject.transform.Find("log").GetComponent<MeshRenderer>().material;
					droppedLegsMaterial.SetColor("_Color", UnityEngine.Color.Lerp(droppedLegsMaterial.color, color, .75f));
					break;
			}
		}

		// Use Color.Lerp to transition from the original color to the new color
		//
		// This is accomplished by lookping through the pixels and individually setting the colors
		//
		// Only pixels where alpha > 0, meaning it is not a transparent pixel, will be changed
		public Texture2D RecolorTexture(Texture2D texture, UnityEngine.Color color, float lerpByValue)
		{
			var width = texture.width;
			var height = texture.height;

			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
				{
					var pixel = texture.GetPixel(i, j);
					if (pixel.a > 0f)
						texture.SetPixel(i, j, UnityEngine.Color.Lerp(pixel, color, lerpByValue));
				}

			texture.Apply();

			return texture;
		}

	}
}