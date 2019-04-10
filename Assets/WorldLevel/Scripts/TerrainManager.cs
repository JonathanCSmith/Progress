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
            this.playerViewMap.put(controller, terrainView);
        }

        return this.spawnPlayer(controller, location, terrainView);
    }

    // General update - used to age assess the chunks to be offloaded and enact their offloading if required
    public void OnUpdate() {
        // Iterate through our chunks and check their age - if they are above a threshold mark them for unload
        List<ChunkIndex> offloadedChunks = new List<ChunkIndex>();
        for (KeyValuePair<ChunkIndex, int> chunks in this.scheduledOffloads) {
            if (System.DateTime.Now - chunk.Value >= this.ageThreshold) {
                this.offloadChunk(chunk.Key);
                this.offloadedChunks.add(chunk.Key);
            }
        }

        // Unload
        for (ChunkIndex chunk in offloadedChunks) {
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
            throw new RuntimeException or whatever;
        }

        PlayerTerrainView view = this.playerViewMap[controller];
        view.movePlayer();
    }

    // Internal method to create a terrain
    internal bool spawnChunk(ChunkIndex chunkIndex) {
        if (this.terrain.ContainsKey(chunkIndex)) {
            return true;
        }

        Terrain terrain = this.generateChunk(ChunkIndex chunkIndex);
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
        for (ChunkIndex chunkIndex in chunksToOffload) {

            // Compare this chunk to other players' requirements - we dont want to offload if someone else requires it
            bool found = false;
            for (KeyValuePair playerViews in this.playerViewMap) {
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
    private ContextualLocation pickRandomLocation( {
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
}
