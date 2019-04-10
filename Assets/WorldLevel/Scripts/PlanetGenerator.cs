using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlanetGenerator : WrappingWorldGenerator {

    PlanetController planetController;

    public PlanetGenerator(PlanetController planetController) : base(planetController) {
        this.planetController = planetController;
    }

    internal void generateTerrain(int sourceSize, int targetSize) {
        Terrain terrain = gameObject.AddComponent<Terrain>();
        TerrainCollider terrainCollider = gameObject.AddComponent<TerrainCollider>();

        // Set the data properties - TODO investigate all of the magic numbers here ;)
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = sourceSize + 1;
        terrainData.baseMapResolution = 512 + 1;
        terrainData.SetDetailResolution(1024, 32);
        terrainData.size = new Vector3(targetSize, 256, targetSize);
        float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);

        // Actually set the terrain data
        // Some things to consider - the amound of scale up you will be doing (from heightMapWidth -> size) will dampen your world automatically
        // It would be best to tune this parameter and the just increase the world size as necessary.
        // Additionally - rivers need fixing
        int currentWidth = terrainData.heightmapWidth;
        int currentHeight = terrainData.heightmapHeight;
        for (int x = 0; x < currentWidth; x++) {
            for (int y = 0; y < currentHeight; y++) {
                Color pix = realHeightMapData.GetPixel(x, y);
                heights[x, y] = pix.grayscale;
            }
        }

        terrainData.SetHeights(0, 0, heights);
        terrain.terrainData = terrainData;
        terrainCollider.terrainData = terrainData;
    }

    internal void spawnPlayer(GameObject player, FirstPersonController controller) {
        Debug.Log("Attempting to spawn the player!");

        Vector3 terrainSize = this.terrain.terrainData.size;
        Vector3 terrainOrigin = this.terrain.transform.position;

        // Valid search area in our current terrain
        int xFrom = (int)Mathf.Round(terrainOrigin.x) + this.spawnBufferRadius;
        int xTo = (int)Mathf.Round(terrainOrigin.x + terrainSize.x) - this.spawnBufferRadius;
        int zFrom = (int)Mathf.Round(terrainOrigin.z) + this.spawnBufferRadius;
        int zTo = (int)Mathf.Round(terrainOrigin.z + terrainSize.z) - this.spawnBufferRadius;

        // Loop through our current terrain to identify a spawn point
        for (int x = xFrom; x < xTo; x++) {
            for (int z = zFrom; z < zTo; z++) {
                float height = this.terrain.terrainData.GetHeight(x, z);

                // Dont spawn underwater
                if (height < this.waterHeight) {
                    continue;
                }

                // Check if there is similar sized terrain immediately around us (no steep gradients)
                bool foundSteepArea = false;
                for (int localizedX = x - spawnBufferRadius; localizedX < x + spawnBufferRadius; localizedX++) {
                    for (int localizedZ = z - spawnBufferRadius; localizedZ < z + spawnBufferRadius; localizedZ++) {
                        float localizedHeight = this.terrain.terrainData.GetHeight(localizedX, localizedZ);
                        float diff = Math.Abs(localizedHeight - height);
                        foundSteepArea = diff > this.maxHeightDifferenceOnSpawnPad;
                        if (foundSteepArea) {
                            break;
                        }
                    }

                    if (foundSteepArea) {
                        break;
                    }
                }

                if (foundSteepArea) {
                    continue;
                }

                // TODO Check if we are in a river

                // Let's spawn here!
                player.transform.position = new Vector3(x, (float)height + 3f, z); // Adding character height for fun
                Debug.Log("Spawning the player at: " + player.transform.position.x + ", " + player.transform.position.y + ", " + player.transform.position.z);
                return;
            }
        }

        // No safe spawn so fuck it - lets put it anywhere
        player.transform.position = new Vector3(terrainOrigin.x + this.spawnBufferRadius, this.terrain.terrainData.GetHeight(this.spawnBufferRadius, this.spawnBufferRadius) + 3f, terrainOrigin.z + this.spawnBufferRadius);
        Debug.Log("Spawning the player at: " + player.transform.position.x + ", " + player.transform.position.y + ", " + player.transform.position.z);
    }
}
