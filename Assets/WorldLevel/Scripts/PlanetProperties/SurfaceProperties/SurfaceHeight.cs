using AccidentalNoise;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SurfaceHeight : PropertyDecorator {

    public static readonly string NAME = "height";
    public static readonly string[] DEPENDENCIES = new string[] {};

    // Available tile props
    List<TileHeightProperties> tileProperties;

    // Noise props
    private ImplicitFractal heightMapNoise;
    protected int heightOctaves = 6;
    protected double heightFrequency = 1.25;

    // Data
    protected MapData heightData;

    public SurfaceHeight() : base(SurfaceHeight.NAME) {
        this.tileProperties = SurfaceHeight.generateDefaultTileHeightProperties();
    }

    public override string[] getPropertyDependencies() {
        return SurfaceHeight.DEPENDENCIES;
    }

    // TODO Add constructor for non default worlds!
    // TODO Add constructor for json deserialization too!

    public override bool initialise(Generable generable) {
        return true;
    }

    public override void generateNoise(int seed) {
        this.heightMapNoise = new ImplicitFractal(FractalType.MULTI, BasisType.SIMPLEX, InterpolationType.QUINTIC, this.heightOctaves, this.heightFrequency, seed);
    }

    public override void preallocateMapData(int width, int height) {
        this.heightData = new MapData(width, height);
    }

    public override void generateSurface(int x, int y, float nx, float ny, float nz, float nw) {
        float heightValue = (float)this.heightMapNoise.Get(nx, ny, nz, nw);
        if (heightValue > this.heightData.max) {
            this.heightData.max = heightValue;
        }

        if (heightValue < this.heightData.min) {
            this.heightData.min = heightValue;
        }
        this.heightData.data[x, y] = heightValue;
    }

    public override void generateTile(Tile t, int x, int y) {
        float heightValue = this.heightData.data[x, y];
        heightValue = (heightValue - heightData.min) / (heightData.max - heightData.min); // Normalise
        Height heightProperty = new Height(heightValue, this.pickHeightProperties(heightValue));
        t.setData(SurfaceHeight.NAME, heightProperty);

        // Set our bitmask data
        int bitmask = 0;
        if (heightProperty.getTileProperties().collisionState) {
            int[] bitmaskAdditors = new int[4];
            bitmaskAdditors[0] = 2;
            bitmaskAdditors[1] = 4;
            bitmaskAdditors[2] = 8;
            bitmaskAdditors[3] = 1;

            Tile tileToCheck;
            Height heightToCheck;
            for (int i = 0; i < Tile.NUMBER_OF_DIRECTIONS; i++) {
                tileToCheck = t.getTileByDirection((Direction)i);
                if (tileToCheck == null) {
                    continue;
                }

                heightToCheck = Height.getHeightForTile(tileToCheck);
                if (heightToCheck.getTileProperties().heightClassification == heightProperty.getTileProperties().heightClassification) {
                    bitmask += bitmaskAdditors[i];
                }
            }
        }

        t.setDataBitmask(SurfaceHeight.NAME, bitmask);
    }

    public TileHeightProperties pickHeightProperties(float heightValue) {
        TileHeightProperties currentSelection = null;
        foreach (TileHeightProperties properties in this.tileProperties) {
            if (Mathf.Approximately(properties.targetHeight(), heightValue)) {
                return properties;
            }

            // First random entry is an unpredictable default TODO
            if (currentSelection == null) {
                currentSelection = properties;
                continue;
            }

            // Find closest less than value
            if (heightValue < properties.targetHeight() && (Math.Abs(currentSelection.targetHeight() - heightValue) > Math.Abs(properties.targetHeight() - heightValue))) {
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

public class TileHeightProperties {

    public float maxHeight;
    public string heightClassification;
    public bool collisionState;

    public TileHeightProperties(float maxHeight, string heightClassification, bool collisionState) {
        this.maxHeight = maxHeight;
        this.heightClassification = heightClassification;
        this.collisionState = collisionState;
    }

    public float targetHeight() {
        return this.maxHeight;
    }

    public string getHeightClassification() {
        return this.heightClassification;
    }
}

public class Height : DataBucket {

    public readonly float heightValue;
    public readonly TileHeightProperties tileHeightProperties;

    public Height(float heightValue, TileHeightProperties heightProperties) {
        this.heightValue = heightValue;
        this.tileHeightProperties = heightProperties;
    }

    public TileHeightProperties getTileProperties() {
        return this.tileHeightProperties;
    }

    public static Direction getLowestNeighbour(Tile tile) {
        Tile[] tiles = tile.getOrdinals();
        Height lowestHeight = null;
        int indexOfInterest = -1;
        for (int i = 0; i < Tile.NUMBER_OF_DIRECTIONS; i++) {
            Height testHeight = Height.getHeightForTile(tiles[i]);
            if (lowestHeight == null) {
                lowestHeight = testHeight;
                indexOfInterest = i;
                continue;
            }

            if (testHeight.heightValue < lowestHeight.heightValue) {
                lowestHeight = testHeight;
                indexOfInterest = i;
            }
        }

        return (Direction)indexOfInterest;
    }

    public static Height getHeightForTile(Tile tile) {
        return (Height)tile.getData(SurfaceHeight.NAME);
    }
}
