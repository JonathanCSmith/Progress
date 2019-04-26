using UnityEngine;
using System.Collections.Generic;
using System;

public enum HeightClassification {
    DeepWater = 1,
    ShallowWater = 2,
    Shore = 3,
    Sand = 4,
    Grass = 5,
    Forest = 6,
    Rock = 7,
    Snow = 8,
    River = 9
}

public class Tile {

    // TODO: Clean up these props
    public static readonly int NUMBER_OF_DIRECTIONS = 4;
    public static readonly int LEFT_INDEX = (int)Direction.Left;
    public static readonly int ABOVE_INDEX = (int)Direction.Above;
    public static readonly int RIGHT_INDEX = (int)Direction.Right;
    public static readonly int BELOW_INDEX = (int)Direction.Below;

    public readonly Dictionary<string, int> dataBitmasks = new Dictionary<string, int>();
    public readonly Dictionary<string, DataBucket> tileData = new Dictionary<string, DataBucket>();
    public readonly Tile[] ordinals = new Tile[4];
    public readonly TileIndex index;

    public HeightClassification heightClassification;
    public HeatType HeatType;
    public MoistureType MoistureType;
    public BiomeType BiomeType;

    public float Cloud1Value { get; set; }
    public float Cloud2Value { get; set; }
    public float heightValue { get; set; }
    public float HeatValue { get; set; }
    public float MoistureValue { get; set; }
    public int Bitmask;
    public int BiomeBitmask;

    public bool collisionState;
    public bool FloodFilled;

    public Color Color = Color.black;

    public Tile(int x, int y) {
        for (int i = 0; i < 4; i++) {
            this.ordinals[i] = null;
        }

        this.index = new TileIndex(x, y);
    }

    public Tile getLeft() {
        return this.ordinals[Tile.LEFT_INDEX];
    }

    public void setLeft(Tile tile) {
        this.ordinals[Tile.LEFT_INDEX] = tile;
    }

    public Tile getAbove() {
        return this.ordinals[Tile.ABOVE_INDEX];
    }

    public void setAbove(Tile tile) {
        this.ordinals[Tile.ABOVE_INDEX] = tile;
    }

    public Tile getRight() {
        return this.ordinals[Tile.RIGHT_INDEX];
    }

    public void setRight(Tile tile) {
        this.ordinals[Tile.RIGHT_INDEX] = tile;
    }

    public Tile getBelow() {
        return this.ordinals[Tile.BELOW_INDEX];
    }

    public void setBelow(Tile tile) {
        this.ordinals[Tile.BELOW_INDEX] = tile;
    }

    public Tile getTileByDirection(Direction direction) {
        switch (direction) {
            case Direction.Left:
                return this.getLeft();

            case Direction.Above:
                return this.getAbove();

            case Direction.Right:
                return this.getRight();

            case Direction.Below:
            default:
                return this.getBelow();
        }
    }

    public Tile[] getOrdinals() {
        return this.ordinals;
    }

    public TileIndex getTileIndex() {
        return this.index;
    }

    public DataBucket getData(string key) {
        if (!this.tileData.ContainsKey(key)) {
            return null;
        }

        return this.tileData[key];
    }

    public void setData(string key, DataBucket bucket) {
        this.tileData.Add(key, bucket);
    }

    public int getDataBitmask(string key) {
        if (!this.dataBitmasks.ContainsKey(key)) {
            return -1;
        }

        return this.dataBitmasks[key];
    }

    public void setDataBitmask(string key, int value) {
        this.dataBitmasks.Add(key, value);
    }
}
