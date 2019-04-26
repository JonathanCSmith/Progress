public abstract class FeatureDecorator : PlanetDecorator {

    private readonly string featureName;

    public FeatureDecorator(string name) {
        this.featureName = name;
    }

    public string getName() {
        return this.featureName;
    }

    public abstract string[] getFeatureDependencies();

    public abstract bool generate(Generable generable);
}
