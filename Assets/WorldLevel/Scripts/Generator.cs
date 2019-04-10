using System.Collections.Generic;
using UnityEngine;

public abstract class Generator {

    protected Generable generable;
    protected int seed;
    protected int width = 512;
    protected int height = 512;

    // Height map properties
    protected int terrainOctaves = 6;
    protected double terrainFrequency = 1.25;
    protected float DeepWater = 0.2f;
    protected float ShallowWater = 0.4f;
    protected float Sand = 0.5f;
    protected float Grass = 0.7f;
    protected float Forest = 0.8f;
    protected float Rock = 0.9f;

    // Heat properties
    protected int heatOctaves = 4;
    protected double heatFrequency = 3.0;
    protected float ColdestValue = 0.05f;
    protected float ColderValue = 0.18f;
    protected float ColdValue = 0.4f;
    protected float WarmValue = 0.6f;
    protected float WarmerValue = 0.8f;

    // Moisture map
    protected int moistureOctaves = 4;
    protected double moistureFrequency = 3.0;
    protected float DryerValue = 0.27f;
    protected float DryValue = 0.4f;
    protected float WetValue = 0.6f;
    protected float WetterValue = 0.8f;
    protected float WettestValue = 0.9f;

    // Moisture map
    protected int RiverCount = 40;
    protected float MinRiverHeight = 0.6f;
    protected int MaxRiverAttempts = 1000;
    protected int MinRiverTurns = 18;
    protected int MinRiverLength = 20;
    protected int MaxRiverIntersections = 2;

    public Generator(Generable generable) {
        this.generable = generable;

        seed = UnityEngine.Random.Range(0, int.MaxValue);

        HeightMapRenderer = this.generable.transform.Find("HeightTexture").GetComponent<MeshRenderer>();
        HeatMapRenderer = this.generable.transform.Find("HeatTexture").GetComponent<MeshRenderer>();
        MoistureMapRenderer = this.generable.transform.Find("MoistureTexture").GetComponent<MeshRenderer>();
        BiomeMapRenderer = this.generable.transform.Find("BiomeTexture").GetComponent<MeshRenderer>();
    }

	protected MapData heightData;
	protected MapData heatData;
	protected MapData moistureData;
	protected MapData Clouds1;
    protected MapData Clouds2;

    protected Texture2D realHeightMapData;
    protected Texture2D heightMapData;
    protected Texture2D heatMapData;
    protected Texture2D moistureMapData;
    protected Texture2D biomeMapData;

    protected Tile[,] tiles;

	protected List<TileGroup> Waters = new List<TileGroup> ();
	protected List<TileGroup> Lands = new List<TileGroup> ();

	protected List<River> Rivers = new List<River>();	
	protected List<RiverGroup> RiverGroups = new List<RiverGroup>();
		
	// Our texture output gameobject
	protected MeshRenderer HeightMapRenderer;
	protected MeshRenderer HeatMapRenderer;
	protected MeshRenderer MoistureMapRenderer;
	protected MeshRenderer BiomeMapRenderer;






    public void setSeed(int seed) {
        this.seed = seed;
    }

    public void setDimensions(int width, int height) {
        this.width = width;
        this.height = height;
    }

	protected BiomeType[,] BiomeTable = new BiomeType[6,6] {   
		//COLDEST        //COLDER          //COLD                  //HOT                          //HOTTER                       //HOTTEST
		{ BiomeType.Ice, BiomeType.Tundra, BiomeType.Grassland,    BiomeType.Desert,              BiomeType.Desert,              BiomeType.Desert },              //DRYEST
		{ BiomeType.Ice, BiomeType.Tundra, BiomeType.Grassland,    BiomeType.Desert,              BiomeType.Desert,              BiomeType.Desert },              //DRYER
		{ BiomeType.Ice, BiomeType.Tundra, BiomeType.Woodland,     BiomeType.Woodland,            BiomeType.Savanna,             BiomeType.Savanna },             //DRY
		{ BiomeType.Ice, BiomeType.Tundra, BiomeType.BorealForest, BiomeType.Woodland,            BiomeType.Savanna,             BiomeType.Savanna },             //WET
		{ BiomeType.Ice, BiomeType.Tundra, BiomeType.BorealForest, BiomeType.SeasonalForest,      BiomeType.TropicalRainforest,  BiomeType.TropicalRainforest },  //WETTER
		{ BiomeType.Ice, BiomeType.Tundra, BiomeType.BorealForest, BiomeType.TemperateRainforest, BiomeType.TropicalRainforest,  BiomeType.TropicalRainforest }   //WETTEST
	};

    public void GenerateSurface() {
        Generate();
    }

    protected abstract void getData();
    protected abstract Tile getTileAbove(Tile tile);
    protected abstract Tile getTileBelow(Tile tile);
    protected abstract Tile getTileOnLeft(Tile tile);
    protected abstract Tile getTileOnRight(Tile tile);

	protected virtual void Generate()
	{		
		getData ();
		LoadTiles ();		
		UpdateNeighbors ();
		
		GenerateRivers ();
		BuildRiverGroups ();
		DigRiverGroups ();
		AdjustMoistureMap ();
		
		UpdateBitmasks ();
		FloodFill ();
		
		GenerateBiomeMap ();
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

	void Update() {
        // Refresh with inspector values
		if (Input.GetKeyDown (KeyCode.F5)) {
            seed = UnityEngine.Random.Range(0, int.MaxValue);
            Generate();
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
				
				if (!tiles[x, y].Collidable) continue;
				
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
				if (t.HeightType == HeightType.River)
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

	private void BuildRiverGroups()
	{
		//loop each tile, checking if it belongs to multiple rivers
		for (var x = 0; x < width; x++) {
			for (var y = 0; y < height; y++) {
				Tile t = tiles[x,y];

				if (t.Rivers.Count > 1)
				{
					// multiple rivers == intersection
					RiverGroup group = null;

					// Does a rivergroup already exist for this group?
					for (int n=0; n < t.Rivers.Count; n++)
					{
						River tileriver = t.Rivers[n];
						for (int i = 0; i < RiverGroups.Count; i++)
						{
							for (int j = 0; j < RiverGroups[i].Rivers.Count; j++)
							{
								River river = RiverGroups[i].Rivers[j];
								if (river.ID == tileriver.ID)
								{
									group = RiverGroups[i];
								}
								if (group != null) break;
							}
							if (group != null) break;
						}
						if (group != null) break;
					}

					// existing group found -- add to it
					if (group != null)
					{
						for (int n=0; n < t.Rivers.Count; n++)
						{
							if (!group.Rivers.Contains(t.Rivers[n]))
								group.Rivers.Add(t.Rivers[n]);
						}
					}
					else   //No existing group found - create a new one
					{
						group = new RiverGroup();
						for (int n=0; n < t.Rivers.Count; n++)
						{
							group.Rivers.Add(t.Rivers[n]);
						}
						RiverGroups.Add (group);
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
			return tile.HeightValue;
	}

	private void GenerateRivers()
	{
		int attempts = 0;
		int rivercount = RiverCount;
		Rivers = new List<River> ();

		// Generate some rivers
		while (rivercount > 0 && attempts < MaxRiverAttempts) {

			// Get a random tile
			int x = UnityEngine.Random.Range (0, width);
			int y = UnityEngine.Random.Range (0, height);			
			Tile tile = tiles[x,y];

			// validate the tile
			if (!tile.Collidable) continue;
			if (tile.Rivers.Count > 0) continue;

			if (tile.HeightValue > MinRiverHeight)
			{				
				// Tile is good to start river from
				River river = new River(rivercount);

				// Figure out the direction this river will try to flow
				river.CurrentDirection = tile.GetLowestNeighbor (this);

				// Recursively find a path to water
				FindPathToWater(tile, river.CurrentDirection, ref river);

				// Validate the generated river 
				if (river.TurnCount < MinRiverTurns || river.Tiles.Count < MinRiverLength || river.Intersections > MaxRiverIntersections)
				{
					//Validation failed - remove this river
					for (int i = 0; i < river.Tiles.Count; i++)
					{
						Tile t = river.Tiles[i];
						t.Rivers.Remove (river);
					}
				}
				else if (river.Tiles.Count >= MinRiverLength)
				{
					//Validation passed - Add river to list
					Rivers.Add (river);
					tile.Rivers.Add (river);
					rivercount--;	
				}
			}		
			attempts++;
		}
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
		river.Length = river.Tiles.Count;  

		// randomize size change
		int two = river.Length / 2;
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
		if (count4 > river.Length) {
			int extra = count4 - river.Length;
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
		river.Length = river.Tiles.Count;  

		// randomize size change
		int two = river.Length / 2;
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
		if (count4 > river.Length) {
			int extra = count4 - river.Length;
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
	
	private void FindPathToWater(Tile tile, Direction direction, ref River river)
	{
		if (tile.Rivers.Contains (river))
			return;

		// check if there is already a river on this tile
		if (tile.Rivers.Count > 0)
			river.Intersections++;

		river.AddTile (tile);

		// get neighbors
		Tile left = getTileOnLeft (tile);
		Tile right = getTileOnRight (tile);
		Tile top = getTileAbove (tile);
		Tile bottom = getTileBelow (tile);
		
		float leftValue = int.MaxValue;
		float rightValue = int.MaxValue;
		float topValue = int.MaxValue;
		float bottomValue = int.MaxValue;
		
		// query height values of neighbors
		if (left != null && left.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(left)) 
			leftValue = left.HeightValue;
		if (right != null && right.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(right)) 
			rightValue = right.HeightValue;
		if (top != null && top.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(top)) 
			topValue = top.HeightValue;
		if (bottom != null && bottom.GetRiverNeighborCount(river) < 2 && !river.Tiles.Contains(bottom)) 
			bottomValue = bottom.HeightValue;
		
		// if neighbor is existing river that is not this one, flow into it
		if (bottom != null && bottom.Rivers.Count == 0 && !bottom.Collidable)
			bottomValue = 0;
		if (top != null && top.Rivers.Count == 0 && !top.Collidable)
			topValue = 0;
		if (left != null && left.Rivers.Count == 0 && !left.Collidable)
			leftValue = 0;
		if (right != null && right.Rivers.Count == 0 && !right.Collidable)
			rightValue = 0;
		
		// override flow direction if a tile is significantly lower
		if (direction == Direction.Left)
			if (Mathf.Abs (rightValue - leftValue) < 0.1f)
				rightValue = int.MaxValue;
		if (direction == Direction.Right)
			if (Mathf.Abs (rightValue - leftValue) < 0.1f)
				leftValue = int.MaxValue;
		if (direction == Direction.Top)
			if (Mathf.Abs (topValue - bottomValue) < 0.1f)
				bottomValue = int.MaxValue;
		if (direction == Direction.Bottom)
			if (Mathf.Abs (topValue - bottomValue) < 0.1f)
				topValue = int.MaxValue;
		
		// find mininum
		float min = Mathf.Min (Mathf.Min (Mathf.Min (leftValue, rightValue), topValue), bottomValue);
		
		// if no minimum found - exit
		if (min == int.MaxValue)
			return;
		
		//Move to next neighbor
		if (min == leftValue) {
			if (left != null && left.Collidable)
			{
				if (river.CurrentDirection != Direction.Left){
					river.TurnCount++;
					river.CurrentDirection = Direction.Left;
				}
				FindPathToWater (left, direction, ref river);
			}
		} else if (min == rightValue) {
			if (right != null && right.Collidable)
			{
				if (river.CurrentDirection != Direction.Right){
					river.TurnCount++;
					river.CurrentDirection = Direction.Right;
				}
				FindPathToWater (right, direction, ref river);
			}
		} else if (min == bottomValue) {
			if (bottom != null && bottom.Collidable)
			{
				if (river.CurrentDirection != Direction.Bottom){
					river.TurnCount++;
					river.CurrentDirection = Direction.Bottom;
				}
				FindPathToWater (bottom, direction, ref river);
			}
		} else if (min == topValue) {
			if (top != null && top.Collidable)
			{
				if (river.CurrentDirection != Direction.Top){
					river.TurnCount++;
					river.CurrentDirection = Direction.Top;
				}
				FindPathToWater (top, direction, ref river);
			}
		}
	}

	// Build a Tile array from our data
	private void LoadTiles()
	{
		tiles = new Tile[width, height];
		
		for (var x = 0; x < width; x++)
		{
			for (var y = 0; y < height; y++)
			{
				Tile t = new Tile();
				t.x = x;
				t.y = y;

				//set heightmap value
				float heightValue = heightData.Data[x, y];
				heightValue = (heightValue - heightData.Min) / (heightData.Max - heightData.Min);
				t.HeightValue = heightValue;
					

				if (heightValue < DeepWater)  {
					t.HeightType = HeightType.DeepWater;
					t.Collidable = false;
				}
				else if (heightValue < ShallowWater)  {
					t.HeightType = HeightType.ShallowWater;
					t.Collidable = false;
				}
				else if (heightValue < Sand) {
					t.HeightType = HeightType.Sand;
					t.Collidable = true;
				}
				else if (heightValue < Grass) {
					t.HeightType = HeightType.Grass;
					t.Collidable = true;
				}
				else if (heightValue < Forest) {
					t.HeightType = HeightType.Forest;
					t.Collidable = true;
				}
				else if (heightValue < Rock) {
					t.HeightType = HeightType.Rock;
					t.Collidable = true;
				}
				else  {
					t.HeightType = HeightType.Snow;
					t.Collidable = true;
				}


				//adjust moisture based on height
				if (t.HeightType == HeightType.DeepWater) {
					moistureData.Data[t.x, t.y] += 8f * t.HeightValue;
				}
				else if (t.HeightType == HeightType.ShallowWater) {
					moistureData.Data[t.x, t.y] += 3f * t.HeightValue;
				}
				else if (t.HeightType == HeightType.Shore) {
					moistureData.Data[t.x, t.y] += 1f * t.HeightValue;
				}
				else if (t.HeightType == HeightType.Sand) {
					moistureData.Data[t.x, t.y] += 0.2f * t.HeightValue;
				}				
				
				//Moisture Map Analyze	
				float moistureValue = moistureData.Data[x,y];
				moistureValue = (moistureValue - moistureData.Min) / (moistureData.Max - moistureData.Min);
				t.MoistureValue = moistureValue;
				
				//set moisture type
				if (moistureValue < DryerValue) t.MoistureType = MoistureType.Dryest;
				else if (moistureValue < DryValue) t.MoistureType = MoistureType.Dryer;
				else if (moistureValue < WetValue) t.MoistureType = MoistureType.Dry;
				else if (moistureValue < WetterValue) t.MoistureType = MoistureType.Wet;
				else if (moistureValue < WettestValue) t.MoistureType = MoistureType.Wetter;
				else t.MoistureType = MoistureType.Wettest;


				// Adjust Heat Map based on Height - Higher == colder
				if (t.HeightType == HeightType.Forest) {
					heatData.Data[t.x, t.y] -= 0.1f * t.HeightValue;
				}
				else if (t.HeightType == HeightType.Rock) {
					heatData.Data[t.x, t.y] -= 0.25f * t.HeightValue;
				}
				else if (t.HeightType == HeightType.Snow) {
					heatData.Data[t.x, t.y] -= 0.4f * t.HeightValue;
				}
				else {
					heatData.Data[t.x, t.y] += 0.01f * t.HeightValue;
				}

				// Set heat value
				float heatValue = heatData.Data[x,y];
				heatValue = (heatValue - heatData.Min) / (heatData.Max - heatData.Min);
				t.HeatValue = heatValue;

				// set heat type
				if (heatValue < ColdestValue) t.HeatType = HeatType.Coldest;
				else if (heatValue < ColderValue) t.HeatType = HeatType.Colder;
				else if (heatValue < ColdValue) t.HeatType = HeatType.Cold;
				else if (heatValue < WarmValue) t.HeatType = HeatType.Warm;
				else if (heatValue < WarmerValue) t.HeatType = HeatType.Warmer;
				else t.HeatType = HeatType.Warmest;

                if (Clouds1 != null)
                {
                    t.Cloud1Value = Clouds1.Data[x, y];
                    t.Cloud1Value = (t.Cloud1Value - Clouds1.Min) / (Clouds1.Max - Clouds1.Min);
                }

                if (Clouds2 != null)
                {
                    t.Cloud2Value = Clouds2.Data[x, y];
                    t.Cloud2Value = (t.Cloud2Value - Clouds2.Min) / (Clouds2.Max - Clouds2.Min);
                }

                tiles[x,y] = t;
			}
		}
	}
	
	private void UpdateNeighbors()
	{
		for (var x = 0; x < width; x++)
		{
			for (var y = 0; y < height; y++)
			{
				Tile t = tiles[x,y];
				
				t.Top = getTileAbove(t);
				t.Bottom = getTileBelow (t);
				t.Left = getTileOnLeft (t);
				t.Right = getTileOnRight (t);
			}
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
				if (t.Collidable)   
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
		if (tiles.Type == TileGroupType.Land && !tile.Collidable)
			return;
		if (tiles.Type == TileGroupType.Water && tile.Collidable)
			return;

		// Add to TileGroup
		tiles.Tiles.Add (tile);
		tile.FloodFilled = true;

		// floodfill into neighbors
		Tile t = getTileAbove (tile);
		if (t != null && !t.FloodFilled && tile.Collidable == t.Collidable)
			stack.Push (t);
		t = getTileBelow (tile);
		if (t != null && !t.FloodFilled && tile.Collidable == t.Collidable)
			stack.Push (t);
		t = getTileOnLeft (tile);
		if (t != null && !t.FloodFilled && tile.Collidable == t.Collidable)
			stack.Push (t);
		t = getTileOnRight (tile);
		if (t != null && !t.FloodFilled && tile.Collidable == t.Collidable)
			stack.Push (t);
	}
    
}
