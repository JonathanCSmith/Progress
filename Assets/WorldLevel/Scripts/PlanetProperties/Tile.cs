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

public enum HeatType
{
	Coldest = 0,
	Colder = 1,
	Cold = 2,
	Warm = 3,
	Warmer = 4,
	Warmest = 5
}

public enum MoistureType
{
	Wettest = 5,
	Wetter = 4,
	Wet = 3,
	Dry = 2,
	Dryer = 1,
	Dryest = 0
}

public enum BiomeType
{
	Desert,
	Savanna,
	TropicalRainforest,
	Grassland,
	Woodland,
	SeasonalForest,
	TemperateRainforest,
	BorealForest,
	Tundra,
	Ice
}

public class Tile
{
	public HeightClassification heightClassification;
	public HeatType HeatType;
	public MoistureType MoistureType;
	public BiomeType BiomeType;

    public float Cloud1Value { get; set; }
    public float Cloud2Value { get; set; }
	public float heightValue { get; set; }
	public float HeatValue { get; set; }
	public float MoistureValue { get ; set; }
	public int x, y;
	public int Bitmask;
	public int BiomeBitmask;

	public Tile left;
	public Tile right;
	public Tile above;
	public Tile below;

	public bool collisionState;
	public bool FloodFilled;

	public Color Color = Color.black;

	public List<River> Rivers = new List<River>();

	public int RiverSize { get ;set; }
		
	public Tile()
	{
	}

	public void UpdateBiomeBitmask()
	{
		int count = 0;
		
		if (collisionState && above != null && above.BiomeType == BiomeType)
			count += 1;
		if (collisionState && below != null && below.BiomeType == BiomeType)
			count += 4;
		if (collisionState && left != null && left.BiomeType == BiomeType)
			count += 8;
		if (collisionState && right != null && right.BiomeType == BiomeType)
			count += 2;
		
		BiomeBitmask = count;
	}

	public void UpdateBitmask()
	{
		int count = 0;
		
		if (collisionState && above != null && above.heightClassification == heightClassification)
			count += 1;
		if (collisionState && right != null && right.heightClassification == heightClassification)
			count += 2;
		if (collisionState && below != null && below.heightClassification == heightClassification)
			count += 4;
		if (collisionState && left != null && left.heightClassification == heightClassification)
			count += 8;
		
		Bitmask = count;
	}

    public int GetRiverNeighborCount(River river)
	{
		int count = 0;
		if (left != null && left.Rivers.Count > 0 && left.Rivers.Contains (river))
			count++;
		if (right != null && right.Rivers.Count > 0 && right.Rivers.Contains (river))
			count++;
		if (above != null && above.Rivers.Count > 0 && above.Rivers.Contains (river))
			count++;
		if (below != null && below.Rivers.Count > 0 && below.Rivers.Contains (river))
			count++;
		return count;
    }
	
	public void SetRiverPath(River river)
	{
		if (!collisionState)
			return;
		
		if (!Rivers.Contains (river)) {
			Rivers.Add (river);
		}
	}

	private void SetRiverTile(River river)
	{
		SetRiverPath (river);
		heightClassification = HeightClassification.River;
		heightValue = 0;
		collisionState = false;
	}

    // This function got messy.  Sorry.
	public void DigRiver(River river, int size)
	{
		SetRiverTile (river);
		RiverSize = size;

		if (size == 1) {
			if (below != null) 
			{ 
				below.SetRiverTile (river);
				if (below.right != null) below.right.SetRiverTile (river);		
			}
			if (right != null) right.SetRiverTile (river);					
		}

		if (size == 2) {
			if (below != null) { 
				below.SetRiverTile (river);
				if (below.right != null) below.right.SetRiverTile (river);		
			}
			if (right != null) {
				right.SetRiverTile (river);
			}
			if (above != null) {
				above.SetRiverTile (river);
				if (above.left != null) above.left.SetRiverTile (river);
				if (above.right != null)above.right.SetRiverTile (river);
			}
			if (left != null) {
				left.SetRiverTile (river);
				if (left.below != null) left.below.SetRiverTile (river);
			}
		}

		if (size == 3) {
			if (below != null) { 
				below.SetRiverTile (river);
				if (below.right != null) below.right.SetRiverTile (river);	
				if (below.below != null)
				{
					below.below.SetRiverTile (river);
					if (below.below.right != null) below.below.right.SetRiverTile (river);
				}
			}
			if (right != null) {
				right.SetRiverTile (river);
				if (right.right != null) 
				{
					right.right.SetRiverTile (river);
					if (right.right.below != null) right.right.below.SetRiverTile (river);
				}
			}
			if (above != null) {
				above.SetRiverTile (river);
				if (above.left != null) above.left.SetRiverTile (river);
				if (above.right != null)above.right.SetRiverTile (river);
			}
			if (left != null) {
				left.SetRiverTile (river);
				if (left.below != null) left.below.SetRiverTile (river);
			}
		}

		if (size == 4) {

			if (below != null) { 
				below.SetRiverTile (river);
				if (below.right != null) below.right.SetRiverTile (river);	
				if (below.below != null)
				{
					below.below.SetRiverTile (river);
					if (below.below.right != null) below.below.right.SetRiverTile (river);
				}
			}
			if (right != null) {
				right.SetRiverTile (river);
				if (right.right != null) 
				{
					right.right.SetRiverTile (river);
					if (right.right.below != null) right.right.below.SetRiverTile (river);
				}
			}
			if (above != null) {
				above.SetRiverTile (river);
				if (above.right != null) { 
					above.right.SetRiverTile (river);
					if (above.right.right != null) above.right.right.SetRiverTile (river);
				}
				if (above.above != null)
				{
					above.above.SetRiverTile (river);
					if (above.above.right != null) above.above.right.SetRiverTile (river);
				}
			}
			if (left != null) {
				left.SetRiverTile (river);
				if (left.below != null) {
					left.below.SetRiverTile (river);
					if (left.below.below != null) left.below.below.SetRiverTile (river);
				}

				if (left.left != null) {
					left.left.SetRiverTile (river);
					if (left.left.below != null) left.left.below.SetRiverTile (river);
					if (left.left.above != null) left.left.above.SetRiverTile (river);
				}

				if (left.above != null)
				{
					left.above.SetRiverTile (river);
					if (left.above.above != null) left.above.above.SetRiverTile (river);
				}
			}
		}
    }

    public readonly Dictionary<string, DataBucket> tileData = new Dictionary<string, DataBucket>();

    public DataBucket getData(string v) {
        if (!this.tileData.ContainsKey(v)) {
            return null;
        }

        return this.tileData[v];
    }

    public void addData(string key, DataBucket bucket) { 
        this.tileData.Add(key, bucket);
    }
}
