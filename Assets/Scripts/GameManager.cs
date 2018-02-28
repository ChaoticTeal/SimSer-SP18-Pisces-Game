using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        for(int i = 0; i < totalPlayers; i++)
        {
            villages.Add(Instantiate(villagePrefab));
            villages[i].GetComponent<Village>().NumberOfVillagers = Random.Range(1, 4);
            totalVillagers += villages[i].GetComponent<Village>().NumberOfVillagers;
            for(int j = 0; j < villages[i].GetComponent<Village>().NumberOfVillagers; j++)
            {
                GameObject villager = Instantiate(villagerPrefab, villages[i].transform);
                villager.transform.position = villages[i].GetComponent<Village>().villagerSpawnPoints[j].transform.position;
                villager.GetComponent<Renderer>().material.color = playerColors[i];
            }
            villages[i].GetComponent<Village>().hut.GetComponent<Renderer>().material.color = playerColors[i];
            Vector3 pos = new Vector3();
            pos.x = villageRadius * Mathf.Sin((360 / totalPlayers * i) * Mathf.Deg2Rad);
            pos.z = villageRadius * Mathf.Cos((360 / totalPlayers * i) * Mathf.Deg2Rad);
            pos.y = 0;
            villages[i].transform.position = pos;
            villages[i].transform.LookAt(transform);
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
            for(activeVillageNumber = 1; activeVillageNumber<=totalPlayers; activeVillageNumber++)
            {
                villages[activeVillageNumber - 1].GetComponent<Village>().IsActiveTurn = true;
                cameraRig.transform.LookAt(villages[activeVillageNumber - 1].transform);
                //cameraRig.transform.rotation = Quaternion.Inverse(cameraRig.transform.rotation);
            }
        }
        yield return null;
    }
}
