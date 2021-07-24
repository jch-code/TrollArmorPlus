# Valheim Custom Troll Armor
This mod is a re-created based off of [Rainbow Troll Armor Generator](https://www.nexusmods.com/valheim/mods/1309) by Nexus Mods user [@Astafraios](https://www.nexusmods.com/valheim/users/33749005).

Using the armor-config.json, TrollArmorPlus will generate new Troll Armor pieces on load with custom stats, color, and recipes.

## Features

### Armor Config
Each entry in armor-config.json requires following preoperties


- NewPrefabName
   - Name of new item prefab 
- SourcePrefabName
   - Name of the original troll armor item prefab. Must be one of the following
   - `[ArmorTrollLeatherChest, ArmorTrollLeatherLegs, CapeTrollHide, HelmetTrollLeather]`
- DisplayName
  - Name that will actually be seen in the game (crafting station, inventory, etc)
- ArmorType
  - Type of armor. This is used to set proper textures/materials when generating the items. Must be one of the following.
  - `[tunic, legs, cape, hood]`
- Color
  - JSON object containing RGB values
    ```
    {
        "Red": 255,
        "Green": 1,
        "Blue": 1
    }
    ```            
- CraftingStation
  - Name of the prefab of the station that you want to craft the armor at
  - `[piece_workbench, forge, piece_artisanstation, ...]` 
- MaxUpgradeQuality
  - How many times this item can be upgraded at the crafting station
- StartingArmor
  - How much armor this item starts with at base level
- ArmorUpdgradePerLevel
  - How much armor this item gains from an upgrade at the crafting sation
- UseOriginalRecipe
  - If this is set to true, the original recipe item recipe will be added before adding any additonal items
- StartingDurability
  - How much durability this item starts with at base level
- DurabilityUpgradePerLevel
  - How much durability this item gains from an upgrade at the crafting station
- AdditionalRecipeRequirements
  - JSON array containing JSON objects with recipe requirement data.
  - ***The recipe can only contain a maximum of four total items.***
    - E.g. If the original recipe is being used and has two standard requirements, you can only add 2 more additional requirements. The mod will ignore any more requirements after hitting the limit.

Example of an entry in `armor-config.json`.

```
  {
    "NewPrefabName": "ArmorRedTrollLeatherChest",
    "SourcePrefabName": "ArmorTrollLeatherChest",
    "DisplayName": "Red Troll Leather Tunic",
    "ArmorType": "tunic",
    "Color": 
      {
        "Red": 255,
        "Green": 1,
        "Blue": 1
      },
    
    "CraftingStation" : "piece_workbench",
    "MaxUpgradeQuality": 4,

    "StartingArmor": 14,
    "ArmorUpdgradePerLevel": 2,
    "UseOriginalRecipe" : true,

    "StartingDurability": 1200,
    "DurabilityUpgradePerLevel": 200,

    "AdditionalRecipeRequirements": 
      [
        {
          "RequirementPrefabName" :"Iron",
          "RequirementAmount" : "10",
          "RequirementAmountMorePerLevel" : "5"
        }
      ]
  }
```  
  
***Full example armor-config.json can be found in the Resources folder.***  
***[Full prefab list available here](https://valheim-modding.github.io/Jotunn/data/prefabs/prefab-list.html)***

## Usage
This project was built with the Jötunn Mod Stub project so it benefits from the standard pre/post build auutomations.

### Building Debug
- The compiled dll file for this project is copied to `<ValheimDir>\BepInEx\plugins`.
- A .mdb file is generated for the compiled project dll and copied to `<ValheimDir>\BepInEx\plugins`.
- `<ValheimModStub>\libraries\Debug\mono-2.0-bdwgc.dll` is copied to `<ValheimDir>\MonoBleedingEdge\EmbedRuntime` replacing the original file (a backup is created before).

### Building Release
- The README.md in `SolutionDir/ProjectDir/package/README.md` is copied to `SolutionDir/ProjectDir/README.md` so that it is present and readable in single-solution-multi-project githubs to give an overview of the project.
- The compiled binary is placed inside of `SolutionDir/ProjectDir/package/plugins`
- The contents of `SolutionDir/ProjectDir/package/*` is archived into a zip, ready for thunderstore upload.