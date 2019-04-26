public class MapData {

	public float[,] data;
	public float min { get; set; }
	public float max { get; set; }

	public MapData(int width, int height) {
		this.data = new float[width, height];
		this.min = float.MaxValue;
		this.max = float.MinValue;
	}
}
