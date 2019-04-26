using System;
using System.Collections.Generic;
using AccidentalNoise;
using UnityEngine;

public class MoisturePlanetProperty : PlanetPropertyGenerator {

    public static readonly string NAME = "moisture";

    // Available tile props
    List<TileMoistureProperties> tileProperties;
    List<MoistureHeightClassificationModifier> heightClassificationModifiers;

    private ImplicitFractal moistureMapNoise;
    protected int moistureOctaves = 4;
    protected double moistureFrequency = 3.0;

    protected MapData moistureData;

    public MoisturePlanetProperty() : base(MoisturePlanetProperty.NAME) {
        this.tileProperties = MoisturePlanetProperty.generateDefaultTileMoistureProperties();
        this.heightClassificationModifiers = MoisturePlanetProperty.generateDefaultMoistureClassificationModifiers();
    }

    public override bool initialise(Generable generable) {
        return generable.checkPropertyEnabled(HeightPlanetProperty.NAME);
    }

    public override void generateNoise(int seed) {
        this.moistureMapNoise = new ImplicitFractal(FractalType.MULTI, BasisType.SIMPLEX, InterpolationType.QUINTIC, this.moistureOctaves, this.moistureFrequency, seed);
    }

    public override void preallocateMapData(int width, int height) {
        this.moistureData = new MapData(width, height);
    }

    public override void generateSurface(int x, int y, float nx, float ny, float nz, float nw) {
        float moistureValue = (float)this.moistureMapNoise.Get(nx, ny, nz, nw);
        if (moistureValue > this.moistureData.max) {
            this.moistureData.max = moistureValue;
        }

        if (moistureValue < this.moistureData.min) {
            this.moistureData.min = moistureValue;
        }
        this.moistureData.data[x, y] = moistureValue;
    }

    public override void generateTile(Tile t, int x, int y) {
        Height height = Height.getHeightForTile(t);
        string heightClassification = height.getTileProperties().getHeightClassification();
        MoistureHeightClassificationModifier defaultModifier = null;
        bool appliedTransform = false;
        foreach (MoistureHeightClassificationModifier modifier in this.heightClassificationModifiers) {
            if (modifier.getClassification() == "default") {
                defaultModifier = modifier;
            }

            if (modifier.getClassification() == heightClassification) {
                appliedTransform = true;
                if (modifier.getSign() == "-") {
                    this.moistureData.data[x, y] -= modifier.getMultiplicativeModifier() * height.heightValue;
                    break;
                }

                else {
                    this.moistureData.data[x, y] += modifier.getMultiplicativeModifier() * height.heightValue;
                    break;
                }
            }
        }

        if (!appliedTransform && defaultModifier != null) {
            if (defaultModifier.getSign() == "-") {
                this.moistureData.data[x, y] -= defaultModifier.getMultiplicativeModifier() * height.heightValue;
            }

            else {
                this.moistureData.data[x, y] += defaultModifier.getMultiplicativeModifier() * height.heightValue;
            }
        }

        //Moisture Map Analyze  
        float moistureValue = moistureData.data[x, y];
        moistureValue = (moistureValue - moistureData.min) / (moistureData.max - moistureData.min);
        Moisture m = new Moisture(moistureValue, this.pickMoistureProperties(moistureValue));
        Moisture.setMoisture(t, m);
    }

    public TileMoistureProperties pickMoistureProperties(float moistureValue) {
        TileMoistureProperties currentSelection = null;
        foreach (TileMoistureProperties properties in this.tileProperties) {
            if (Mathf.Approximately(properties.targetMoisture(), moistureValue)) {
                return properties;
            }

            // First random entry is an unpredictable default TODO
            if (currentSelection == null) {
                currentSelection = properties;
                continue;
            }

            // Find closest less than value
            if (moistureValue < properties.targetMoisture() && (Math.Abs(currentSelection.targetMoisture() - moistureValue) > Math.Abs(properties.targetMoisture() - moistureValue))) {
                currentSelection = properties;
            }
        }

        return currentSelection;
    }

    public void addMoistureToTile(Tile t, float amount) {
        TileIndex tileIndex = t.getTileIndex();
        this.moistureData.data[tileIndex.x, tileIndex.y] += amount;

        Moisture m = Moisture.getMoisture(t);
        float newAmount = m.moistureValue + amount;
        if (newAmount > 1) {
            newAmount = 1;
        }

        Moisture newMoisture = new Moisture(newAmount, this.pickMoistureProperties(newAmount));
        Moisture.setMoisture(t, newMoisture);
    }

    public static List<MoistureHeightClassificationModifier> generateDefaultMoistureClassificationModifiers() {
        List<MoistureHeightClassificationModifier> list = new List<MoistureHeightClassificationModifier>();

        // Props
        list.Add(new MoistureHeightClassificationModifier("DeepWater", 8f, "+"));
        list.Add(new MoistureHeightClassificationModifier("MidWater", 5f, "+"));
        list.Add(new MoistureHeightClassificationModifier("ShallowWater", 3f, "+"));
        list.Add(new MoistureHeightClassificationModifier("Shoreline", 1f, "+"));
        list.Add(new MoistureHeightClassificationModifier("Lowlands", 0.2f, "+"));

        return list;
    }

    public static List<TileMoistureProperties> generateDefaultTileMoistureProperties() {
        List<TileMoistureProperties> list = new List<TileMoistureProperties>();

        // Props
        list.Add(new TileMoistureProperties(0.27f, MoistureType.Dryest));
        list.Add(new TileMoistureProperties(0.4f, MoistureType.Dryer));
        list.Add(new TileMoistureProperties(0.6f, MoistureType.Dry));
        list.Add(new TileMoistureProperties(0.8f, MoistureType.Wet));
        list.Add(new TileMoistureProperties(0.9f, MoistureType.Wetter));
        list.Add(new TileMoistureProperties(1f, MoistureType.Wettest));

        return list;
    }
}

public enum MoistureType {
    Wettest = 5,
    Wetter = 4,
    Wet = 3,
    Dry = 2,
    Dryer = 1,
    Dryest = 0
}


public class MoistureHeightClassificationModifier {

    public readonly string targetClassification;
    public readonly float multiplicativeModifier;
    public readonly string sign;

    public MoistureHeightClassificationModifier(string targetClassification, float multiplicativeModifier, string sign) {
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

public class TileMoistureProperties {

    public readonly float maxMoisture;
    public readonly MoistureType moistureClassification;

    public TileMoistureProperties(float maxMoisture, MoistureType moistureClassification) {
        this.maxMoisture = maxMoisture;
        this.moistureClassification = moistureClassification;
    }

    public float targetMoisture() {
        return this.maxMoisture;
    }

    public MoistureType getMoistureClassification() {
        return this.moistureClassification;
    }
}

public class Moisture : DataBucket {

    public readonly float moistureValue;
    public readonly TileMoistureProperties moistureType;

    public Moisture(float moistureValue, TileMoistureProperties moistureType) {
        this.moistureValue = moistureValue;
        this.moistureType = moistureType;
    }

    public static Moisture getMoisture(Tile t) {
        return (Moisture)t.getData(MoisturePlanetProperty.NAME);
    }

    public static void setMoisture(Tile t, Moisture moisture) {
        t.setData(MoisturePlanetProperty.NAME, moisture);
    }
}

