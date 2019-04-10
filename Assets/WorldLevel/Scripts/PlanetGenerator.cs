using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlanetGenerator : WrappingWorldGenerator {

    PlanetController planetController;

    public PlanetGenerator(PlanetController planetController) : base(planetController) {
        this.planetController = planetController;
    }

    public override void addProperties() {
        this.properties.Add(new HeightPlanetProperty());
    }
}
