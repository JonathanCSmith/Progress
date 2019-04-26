using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rivers : FeatureDecorator {

    public static readonly string NAME = "rivers";
    public static readonly string[] DEPENDENCIES = new string[] { };

    // Data storage for action of this generator
    // TODO: Assess uses and remove if its irrelevant (i.e. delete after generation)
    private List<River> allRivers = new List<River>();
    private Dictionary<TileIndex, List<River>> riverTiles = new Dictionary<TileIndex, List<River>>();
    private List<RiverTree> riverTrees = new List<RiverTree>();

    protected int riverCount = 40;
    protected float minRiverHeight = 0.6f;
    protected int maxRiverAttempts = 1000;
    protected int minRiverTurns = 18;
    protected int minRiverLength = 20;
    protected int maxRiverIntersections = 2;
    private int maxRiverWidth = 5;
    private double bufferFraction = 0.1d;
    private int heightSampleBuffer = 2;
    private float waterLevelBelowAverage = 0.005f;
    private int riverMoistureRadiusAffect = 60;

    private SurfaceMoisture moistureProperty;

    public Rivers(int riverCount, float minRiverHeight, int maxRiverAttempts, int minRiverTurns, int minRiverLength, int maxRiverIntersections, int maxRiverWidth, double bufferFraction, int heightSampleBuffer, float waterLevelBelowAverage, int riverMoistureRadiusAffect) : base(RiverFeatureGenerator.NAME) {
        this.riverCount = riverCount;
        this.minRiverHeight = minRiverHeight;
        this.maxRiverAttempts = maxRiverAttempts;
        this.minRiverTurns = minRiverTurns;
        this.minRiverLength = minRiverLength;
        this.maxRiverIntersections = maxRiverIntersections;
        this.maxRiverWidth = maxRiverWidth;
        this.bufferFraction = bufferFraction;
        this.heightSampleBuffer = heightSampleBuffer;
        this.waterLevelBelowAverage = waterLevelBelowAverage;
        this.riverMoistureRadiusAffect = riverMoistureRadiusAffect;
    }

    public Rivers() : base(Rivers.NAME) { }

    public override string[] getFeatureDependencies() {
        return Rivers.DEPENDENCIES;
    }

    public List<River> getRiversForTileIndexFromCache(TileIndex tileIndex) {
        if (!this.riverTiles.ContainsKey(tileIndex)) {
            return null;
        }

        return this.riverTiles[tileIndex];
    }

    public void setRiversForTileIndexInCache(TileIndex tileIndex, List<River> rivers) {
        this.riverTiles.Add(tileIndex, rivers);
    }

    public int getRiverNeighborCount(Tile tile, River river) {
        int count = 0;

        Tile[] tiles = tile.getOrdinals();
        for (int i = 0; i < Tile.NUMBER_OF_DIRECTIONS; i++) {
            Tile tileToTest = tiles[i];
            if (tileToTest == null) {
                continue;
            }

            List<River> rivers = this.getRiversForTileIndexFromCache(tileToTest.getTileIndex());
            if (rivers.Count > 0 && rivers.Contains(river)) {
                count++;
            }
        }

        return count;
    }

    public float evaluateTileScore(Tile tile, River river) {
        float score = int.MaxValue;
        if (tile == null) {
            return score;
        }

        Height height = Height.getHeightForTile(tile);
        if (this.canRiverFlowHere(tile, river)) {
            score = height.heightValue;
        }

        if (this.canRiverEndHere(tile, height)) {
            score = 0;
        }

        return score;
    }

    public bool canRiverFlowHere(Tile tile, River river) {
        return this.getRiverNeighborCount(tile, river) < 2 && !river.tileIndices.Contains(tile.getTileIndex());
    }

    public bool canRiverEndHere(Tile tile, Height targetHeight) {
        List<River> targetRivers = this.getRiversForTileIndexFromCache(tile.getTileIndex());
        return targetRivers.Count == 0 && !targetHeight.getTileProperties().collisionState;
    }

    public void findPathToStandingWater(Tile tile, Direction direction, ref River currentRiver) {
        List<River> rivers = this.getRiversForTileIndexFromCache(tile.getTileIndex());
        if (rivers.Contains(currentRiver)) {
            return; // No loops? This shouldn't happen though
        }

        // check if there is already a river on this tile
        if (rivers.Count > 0) {
            currentRiver.intersections++; // TODO: Do we not need to increase the intersections of the other river?
        }

        // Calculate which direction we should not be searching in
        Direction opposingDirection;
        switch (direction) {
            case Direction.Left:
                opposingDirection = Direction.Right;
                break;

            case Direction.Above:
                opposingDirection = Direction.Below;
                break;

            case Direction.Right:
                opposingDirection = Direction.Left;
                break;

            case Direction.Below:
            default:
                opposingDirection = Direction.Above;
                break;
        }

        // Now search for the next place for it to go
        Tile bestTile = null;
        float score = int.MaxValue;
        int ordinalIndex = int.MaxValue;
        for (int i = 0; i < Tile.NUMBER_OF_DIRECTIONS; i++) {
            // Don't search where we came from.
            if (i == (int)opposingDirection) {
                continue;
            }

            // Search the tile
            Tile tileToSearch = tile.ordinals[i];
            float newScore = this.evaluateTileScore(tileToSearch, currentRiver);
            if (bestTile == null) {
                bestTile = tileToSearch;
                score = newScore;
                ordinalIndex = i;
            }

            else if (newScore < score) {
                bestTile = tileToSearch;
                score = newScore;
                ordinalIndex = i;
            }
        }

        // if no minimum found - exit
        if (score == int.MaxValue) {
            return;
        }

        // Check again if the selected tile ends here
        Height heightProp = Height.getHeightForTile(bestTile);
        if (this.canRiverEndHere(bestTile, heightProp)) {
            return;
        }

        if (currentRiver.currentDirection != (Direction)ordinalIndex) {
            currentRiver.turnCount++;
            currentRiver.currentDirection = (Direction)ordinalIndex;
        }

        // Add the current tile to our river and recurse
        currentRiver.addTile(this, tile.getTileIndex(), (Direction)ordinalIndex);
        this.findPathToStandingWater(bestTile, (Direction)ordinalIndex, ref currentRiver);
    }

    public override bool generate(Generable generable) {
        if (!generable.checkPropertyEnabled(SurfaceHeight.NAME) || !generable.checkPropertyEnabled(SurfaceMoisture.NAME)) {
            return false;
        }
        // TODO: THis can be shifted out now
        this.moistureProperty = (SurfaceMoisture) generable.getProperty(SurfaceMoisture.NAME);

        this.generateRiverNetwork(generable);
        return true;
    }

    private void generateRiverNetwork(Generable generable) {
        this.generateRivers(generable);

        // TODO: Meander?

        this.calculateAssociatedRivers(generable);
        this.digRiverTrees(generable);
        this.adjustMoistureMap(generable);
    }

    private void generateRivers(Generable generable) {
        int attempts = 0;
        int rivercount = this.riverCount;

        // Generate some rivers
        while (rivercount > 0 && attempts < this.maxRiverAttempts) {

            // Get a random tile
            int x = UnityEngine.Random.Range(0, generable.getWidth());
            int y = UnityEngine.Random.Range(0, generable.getHeight());
            Tile tile = generable.getTile(x, y);
            Height heightProp = Height.getHeightForTile(tile);

            // validate the tile
            if (!heightProp.getTileProperties().collisionState) {
                continue;
            }

            List<River> rivers = this.getRiversForTileIndexFromCache(tile.getTileIndex());
            if (rivers == null) {
                rivers = new List<River>();
            }

            // Skip existing rivers as sources for new rivers
            if (rivers.Count > 0) {
                continue;
            }

            // Is this a valid tile for spawning a source
            if (heightProp.heightValue < this.minRiverHeight) {
                continue;
            }

            // Tile is good to start river from
            River river = new River(rivercount);

            // Figure out the direction this river will try to flow
            // TODO: Rather than doing this manually - we could add a behaviour in findPathToStandingWater when direction is null
            river.currentDirection = Height.getLowestNeighbour(tile);

            // Recursively find a path to water
            this.findPathToStandingWater(tile, river.currentDirection, ref river);

            // Validate the generated river 
            if (river.turnCount < this.minRiverTurns || river.tileIndices.Count < this.minRiverLength || river.intersections > this.maxRiverIntersections) {
                //Validation failed - remove this river
                for (int i = 0; i < river.tileIndices.Count; i++) {
                    List<River> currentRivers = this.getRiversForTileIndexFromCache(river.tileIndices[i]);
                    currentRivers.Remove(river);
                }
            }

            else if (river.tileIndices.Count >= this.minRiverLength) {
                //Validation passed - Add river to list
                this.allRivers.Add(river);
                rivers.Add(river);
                rivercount--;
            }

            // Don't store empty data
            if (rivers.Count > 0) {
                this.setRiversForTileIndexInCache(tile.getTileIndex(), rivers);
            }

            attempts++;
        }
    }

    private void calculateAssociatedRivers(Generable generable) {
        // Loop through every river - gather tiles and build river groups
        List<River> exploredRivers = new List<River>();
        List<List<River>> linkedRivers = new List<List<River>>();
        foreach (River river in this.allRivers) {

            // Don't go over the same ground
            if (exploredRivers.Contains(river)) {
                continue;
            }

            // Add this river to our rivers that we need to follow
            Queue<River> riversToFollow = new Queue<River>();
            riversToFollow.Enqueue(river);

            // Initialise a list of associated rivers
            List<River> associatedRivers = new List<River>();

            // While there are still rivers that we are following keep going
            River currentRiver;
            while (riversToFollow.Count > 0) {

                // River currently being followed - dont tread old ground
                currentRiver = riversToFollow.Dequeue();
                if (exploredRivers.Contains(currentRiver)) {
                    continue;
                }

                // Go through each tile in this river and search for new rivers
                foreach (TileIndex tileIndex in currentRiver.tileIndices) {
                    List<River> group = this.getRiversForTileIndexFromCache(tileIndex);
                    foreach (River associate in group) {

                        // Aggregate associated rivers
                        if (associate != river && !associatedRivers.Contains(associate)) {
                            associatedRivers.Add(associate);
                            riversToFollow.Enqueue(associate);
                        }
                    }
                }
            }

            // Create our river associations
            linkedRivers.Add(associatedRivers);
        }

        // Add this data to our cache
        this.riverTrees.Clear();
        int currentCount = this.riverTrees.Count;
        foreach (List<River> rivers in linkedRivers) {
            RiverTree tree = new RiverTree();
            tree.setRivers(rivers);
            tree.calculateBranchPoints();
            this.riverTrees.Add(tree);
        }
    }

    private void digRiverTrees(Generable generable) {
        // Get the rivers
        foreach (RiverTree river in this.riverTrees) {
            this.digRiverTree(generable, river, 0, this.maxRiverWidth);
        }
    }

    private void digRiverTree(Generable generable, RiverTree riverTree, int startingIndex, int maxWidth) {
        River trunk = riverTree.getTrunk();
        this.digRiver(generable, riverTree, trunk, startingIndex, maxWidth);

        foreach (KeyValuePair<int, RiverTree> entry in riverTree.getBranches()) {
            this.digRiverTree(generable, entry.Value, entry.Key, trunk.getRadiusAt(entry.Key));
        }
    }

    private void digRiver(Generable generable, RiverTree tree, River currentRiver, int startingIndex, int maxWidth) {
        int length = currentRiver.tileIndices.Count;
        int maxLength = length - startingIndex;
        int tributaryCount = tree.getNumberOfTributaries();

        // Random cross section radius
        int radius = UnityEngine.Random.Range(1, maxWidth);

        // Real max size (i.e. max river size is affected by its tributary count)
        radius = radius + tributaryCount;
        currentRiver.riverCrossSectionSize = radius;

        // Buffer calc
        int bufferSize = (int)Math.Round(maxLength * this.bufferFraction);

        // Calculate size change points
        List<int> stepChangeIndices = new List<int>();
        if (radius != 1) {
            int currentIndex = startingIndex;
            for (int i = 1; i < radius; i++) {
                int mindex = currentIndex + bufferSize - 1; // Minus one for zero indexing
                int maxdex = (length - 1) - (bufferSize * (radius - i)); // Minus one for indexing
                currentIndex = UnityEngine.Random.Range(mindex, maxdex);
                stepChangeIndices.Add(currentIndex);
            }
        }

        // TODO Add step changes due to tributaries

        currentRiver.setStepChanges(stepChangeIndices);

        // Start walking backwards on the trunk
        int currentRadius = radius;
        Direction previousDirection = Direction.Left;
        Direction currentDirection;
        for (int i = startingIndex; i < length; i++) {
            // Calculate our relative directions
            currentDirection = currentRiver.getDirectionAt(i);
            if (i == startingIndex) {
                previousDirection = currentDirection;
            }

            // Dig the tile
            this.digTile(generable, currentRiver, i, previousDirection, currentDirection);
            previousDirection = currentDirection;
        }
    }

    private void digTile(Generable generable, River river, int index, Direction previous, Direction current) {
        int targetRadius = river.getRadiusAt(index);
        Direction direction = river.getDirectionAt(index);

        // Generate a cross section that we will sample for our height calculations - note this is not a corner sample 
        // TODO: Assess if this is appropriate when there may be interfering river tiles
        Dictionary<TileIndex, float> sampleSpace = this.generateCrossSectionSamplePattern(generable, river.tileIndices[index], targetRadius + this.heightSampleBuffer, direction);

        // Average the heights - ignore the modifiers in this pattern as it will not be our true trench profile
        float heights = 0f;
        foreach (TileIndex tileIndex in sampleSpace.Keys) {
            Tile tile = generable.getTileByIndex(tileIndex);
            Height height = Height.getHeightForTile(tile);
            heights += height.heightValue;
        }
        float avergeHeight = heights / sampleSpace.Count;

        // Calculate the min max of the water
        float waterLevel = avergeHeight - this.waterLevelBelowAverage;
        float allowedDepth = targetRadius / 100f;
        float lowHeight = waterLevel - allowedDepth;

        // Assign the tiles
        Dictionary<TileIndex, float> tilesToAssign;
        if (previous == current) {
            tilesToAssign = this.generateCrossSectionSamplePattern(generable, river.tileIndices[index], targetRadius, direction);
        }

        else {
            // Top Left Quadrant
            if ((previous == Direction.Left && current == Direction.Below) || (previous == Direction.Above && current == Direction.Right)) {
                tilesToAssign = this.generateCrossSectionSamplePatternAtCorner(generable, river.tileIndices[index], targetRadius, -targetRadius, 0, 0, targetRadius);
            }

            // Top Right Quadrant
            else if ((previous == Direction.Right && current == Direction.Below) || (previous == Direction.Above && current == Direction.Left)) {
                tilesToAssign = this.generateCrossSectionSamplePatternAtCorner(generable, river.tileIndices[index], targetRadius, 0, targetRadius, 0, targetRadius);

            }

            // Bottom Right Quadrant
            else if ((previous == Direction.Below && current == Direction.Left) || (previous == Direction.Right && current == Direction.Above)) {
                tilesToAssign = this.generateCrossSectionSamplePatternAtCorner(generable, river.tileIndices[index], targetRadius, 0, targetRadius, -targetRadius, 0);

            }

            // Bottom Left Quadrant
            else if ((previous == Direction.Below && current == Direction.Right) || (previous == Direction.Left && current == Direction.Above)) {
                tilesToAssign = this.generateCrossSectionSamplePatternAtCorner(generable, river.tileIndices[index], targetRadius, targetRadius, 0, targetRadius, 0);

            }

            else {
                return; // LOLWUT
            }
        }

        // Dig the tile pattern
        foreach (KeyValuePair<TileIndex, float> entry in tilesToAssign) {
            // Dig it!
            Tile tile = generable.getTileByIndex(entry.Key);
            this.digTileForRiver(tile, river, lowHeight * entry.Value, waterLevel, direction);
        }

        // special case zero needs to all run to water
        if (index == 0) {
            foreach (KeyValuePair<TileIndex, float> entry in tilesToAssign) {
                bool isWater = false;
                while (!isWater) {
                    TileIndex furtherTile = generable.getTileIndexInDirection(entry.Key, direction);

                    // Check if its water
                    Tile tile = generable.getTileByIndex(furtherTile);
                    Height height = Height.getHeightForTile(tile);
                    if (this.canRiverEndHere(tile, height)) {
                        isWater = true;
                        continue;
                    }

                    // If not dig it!
                    this.digTileForRiver(tile, river, lowHeight * entry.Value, waterLevel, direction);
                }
            }
        }
    }

    private Dictionary<TileIndex, float> generateCrossSectionSamplePattern(Generable generable, TileIndex tileIndex, int radius, Direction direction) {
        Dictionary<TileIndex, float> pattern = new Dictionary<TileIndex, float>();

        // Build steps
        float bufferSize = 0.05f; // TODO
        float depthFraction = 1f / radius;
        float depthModifier = UnityEngine.Random.Range(1 - bufferSize, bufferSize);
        pattern.Add(tileIndex, depthModifier);
        TileIndex tmpIndex;
        switch (direction) {
            case Direction.Left:
            case Direction.Right:
                tmpIndex = tileIndex;
                for (int i = 1; i < radius; i++) {
                    tmpIndex = generable.getTileIndexAbove(tmpIndex);
                    depthModifier = 1 - (i * depthFraction);
                    depthModifier = UnityEngine.Random.Range(depthModifier - bufferSize, depthModifier + bufferSize);
                    pattern.Add(tmpIndex, depthFraction);
                }

                tmpIndex = tileIndex;
                for (int i = 1; i < radius; i++) {
                    tmpIndex = generable.getTileIndexBelow(tileIndex);
                    depthModifier = 1 - (i * depthFraction);
                    depthModifier = UnityEngine.Random.Range(depthModifier - bufferSize, depthModifier + bufferSize);
                    pattern.Add(tmpIndex, depthFraction);
                }
                break;

            case Direction.Above:
            case Direction.Below:
                tmpIndex = tileIndex;
                for (int i = 1; i < radius; i++) {
                    tmpIndex = generable.getTileIndexOnLeft(tmpIndex);
                    depthModifier = 1 - (i * depthFraction);
                    depthModifier = UnityEngine.Random.Range(depthModifier - bufferSize, depthModifier + bufferSize);
                    pattern.Add(tmpIndex, depthFraction);
                }

                tmpIndex = tileIndex;
                for (int i = 1; i < radius; i++) {
                    tmpIndex = generable.getTileIndexOnRight(tileIndex);
                    depthModifier = 1 - (i * depthFraction);
                    depthModifier = UnityEngine.Random.Range(depthModifier - bufferSize, depthModifier + bufferSize);
                    pattern.Add(tmpIndex, depthFraction);
                }
                break;
        }

        return pattern;
    }

    private Dictionary<TileIndex, float> generateCrossSectionSamplePatternAtCorner(Generable generable, TileIndex tileIndex, int radius, int fromX, int toX, int fromY, int toY) {
        Dictionary<TileIndex, float> pattern = new Dictionary<TileIndex, float>();

        // Build steps
        float bufferSize = 0.05f; // TODO
        float depthFraction = 1f / radius;
        float depthModifier = UnityEngine.Random.Range(1 - bufferSize, bufferSize);
        pattern.Add(tileIndex, depthModifier);

        int squareRadius = radius * radius;
        for (int x = fromX; x <= toX; x++) {
            for (int y = fromY; y <= toY; y++) {
                if (x == 0 && y == 0) {
                    continue;
                }

                int distance = (x * x) + (y * y);
                if (distance <= squareRadius) {
                    depthModifier = 1 - (distance * depthFraction);
                    depthModifier = UnityEngine.Random.Range(depthModifier - bufferSize, depthModifier + bufferSize);
                    pattern.Add(new TileIndex(x, y), depthModifier);
                }
            }
        }

        return pattern;
    }

    private void digTileForRiver(Tile tile, River river, float lowHeight, float waterLevel, Direction direction) {
        RiverData riverData = new RiverData(river.id, waterLevel, lowHeight, direction);
        Rivers.setRiverData(tile, riverData);
    }

    private void adjustMoistureMap(Generable generable) {
        foreach (TileIndex tile in this.riverTiles.Keys) {
            this.adjustMoistureForTile(generable, tile, this.riverMoistureRadiusAffect);
        }
    }

    private void adjustMoistureForTile(Generable generable, TileIndex index, int radius) {
        int startX = generable.getRelativeSanitizedXCoordinate(index.x, -radius);
        int endX = generable.getRelativeSanitizedXCoordinate(index.x, radius);

        Vector2 center = new Vector2(index.x, index.y);
        int curr = radius;
        while (curr > 0) {
            int x1 = generable.getRelativeSanitizedXCoordinate(index.x, -curr);
            int x2 = generable.getRelativeSanitizedXCoordinate(index.x, curr);
            int y = index.y;

            this.moistureProperty.addMoistureToTile(generable.getTile(x1, y), 0.025f / (center - new Vector2(x1, y)).magnitude);

            for (int i = 0; i < curr; i++) {
                int tempYPlusIndexAndOne = generable.getRelativeSanitizedYCoordinate(y, i + 1);
                int tempYMinusIndexAndOne = generable.getRelativeSanitizedYCoordinate(y, -(i + 1));
                this.moistureProperty.addMoistureToTile(generable.getTile(x1, tempYPlusIndexAndOne), 0.025f / (center - new Vector2(x1, tempYPlusIndexAndOne)).magnitude);
                this.moistureProperty.addMoistureToTile(generable.getTile(x1, tempYMinusIndexAndOne), 0.025f / (center - new Vector2(x1, tempYMinusIndexAndOne)).magnitude);

                this.moistureProperty.addMoistureToTile(generable.getTile(x2, tempYPlusIndexAndOne), 0.025f / (center - new Vector2(x2, tempYPlusIndexAndOne)).magnitude);
                this.moistureProperty.addMoistureToTile(generable.getTile(x2, tempYMinusIndexAndOne), 0.025f / (center - new Vector2(x2, tempYMinusIndexAndOne)).magnitude);
            }
            curr--;
        }
    }

    public static RiverData getRiverData(Tile tile) {
        return (RiverData)tile.getData(Rivers.NAME);
    }

    public static void setRiverData(Tile tile, RiverData riverData) {
        tile.setData(Rivers.NAME, riverData);
    }
}

public class RiverData : DataBucket {

    public readonly int treeId;
    public readonly float waterHeight;
    public readonly float floorHeight;
    public Direction flowDirection;

    public RiverData(int riverId, float waterHeight, float floorHeight, Direction flowDirection) {
        this.treeId = riverId;
        this.waterHeight = waterHeight;
        this.floorHeight = floorHeight;
        this.flowDirection = flowDirection;
    }
}

public class River {

    public readonly List<River> linkedRivers = new List<River>();

    public readonly int id;
    public readonly List<TileIndex> tileIndices;
    public readonly List<Direction> directions;

    public int length;
    public int intersections;
    public float turnCount;
    public Direction currentDirection;
    public int riverCrossSectionSize;
    public List<int> stepChanges;

    public River(int id) {
        this.id = id;
        this.tileIndices = new List<TileIndex>();
        this.directions = new List<Direction>();
    }

    public void addTile(Rivers riverFeatureGenerator, TileIndex tileIndex, Direction direction) {
        this.tileIndices.Add(tileIndex);
        this.directions.Add(direction);

        List<River> riverGroup = riverFeatureGenerator.getRiversForTileIndexFromCache(tileIndex);
        if (!riverGroup.Contains(this)) {
            riverGroup.Add(this);
        }
        riverFeatureGenerator.setRiversForTileIndexInCache(tileIndex, riverGroup);
    }

    public void setStepChanges(List<int> stepChanges) {
        this.stepChanges = stepChanges;
    }

    public int getRadiusAt(int index) {
        if (this.stepChanges.Count == 0) {
            return 0;
        }

        int currentStepChange = 0;
        foreach (int stepChange in this.stepChanges) {
            if (index >= stepChange && stepChange > currentStepChange) {
                currentStepChange = stepChange;
            }
        }

        return currentStepChange;
    }

    internal Direction getDirectionAt(int index) {
        return this.directions[index];
    }
}

public class RiverTree {
    // All associated rivers... 
    public List<River> rivers = new List<River>();

    // Trunk
    public River mainRiver;

    // Location of branches
    public Dictionary<int, RiverTree> branches = new Dictionary<int, RiverTree>();

    private int parentId = -1;
    private int id;

    public River getTrunk() {
        return this.mainRiver;
    }

    private void setTrunk(River river) {
        this.mainRiver = river;
        this.id = river.id;
    }

    public void addRiver(River river) {
        this.rivers.Add(river);
    }

    public void setRivers(List<River> rivers) {
        this.rivers = rivers;
    }

    public int getNumberOfTributaries() {
        return branches.Count;
    }

    public void calculateBranchPoints() {
        // Longest river identificaton
        River longest = null;
        foreach (River river in this.rivers) {
            if (longest == null) {
                longest = river;
                continue;
            }

            if (river.tileIndices.Count > longest.tileIndices.Count) {
                longest = river;
            }
        }

        this.setTrunk(longest);
        foreach (River river in this.rivers) {
            if (river == longest) {
                continue;
            }

            // Walk backwards
            for (int j = 0; j < river.tileIndices.Count; j++) {
                if (river.tileIndices[j] == longest.tileIndices[j]) {
                    continue;
                }

                this.addBranch(j, river);
                break;
            }
        }

        foreach (RiverTree branch in this.branches.Values) {
            branch.setParentTree(this.id);
            branch.calculateBranchPoints();
        }
    }

    public Dictionary<int, RiverTree> getBranches() {
        return this.branches;
    }

    private void addBranch(int branchPoint, River river) {
        if (this.branches.ContainsKey(branchPoint)) {
            RiverTree riverTree = this.branches[branchPoint];
            riverTree.addRiver(river);
        }

        else {
            RiverTree riverTree = new RiverTree();
            riverTree.addRiver(river);
            this.branches.Add(branchPoint, riverTree);
        }
    }

    private void setParentTree(int newParentId) {
        this.parentId = newParentId;
    }
}
