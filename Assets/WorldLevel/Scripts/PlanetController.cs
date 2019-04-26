using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlanetController : Generable {

    PlanetGenerator planetGenerator;
    TerrainManager terrainManager;

    // TODO: Provide a spherical view of the surface

    Terrain terrain;

    float waterHeight = 102.4f; // TODO: this is height * 0.4f;
    int spawnBufferRadius = 5;
    float maxHeightDifferenceOnSpawnPad = 5f;

    // Props
    private SurfaceHeight heightProperty = new SurfaceHeight();
    private SurfaceHeat heatProperty = new SurfaceHeat();
    private SurfaceMoisture moistureProperty = new SurfaceMoisture();

    // Features
    private Rivers riverFeatureGenerator = new Rivers();
    private BodiesOf lakeFeatureGenerator = new BodiesOf();
    private Biomes biomeFeatureGenerator = new Biomes();

    private bool hasCleanedGenerator = false;
    private bool hasGenerated = false;

    public PlanetController() {
        // Create a nice default seed
        this.seed = UnityEngine.Random.Range(0, int.MaxValue);
    }

    public void Start() {
        this.planetGenerator = new PlanetGenerator(this);

        this.planetGenerator.addPropertyGenerator(this.heightProperty);
        this.planetGenerator.addPropertyGenerator(this.heatProperty);
        this.planetGenerator.addPropertyGenerator(this.moistureProperty);

        this.planetGenerator.addFeatureGenerator(this.riverFeatureGenerator);
        this.planetGenerator.addFeatureGenerator(this.lakeFeatureGenerator);
        this.planetGenerator.addFeatureGenerator(this.biomeFeatureGenerator);

        this.planetGenerator.init();

        // TODO: Set our terrain manager if necessary
        //this.terrainManager = new TerrainManager(this);
    }

    public void Update() {
        if (this.hasGenerated && !this.hasCleanedGenerator) {
            this.planetGenerator = null;
        }
    }

    public void generate() {
        this.planetGenerator.generate();
        this.hasGenerated = true;

        // TODO: this shouldn't be needed as it should be handled by the spawn player now
        //this.planetGenerator.generateTerrain(this.sourcePatchSize, this.targetPatchSize);
    }

    public Texture2D[] getTextures() {
        Texture2D[] textures = new Texture2D[this.planetData.Count];
        foreach (MapData mapData in this.planetData) {
            texture
        }

        return textures;
    }

    //public void spawnPlayer(GameObject player, FirstPersonController controller) {
    //    this.terrainManager.spawnPlayer(controller, null); // TODO - not null as default - this should not be the general case
    //}

    public override bool checkPropertyEnabled(string type) {
        return this.planetGenerator.properties.ContainsKey(type);
    }

    public override PropertyDecorator getProperty(string type) {
        return this.planetGenerator.properties[type];
    }

    public override bool checkFeatureEnabled(string type) {
        return this.planetGenerator.features.ContainsKey(type);
    }
}
