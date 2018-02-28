using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Village : MonoBehaviour 
{
    // SerializeFields for assignment in-editor
    /// <summary>
    /// Reference to hut in prefab for recoloring
    /// </summary>
    [Tooltip("The hut portion of the prefab.")]
    [SerializeField]
    public GameObject hut;
    /// <summary>
    /// List of possible spawn points for villagers
    /// </summary>
    [Tooltip("List of possible spawn points for villagers.")]
    [SerializeField]
    public List<GameObject> villagerSpawnPoints;

    // Private fields
    /// <summary>
    /// Is the village active?
    /// </summary>
    bool isActiveTurn_UseProperty;
    /// <summary>
    /// Restock rate of wood rack
    /// </summary>
    float woodRackStockRate_UseProperty;
    /// <summary>
    /// Number of residents in the village - must be between 1-3
    /// </summary>
    int numberOfVillagers_UseProperty;
    /// <summary>
    /// Total logs consumed for daily heating
    /// </summary>
    int totalLogsConsumed;
    /// <summary>
    /// Capacity of wood rack
    /// </summary>
    int woodRackCapacity;
    /// <summary>
    /// Logs invested into wood rack capacity
    /// </summary>
    int woodRackInvestment;
    /// <summary>
    /// Logs stocked in private wood rack
    /// </summary>
    int woodRackStock;

    // Properties
    /// <summary>
    /// Public accessor for active turn state
    /// </summary>
    public bool IsActiveTurn
    {
        get { return isActiveTurn_UseProperty; }
        set { isActiveTurn_UseProperty = value; }
    }
    /// <summary>
    /// Public accessor for wood rack stock rate
    /// </summary>
    public float WoodRackStockRate
    {
        get { return woodRackStockRate_UseProperty; }
        set { woodRackStockRate_UseProperty = value; }
    }
    /// <summary>
    /// Public accessor for number of villagers
    /// </summary>
    public int NumberOfVillagers
    {
        get { return numberOfVillagers_UseProperty; }
        set
        {
            numberOfVillagers_UseProperty = Mathf.Min(value, 3);
            numberOfVillagers_UseProperty = Mathf.Max(numberOfVillagers_UseProperty, 1);
        }
    }

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

    /// <summary>
    /// Set capacity of private wood rack based on wood invested into it
    /// </summary>
    void SetCapacity()
    {
        woodRackCapacity = (int)Mathf.Pow(woodRackInvestment / 2, 1.5f);
    }

    /// <summary>
    /// Take wood from wood rack to use for daily heat
    /// </summary>
    /// <param name="amount">Amount of wood to remove</param>
    protected void TakeWoodFromRack(int amount)
    {
        woodRackStock -= amount;
        totalLogsConsumed += amount;
    }

    /// <summary>
    /// Add wood to wood rack for later use
    /// </summary>
    /// <param name="amount">Amount of wood to add</param>
    protected void AddWoodToRack(int amount)
    {
        woodRackStock += amount;
    }

    /// <summary>
    /// Restock rack between rounds based on present stock
    /// </summary>
    protected void RestockRack()
    {
        woodRackStock += (int)(woodRackStock * WoodRackStockRate);
        woodRackStock = Mathf.Min(woodRackStock, woodRackCapacity);
    }
}
