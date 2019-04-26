using System.Collections.Generic;
using UnityEngine;

public abstract class Generator {

    public readonly Generable generable;

    protected Tile[,] tiles;

    // Constructor with seed if none provided

    protected Generator(Generable generable) {
        this.generable = generable;

        //HeightMapRenderer = this.generable.transform.Find("HeightTexture").GetComponent<MeshRenderer>();
        //HeatMapRenderer = this.generable.transform.Find("HeatTexture").GetComponent<MeshRenderer>();
        //MoistureMapRenderer = this.generable.transform.Find("MoistureTexture").GetComponent<MeshRenderer>();
        //BiomeMapRenderer = this.generable.transform.Find("BiomeTexture").GetComponent<MeshRenderer>();
    }

    public virtual void generate() {
        this.generateNoise();
        this.generateSurfaceData();
        this.generateTiles();
        this.updateNeighbours();

        // Paint our maps
        this.fillTiles();

        // Features
        this.generateFeatures();

        UpdateBitmasks();
        FloodFill();

        GenerateBiomeMap();
        UpdateBiomeBitmask();

        realHeightMapData = TextureGenerator.GetRealHeightMapData(width, height, tiles);
        heightMapData = TextureGenerator.GetHeightMapTexture(width, height, tiles);
        heatMapData = TextureGenerator.GetHeatMapTexture(width, height, tiles);
        moistureMapData = TextureGenerator.GetMoistureMapTexture(width, height, tiles);
        biomeMapData = TextureGenerator.GetBiomeMapTexture(width, height, tiles, ColdestValue, ColderValue, ColdValue);

        HeightMapRenderer.materials[0].mainTexture = heightMapData;
        HeatMapRenderer.materials[0].mainTexture = heatMapData;
        MoistureMapRenderer.materials[0].mainTexture = moistureMapData;
        BiomeMapRenderer.materials[0].mainTexture = biomeMapData;
    }

    protected abstract void generateNoise();

    protected abstract void generateSurfaceData();

    // Build a Tile array from our data
    private void generateTiles() {
        this.tiles = new Tile[this.generable.getWidth(), this.generable.getHeight()];

        for (var x = 0; x < this.generable.getWidth(); x++) {
            for (var y = 0; y < this.generable.getHeight(); y++) {
                Tile t = new Tile();
                t.x = x;
                t.y = y;

                // Assess our tile properties in the context of the current data
                t = this.fillTileData(t, x, y);
                tiles[x, y] = t;
            }
        }
    }

    private void updateNeighbours() {
        for (var x = 0; x < this.generable.getWidth(); x++) {
            for (var y = 0; y < this.generable.getHeight(); y++) {
                Tile t = this.tiles[x, y];
                t.setLeft(TileHelper.getTileOnLeft(t, this.generable));
                t.setAbove(TileHelper.getTileAbove(t, this.generable));
                t.setRight(TileHelper.getTileOnRight(t, this.generable));
                t.setBelow(TileHelper.getTileBelow(t, this.generable));
            }
        }
    }

    // Fill the tiles with data
    private void fillTiles() {
        for (var x = 0; x < this.generable.getWidth(); x++) {
            for (var y = 0; y < this.generable.getHeight(); y++) {
                Tile t = this.tiles[x, y];

                // Assess our tile properties in the context of the current data
                t = this.fillTileData(t, x, y);
            }
        }
    }

    protected abstract Tile fillTileData(Tile t, int x, int y);

    protected abstract void generateFeatures();










    protected Texture2D realHeightMapData;
    protected Texture2D heightMapData;
    protected Texture2D heatMapData;
    protected Texture2D moistureMapData;
    protected Texture2D biomeMapData;

	protected List<TileGroup> Waters = new List<TileGroup> ();
	protected List<TileGroup> Lands = new List<TileGroup> ();
	
	protected List<RiverGroup> RiverGroups = new List<RiverGroup>();
		
	// Our texture output gameobject
	protected MeshRenderer HeightMapRenderer;
	protected MeshRenderer HeatMapRenderer;
	protected MeshRenderer MoistureMapRenderer;
	protected MeshRenderer BiomeMapRenderer;





    void Update() {
        // Refresh with inspector values
		if (Input.GetKeyDown (KeyCode.F5)) {
            seed = UnityEngine.Random.Range(0, int.MaxValue);
            generate();
		}
	}

	private void UpdateBiomeBitmask() {
		for (var x = 0; x < width; x++) {
			for (var y = 0; y < height; y++) {
				tiles [x, y].UpdateBiomeBitmask();
			}
		}
	}

	public BiomeType GetBiomeType(Tile tile) {
		return BiomeTable [(int)tile.MoistureType, (int)tile.HeatType];
	}
	
	private void GenerateBiomeMap()
	{
		for (var x = 0; x < width; x++) {
			for (var y = 0; y < height; y++) {
				
				if (!tiles[x, y].collisionState) continue;
				
				Tile t = tiles[x,y];
				t.BiomeType = GetBiomeType(t);
			}
		}
	}

	private void AddMoisture(Tile t, int radius)
	{
		int startx = MathHelper.Mod (t.x - radius, width);
		int endx = MathHelper.Mod (t.x + radius, width);
		Vector2 center = new Vector2(t.x, t.y);
		int curr = radius;

		while (curr > 0) {

			int x1 = MathHelper.Mod (t.x - curr, width);
			int x2 = MathHelper.Mod (t.x + curr, width);
			int y = t.y;

			AddMoisture(tiles[x1, y], 0.025f / (center - new Vector2(x1, y)).magnitude);

			for (int i = 0; i < curr; i++)
			{
				AddMoisture (tiles[x1, MathHelper.Mod (y + i + 1, height)], 0.025f / (center - new Vector2(x1, MathHelper.Mod (y + i + 1, height))).magnitude);
				AddMoisture (tiles[x1, MathHelper.Mod (y - (i + 1), height)], 0.025f / (center - new Vector2(x1, MathHelper.Mod (y - (i + 1), height))).magnitude);

				AddMoisture (tiles[x2, MathHelper.Mod (y + i + 1, height)], 0.025f / (center - new Vector2(x2, MathHelper.Mod (y + i + 1, height))).magnitude);
				AddMoisture (tiles[x2, MathHelper.Mod (y - (i + 1), height)], 0.025f / (center - new Vector2(x2, MathHelper.Mod (y - (i + 1), height))).magnitude);
			}
			curr--;
		}
	}

	private void AddMoisture(Tile t, float amount)
	{
		moistureData.Data[t.x, t.y] += amount;
		t.MoistureValue += amount;
		if (t.MoistureValue > 1)
			t.MoistureValue = 1;
				
		//set moisture type
		if (t.MoistureValue < DryerValue) t.MoistureType = MoistureType.Dryest;
		else if (t.MoistureValue < DryValue) t.MoistureType = MoistureType.Dryer;
		else if (t.MoistureValue < WetValue) t.MoistureType = MoistureType.Dry;
		else if (t.MoistureValue < WetterValue) t.MoistureType = MoistureType.Wet;
		else if (t.MoistureValue < WettestValue) t.MoistureType = MoistureType.Wetter;
		else t.MoistureType = MoistureType.Wettest;
	}

	private void AdjustMoistureMap()
	{
		for (var x = 0; x < width; x++) {
			for (var y = 0; y < height; y++) {

				Tile t = tiles[x,y];
				if (t.heightClassification == HeightClassification.River)
				{
					AddMoisture (t, (int)60);
				}
			}
		}
	}

	private void DigRiverGroups()
	{
		for (int i = 0; i < RiverGroups.Count; i++) {

			RiverGroup group = RiverGroups[i];
			River longest = null;

			//Find longest river in this group
			for (int j = 0; j < group.Rivers.Count; j++)
			{
				River river = group.Rivers[j];
				if (longest == null)
					longest = river;
				else if (longest.Tiles.Count < river.Tiles.Count)
					longest = river;
			}

			if (longest != null)
			{				
				//Dig out longest path first
				DigRiver (longest);

				for (int j = 0; j < group.Rivers.Count; j++)
				{
					River river = group.Rivers[j];
					if (river != longest)
					{
						DigRiver (river, longest);
					}
				}
			}
		}
	}

	

	public float GetHeightValue(Tile tile)
	{
		if (tile == null)
			return int.MaxValue;
		else
			return tile.heightValue;
	}

	

	// Dig river based on a parent river vein
	private void DigRiver(River river, River parent)
	{
		int intersectionID = 0;
		int intersectionSize = 0;

		// determine point of intersection
		for (int i = 0; i < river.Tiles.Count; i++) {
			Tile t1 = river.Tiles[i];
			for (int j = 0; j < parent.Tiles.Count; j++) {
				Tile t2 = parent.Tiles[j];
				if (t1 == t2)
				{
					intersectionID = i;
					intersectionSize = t2.RiverSize;
				}
			}
		}

		int counter = 0;
		int intersectionCount = river.Tiles.Count - intersectionID;
		int size = UnityEngine.Random.Range(intersectionSize, 5);
		river.length = river.Tiles.Count;  

		// randomize size change
		int two = river.length / 2;
		int three = two / 2;
		int four = three / 2;
		int five = four / 2;
		
		int twomin = two / 3;
		int threemin = three / 3;
		int fourmin = four / 3;
		int fivemin = five / 3;
		
		// randomize length of each size
		int count1 = UnityEngine.Random.Range (fivemin, five);  
		if (size < 4) {
			count1 = 0;
		}
		int count2 = count1 + UnityEngine.Random.Range(fourmin, four);  
		if (size < 3) {
			count2 = 0;
			count1 = 0;
		}
		int count3 = count2 + UnityEngine.Random.Range(threemin, three); 
		if (size < 2) {
			count3 = 0;
			count2 = 0;
			count1 = 0;
		}
		int count4 = count3 + UnityEngine.Random.Range (twomin, two); 

		// Make sure we are not digging past the river path
		if (count4 > river.length) {
			int extra = count4 - river.length;
			while (extra > 0)
			{
				if (count1 > 0) { count1--; count2--; count3--; count4--; extra--; }
				else if (count2 > 0) { count2--; count3--; count4--; extra--; }
				else if (count3 > 0) { count3--; count4--; extra--; }
				else if (count4 > 0) { count4--; extra--; }
			}
		}
				
		// adjust size of river at intersection point
		if (intersectionSize == 1) {
			count4 = intersectionCount;
			count1 = 0;
			count2 = 0;
			count3 = 0;
		} else if (intersectionSize == 2) {
			count3 = intersectionCount;		
			count1 = 0;
			count2 = 0;
		} else if (intersectionSize == 3) {
			count2 = intersectionCount;
			count1 = 0;
		} else if (intersectionSize == 4) {
			count1 = intersectionCount;
		} else {
			count1 = 0;
			count2 = 0;
			count3 = 0;
			count4 = 0;
		}

		// dig out the river
		for (int i = river.Tiles.Count - 1; i >= 0; i--) {

			Tile t = river.Tiles [i];

			if (counter < count1) {
				t.DigRiver (river, 4);				
			} else if (counter < count2) {
				t.DigRiver (river, 3);				
			} else if (counter < count3) {
				t.DigRiver (river, 2);				
			} 
			else if ( counter < count4) {
				t.DigRiver (river, 1);
			}
			else {
				t.DigRiver (river, 0);
			}			
			counter++;			
		}
	}

	// Dig river
	private void DigRiver(River river)
	{
		int counter = 0;
		
		// How wide are we digging this river?
		int size = UnityEngine.Random.Range(1,5);
		river.length = river.Tiles.Count;  

		// randomize size change
		int two = river.length / 2;
		int three = two / 2;
		int four = three / 2;
		int five = four / 2;
		
		int twomin = two / 3;
		int threemin = three / 3;
		int fourmin = four / 3;
		int fivemin = five / 3;

		// randomize lenght of each size
		int count1 = UnityEngine.Random.Range (fivemin, five);             
		if (size < 4) {
			count1 = 0;
		}
		int count2 = count1 + UnityEngine.Random.Range(fourmin, four); 
		if (size < 3) {
			count2 = 0;
			count1 = 0;
		}
		int count3 = count2 + UnityEngine.Random.Range(threemin, three); 
		if (size < 2) {
			count3 = 0;
			count2 = 0;
			count1 = 0;
		}
		int count4 = count3 + UnityEngine.Random.Range (twomin, two);  
		
		// Make sure we are not digging past the river path
		if (count4 > river.length) {
			int extra = count4 - river.length;
			while (extra > 0)
			{
				if (count1 > 0) { count1--; count2--; count3--; count4--; extra--; }
				else if (count2 > 0) { count2--; count3--; count4--; extra--; }
				else if (count3 > 0) { count3--; count4--; extra--; }
				else if (count4 > 0) { count4--; extra--; }
			}
		}

		// Dig it out
		for (int i = river.Tiles.Count - 1; i >= 0 ; i--)
		{
			Tile t = river.Tiles[i];

			if (counter < count1) {
				t.DigRiver (river, 4);				
			}
			else if (counter < count2) {
				t.DigRiver (river, 3);				
			} 
			else if (counter < count3) {
				t.DigRiver (river, 2);				
			} 
			else if ( counter < count4) {
				t.DigRiver (river, 1);
			}
			else {
				t.DigRiver(river, 0);
			}			
			counter++;			
		}
	}

	private void UpdateBitmasks()
	{
		for (var x = 0; x < width; x++) {
			for (var y = 0; y < height; y++) {
				tiles [x, y].UpdateBitmask ();
			}
		}
	}

	private void FloodFill()
	{
		// Use a stack instead of recursion
		Stack<Tile> stack = new Stack<Tile>();
		
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				
				Tile t = tiles[x,y];

				//Tile already flood filled, skip
				if (t.FloodFilled) continue;

				// Land
				if (t.collisionState)   
				{
					TileGroup group = new TileGroup();
					group.Type = TileGroupType.Land;
					stack.Push(t);
					
					while(stack.Count > 0) {
						FloodFill(stack.Pop(), ref group, ref stack);
					}
					
					if (group.Tiles.Count > 0)
						Lands.Add (group);
				}
				// Water
				else {				
					TileGroup group = new TileGroup();
					group.Type = TileGroupType.Water;
					stack.Push(t);
					
					while(stack.Count > 0)	{
						FloodFill(stack.Pop(), ref group, ref stack);
					}
					
					if (group.Tiles.Count > 0)
						Waters.Add (group);
				}
			}
		}
	}

	private void FloodFill(Tile tile, ref TileGroup tiles, ref Stack<Tile> stack)
	{
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
		tiles.Tiles.Add (tile);
		tile.FloodFilled = true;

		// floodfill into neighbors
		Tile t = getTileAbove (tile);
		if (t != null && !t.FloodFilled && tile.collisionState == t.collisionState)
			stack.Push (t);
		t = getTileAbove (tile);
		if (t != null && !t.FloodFilled && tile.collisionState == t.collisionState)
			stack.Push (t);
		t = getTileOnLeft (tile);
		if (t != null && !t.FloodFilled && tile.collisionState == t.collisionState)
			stack.Push (t);
		t = getTileOnRight (tile);
		if (t != null && !t.FloodFilled && tile.collisionState == t.collisionState)
			stack.Push (t);
	}
    
}
