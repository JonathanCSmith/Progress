using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlanetFeatureGenerator {

    private readonly string featureName;

    public PlanetFeatureGenerator(string name) {
        this.featureName = name;
    }

    public string getName() {
        return this.featureName;
    }

    public abstract bool generate(Generable generable);
}
