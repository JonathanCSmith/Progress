public class TileIndex {
    public int x;
    public int y;

    public TileIndex(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object o) {
        if ((o == null) || !this.GetType().Equals(o.GetType())) {
            return false;
        }

        else {
            TileIndex comparison = (TileIndex)o;
            return this.x == comparison.x && this.y == comparison.y;
        }
    }
}
