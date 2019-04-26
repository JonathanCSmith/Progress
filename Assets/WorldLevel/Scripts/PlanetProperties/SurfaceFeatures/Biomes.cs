public class BiomeFeatureGenerator : FeatureDecorator {

    public static readonly string NAME = "biomes";

    // TODO: Allow a way to deserialize this - that way we can reuse this object with different props
    protected BiomeType[,] LandBiomeTable = new BiomeType[6, 6] {   
        //COLDEST        //COLDER          //COLD                  //HOT                          //HOTTER                       //HOTTEST
        { BiomeType.Ice, BiomeType.Tundra, BiomeType.Grassland,    BiomeType.Desert,              BiomeType.Desert,              BiomeType.Desert },              //DRYEST
        { BiomeType.Ice, BiomeType.Tundra, BiomeType.Grassland,    BiomeType.Desert,              BiomeType.Desert,              BiomeType.Desert },              //DRYER
        { BiomeType.Ice, BiomeType.Tundra, BiomeType.Woodland,     BiomeType.Woodland,            BiomeType.Savanna,             BiomeType.Savanna },             //DRY
        { BiomeType.Ice, BiomeType.Tundra, BiomeType.BorealForest, BiomeType.Woodland,            BiomeType.Savanna,             BiomeType.Savanna },             //WET
        { BiomeType.Ice, BiomeType.Tundra, BiomeType.BorealForest, BiomeType.SeasonalForest,      BiomeType.TropicalRainforest,  BiomeType.TropicalRainforest },  //WETTER
        { BiomeType.Ice, BiomeType.Tundra, BiomeType.BorealForest, BiomeType.TemperateRainforest, BiomeType.TropicalRainforest,  BiomeType.TropicalRainforest }   //WETTEST
    };

    public BiomeFeatureGenerator() : base(BiomeFeatureGenerator.NAME) { }

    public override bool generate(Generable generable) {
        if (!generable.checkPropertyEnabled(SurfaceMoisture.NAME) || !generable.checkPropertyEnabled(SurfaceHeat.NAME)) {
            return false;
        }

        this.generateBiomeMap(generable);
        this.generateBiomeBitmasks(generable);
        return true;
    }

    private void generateBiomeMap(Generable generable) {
        for (var x = 0; x < generable.getWidth(); x++) {
            for (var y = 0; y < generable.getHeight(); y++) {

                // TODO: Water biomes
                Tile t = generable.getTile(x, y);
                Height height = Height.getHeightForTile(t);
                if (!height.getTileProperties().collisionState) {
                    continue;
                }

                BiomeType b = this.getBiomeType(t);
                BiomeFeatureGenerator.setBiomeTypeForTile(t, b);
            }
        }
    }

    public BiomeType getBiomeType(Tile tile) {
        Moisture m = Moisture.getMoisture(tile);
        Heat h = Heat.getHeat(tile);

        return LandBiomeTable[(int)m.moistureType.getMoistureClassification(), (int)h.heatType.getHeatClassification()];
    }

    public static void setBiomeTypeForTile(Tile t, BiomeType b) {
        t.setData(BiomeFeatureGenerator.NAME, new BiomeData(b));
    }

    public static BiomeData getBiomeTypeForTile(Tile t) {
        return (BiomeData)t.getData(BiomeFeatureGenerator.NAME);
    }

    private void generateBiomeBitmasks(Generable generable) {
        for (int x = 0; x < generable.getWidth(); x++) {
            for (int y = 0; y < generable.getHeight(); y++) {
                Tile t = generable.getTile(x, y);
                this.generateBiomeBitmask(t);
            }
        }
    }

    private void generateBiomeBitmask(Tile t) {
        BiomeData biomeData = BiomeFeatureGenerator.getBiomeTypeForTile(t);
        Height heightProperty = Height.getHeightForTile(t);

        // Set our bitmask data
        int bitmask = 0;
        if (heightProperty.getTileProperties().collisionState) {
            int[] bitmaskAdditors = new int[4];
            bitmaskAdditors[0] = 8;
            bitmaskAdditors[1] = 1;
            bitmaskAdditors[2] = 2;
            bitmaskAdditors[3] = 4;

            Tile tileToCheck;
            BiomeData biomeToCheck;
            for (int i = 0; i < Tile.NUMBER_OF_DIRECTIONS; i++) {
                tileToCheck = t.getTileByDirection((Direction)i);
                if (tileToCheck == null) {
                    continue;
                }

                biomeToCheck = BiomeFeatureGenerator.getBiomeTypeForTile(tileToCheck);
                if (biomeToCheck.getBiomeType() == biomeData.biomeType) {
                    bitmask += bitmaskAdditors[i];
                }
            }
        }

        t.setDataBitmask(BiomeFeatureGenerator.NAME, bitmask);
    }
}

public class BiomeData : DataBucket {
    public readonly BiomeType biomeType;

    public BiomeData(BiomeType biomeType) {
        this.biomeType = biomeType;
    }

    public BiomeType getBiomeType() {
        return this.biomeType;
    }
}

// TODO: Biomes should be deserializable and not hardcoded
public enum BiomeType {
    Desert,
    Savanna,
    TropicalRainforest,
    Grassland,
    Woodland,
    SeasonalForest,
    TemperateRainforest,
    BorealForest,
    Tundra,
    Ice
}