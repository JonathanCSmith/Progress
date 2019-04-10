using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlanetController : Generable {

    [Header("Planet Properties")]
    [SerializeField]
    int width = 1024;
    [SerializeField]
    int height = 512;

    [Header("Generator Controls")]
    [SerializeField]
    protected bool forceGenerate = false;
    [SerializeField]
    protected int seed;
    [SerializeField]
    int sourcePatchSize = 128;
    [SerializeField]
    int targetPatchSize = 512;

    PlanetGenerator planetGenerator;
    TerrainManager terrainManager;

    // TODO: Provide a spherical view of the surface

    Terrain terrain;

    float waterHeight = 102.4f; // TODO: this is height * 0.4f;
    int spawnBufferRadius = 5;
    float maxHeightDifferenceOnSpawnPad = 5f;

    public PlanetController() {
        this.planetGenerator = new PlanetGenerator(this);
        this.terrainManager = new TerrainManager(this);

        // Create a nice default seed
        this.seed = UnityEngine.Random.Range(0, int.MaxValue);
    }

    public void setDimensions(int width, int height) {
        this.width = width;
        this.height = height;
    }

    public void setSeed(int seed) {
        this.seed = seed;
    }

    public void setPatchScaling(int sourcePatchSize, int targetPatchSize) {
        this.sourcePatchSize = sourcePatchSize;
        this.targetPatchSize = targetPatchSize;

        // TODO: Forward to terrain manager?
    }

    public void generate() {
        this.planetGenerator.setDimensions(this.width, this.height);
        this.planetGenerator.setSeed(this.seed);
        this.planetGenerator.generate();


        // TODO: this shouldn't be needed as it should be handled by the spawn player now
        //this.planetGenerator.generateTerrain(this.sourcePatchSize, this.targetPatchSize);
    }

    public void spawnPlayer(GameObject player, FirstPersonController controller) {
        this.terrainManager.spawnPlayer(controller, null); // TODO - not null as default - this should not be the general case
    }
}
