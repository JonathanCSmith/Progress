using UnityEngine;
using AccidentalNoise;
using System.Collections.Generic;

public abstract class WrappingWorldGenerator : Generator {

    protected MapData Clouds1;
    protected MapData Clouds2;

    // Height map properties
    protected float DeepWater = 0.2f;
    protected float ShallowWater = 0.4f;
    protected float Sand = 0.5f;
    protected float Grass = 0.7f;
    protected float Forest = 0.8f;
    protected float Rock = 0.9f;

    // Heat properties
    protected float ColdestValue = 0.05f;
    protected float ColderValue = 0.18f;
    protected float ColdValue = 0.4f;
    protected float WarmValue = 0.6f;
    protected float WarmerValue = 0.8f;

    // Moisture map
    protected float DryerValue = 0.27f;
    protected float DryValue = 0.4f;
    protected float WetValue = 0.6f;
    protected float WetterValue = 0.8f;
    protected float WettestValue = 0.9f;

    public readonly List<PlanetProperty> properties = new List<PlanetProperty>();

    public WrappingWorldGenerator(Generable generable, int width, int height) : base(generable, width, height) {
        this.addProperties();
    }

    public WrappingWorldGenerator(Generable generable, int seed, int width, int height) : base(generable, seed, width, height) {
        this.addProperties();
    }

    public abstract void addProperties();

    protected override void generateNoise() {
        foreach (PlanetProperty property in this.properties) {
            property.generateNoise(this.seed);
        }
    }

    protected override void generateSurfaceData() {
        foreach (PlanetProperty property in this.properties) {
            property.preallocateMapData(this.seed);
        }
		
		// loop through each x,y point - get height value
		for (var x = 0; x < this.width; x++) {
			for (var y = 0; y < this.height; y++) {
				
				// WRAP ON BOTH AXIS
				// Noise range
				float x1 = 0, x2 = 2;
				float y1 = 0, y2 = 2;				
				float dx = x2 - x1;
				float dy = y2 - y1;
				
				// Sample noise at smaller intervals
				float s = x / (float)this.width;
				float t = y / (float)this.height;
				
				// Calculate our 4D coordinates
				float nx = x1 + Mathf.Cos (s*2*Mathf.PI) * dx/(2*Mathf.PI);
				float ny = y1 + Mathf.Cos (t*2*Mathf.PI) * dy/(2*Mathf.PI);
				float nz = x1 + Mathf.Sin (s*2*Mathf.PI) * dx/(2*Mathf.PI);
				float nw = y1 + Mathf.Sin (t*2*Mathf.PI) * dy/(2*Mathf.PI);	

                foreach (PlanetProperty property in this.properties) {
                    property.generateSurface(x, y, nx, ny, nz, nw);
                }
            }
		}			
	}
	
	protected override Tile getTileAbove(Tile tile) {
		return this.tiles[tile.x, MathHelper.Mod (tile.y - 1, this.height)];
	}

	protected override Tile getTileBelow(Tile tile) {
		return this.tiles[tile.x, MathHelper.Mod (tile.y + 1, this.height)];
	}

	protected override Tile getTileOnLeft(Tile tile) {
		return this.tiles[MathHelper.Mod(tile.x - 1, this.width), tile.y];
	}

	protected override Tile getTileOnRight(Tile tile) {
		return this.tiles[MathHelper.Mod (tile.x + 1, this.width), tile.y];
	}

    protected override Tile fillTileData(Tile t, int x, int y) {
        foreach (PlanetProperty property in this.properties) {
            property.generateTile(t, x, y);
        }


        ////set heightmap value and gather height properties
        //float heightValue = this.heightData.Data[x, y];
        //heightValue = (heightValue - heightData.Min) / (heightData.Max - heightData.Min);
        //t.heightValue = heightValue;
        //t.heightClassification = this.getHeightClassification(heightValue);
        //t.collisionState = this.getCollisionStateFromHeightClassification(t.heightClassification);


        //adjust moisture based on height
        if (t.heightClassification == HeightClassification.DeepWater) {
            moistureData.Data[t.x, t.y] += 8f * t.heightValue;
        }
        else if (t.heightClassification == HeightClassification.ShallowWater) {
            moistureData.Data[t.x, t.y] += 3f * t.heightValue;
        }
        else if (t.heightClassification == HeightClassification.Shore) {
            moistureData.Data[t.x, t.y] += 1f * t.heightValue;
        }
        else if (t.heightClassification == HeightClassification.Sand) {
            moistureData.Data[t.x, t.y] += 0.2f * t.heightValue;
        }

        //Moisture Map Analyze  
        float moistureValue = moistureData.Data[x, y];
        moistureValue = (moistureValue - moistureData.Min) / (moistureData.Max - moistureData.Min);
        t.MoistureValue = moistureValue;

        //set moisture type
        if (moistureValue < DryerValue) t.MoistureType = MoistureType.Dryest;
        else if (moistureValue < DryValue) t.MoistureType = MoistureType.Dryer;
        else if (moistureValue < WetValue) t.MoistureType = MoistureType.Dry;
        else if (moistureValue < WetterValue) t.MoistureType = MoistureType.Wet;
        else if (moistureValue < WettestValue) t.MoistureType = MoistureType.Wetter;
        else t.MoistureType = MoistureType.Wettest;




        if (Clouds1 != null) {
            t.Cloud1Value = Clouds1.Data[x, y];
            t.Cloud1Value = (t.Cloud1Value - Clouds1.Min) / (Clouds1.Max - Clouds1.Min);
        }

        if (Clouds2 != null) {
            t.Cloud2Value = Clouds2.Data[x, y];
            t.Cloud2Value = (t.Cloud2Value - Clouds2.Min) / (Clouds2.Max - Clouds2.Min);
        }

        return t;
    }

    private HeightClassification getHeightClassification(float heightValue) {
        if (heightValue < DeepWater) {
            return HeightClassification.DeepWater;
        }

        else if (heightValue < ShallowWater) {
            return HeightClassification.ShallowWater;
        }

        else if (heightValue < Sand) {
            return HeightClassification.Sand;
        }

        else if (heightValue < Grass) {
            return HeightClassification.Grass;
        }

        else if (heightValue < Forest) {
            return HeightClassification.Forest;
        }

        else if (heightValue < Rock) {
            return HeightClassification.Rock;
        }

        else {
            return HeightClassification.Snow;
        }
    }

    private bool getCollisionStateFromHeightClassification(HeightClassification classification) {
        switch (classification) {
            case HeightClassification.DeepWater:
            case HeightClassification.ShallowWater:
                return false;

            case HeightClassification.Sand:
            case HeightClassification.Grass:
            case HeightClassification.Forest:
            case HeightClassification.Rock:
            case HeightClassification.Snow:
            default:
                return true;
        }
    }
}
