using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // SerializeFields for use in the editor
    /// <summary>
    /// List of colors for villages
    /// </summary>
    [Tooltip("List of color options for villages.")]
    [SerializeField]
    List<Color> playerColors;
    /// <summary>
    /// Communal growth rate
    /// </summary>
    [Tooltip("Growth rate in communal fire.")]
    [SerializeField]
    float commonGrowthRate = .3f;
    /// <summary>
    /// Private growth rate
    /// </summary>
    [Tooltip("Growth rate in private fires.")]
    [SerializeField]
    float privateGrowthRate = .3f;
    /// <summary>
    /// Multiplier for starting bonfire
    /// </summary>
    [Tooltip("Starting bonfire multiplier. Fire starts at the given proportion of capacity.")]
    [SerializeField]
    float startingPopulationMultiplier = .6f;
    /// <summmary>
    /// Radius of village
    /// </summmary>
    [Tooltip("Village radius. Distance from fire to instantiate villages.")]
    [SerializeField]
    float villageRadius = 15f;
    /// <summary>
    /// The camera rig, used to turn the camera each turn
    /// </summary>
    [Tooltip("The camera rig object.")]
    [SerializeField]
    GameObject cameraRig;
    /// <summary>
    /// Reference to village prefab for instantiation
    /// </summary>
    [Tooltip("Village prefab. Used to instantiate for each player.")]
    [SerializeField]
    GameObject villagePrefab;
    /// <summary>
    /// Reference to basic capsule for villagers
    /// </summary>
    [Tooltip("Villager prefab. It's just a capsule.")]
    [SerializeField]
    GameObject villagerPrefab;
    /// <summary>
    /// Multiplier for use in common capacity calculation.
    /// </summary>
    [Tooltip("Multiplier for use in common capacity calculation.")]
    [SerializeField]
    int capacityMultiplier = 60;
    /// <summary>
    /// Logs required for survival
    /// </summary>
    [Tooltip("Logs required per person per round to survive.")]
    [SerializeField]
    int logsToLive = 4;
    /// <summary>
    /// The number of generations in the game.
    /// </summary>
    [Tooltip("Number of generations in a game.")]
    [SerializeField]
    int numberOfGenerations = 1;
    /// <summary>
    /// The number of rounds in a game.
    /// </summary>
    [Tooltip("Number of rounds per game.")]
    [SerializeField]
    int numberOfRounds = 8;

    //SerializeFields related to the UI
    [SerializeField]
    Text currentPlayerTag;
    [SerializeField]
    GameObject summary, share, collect, allocation;
    [SerializeField]
    Slider shareAmount;
    [SerializeField]
    InputField teamNumber;
    [SerializeField]
    Slider commonCollect, privateCollect;


    // Private fields
    /// <summary>
    /// Bonfire population
    /// </summary>
    float bonfirePopulation;
    /// <summary>
    /// Active village number
    /// </summary>
    int activeVillageNumber;
    /// <summary>
    /// Maximum capacity of wood in the fire
    /// </summary>
    int commonFireCapacity;
    /// <summary>
    /// Number of human players in the game. Use property instead.
    /// </summary>
    int humanPlayers_UseProperty;
    /// <summary>
    /// Current round number
    /// </summary>
    int roundNumber;
    /// <summary>
    /// Number of total players (villages) in the game 
    /// </summary>
    int totalPlayers = 12;
    /// <summary>
    /// Total number of villagers across all villagers
    /// </summary>
    int totalVillagers;
    /// <summary>
    /// List of village instances
    /// </summary>
    List<GameObject> villages = new List<GameObject>();
    int totalWoodToAllocate;
    bool turnEnd;

    // Properties
    /// <summary>
    /// Public property for setting the number of human players in the game
    /// </summary>
    public int HumanPlayers
    {
        get { return humanPlayers_UseProperty; }
        set { humanPlayers_UseProperty = value; }
    }

	// Use this for initialization
	void Start () 
	{
        InitializeVillages();
        InitializeCapacities();
        StartCoroutine(GameLoop());
	}
	
	// Update is called once per frame
	void Update () 
	{
        
	}

    /// <summary>
    /// Set starting values for common capacity and population
    /// </summary>
    void InitializeCapacities()
    {
        // Set capacity for common fire
        commonFireCapacity = (int)(((totalPlayers * capacityMultiplier) / numberOfGenerations) /
            (numberOfRounds / numberOfGenerations) / (commonGrowthRate / 2f));

        // Use capacity to determine starting population
        bonfirePopulation = commonFireCapacity * startingPopulationMultiplier;
    }

    /// <summary>
    /// Initialize villages and villagers within them
    /// </summary>
    void InitializeVillages()
    {
        // TODO modify total number of players
        totalPlayers = NumberOfPlayers.numberOfPlayers;
        // Loop per player
        for(int i = 0; i < totalPlayers; i++)
        {
            // Instantiate the village
            villages.Add(Instantiate(villagePrefab));

            // Randomly add 1 to 3 villagers
            villages[i].GetComponent<Village>().NumberOfVillagers = Random.Range(1, 4);
            // Add the number to the total villagers
            totalVillagers += villages[i].GetComponent<Village>().NumberOfVillagers;
            // Instantiate villagers
            for(int j = 0; j < villages[i].GetComponent<Village>().NumberOfVillagers; j++)
            {
                GameObject villager = Instantiate(villagerPrefab, villages[i].transform);
                // Place the villager at the spawn point
                villager.transform.position = villages[i].GetComponent<Village>().villagerSpawnPoints[j].transform.position;
                // Recolor the villager
                villager.GetComponent<Renderer>().material.color = playerColors[i];
            }
            // Recolor the hut
            villages[i].GetComponent<Village>().hut.GetComponent<Renderer>().material.color = playerColors[i];

            // Make a new vector to set the village position
            Vector3 pos = new Vector3();
            // Set the X and Z to the proper points along the circle with the defined radius around the fire
            pos.x = villageRadius * Mathf.Sin((360 / totalPlayers * i) * Mathf.Deg2Rad);
            pos.z = villageRadius * Mathf.Cos((360 / totalPlayers * i) * Mathf.Deg2Rad);
            // Make the Y lame
            pos.y = 0;
            // Move the village to the position
            villages[i].transform.position = pos;
            // Rotate the village to face the fire
            villages[i].transform.LookAt(transform);
        }
        // Set max logs per turn
        foreach (GameObject v in villages)
        {
            v.GetComponent<Village>().MaxLogsPerTurn = totalVillagers;
            v.GetComponent<Village>().WoodRackStockRate = privateGrowthRate;
        }
    }

    /// <summary>
    /// The core loop of the game. 
    /// Runs through the given number of rounds for the given number of villages.
    /// </summary>
    /// <returns></returns>
    IEnumerator GameLoop()
    {
        for (roundNumber = 1; roundNumber <= numberOfRounds; roundNumber++)
        {
            // Reset each village so giving is possible
            foreach (GameObject v in villages)
            {
                v.GetComponent<Village>().IsActiveTurn = false;
                // Restock private wood racks
                v.GetComponent<Village>().RestockRack();
            }
            // Repopulate bonfire
            bonfirePopulation = Mathf.Min(commonFireCapacity,
                (int)((bonfirePopulation * commonGrowthRate) - ((bonfirePopulation * commonGrowthRate) *
                (bonfirePopulation / commonFireCapacity)) + bonfirePopulation));
            for(activeVillageNumber = 1; activeVillageNumber <= totalPlayers; activeVillageNumber++)
            {
                // If the village is dead, skip it
                if (villages[activeVillageNumber - 1].GetComponent<Village>().IsDead)
                    continue;
                // Note that village turn has passed
                villages[activeVillageNumber - 1].GetComponent<Village>().IsActiveTurn = true;
                cameraRig.transform.LookAt(villages[activeVillageNumber - 1].transform);
                // TODO identify active player in player text
                currentPlayerTag.text = "Player " + (activeVillageNumber - 1);

                //shareAmount.maxValue = villages[activeVillageNumber - 1].GetComponent<Village>().wo
                
                    /*while(!cameraMoveTest)
                 * TODO Whatever logic should determine that a turn has ended
                    yield return null;*/
            }
        }
        yield return null;
    }

    // TODO Write function for "Collect" button
    // Should hide initial panel and show collect panel
    public void Collect()
    {
        summary.SetActive(false);
        collect.SetActive(true);
    }

    // TODO Write function for first "Share" button
    // Should hide initial panel and show share panel
    public void ShareScreen()
    {
        summary.SetActive(false);
        share.SetActive(true);
    }

    /* TODO Write function for second "Share" button
    Should validate that the target village hasn't gone and fail if it has
    (should be simple, check whether the village of the given number's IsActiveTurn is true, if so, it's gone)
    Set the slider max value to the total logs possessed by village of number activeVillageNumber - 1
    If the target village is eligible to receive wood, add to the target village and subtract from the current village
    Finally, hide share panel and show initial panel */
    public void ShareWithTeam()
    {
        int team = System.Convert.ToInt32(teamNumber);
        if (villages[team-1].GetComponent<Village>().IsActiveTurn == true)
        {

        }
        else
        {

        }

    }


    /* TODO Write function for collect "Continue" button
    Should set slider max value to the maximum logs possible per round
    Validate that the common fire contains that much wood
    If all goes well, save the value for the next panel, hide the collect panel and show the allocation panel
    */
    public void Continue()
    {



    }


    /* TODO Write function for "End Turn" button
    Should validate the values in each input field
    If they add up properly, add each value to its respective place
    Check if the village dies (can't sustain all members) and assign accordingly
    Finally, tell the coroutine to continue, hide the allocation panel, and show the initial panel
    */ 

    public void EndTurn()
    {



    }
}
