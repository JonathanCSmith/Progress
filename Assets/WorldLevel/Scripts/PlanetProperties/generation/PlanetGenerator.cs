using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlanetGenerator : WrappingWorldGenerator {

    PlanetController planetController;

    public PlanetGenerator(PlanetController planetController, int seed, int width, int height) : base(planetController, seed, width, height) {
        this.planetController = planetController;
    }

    public PlanetGenerator(PlanetController planetController, int width, int height) : base(planetController, width, height) {
        this.planetController = planetController;
    }

    public override void addProperties() {
        this.properties.Add(new HeightPlanetProperty());
        this.properties.Add(new HeatPlanetProperty());
        this.properties.Add(new MoisturePlanetProperty());
    }
}
