using UnityEngine;
using AccidentalNoise;

public class WrappingWorldGenerator : Generator {

    internal ImplicitFractal heightMap;
    internal ImplicitCombiner heatMap;
    internal ImplicitFractal moistureMap;

    public WrappingWorldGenerator(Generable generable) : base(generable) {
        // HeightMap
        heightMap = new ImplicitFractal(FractalType.MULTI, BasisType.SIMPLEX, InterpolationType.QUINTIC, this.terrainOctaves, this.terrainFrequency, this.seed);

        // Heat Map
        ImplicitGradient gradient = new ImplicitGradient(1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1);
        ImplicitFractal heatFractal = new ImplicitFractal(FractalType.MULTI, BasisType.SIMPLEX, InterpolationType.QUINTIC, this.heatOctaves, this.heatFrequency, this.seed);

        heatMap = new ImplicitCombiner(CombinerType.MULTIPLY);
        heatMap.AddSource(gradient);
        heatMap.AddSource(heatFractal);

        // Moisture Map
        moistureMap = new ImplicitFractal(FractalType.MULTI, BasisType.SIMPLEX, InterpolationType.QUINTIC, this.moistureOctaves, this.moistureFrequency, this.seed);
    }


	protected override void getData() {
		this.heightData = new MapData(this.width, this.height);
		this.heatData = new MapData(this.width, this.height);
		this.moistureData = new MapData(width, this.height);
		
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

				float heightValue = (float)this.heightMap.Get (nx, ny, nz, nw);
				float heatValue = (float)this.heatMap.Get (nx, ny, nz, nw);
				float moistureValue = (float)this.moistureMap.Get (nx, ny, nz, nw);
				
				// keep track of the max and min values found
				if (heightValue > heightData.Max) heightData.Max = heightValue;
				if (heightValue < heightData.Min) heightData.Min = heightValue;
				
				if (heatValue > heatData.Max) heatData.Max = heatValue;
				if (heatValue < heatData.Min) heatData.Min = heatValue;
				
				if (moistureValue > moistureData.Max) moistureData.Max = moistureValue;
				if (moistureValue < moistureData.Min) moistureData.Min = moistureValue;
				
				heightData.Data[x,y] = heightValue;
				heatData.Data[x,y] = heatValue;
				moistureData.Data[x,y] = moistureValue;		
			}
		}			
	}
	
	protected override Tile getTileAbove(Tile t) {
		return this.tiles[t.x, MathHelper.Mod (t.y - 1, this.height)];
	}

	protected override Tile getTileBelow(Tile t) {
		return this.tiles[t.x, MathHelper.Mod (t.y + 1, this.height)];
	}

	protected override Tile getTileOnLeft(Tile t) {
		return this.tiles[MathHelper.Mod(t.x - 1, this.width), t.y];
	}

	protected override Tile getTileOnRight(Tile t) {
		return this.tiles[MathHelper.Mod (t.x + 1, this.width), t.y];
	}
}
