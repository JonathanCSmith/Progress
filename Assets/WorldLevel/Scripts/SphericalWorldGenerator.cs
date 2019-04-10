using UnityEngine;
using AccidentalNoise;

public class SphericalWorldGenerator : Generator {
		
	MeshRenderer Sphere;
    MeshRenderer Atmosphere1;
    MeshRenderer Atmosphere2;
    MeshRenderer BumpTexture;
    MeshRenderer PaletteTexture;

    protected ImplicitFractal HeightMap;
	protected ImplicitFractal HeatMap;
	protected ImplicitFractal MoistureMap;
    protected ImplicitFractal Cloud1Map;
    protected ImplicitFractal Cloud2Map;

	protected override void Instantiate()
	{
		base.Instantiate ();
		Sphere = transform.Find("Globe").Find ("Sphere").GetComponent<MeshRenderer> ();
        Atmosphere1 = transform.Find("Globe").Find("Atmosphere1").GetComponent<MeshRenderer>();
        Atmosphere2 = transform.Find("Globe").Find("Atmosphere2").GetComponent<MeshRenderer>();

        BumpTexture = transform.Find("BumpTexture").GetComponent<MeshRenderer>();
        PaletteTexture = transform.Find("PaletteTexture").GetComponent<MeshRenderer>();
    }

	protected override void generate()
	{
		base.generate ();

        Texture2D bumpTexture = TextureGenerator.GetBumpMap (width, height, tiles);
		Texture2D normal = TextureGenerator.CalculateNormalMap(bumpTexture, 3);

		Sphere.materials [0].mainTexture = BiomeMapRenderer.materials[0].mainTexture;
		Sphere.GetComponent<MeshRenderer> ().materials [0].SetTexture ("_BumpMap", normal);
		Sphere.GetComponent<MeshRenderer> ().materials [0].SetTexture ("_ParallaxMap", HeightMapRenderer.materials[0].mainTexture);

        Atmosphere1.materials[0].mainTexture = TextureGenerator.GetCloud1Texture(width, height, tiles);
        Atmosphere2.materials [0].mainTexture = TextureGenerator.GetCloud2Texture (width, height, tiles); 

        BumpTexture.materials[0].mainTexture = Atmosphere1.materials[0].mainTexture;
        PaletteTexture.materials[0].mainTexture = Atmosphere2.materials[0].mainTexture;
    }

	protected override void intialize()
	{
		HeightMap = new ImplicitFractal (FractalType.MULTI, 
		                                 BasisType.SIMPLEX, 
		                                 InterpolationType.QUINTIC, 
		                                 terrainOctaves, 
		                                 terrainFrequency, 
		                                 seed);		
		
		HeatMap = new ImplicitFractal(FractalType.MULTI, 
		                              BasisType.SIMPLEX, 
		                              InterpolationType.QUINTIC, 
		                              heatOctaves, 
		                              heatFrequency, 
		                              seed);
		
		MoistureMap = new ImplicitFractal (FractalType.MULTI, 
		                                   BasisType.SIMPLEX, 
		                                   InterpolationType.QUINTIC, 
		                                   moistureOctaves, 
		                                   moistureFrequency, 
		                                   seed);

        Cloud1Map = new ImplicitFractal(FractalType.BILLOW,
                                        BasisType.SIMPLEX,
                                        InterpolationType.QUINTIC,
                                        4,
                                        1.55f,
                                        seed);

        Cloud2Map = new ImplicitFractal (FractalType.BILLOW, 
		                                BasisType.SIMPLEX, 
		                                InterpolationType.QUINTIC, 
		                                5, 
		                                1.75f, 
		                                seed);
	}

	protected override void generateSurfaceData()
	{
		heightData = new MapData (width, height);
		heatData = new MapData (width, height);
		moistureData = new MapData (width, height);
		Clouds1 = new MapData (width, height);
        Clouds2 = new MapData(width, height);

        // Define our map area in latitude/longitude
        float southLatBound = -180;
		float northLatBound = 180;
		float westLonBound = -90;
		float eastLonBound = 90; 
		
		float lonExtent = eastLonBound - westLonBound;
		float latExtent = northLatBound - southLatBound;
		
		float xDelta = lonExtent / (float)width;
		float yDelta = latExtent / (float)height;
		
		float curLon = westLonBound;
		float curLat = southLatBound;
		
        // Loop through each tile using its lat/long coordinates
		for (var x = 0; x < width; x++) {
			
			curLon = westLonBound;
			
			for (var y = 0; y < height; y++) {
				
				float x1 = 0, y1 = 0, z1 = 0;
				
                // Convert this lat/lon to x/y/z
				LatLonToXYZ (curLat, curLon, ref x1, ref y1, ref z1);

                // Heat data
				float sphereValue = (float)HeatMap.Get (x1, y1, z1);					
				if (sphereValue > heatData.Max)
					heatData.Max = sphereValue;
				if (sphereValue < heatData.Min)
					heatData.Min = sphereValue;				
				heatData.Data [x, y] = sphereValue;
				
				float coldness = Mathf.Abs (curLon) / 90f;
				float heat = 1 - Mathf.Abs (curLon) / 90f;				
				heatData.Data [x, y] += heat;
				heatData.Data [x, y] -= coldness;
				
                // Height Data
				float heightValue = (float)HeightMap.Get (x1, y1, z1);
				if (heightValue > heightData.Max)
					heightData.Max = heightValue;
				if (heightValue < heightData.Min)
					heightData.Min = heightValue;				
				heightData.Data [x, y] = heightValue;
				
				// Moisture Data
				float moistureValue = (float)MoistureMap.Get (x1, y1, z1);
				if (moistureValue > moistureData.Max)
					moistureData.Max = moistureValue;
				if (moistureValue < moistureData.Min)
					moistureData.Min = moistureValue;				
				moistureData.Data [x, y] = moistureValue;

                // Cloud Data
				Clouds1.Data[x,y] = (float)Cloud1Map.Get (x1, y1, z1);
				if (Clouds1.Data[x,y] > Clouds1.Max)
					Clouds1.Max = Clouds1.Data[x,y];
				if (Clouds1.Data[x,y] < Clouds1.Min)
					Clouds1.Min = Clouds1.Data[x,y];

                Clouds2.Data[x, y] = (float)Cloud2Map.Get(x1, y1, z1);
                if (Clouds2.Data[x, y] > Clouds2.Max)
                    Clouds2.Max = Clouds2.Data[x, y];
                if (Clouds2.Data[x, y] < Clouds2.Min)
                    Clouds2.Min = Clouds2.Data[x, y];

                curLon += xDelta;
			}			
			curLat += yDelta;
		}
	}
    
	// Convert Lat/Long coordinates to x/y/z for spherical mapping
	void LatLonToXYZ(float lat, float lon, ref float x, ref float y, ref float z)
	{
		float r = Mathf.Cos(Mathf.Deg2Rad * lon);

		//Longitude Optimization
		float sin = Mathf.Sqrt(1 * 1 - r * r);
		if (lon < 0)
		    sin = -sin;

		x = r * Mathf.Cos(Mathf.Deg2Rad * lat);
		y = sin;
		z = r * Mathf.Sin(Mathf.Deg2Rad * lat);
	}
    
	protected override Tile getTileAbove(Tile t)
	{
		if (t.y - 1 > 0)
			return tiles [t.x, t.y - 1];
		else 
			return null;
	}
	protected override Tile getTileBelow(Tile t)
	{
		if (t.y + 1 < height)
			return tiles [t.x, t.y + 1];
		else
			return null;
	}
	protected override Tile getTileOnLeft(Tile t)
	{
		return tiles [MathHelper.Mod(t.x - 1, width), t.y];
	}
	protected override Tile getTileOnRight(Tile t)
	{
		return tiles [MathHelper.Mod (t.x + 1, width), t.y];
	}

}
