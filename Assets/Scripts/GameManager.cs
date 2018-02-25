using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // SerializeFields for use in the editor
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
    /// Maximum capacity of wood in the fire
    /// </summary>
    int commonFireCapacity;
    /// <summary>
    /// Number of human players in the game. Use property instead.
    /// </summary>
    int humanPlayers_UseProperty;
    /// <summary>
    /// Number of total players (villages) in the game 
    /// </summary>
    int totalPlayers;
    /// <summary>
    /// Total number of villagers across all villagers
    /// </summary>
    int totalVillagers;

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
        InitializeCapacities();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

    // Set starting values for common capacity and population
    void InitializeCapacities()
    {
        // Set capacity for common fire
        commonFireCapacity = ((totalPlayers * capacityMultiplier) / numberOfGenerations) /
            (numberOfRounds / numberOfGenerations) / ((int)commonGrowthRate / 2);

        // Use capacity to determine starting population
        bonfirePopulation = commonFireCapacity * startingPopulationMultiplier;
    }
}
