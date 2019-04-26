using System.Collections.Generic;
using UnityEngine;

public class LakeFeatureGenerator : PlanetFeatureGenerator {

    public static readonly string NAME = "lakes";

    protected List<TileGroup> Waters = new List<TileGroup>();
    protected List<TileGroup> Lands = new List<TileGroup>();

    public LakeFeatureGenerator() : base(LakeFeatureGenerator.NAME) { }

    public override void generate(Generable generable) {
        this.floodFill(generable);
    }

    private void floodFill(Generable generable) {
        // Use a stack instead of recursion
        Stack<Tile> stack = new Stack<Tile>();

        for (int x = 0; x < generable.getWidth(); x++) {
            for (int y = 0; y < generable.getHeight(); y++) {
                Tile t = generable.getTile(x, y);

                //Tile already flood filled, skip
                if (t.FloodFilled) continue;

                // Land
                if (t.collisionState) {
                    TileGroup group = new TileGroup();
                    group.Type = TileGroupType.Land;
                    stack.Push(t);

                    while (stack.Count > 0) {
                        floodFill(stack.Pop(), ref group, ref stack);
                    }

                    if (group.Tiles.Count > 0)
                        Lands.Add(group);
                }
                // Water
                else {
                    TileGroup group = new TileGroup();
                    group.Type = TileGroupType.Water;
                    stack.Push(t);

                    while (stack.Count > 0) {
                        floodFill(stack.Pop(), ref group, ref stack);
                    }

                    if (group.Tiles.Count > 0)
                        Waters.Add(group);
                }
            }
        }
    }

    private void floodFill(Tile tile, ref TileGroup tiles, ref Stack<Tile> stack) {
        // Validate
        if (tile == null)
            return;
        if (tile.FloodFilled)
            return;
        if (tiles.Type == TileGroupType.Land && !tile.collisionState)
            return;
        if (tiles.Type == TileGroupType.Water && tile.collisionState)
            return;

        // Add to TileGroup
        tiles.Tiles.Add(tile);
        tile.FloodFilled = true;

        // floodfill into neighbors
        Tile t = getTileAbove(tile);
        if (t != null && !t.FloodFilled && tile.collisionState == t.collisionState)
            stack.Push(t);
        t = getTileAbove(tile);
        if (t != null && !t.FloodFilled && tile.collisionState == t.collisionState)
            stack.Push(t);
        t = getTileOnLeft(tile);
        if (t != null && !t.FloodFilled && tile.collisionState == t.collisionState)
            stack.Push(t);
        t = getTileOnRight(tile);
        if (t != null && !t.FloodFilled && tile.collisionState == t.collisionState)
            stack.Push(t);
    }
}