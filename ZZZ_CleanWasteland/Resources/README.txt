================================================================================
RESOURCES FOLDER - Unity Prefabs for Custom Wasteland Vegetation
================================================================================

This folder contains Unity prefab bundles for custom vegetation added to the
wasteland biome.

CURRENT PREFABS:
----------------
1. aliengrass - Alien grass ground cover (Very Dense: 80% spawn probability)


HOW TO ADD UNITY PREFABS:
--------------------------

1. CREATE YOUR UNITY PREFAB:
   - Build your prefab in Unity using 7 Days to Die's Unity version
   - Export as an AssetBundle (.unity3d file)

2. PLACE THE FILE HERE:
   - Copy your .unity3d file to this Resources folder
   - Example: aliengrass.unity3d

3. REGISTER IN blocks.xml:
   - Create a Config/blocks.xml file in the mod
   - Define your block with the prefab reference
   - Example:
     <block name="aliengrass">
         <property name="Extends" value="treeShortGrass"/>
         <property name="Model" value="#@modfolder:Resources/aliengrass.unity3d?aliengrass"/>
     </block>

4. ADD TO BIOMES:
   - Already configured in Config/biomes.xml
   - Alien grass is set to spawn at 80% probability across wasteland


FILE NAMING CONVENTION:
-----------------------
- Unity3D file: lowercase, no spaces (e.g., aliengrass.unity3d)
- Prefab name inside bundle: should match the file name
- Block name in blocks.xml: should match the blockname in biomes.xml


NOTES:
------
- The mod loads AFTER District Zero (ZZZ_ prefix ensures this)
- All vanilla wasteland vegetation is removed first
- Custom vegetation is then added back
- Preserves all loot, ores, and resources


================================================================================

