using System;
using System.Collections.Generic;




// TODO: Could it be a struct?


public class TilePropertiesHandler {

    public static List<TileAffectors> generateDefaultTileAffectorsList() {

    }

    public static List<TileHeightProperties> generateDefaultTileHeightProperties() {
        List<TileHeightProperties> tileProperties = new List<TileHeightProperties>();

        // TODO: This could be jsonified
        // Generate our defaults
        tileProperties.Add(new TileHeightProperties(0.1f, HeightClassification.DeepWater, false));
        tileProperties.Add(new TileHeightProperties(0.25f, HeightClassification.MidWater, false));
        tileProperties.Add(new TileHeightProperties(0.4f, HeightClassification.ShallowWater, false));
        tileProperties.Add(new TileHeightProperties(0.7f, HeightClassification.Shore, true));
        tileProperties.Add(new TileHeightProperties(0.8f, HeightClassification.Lowlands, true));
        tileProperties.Add(new TileHeightProperties(0.9f, HeightClassification.Highlands, true));
        tileProperties.Add(new TileHeightProperties(0.2f, HeightClassification.Mountains, true));

        // Return the map!
        return tileProperties;
    }

}
