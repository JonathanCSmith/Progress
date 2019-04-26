using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class GameController : MonoBehaviour
{

    public GameObject planet;
    public PlanetController planetController;

    public GameObject player;
    public FirstPersonController playerController;

    [SerializeField]
    bool shouldGenerateAPlanet = false;

    [SerializeField]
    int sourcePatchSize = 128;

    [SerializeField]
    int targetPatchSize = 512;

    private bool shouldSpawnPlayer = false;

    // Start is called before the first frame update
    void Start()
    {
        if (this.player == null) {
            this.player = transform.Find("FPSController").gameObject;
            this.playerController = this.player.GetComponent<FirstPersonController>();
            // TODO: playerController.setImmortalizedState(true);
        }

        if (this.shouldGenerateAPlanet) {
            this.planet = (GameObject)Instantiate(Resources.Load("WorldLevel/Prefabs/Planet"));

            // Setup the controller - specifically for generation
            this.planetController = this.planet.GetComponent<PlanetController>();
            this.planetController.setSeed(10);
            this.planetController.setPatchScaling(this.sourcePatchSize, this.targetPatchSize);

            // Generate the planet
            this.planetController.generate();

            // TODO: This is a temp - we only want to generate once
            this.shouldGenerateAPlanet = false;
            this.shouldSpawnPlayer = true;
        }
    }

    // Update is called once per frame
    void Update() {
        if (this.shouldSpawnPlayer) {
            // Spawn the player
            if (this.planet != null) {
                if (this.planetController == null) {
                    planetController = planet.GetComponent<PlanetController>();
                }

                this.planetController.spawnPlayer(this.player, this.playerController);
                this.player.SetActive(true);
                this.shouldSpawnPlayer = false;
            }
        }
    }
}
