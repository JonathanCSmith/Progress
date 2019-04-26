using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlanetGenerator : Generator {

    public readonly Dictionary<string, PropertyDecorator> properties = new Dictionary<string, PropertyDecorator>();
    public readonly Dictionary<string, FeatureDecorator> features = new Dictionary<string, FeatureDecorator>();

    private List<string> propertyOrder;
    private List<string> featureOrder;

    //protected MapData Clouds1;
    //protected MapData Clouds2;

    PlanetController planetController;

    public PlanetGenerator(PlanetController planetController) : base(planetController) {
        this.planetController = planetController;
    }

    public bool init() {
        TopologicalSorter sorter = new TopologicalSorter(this.properties.Count);
        List<StringDependencyMap> stringDependencies = new List<StringDependencyMap>();
        foreach (PropertyDecorator decorator in this.properties.Values) {
            StringDependencyMap map = new StringDependencyMap();
            map.name = decorator.getName();
            map.dependencies = decorator.getDependencies();
            stringDependencies.Add(map);
        }
        this.propertyOrder = TopologicalSorter.sortByString(stringDependencies);

        // Clear and remake for features
        stringDependencies.Clear();
        sorter = new TopologicalSorter(this.features.Count);
        foreach (FeatureDecorator decorator in this.features.Values) {
            StringDependencyMap map = new StringDependencyMap();
            map.name = decorator.getName();
            map.dependencies = decorator.getDependencies();
            stringDependencies.Add(map);
        }
        this.featureOrder = TopologicalSorter.sortByString(stringDependencies);

        return this.propertyOrder != null && this.featureOrder != null;
    }

    public void addPropertyGenerator(PropertyDecorator propertyGenerator) {
        this.properties.Add(propertyGenerator.getName(), propertyGenerator);
    }

    public PropertyDecorator getPropertyGenerator(string index) {
        // TODO Error catch
        return this.properties[index];
    }

    public void addFeatureGenerator(FeatureDecorator featureGenerator) {
        this.features.Add(featureGenerator.getName(), featureGenerator);
    }

    public FeatureDecorator getFeatureGenerator(string index) {
        // TODO Error catch
        return this.features[index];
    }

    protected override bool validateProperties() {
        foreach (PropertyDecorator propertyGenerator in this.properties.Values) {
            if (!propertyGenerator.initialise(this.generable)) {
                return false;
            }
        }

        return true;
    }

    protected override void generateNoise() {
        foreach (string propertyName in this.propertyOrder) {
            PropertyDecorator property = this.properties[propertyName];
            property.generateNoise(this.generable.getSeed());
        }
    }

    protected override void generateSurfaceData() {
        foreach (string propertyName in this.propertyOrder) {
            PropertyDecorator property = this.properties[propertyName];
            property.preallocateMapData(this.generable.getWidth(), this.generable.getHeight());
        }

        // loop through each x,y point - get height value
        for (var x = 0; x < this.generable.getWidth(); x++) {
            for (var y = 0; y < this.generable.getHeight(); y++) {

                // WRAP ON BOTH AXIS
                // Noise range
                float x1 = 0, x2 = 2;
                float y1 = 0, y2 = 2;
                float dx = x2 - x1;
                float dy = y2 - y1;

                // Sample noise at smaller intervals
                float s = x / (float)this.generable.getWidth();
                float t = y / (float)this.generable.getHeight();

                // Calculate our 4D coordinates
                float nx = x1 + Mathf.Cos(s * 2 * Mathf.PI) * dx / (2 * Mathf.PI);
                float ny = y1 + Mathf.Cos(t * 2 * Mathf.PI) * dy / (2 * Mathf.PI);
                float nz = x1 + Mathf.Sin(s * 2 * Mathf.PI) * dx / (2 * Mathf.PI);
                float nw = y1 + Mathf.Sin(t * 2 * Mathf.PI) * dy / (2 * Mathf.PI);

                foreach (PropertyDecorator property in this.properties.Values) {
                    property.generateSurface(x, y, nx, ny, nz, nw);
                }
            }
        }
    }

    protected override Tile fillTileData(Tile t, int x, int y) {
        foreach (string propertyName in this.propertyOrder) {
            PropertyDecorator property = this.properties[propertyName];
            property.generateTile(t, x, y);
        }

        // TODO: Clouds should be rendered on the sphere when we come back to it
        //if (Clouds1 != null) {
        //    t.Cloud1Value = Clouds1.Data[x, y];
        //    t.Cloud1Value = (t.Cloud1Value - Clouds1.Min) / (Clouds1.Max - Clouds1.Min);
        //}

        //if (Clouds2 != null) {
        //    t.Cloud2Value = Clouds2.Data[x, y];
        //    t.Cloud2Value = (t.Cloud2Value - Clouds2.Min) / (Clouds2.Max - Clouds2.Min);
        //}

        return t;
    }

    protected override bool generateFeatures() {
        foreach (string featureName in this.featureOrder) {
            FeatureDecorator feature = this.features[featureName];
            if (!feature.generate(this.generable)) {
                return false;
            }
        }

        return true;
    }
}
