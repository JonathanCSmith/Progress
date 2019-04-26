using System.Collections.Generic;
using UnityEngine;

public abstract class Generator {

    //protected Texture2D realHeightMapData;
    //protected Texture2D heightMapData;
    //protected Texture2D heatMapData;
    //protected Texture2D moistureMapData;
    //protected Texture2D biomeMapData;

    //// Our texture output gameobject
    //protected MeshRenderer HeightMapRenderer;
    //protected MeshRenderer HeatMapRenderer;
    //protected MeshRenderer MoistureMapRenderer;
    //protected MeshRenderer BiomeMapRenderer;

    public readonly Generable generable;

    // Constructor with seed if none provided

    protected Generator(Generable generable) {
        this.generable = generable;

        //HeightMapRenderer = this.generable.transform.Find("HeightTexture").GetComponent<MeshRenderer>();
        //HeatMapRenderer = this.generable.transform.Find("HeatTexture").GetComponent<MeshRenderer>();
        //MoistureMapRenderer = this.generable.transform.Find("MoistureTexture").GetComponent<MeshRenderer>();
        //BiomeMapRenderer = this.generable.transform.Find("BiomeTexture").GetComponent<MeshRenderer>();
    }

    public virtual void generate() {
        // Set the seed for consistent results
        UnityEngine.Random.InitState(this.generable.getSeed());

        // Start by prepping our map layers
        if (!this.validateProperties()) {
            return;
        }

        // Run through our properties which are a little special vs features
        this.generateNoise();
        this.generateSurfaceData();
        this.generateTiles();
        this.updateNeighbours();

        // Features
        this.generateFeatures();

        //realHeightMapData = TextureGenerator.GetRealHeightMapData(width, height, tiles);
        //heightMapData = TextureGenerator.GetHeightMapTexture(width, height, tiles);
        //heatMapData = TextureGenerator.GetHeatMapTexture(width, height, tiles);
        //moistureMapData = TextureGenerator.GetMoistureMapTexture(width, height, tiles);
        //biomeMapData = TextureGenerator.GetBiomeMapTexture(width, height, tiles, ColdestValue, ColderValue, ColdValue);

        //HeightMapRenderer.materials[0].mainTexture = heightMapData;
        //HeatMapRenderer.materials[0].mainTexture = heatMapData;
        //MoistureMapRenderer.materials[0].mainTexture = moistureMapData;
        //BiomeMapRenderer.materials[0].mainTexture = biomeMapData;
    }

    protected abstract bool validateProperties();

    protected abstract void generateNoise();

    protected abstract void generateSurfaceData();

    // Build a Tile array from our data
    private void generateTiles() {
        this.generable.preallocateTiles();
        for (var x = 0; x < this.generable.getWidth(); x++) {
            for (var y = 0; y < this.generable.getHeight(); y++) {
                Tile t = new Tile(x, y);

                // Assess our tile properties in the context of the current data
                t = this.fillTileData(t, x, y);
                this.generable.setTile(t);
            }
        }
    }

    protected abstract Tile fillTileData(Tile t, int x, int y);

    private void updateNeighbours() {
        for (var x = 0; x < this.generable.getWidth(); x++) {
            for (var y = 0; y < this.generable.getHeight(); y++) {
                Tile t = this.generable.getTile(x, y);
                t.setLeft(this.generable.getTileByIndex(this.generable.getTileIndexOnLeft(t.getTileIndex())));
                t.setAbove(this.generable.getTileByIndex(this.generable.getTileIndexAbove(t.getTileIndex())));
                t.setRight(this.generable.getTileByIndex(this.generable.getTileIndexOnRight(t.getTileIndex())));
                t.setBelow(this.generable.getTileByIndex(this.generable.getTileIndexBelow(t.getTileIndex())));
            }
        }
    }

    protected abstract bool generateFeatures();
}
