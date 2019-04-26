using System;
using System.Collections.Generic;

public abstract class PlanetPropertyGenerator : PlanetProperty {

    private readonly string planetPropertyName;

    public PlanetPropertyGenerator(string planetPropertyName) {
        this.planetPropertyName = planetPropertyName;
    }

    public string getName() {
        return this.planetPropertyName;
    }

    public abstract List<String> getDependencies();

    public abstract bool initialise(Generable generable);

    public abstract void generateNoise(int seed);

    public abstract void preallocateMapData(int width, int height);

    public abstract void generateSurface(int x, int y, float nx, float ny, float nz, float nw);

    /// <summary>
    /// Note - if this function depends upon another property it should be declared in the dependencies
    /// 
    /// Moreover each tile here should be generated independently. If you have a tile that affects or is affected by neighbouring tiles it should be a feature and not a property.
    /// We can only guarantee that the current tile has run its dependencies prior to this function's execution and NOT any neighbouring tiles  
    /// </summary>
    public abstract void generateTile(Tile t, int x, int y);
}