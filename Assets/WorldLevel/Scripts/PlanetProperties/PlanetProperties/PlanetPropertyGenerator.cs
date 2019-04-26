using System;

public abstract class PlanetPropertyGenerator {

    private readonly string planetPropertyName;

    public PlanetPropertyGenerator(string planetPropertyName) {
        this.planetPropertyName = planetPropertyName;
    }

    public string getName() {
        return this.planetPropertyName;
    }

    public abstract bool initialise(Generable generable);

    public abstract void generateNoise(int seed);

    public abstract void preallocateMapData(int width, int height);

    public abstract void generateSurface(int x, int y, float nx, float ny, float nz, float nw);

    public abstract void generateTile(Tile t, int x, int y);
}