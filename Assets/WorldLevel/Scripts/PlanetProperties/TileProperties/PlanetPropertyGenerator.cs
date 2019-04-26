using System;

public abstract class PlanetProperty {

    public readonly string planetPropertyName;

    public PlanetProperty(string planetPropertyName) {
        this.planetPropertyName = planetPropertyName;
    }

    public abstract void generateNoise(int seed);

    public abstract void preallocateMapData(int width, int height);

    public abstract void generateSurface(int x, int y, float nx, float ny, float nz, float nw);

    public abstract void generateTile(Tile t, int x, int y);
}