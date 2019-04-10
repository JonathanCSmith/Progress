using System;
using AccidentalNoise;

public class MoisturePlanetProperty : PlanetProperty {

    private ImplicitFractal moistureMapNoise;
    protected int moistureOctaves = 4;
    protected double moistureFrequency = 3.0;

    protected MapData moistureData;

    public MoisturePlanetProperty() : base("moisture") { }

    public override void generateNoise(int seed) {
        this.moistureMapNoise = new ImplicitFractal(FractalType.MULTI, BasisType.SIMPLEX, InterpolationType.QUINTIC, this.moistureOctaves, this.moistureFrequency, seed);
    }

    public override void preallocateMapData(int width, int height) {
        this.moistureData = new MapData(width, height);
    }

    public override void generateSurface(int x, int y, float nx, float ny, float nz, float nw) {
        float moistureValue = (float)this.moistureMapNoise.Get(nx, ny, nz, nw);
        if (moistureValue > this.moistureData.Max) {
            this.moistureData.Max = moistureValue;
        }

        if (moistureValue < this.moistureData.Min) {
            this.moistureData.Min = moistureValue;
        }
        this.moistureData.Data[x, y] = moistureValue;
    }

    public override void generateTile(Tile t, int x, int y) {
        throw new System.NotImplementedException();
    }
}
