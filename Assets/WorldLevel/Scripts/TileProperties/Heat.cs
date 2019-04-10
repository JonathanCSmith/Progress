using System;

public class TileHeatProperties {

    public float maxHeight;
    public string heatClassification;

    public TileHeightProperties(float maxHeight, string heatClassification) {
        this.maxHeight = maxHeight;
        this.heatClassification = heatClassification;
    }

    public float targetHeight() {
        return this.maxHeight;
    }

    public string getHeatClassification() {
        return this.heatClassification;
    }
}

public class Heat : DataBucket {

    public readonly float heatValue;
    public readonly string heatType;

    public Heat(float heatValue) {
        this.heatValue = heatValue;


        // set heat type
        if (heatValue < ColdestValue) {
            this.heatType = "coldest";
        }

        else if (heatValue < ColderValue) {
            this.heatType = "colder";
        }

        else if (heatValue < ColdValue) {
            this.heatType = "cold";
        }

        else if (heatValue < WarmValue) {
            this.heatType = "warm";
        }

        else if (heatValue < WarmerValue) {
            this.heatType = "warmer";
        }

        else {
            this.heatType = "warmest";
        }
    }
}
