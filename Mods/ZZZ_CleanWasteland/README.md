# Clean Wasteland Mod

A 7 Days to Die mod that removes vanilla debris, trees, and vegetation from the wasteland biome, then adds custom alien vegetation for a unique post-apocalyptic alien landscape.

## What This Mod Does

### Removed Elements
- **Debris**: Rubble piles, cinder blocks, scrap metal piles, driftwood, rocks, tires
- **Trees**: All burnt maple variants, dead pine trees
- **Vegetation**: Dead shrubs, brown grass, short grass

### Added Elements
- **Alien Grass**: Custom alien grass ground cover (Very Dense - 80% spawn rate)
- **Unity Prefabs**: Support for custom Unity prefab vegetation

### Preserved Elements
- **Ore Veins**: Coal, iron, lead, potassium nitrate, and **cobalt** veins (from District Zero mod)
- **Loot Containers**: All barrels, wasteland loot helpers, bird nests, and radiation barrels
- **Mineable Items**: Hubcaps and candy tins (valuable loot items)
- **Resource Rocks**: resourceRock01 and resourceRock02 (for stone/clay)
- **Car Wrecks**: Both carWrecksRandomHelper and carsRandomHelper remain
- **Radiated Mushrooms**: mushroomRadiated02 and mushroomRadiated04 remain for harvesting
- **Terrain**: All terrain layers and underground ore resources unchanged
- **Subbiomes**: All wasteland subbiomes remain functional (including District Zero's cobalt subbiome)
- **Biome Properties**: Difficulty, loot bonuses, weather patterns unchanged

## Installation

1. Download or clone this mod
2. Place the `ZZZ_CleanWasteland` folder in your `7 Days to Die/Mods/` directory
3. **IMPORTANT**: The mod name starts with `ZZZ_` to ensure it loads AFTER District Zero mod
4. Place your Unity prefab files (`.unity3d`) in the `Resources` folder
5. Start the game - the mod will automatically load

## Adding Custom Vegetation

1. **Create Unity Prefab**: Build your vegetation prefab in Unity (use 7DTD's Unity version)
2. **Export as AssetBundle**: Export as `.unity3d` file (e.g., `aliengrass.unity3d`)
3. **Place in Resources**: Copy the file to `Mods/ZZZ_CleanWasteland/Resources/`
4. **Define Block**: Add block definition in `Config/blocks.xml` (see template in file)
5. **Add to Biome**: Add decoration entry in `Config/biomes.xml` (see examples in file)

See `Resources/README.txt` for detailed instructions.

## Compatibility

- **Server/Client**: This mod must be installed on both server and client
- **New Worlds Required**: Yes - changes only affect newly generated worlds
- **Existing Worlds**: Will not affect already generated chunks
- **Load Order**: Loads AFTER District Zero (ZZZ_ prefix ensures this)
- **District Zero Compatibility**: Fully compatible - preserves cobalt veins and subbiome

## Technical Details

The mod uses simplified XPath operations for maximum compatibility:

### Removal Phase
- Uses `//biome[@name='wasteland']//decoration` to target ALL subbiomes at once
- Uses `starts-with()` for pattern matching (e.g., removes all `treeBurnt*` variants)
- Much more reliable than targeting individual subbiomes

### Addition Phase
- Uses `<append>` to add custom alien vegetation
- References Unity prefabs via `#@modfolder:Resources/` syntax
- Supports multiple custom vegetation types

### Load Order
- Mod name starts with `ZZZ_` to load alphabetically after District Zero
- This ensures vanilla vegetation is removed AFTER District Zero adds its cobalt subbiome
- Custom vegetation is then added on top of the clean wasteland

## Why Use This Mod?

- **Unique Aesthetic**: Transform wasteland into an alien landscape
- **Better Performance**: Fewer vanilla decorations may improve FPS
- **Easier Navigation**: Less debris makes movement and building easier
- **Preserved Gameplay**: All valuable loot and resources remain available
- **Customizable**: Easy to add your own Unity prefab vegetation

## Mod Structure

```
ZZZ_CleanWasteland/
├── Config/
│   ├── biomes.xml      # Removes vanilla vegetation, adds alien grass
│   └── blocks.xml      # Defines custom vegetation blocks
├── Resources/
│   ├── README.txt      # Instructions for adding Unity prefabs
│   └── [.unity3d files go here]
├── ModInfo.xml         # Mod metadata
└── README.md           # This file
```

## Version

- **Version**: 1.0.2
- **Game Version**: 7 Days to Die (Vanilla + District Zero mod)
- **Author**: District Zero

## License

Free to use and modify for personal use.

