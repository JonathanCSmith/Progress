using System;
using AccidentalNoise;

public class HeatPlanetProperty : PlanetProperty {

    // Noise props
    private ImplicitCombiner heatMapNoise;
    protected int heatOctaves = 4;
    protected double heatFrequency = 3.0;

    // Data
    protected MapData heatData;

    public HeatPlanetProperty() : base("heat") { }

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
        float heatValue = (float) this.heatMapNoise.Get(nx, ny, nz, nw);
        if (heatValue > this.heatData.Max) {
            this.heatData.Max = heatValue;
        }

        if (heatValue < this.heatData.Min) {
            this.heatData.Min = heatValue;
        }
        this.heatData.Data[x,y] = heatValue;
    }

    public override void generateTile(Tile t, int x, int y) {
        Height height = (Height) t.getData("height");
        if (height == null) {
            height = new Height(); // TODO: Defaults? Planet specific?
        }

        switch (height.getTileProperties().getHeightClassification()) {
            case "midlands":
                this.heatData.Data[x, y] -= 0.1f * height.heightValue;
                break;
            
            case "highlands":
                this.heatData.Data[x, y] -= 0.25f * height.heightValue;
                break;

            case "mountains":
                this.heatData.Data[x, y] -= 0.4f * height.heightValue;
                break;

            default:
                this.heatData[x, y] += 0.01f * height.heightValue;
                break;
        }

        // Set heat value
        float heatValue = this.heatData.Data[x, y];
        heatValue = (heatValue - heatData.Min) / (heatData.Max - heatData.Min);
        t.addData(new Heat(heatValue));
    }

    public static List<TileHeatProperties> generateDefaultTileHeatProperties() {
        List<TileHeatProperties> list = new List<TileHeatProperties> ();

        // Props
        list.Add(new TileHeatProperties(, "", ));

        return list;
    }
}
