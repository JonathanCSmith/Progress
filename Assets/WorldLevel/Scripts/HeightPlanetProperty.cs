using AccidentalNoise;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HeightPlanetProperty : PlanetProperty {

    // Available tile props
    List<TileHeightProperties> tileProperties;

    // Noise props
    private ImplicitFractal heightMapNoise;
    protected int heightOctaves = 6;
    protected double heightFrequency = 1.25;

    // Data
    protected MapData heightData;

    public HeightPlanetProperty() : base("height") {
        this.tileProperties = HeightPlanetProperty.generateDefaultTileHeightProperties();
    }

    // TODO Add constructor for non default worlds!
    // TODO Add constructor for json deserialization too!

    public override void generateNoise(int seed) {
        this.heightMapNoise = new ImplicitFractal(FractalType.MULTI, BasisType.SIMPLEX, InterpolationType.QUINTIC, this.heightOctaves, this.heightFrequency, seed);
    }

    public override void preallocateMapData(int width, int height) {
        this.heightData = new MapData(width, height);
    }

    public override void generateSurface(int x, int y, float nx, float ny, float nz, float nw) {
        float heightValue = (float)this.heightMapNoise.Get (nx, ny, nz, nw);
        if (heightValue > this.heightData.Max) {
            this.heightData.Max = heightValue;
        }

        if (heightValue < this.heightData.Min) {
            this.heightData.Min = heightValue;
        }
        this.heightData.Data[x,y] = heightValue;
    }

    public override void generateTile(Tile t, int x, int y) {
        float heightValue = this.heightData.Data[x, y];
        heightValue = (heightValue - heightData.Min) / (heightData.Max - heightData.Min); // Normalise
        t.addData("height", new Height(heightValue, this.pickHeightProperties(heightValue)));
    }

    public TileHeightProperties pickHeightProperties(float heightValue) {
        TileHeightProperties currentSelection = null;
        foreach (TileHeightProperties properties in this.tileProperties) {
            if (Mathf.Approximately(properties.targetHeight(), heightValue)) {
                return properties;
            }

            if (currentSelection == null) {
                currentSelection = properties;
                continue;
            }

            if (Mathf.Approximately(Math.Abs(currentSelection.targetHeight() - heightValue), Math.Abs(properties.targetHeight() - heightValue))) {
                currentSelection = properties;
            }
        }

        return currentSelection;
    }

    public static List<TileHeightProperties> generateDefaultTileHeightProperties() {
        List<TileHeightProperties> list = new List<TileHeightProperties>();

        // Props
        list.Add(new TileHeightProperties(0.1f, "DeepWater", false));
        list.Add(new TileHeightProperties(0.2f, "MidWater", false));
        list.Add(new TileHeightProperties(0.3f, "ShallowWater", false));
        list.Add(new TileHeightProperties(0.4f, "Shoreline", true));
        list.Add(new TileHeightProperties(0.5f, "Lowlands", true));
        list.Add(new TileHeightProperties(0.65f, "Midlands", true));
        list.Add(new TileHeightProperties(0.8f, "Highlands", true));
        list.Add(new TileHeightProperties(0.9f, "Mountains", true));

        return list;
    }
}
