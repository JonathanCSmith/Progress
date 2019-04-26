using System;
using System.Collections.Generic;
using AccidentalNoise;
using UnityEngine;

public class PlanetHeat : PlanetPropertyGenerator {

    public static readonly string NAME = "heat";

    // Available tile props
    List<TileHeatProperties> tileProperties;
    List<HeatHeightClassificationModifier> heightClassificationModifiers;

    // Noise props
    private ImplicitCombiner heatMapNoise;
    protected int heatOctaves = 4;
    protected double heatFrequency = 3.0;

    // Data
    protected MapData heatData;

    public PlanetHeat() : base(PlanetHeat.NAME) {
        this.tileProperties = PlanetHeat.generateDefaultTileHeatProperties();
        this.heightClassificationModifiers = PlanetHeat.generateDefaultHeatClassificationModifiers();
    }

    public override bool initialise(Generable generable) {
        return generable.checkPropertyEnabled(HeightPlanetProperty.NAME);
    }

    public override void generateNoise(int seed) {
        // Heat Map
        ImplicitGradient gradient = new ImplicitGradient(1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1);
        ImplicitFractal heatFractal = new ImplicitFractal(FractalType.MULTI, BasisType.SIMPLEX, InterpolationType.QUINTIC, this.heatOctaves, this.heatFrequency, seed);

        this.heatMapNoise = new ImplicitCombiner(CombinerType.MULTIPLY);
        this.heatMapNoise.AddSource(gradient);
        this.heatMapNoise.AddSource(heatFractal);
    }

    public override void preallocateMapData(int width, int height) {
        this.heatData = new MapData(width, height);
    }

    public override void generateSurface(int x, int y, float nx, float ny, float nz, float nw) {
        float heatValue = (float)this.heatMapNoise.Get(nx, ny, nz, nw);
        if (heatValue > this.heatData.max) {
            this.heatData.max = heatValue;
        }

        if (heatValue < this.heatData.min) {
            this.heatData.min = heatValue;
        }
        this.heatData.data[x, y] = heatValue;
    }

    public override void generateTile(Tile t, int x, int y) {
        Height height = Height.getHeightForTile(t);
        string heightClassification = height.getTileProperties().getHeightClassification();
        HeatHeightClassificationModifier defaultModifier = null;
        bool appliedTransform = false;
        foreach (HeatHeightClassificationModifier modifier in this.heightClassificationModifiers) {
            if (modifier.getClassification() == "default") {
                defaultModifier = modifier;
            }

            if (modifier.getClassification() == heightClassification) {
                appliedTransform = true;
                if (modifier.getSign() == "-") {
                    this.heatData.data[x, y] -= modifier.getMultiplicativeModifier() * height.heightValue;
                    break;
                }

                else {
                    this.heatData.data[x, y] += modifier.getMultiplicativeModifier() * height.heightValue;
                    break;
                }
            }
        }

        if (!appliedTransform && defaultModifier != null) {
            if (defaultModifier.getSign() == "-") {
                this.heatData.data[x, y] -= defaultModifier.getMultiplicativeModifier() * height.heightValue;
            }

            else {
                this.heatData.data[x, y] += defaultModifier.getMultiplicativeModifier() * height.heightValue;
            }
        }

        // Set heat value
        float heatValue = this.heatData.data[x, y];
        heatValue = (heatValue - heatData.min) / (heatData.max - heatData.min);
        Heat h = new Heat(heatValue, this.pickHeatProperties(height.heightValue));
        Heat.setHeat(t, h);
    }

    public TileHeatProperties pickHeatProperties(float heightValue) {
        TileHeatProperties currentSelection = null;
        foreach (TileHeatProperties properties in this.tileProperties) {
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

    public static List<TileHeatProperties> generateDefaultTileHeatProperties() {
        List<TileHeatProperties> list = new List<TileHeatProperties>();

        // Props
        list.Add(new TileHeatProperties(0.05f, HeatType.Coldest));
        list.Add(new TileHeatProperties(0.18f, HeatType.Colder));
        list.Add(new TileHeatProperties(0.4f, HeatType.Cold));
        list.Add(new TileHeatProperties(0.6f, HeatType.Warm));
        list.Add(new TileHeatProperties(0.8f, HeatType.Warmer));
        list.Add(new TileHeatProperties(1f, HeatType.Warmest));

        return list;
    }

    public static List<HeatHeightClassificationModifier> generateDefaultHeatClassificationModifiers() {
        List<HeatHeightClassificationModifier> list = new List<HeatHeightClassificationModifier>();

        // Props
        list.Add(new HeatHeightClassificationModifier("midlands", 0.1f, "-"));
        list.Add(new HeatHeightClassificationModifier("highlands", 0.25f, "-"));
        list.Add(new HeatHeightClassificationModifier("mountains", 0.4f, "-"));
        list.Add(new HeatHeightClassificationModifier("default", 0.01f, "+"));

        return list;
    }
}

public enum HeatType {
    Coldest = 0,
    Colder = 1,
    Cold = 2,
    Warm = 3,
    Warmer = 4,
    Warmest = 5
}

public class HeatHeightClassificationModifier {

    public readonly string targetClassification;
    public readonly float multiplicativeModifier;
    public readonly string sign;

    public HeatHeightClassificationModifier(string targetClassification, float multiplicativeModifier, string sign) {
        this.targetClassification = targetClassification;
        this.multiplicativeModifier = multiplicativeModifier;
        this.sign = sign;
    }

    public string getClassification() {
        return this.targetClassification;
    }

    public float getMultiplicativeModifier() {
        return this.multiplicativeModifier;
    }

    public string getSign() {
        return this.sign;
    }
}

public class TileHeatProperties {

    public float maxHeight;
    public HeatType heatClassification;

    public TileHeatProperties(float maxHeight, HeatType heatClassification) {
        this.maxHeight = maxHeight;
        this.heatClassification = heatClassification;
    }

    public float targetHeight() {
        return this.maxHeight;
    }

    public HeatType getHeatClassification() {
        return this.heatClassification;
    }
}

public class Heat : DataBucket {

    public readonly float heatValue;
    public readonly TileHeatProperties heatType;

    public Heat(float heatValue, TileHeatProperties heatType) {
        this.heatValue = heatValue;
        this.heatType = heatType;
    }

    public static Heat getHeat(Tile tile) {
        return (Heat)tile.getData(PlanetHeat.NAME);
    }

    public static void setHeat(Tile tile, Heat heat) {
        tile.setData(PlanetHeat.NAME, heat);
    }
}
