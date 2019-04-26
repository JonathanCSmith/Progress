using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Generable : MonoBehaviour {

    [Header("Generator Controls")]
    [SerializeField]
    int width = 1024;
    [SerializeField]
    int height = 512;
    [SerializeField]
    bool forceGenerate = false;
    [SerializeField]
    internal int seed;
    [SerializeField]
    int sourcePatchSize = 128;
    [SerializeField]
    int targetPatchSize = 512;

    Tile[,] tiles;
    List<MapData> planetData = new List<MapData>();

    public int getWidth() {
        return this.width;
    }

    public int getHeight() {
        return this.height;
    }

    public int getSeed() {
        return this.seed;
    }

    public void setSeed(int seed) {
        this.seed = seed;
    }

    public void setDimensions(int width, int height) {
        this.width = width;
        this.height = height;
    }

    public void setPatchScaling(int sourcePatchSize, int targetPatchSize) {
        this.sourcePatchSize = sourcePatchSize;
        this.targetPatchSize = targetPatchSize;

        // TODO: Forward to terrain manager?
    }

    public Tile[,] getTiles() {
        return this.tiles;
    }

    public int getRelativeSanitizedXCoordinate(int x, int movement) {
        return this.getSanitizedXCoordinate(x + movement);
    }

    public int getRelativeSanitizedYCoordinate(int y, int movement) {
        return this.getSanitizedYCoordinate(y + movement);
    }

    public int getSanitizedXCoordinate(int x) {
        if (x >= this.getWidth() || x < 0) {
            x = MathHelper.Mod(x, this.getWidth());
        }

        return x;
    }

    public int getSanitizedYCoordinate(int y) {
        if (y >= this.getHeight() || y < 0) {
            y = MathHelper.Mod(y, this.getHeight());
        }

        return y;
    }

    public Tile getTile(int x, int y) {
        return this.tiles[this.getSanitizedXCoordinate(x), this.getSanitizedYCoordinate(y)];
    }

    public Tile getTileByIndex(TileIndex tileIndex) {
        return this.getTile(tileIndex.x, tileIndex.y);
    }

    public void setTile(Tile tile) {
        TileIndex index = tile.getTileIndex();
        this.tiles[index.x, index.y] = tile;
    }

    public virtual TileIndex getTileIndexAbove(TileIndex tileIndex) {
        if (tileIndex.y == this.height - 1) {
            return new TileIndex(MathHelper.Mod(tileIndex.x + (this.width / 2), this.width), tileIndex.y);
        }

        return new TileIndex(tileIndex.x, tileIndex.y + 1);
    }

    public virtual TileIndex getTileIndexBelow(TileIndex tileIndex) {
        if (tileIndex.y == 0) {
            return new TileIndex(MathHelper.Mod(tileIndex.x + (this.width / 2), this.width), tileIndex.y);
        }

        return new TileIndex(tileIndex.x, tileIndex.y - 1);
    }

    public virtual TileIndex getTileIndexOnLeft(TileIndex tileIndex) {
        return new TileIndex(tileIndex.x - 1, tileIndex.y);
    }

    public virtual TileIndex getTileIndexOnRight(TileIndex tileIndex) {
        return new TileIndex(tileIndex.x + 1, tileIndex.y);
    }

    public virtual TileIndex getTileIndexInDirection(TileIndex tileIndex, Direction direction) {
        switch (direction) {
            case Direction.Left:
                return this.getTileIndexOnLeft(tileIndex);

            case Direction.Above:
                return this.getTileIndexAbove(tileIndex);

            case Direction.Right:
                return this.getTileIndexOnRight(tileIndex);

            case Direction.Below:
            default:
                return this.getTileIndexBelow(tileIndex);
        }
    }

    public abstract bool checkPropertyEnabled(String type);

    public abstract PropertyDecorator getProperty(String type);

    public abstract bool checkFeatureEnabled(String type);

    internal void preallocateTiles() {
        this.tiles = new Tile[this.width, this.height];
    }
}
