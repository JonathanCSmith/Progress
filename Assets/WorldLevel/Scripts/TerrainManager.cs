using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class TerrainManager {

    PlanetController controller;

    private Dictionary<FirstPersonController, PlayerTerrainView> playerViewMap = new Dictionary<FirstPersonController, PlayerTerrainView>();
    private Dictionary<ChunkIndex, Terrain> terrain = new Dictionary<ChunkIndex, Terrain>();
    private Dictionary<ChunkIndex, int> scheduledOffloads = new Dictionary<ChunkIndex, int>();

    public TerrainManager(PlanetController controller) {
        this.controller = controller;
    }

    // Spawn a player in world - used by game controller or netcode
    public bool spawnPlayer(FirstPersonController controller, ContextualLocation location) {
        PlayerTerrainView terrainView;
        if (this.playerViewMap.ContainsKey(controller)) {
            terrainView = this.playerViewMap[controller];
        }

        else {
            terrainView = new PlayerTerrainView();
            this.playerViewMap.Add(controller, terrainView);
        }

        return this.spawnPlayer(controller, location, terrainView);
    }

    // General update - used to age assess the chunks to be offloaded and enact their offloading if required
    public void OnUpdate() {
        // Iterate through our chunks and check their age - if they are above a threshold mark them for unload
        List<ChunkIndex> offloadedChunks = new List<ChunkIndex>();
        foreach (KeyValuePair<ChunkIndex, int> chunks in this.scheduledOffloads) {
            if (System.DateTime.Now - chunk.Value >= this.ageThreshold) {
                this.offloadChunk(chunk.Key);
                this.offloadedChunks.add(chunk.Key);
            }
        }

        // Unload
        foreach (ChunkIndex chunk in offloadedChunks) {
            // TODO: Call terrain and offload data
            Terrain terrain = this.terrain.get(chunk);
            this.terrain.Remove(chunk);
            terrain.Destroy();

            this.scheduledOffloads.remove(chunk);
        }
    }

    // Interal method to handle a player moving in the terrain. 
    internal void movePlayer(FirstPersonController controller) {
        if (!this.playerViewMap.ContainsKey(controller)) {
            throw new System.NotImplementedException(); // TODO:
        }

        PlayerTerrainView view = this.playerViewMap[controller];
        view.movePlayer();
    }

    // Internal method to create a terrain
    internal bool spawnChunk(ChunkIndex chunkIndex) {
        if (this.terrain.ContainsKey(chunkIndex)) {
            return true;
        }

        Terrain terrain = this.generateChunk(chunkIndex);
        if (terrain == null) {
            return false;
        }

        // TODO: Is there additional data that needs to be deserialized
        // IF YES GET JSON AND ADD TO TERRAIN
    }

    IEnumerator spawnChunks(List<ChunkIndex> chunks) {
        // Iterate through chunks to load with a yeild - may need batching?
        for (int i = 0; i < chunks.length; i++) {
            ChunkIndex chunkIndex = chunks.get(i);
            bool outcome = this.spawnChunk(chunkIndex);
            // TODO: Log outcome - it shouldn't happen but this should help us trace and eliminate.
            yield;
        }
    }

    // Add chunks to the list for offloading
    internal void terrainManagerMarkChunksForOffload(List<ChunkIndex> chunksToOffload) {
        // Loop through the chunks to offload
        foreach (ChunkIndex chunkIndex in chunksToOffload) {

            // Compare this chunk to other players' requirements - we dont want to offload if someone else requires it
            bool found = false;
            foreach (KeyValuePair playerViews in this.playerViewMap) {
                found = playerViews.Value.isChunkInActiveChunks(chunkIndex);
                if (found) {
                    break;
                }
            }

            if (found) {
                continue;
            }

            this.scheduledOffloads.put(chunkIndex, System.DateTime.Now);
        }
    }

    // Generate a chunk!
    private Terrain generateChunk(ChunkIndex chunkIndex) {
        // TODO: We have done most of this code already
        this.terrain.Add(chunkIndex, terrain);
    }

    // Spawn the player with some contextual location pointers - if nulls we will pick
    private bool spawnPlayer(FirstPersonController controller, ContextualLocation location, PlayerTerrainView terrainView) {
        if (location == null) {
            location = this.pickRandomLocation();
            if (location == null) {
                return null;
            }
        }

        if (location.getChildIndex() == null) {
            ContextualLocaiton randomLocation = this.pickRandomLocationInChunk(location.getParentIndex());
            if (randomLocation == null) {
                return false;
            }

            location.setChildIndex(randomLocation);
        }

        return terrainView.initialise(location);
    }

    // Pick a chunk and a location
    private ContextualLocation pickRandomLocation() {
        return new ContextualLocation(this.pickRandomLocationInChunk(new ChunkIndex(new Vector2(0, 0))));
    }

    // Pick a location inside a chunk
    private ContextualLocation pickRandomLocationInChunk(ChunkIndex chunkIndex) {
        // TODO : True spawn logic goes here! We have written some of this already.
        // TODO : This number is also not good for actual spawn location... its a locaiton inside of a chunk
        return new ContextualLocaiton(new AbsoluteLocation(Vector2(0, 0)));
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }




    //internal void generateTerrain(int sourceSize, int targetSize) {
    //    Terrain terrain = gameObject.AddComponent<Terrain>();
    //    TerrainCollider terrainCollider = gameObject.AddComponent<TerrainCollider>();

    //    // Set the data properties - TODO investigate all of the magic numbers here ;)
    //    TerrainData terrainData = new TerrainData();
    //    terrainData.heightmapResolution = sourceSize + 1;
    //    terrainData.baseMapResolution = 512 + 1;
    //    terrainData.SetDetailResolution(1024, 32);
    //    terrainData.size = new Vector3(targetSize, 256, targetSize);
    //    float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);

    //    // Actually set the terrain data
    //    // Some things to consider - the amound of scale up you will be doing (from heightMapWidth -> size) will dampen your world automatically
    //    // It would be best to tune this parameter and the just increase the world size as necessary.
    //    // Additionally - rivers need fixing
    //    int currentWidth = terrainData.heightmapWidth;
    //    int currentHeight = terrainData.heightmapHeight;
    //    for (int x = 0; x < currentWidth; x++) {
    //        for (int y = 0; y < currentHeight; y++) {
    //            Color pix = realHeightMapData.GetPixel(x, y);
    //            heights[x, y] = pix.grayscale;
    //        }
    //    }

    //    terrainData.SetHeights(0, 0, heights);
    //    terrain.terrainData = terrainData;
    //    terrainCollider.terrainData = terrainData;
    //}

    //internal void spawnPlayer(GameObject player, FirstPersonController controller) {
    //    Debug.Log("Attempting to spawn the player!");

    //    Vector3 terrainSize = this.terrain.terrainData.size;
    //    Vector3 terrainOrigin = this.terrain.transform.position;

    //    // Valid search area in our current terrain
    //    int xFrom = (int)Mathf.Round(terrainOrigin.x) + this.spawnBufferRadius;
    //    int xTo = (int)Mathf.Round(terrainOrigin.x + terrainSize.x) - this.spawnBufferRadius;
    //    int zFrom = (int)Mathf.Round(terrainOrigin.z) + this.spawnBufferRadius;
    //    int zTo = (int)Mathf.Round(terrainOrigin.z + terrainSize.z) - this.spawnBufferRadius;

    //    // Loop through our current terrain to identify a spawn point
    //    for (int x = xFrom; x < xTo; x++) {
    //        for (int z = zFrom; z < zTo; z++) {
    //            float height = this.terrain.terrainData.GetHeight(x, z);

    //            // Dont spawn underwater
    //            if (height < this.waterHeight) {
    //                continue;
    //            }

    //            // Check if there is similar sized terrain immediately around us (no steep gradients)
    //            bool foundSteepArea = false;
    //            for (int localizedX = x - spawnBufferRadius; localizedX < x + spawnBufferRadius; localizedX++) {
    //                for (int localizedZ = z - spawnBufferRadius; localizedZ < z + spawnBufferRadius; localizedZ++) {
    //                    float localizedHeight = this.terrain.terrainData.GetHeight(localizedX, localizedZ);
    //                    float diff = Math.Abs(localizedHeight - height);
    //                    foundSteepArea = diff > this.maxHeightDifferenceOnSpawnPad;
    //                    if (foundSteepArea) {
    //                        break;
    //                    }
    //                }

    //                if (foundSteepArea) {
    //                    break;
    //                }
    //            }

    //            if (foundSteepArea) {
    //                continue;
    //            }

    //            // TODO Check if we are in a river

    //            // Let's spawn here!
    //            player.transform.position = new Vector3(x, (float)height + 3f, z); // Adding character height for fun
    //            Debug.Log("Spawning the player at: " + player.transform.position.x + ", " + player.transform.position.y + ", " + player.transform.position.z);
    //            return;
    //        }
    //    }

    //    // No safe spawn so fuck it - lets put it anywhere
    //    player.transform.position = new Vector3(terrainOrigin.x + this.spawnBufferRadius, this.terrain.terrainData.GetHeight(this.spawnBufferRadius, this.spawnBufferRadius) + 3f, terrainOrigin.z + this.spawnBufferRadius);
    //    Debug.Log("Spawning the player at: " + player.transform.position.x + ", " + player.transform.position.y + ", " + player.transform.position.z);
    //}
}
