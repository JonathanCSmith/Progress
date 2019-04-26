using System.Collections.Generic;
using UnityEngine;

public class BodiesOf : FeatureDecorator {

    public static readonly string NAME = "bodies";
    public static readonly string[] DEPENDENCIES = new string[] { };

    private readonly int islandThreshold = 100;
    private readonly int lakeThreshold = 100;

    protected List<BodyOf> bodiesOf = new List<BodyOf>();
    private List<TileIndex> filledTiles = new List<TileIndex>();

    public BodiesOf(int islandThreshold, int lakeThreshold) : base(BodiesOf.NAME) {
        this.islandThreshold = islandThreshold;
        this.lakeThreshold = lakeThreshold;
    }

    public BodiesOf() : base(BodiesOf.NAME) { }

    public override string[] getFeatureDependencies() {
        return BodiesOf.DEPENDENCIES;
    }

    public override bool generate(Generable generable) {
        if (!generable.checkPropertyEnabled(Rivers.NAME)) {
            return false;
        }

        this.floodFill(generable);

        foreach (BodyOf body in this.bodiesOf) {
            if (body.getBodyType() == Body.Continent) {
                if (body.getMembers().Count < this.islandThreshold) {
                    body.changeBodyType(Body.Island);
                }
            }

            else {
                if (body.getMembers().Count < this.lakeThreshold) {
                    body.changeBodyType(Body.Lake);
                }
            }

            foreach (TileIndex tileIndex in body.getMembers()) {
                Tile tile = generable.getTileByIndex(tileIndex);
                BodiesOf.addBodyOf(tile, body);
            }
        }

        // Don't need this anymore
        this.filledTiles.Clear();
        return true;
    }

    private void floodFill(Generable generable) {
        // Use a stack instead of recursion
        Stack<Tile> stack = new Stack<Tile>();

        int bodyCounter = 0;
        for (int x = 0; x < generable.getWidth(); x++) {
            for (int y = 0; y < generable.getHeight(); y++) {
                Tile t = generable.getTile(x, y);

                // This has already been added to a group
                if (filledTiles.Contains(t.getTileIndex())) {
                    continue;
                }

                Height height = Height.getHeightForTile(t);

                // Change our body initial value based on colision
                BodyOf bodyOf;
                bool collisionState = height.getTileProperties().collisionState;
                if (collisionState) {
                    bodyOf = new BodyOf(bodyCounter++, Body.Continent);
                }

                else {
                    bodyOf = new BodyOf(bodyCounter++, Body.Ocean);
                }

                // Add this tile to the stack
                stack.Push(t);

                // Process the stack
                while (stack.Count > 0) {
                    this.floodFill(stack.Pop(), ref bodyOf, ref stack, collisionState);
                }

                // Check that we actually found a body - pretty likely o
                if (bodyOf.getMembers().Count > 0) {
                    this.bodiesOf.Add(bodyOf);
                }
            }
        }
    }

    private void floodFill(Tile tile, ref BodyOf tiles, ref Stack<Tile> stack, bool expectedCollisionState) {
        // Add to TileGroup
        TileIndex tileIndex = tile.getTileIndex();
        tiles.addMember(tileIndex);
        this.filledTiles.Add(tileIndex);

        for (int i = 0; i < Tile.NUMBER_OF_DIRECTIONS; i++) {
            Tile t = tile.getTileByDirection((Direction)i);
            if (t != null && !this.filledTiles.Contains(t.getTileIndex())) {
                Height h = Height.getHeightForTile(t);
                if (h.getTileProperties().collisionState == expectedCollisionState) {
                    stack.Push(t);
                }
            }
        }
    }

    public static void addBodyOf(Tile tile, BodyOf body) {
        tile.setData(BodiesOf.NAME, new BodyOfData(body.getId()));
    }

    public static BodyOfData getBodyFromTile(Tile tile) {
        return (BodyOfData)tile.getData(BodiesOf.NAME);
    }
}

public enum Body {
    Ocean,
    Lake,
    Continent,
    Island
}

public class BodyOfData : DataBucket {

    private readonly int bodyOf;

    public BodyOfData(int id) {
        this.bodyOf = id;
    }
}

public class BodyOf {

    private readonly List<TileIndex> members = new List<TileIndex>();

    private readonly int id;
    private Body body;

    public BodyOf(int id, Body body) {
        this.id = id;
        this.body = body;
    }

    public int getId() {
        return this.id;
    }

    public Body getBodyType() {
        return this.body;
    }

    public void changeBodyType(Body body) {
        this.body = body;
    }

    public List<TileIndex> getMembers() {
        return this.members;
    }

    public void addMember(TileIndex tileIndex) {
        if (this.members.Contains(tileIndex)) {
            return;
        }

        this.members.Add(tileIndex);
    }
}