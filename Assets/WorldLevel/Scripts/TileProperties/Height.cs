using System;

public class TileHeightProperties {

    public float maxHeight;
    public string heightClassification;
    public bool collisionState;

    public TileHeightProperties(float maxHeight, string heightClassification, bool collisionState) {
        this.maxHeight = maxHeight;
        this.heightClassification = heightClassification;
        this.collisionState = collisionState;
    }

    public float targetHeight() {
        return this.maxHeight;
    }

    public string getHeightClassification() {
        return this.heightClassification;
    }
}

public class Height : DataBucket {

    public readonly float heightValue;
    public readonly TileHeightProperties tileHeightProperties;

    public Height(float heightValue, TileHeightProperties heightProperties) {
        this.heightValue = heightValue;
        this.tileHeightProperties = heightProperties;
    }

    public TileHeightProperties getTileProperties() {
        return this.tileHeightProperties;
    }
}
