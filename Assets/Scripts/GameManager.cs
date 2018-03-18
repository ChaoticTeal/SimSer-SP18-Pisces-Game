using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    /// <summary>
    /// Bonfire object
    /// </summary>
    [Tooltip("The bonfire.")]
    [SerializeField]
    GameObject bonfire;
    [SerializeField]
    GameObject playerPanel;

    //SerializeFields related to the UI
    [SerializeField]
    Text currentPlayerTag;
    [SerializeField]
    GameObject summary, share, collect, allocation, end, playerTag, exitGame;
    [SerializeField]
    Slider shareAmount;
    [SerializeField]
    Text teamNumber;
    [SerializeField]
    Slider commonCollect, privateCollect;
    [SerializeField]
    Text consumeWood, addToPrivate, investToPrivate;
    [SerializeField]
    Text summaryText, shareAmountText, commonAmountText, allocationToSurvive, privateAmountText, errorMessageTeam, errorMessageAllocation;
    [SerializeField]
    Text bonFireAmount, endText, communalPopulation, roundNumberText;
    [SerializeField]
    InputField investment;


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
    /// <summary>
    /// List of dead villages
    /// </summary>
    List<int> deadVillages = new List<int>();
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
            // 25/50/25 probability split for 1/2/3
            int villagers = Random.Range(0, 4);
            if (villagers == 0)
                villagers = 2;
            villages[i].GetComponent<Village>().NumberOfVillagers = villagers;
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
            villages[i].GetComponent<Village>().flag.GetComponent<Renderer>().material.color = playerColors[i];

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
        bool deadCheck = false;
        for (roundNumber = 1; roundNumber <= numberOfRounds; roundNumber++)
        {
            if (deadVillages.Count == totalPlayers)
                continue;
            // Reset each village so giving is possible
            foreach (GameObject v in villages)
            {
                v.GetComponent<Village>().IsActiveTurn = false;
                // Restock private wood racks
                v.GetComponent<Village>().RestockRack();
                v.GetComponent<Village>().UpdateWoodRack();
            }
            // Repopulate bonfire
            bonfirePopulation = Mathf.Min(commonFireCapacity,
                (int)((bonfirePopulation * commonGrowthRate) - ((bonfirePopulation * commonGrowthRate) *
                (bonfirePopulation / commonFireCapacity)) + bonfirePopulation));
            //Text for the Total amount in communal bonfire
            communalPopulation.enabled = true;
            communalPopulation.text = "The communal bonfire holds " + bonfirePopulation + " wood.";
            roundNumberText.text = "Round Number: " + roundNumber;
            for (activeVillageNumber = 1; activeVillageNumber <= totalPlayers; activeVillageNumber++)
            {
                if (bonfirePopulation <= 0)
                    bonfire.SetActive(false);
                // If the village is dead, skip it
                if (villages[activeVillageNumber - 1].GetComponent<Village>().IsDead)
                {
                    foreach (int i in deadVillages)
                        if (i == activeVillageNumber)
                            deadCheck = true;
                    if (!deadCheck)
                        deadVillages.Add(activeVillageNumber);
                    deadCheck = false;
                    Debug.Log(deadVillages);
                    continue;
                }
                // Note that village turn has passed
                villages[activeVillageNumber - 1].GetComponent<Village>().IsActiveTurn = true;
                cameraRig.transform.LookAt(villages[activeVillageNumber - 1].transform);
                // Identify active player in player text
                currentPlayerTag.text = "Player " + (activeVillageNumber);
                playerPanel.GetComponent<Image>().color = playerColors[activeVillageNumber - 1];

                // Add summary to the first panel
                summaryText.text = "You have <color=#" + ColorUtility.ToHtmlStringRGB(playerColors[activeVillageNumber - 1]) + ">" 
                    + villages[activeVillageNumber - 1].GetComponent<Village>().NumberOfVillagers + " villager(s)</color> to care for. You need <color=#"
                    + ColorUtility.ToHtmlStringRGB(playerColors[activeVillageNumber - 1]) + ">" 
                    + (logsToLive * villages[activeVillageNumber - 1].GetComponent<Village>().NumberOfVillagers) 
                    + "</color> to warm them all. Your private wood rack contains <color=#" + ColorUtility.ToHtmlStringRGB(playerColors[activeVillageNumber - 1]) 
                    + ">" + villages[activeVillageNumber - 1].GetComponent<Village>().WoodRackStock  + " wood</color> and can hold <color=#" 
                    + ColorUtility.ToHtmlStringRGB(playerColors[activeVillageNumber - 1]) + ">" 
                    + villages[activeVillageNumber - 1].GetComponent<Village>().WoodRackCapacity + "</color>. What would you like to do?";

                allocationToSurvive.text = "You need <color=#" + ColorUtility.ToHtmlStringRGB(playerColors[activeVillageNumber - 1]) + ">" 
                    + (logsToLive * villages[activeVillageNumber - 1].GetComponent<Village>().NumberOfVillagers) 
                    + "</color> to warm all your villagers. How would you like to allocate the wood you collected?";

                shareAmount.maxValue = villages[activeVillageNumber - 1].GetComponent<Village>().WoodRackStock;
                shareAmountText.text = shareAmount.value.ToString("0");
                //Set slider max value to the maximum logs possible per round
                if (villages[activeVillageNumber - 1].GetComponent<Village>().MaxLogsPerTurn <= bonfirePopulation)
                    commonCollect.maxValue = villages[activeVillageNumber - 1].GetComponent<Village>().MaxLogsPerTurn;
                else
                    commonCollect.maxValue = bonfirePopulation;

                commonAmountText.text = commonCollect.value.ToString("0");
                privateCollect.maxValue = villages[activeVillageNumber - 1].GetComponent<Village>().WoodRackStock;
                privateAmountText.text = privateCollect.value.ToString("0");

                while(!turnEnd)
                    yield return null;
                turnEnd = false;
            }
        }
        summary.SetActive(false);
        end.SetActive(true);
        playerTag.SetActive(false);
        bonFireAmount.text = "The amount of wood in the communal bonfire is " + bonfirePopulation;
        if (bonfirePopulation == 0 && deadVillages.Count == totalPlayers)
        {
            endText.text = "Your actions have caused the complete elimination of wood in your willages. Survival was impossible. " +
                "Take in to account the actions for the greater good than actions for selfish greed. Welcome to the Tragedy of the Commons.";
        }
        else if (bonfirePopulation > 0 && deadVillages.Count == totalPlayers)
        {
            endText.text = "The villages have all gone extinct, yet the bonfire remains. Nature heals from the abuse it had received. Your actions have saved the planet from global warming.";
        }
        else
        {
            endText.text = "The villages have survived the years with wood intact. " +
               "Actions that protect the community are far more rewarding than those that focus on a single person's greed. Welcome to the Tragedy of the Commons.";
        }
        yield return null;
    }

   
    // Should hide initial panel and show collect panel
    public void Collect()
    {
        summary.SetActive(false);
        collect.SetActive(true);
        communalPopulation.enabled = false;
    }


    //Hide initial panel and show share panel
    public void ShareScreen()
    {
        summary.SetActive(false);
        share.SetActive(true);
        communalPopulation.enabled = false;
    }
    
    //Hide share panel and show initial panel
    public void ShareBack()
    {
        share.SetActive(false);
        summary.SetActive(true);
        teamNumber.text = "";
        shareAmount.value = 0;
    }

    // Update share value
    public void ShareSliderChanged()
    {
        shareAmountText.text = shareAmount.value.ToString();
    }

    // Update common collect value
    public void CommonSliderChanged()
    {
        commonAmountText.text = commonCollect.value.ToString();
    }

    // Update private collect value
    public void PrivateSliderChanged()
    {
        privateAmountText.text = privateCollect.value.ToString();
    }

    /*
    Validate that the target village hasn't gone and fail if it has
    (should be simple, check whether the village of the given number's IsActiveTurn is true, if so, it's gone)
    Set the slider max value to the total logs possessed by village of number activeVillageNumber - 1
    If the target village is eligible to receive wood, add to the target village and subtract from the current village
    Finally, hide share panel and show initial panel */
    public void ShareWithTeam()
    {
        int team = System.Convert.ToInt32(teamNumber.text);
        if (team <= totalPlayers && !villages[team-1].GetComponent<Village>().IsActiveTurn && !villages[team - 1].GetComponent<Village>().IsDead)
        {
            villages[team - 1].GetComponent<Village>().TheShadowRealm += (int)shareAmount.value;
            villages[activeVillageNumber - 1].GetComponent<Village>().TakeWoodFromRack((int)shareAmount.value);
            share.SetActive(false);
            summary.SetActive(true);

        }
        else
        {
            errorMessageTeam.text = "You can not share with this team, please pick another team or return to the action select.";
        }
        teamNumber.text = "";
        shareAmount.value = 0;

    }


    /*
    Validate that the common fire contains that much wood
    If all goes well, save the value for the next panel, hide the collect panel and show the allocation panel
    */
    public void Continue()
    {
        villages[activeVillageNumber - 1].GetComponent<Village>().TakeWoodFromRack((int)privateCollect.value);
        totalWoodToAllocate = (int)commonCollect.value + (int)privateCollect.value + villages[activeVillageNumber - 1].GetComponent<Village>().TheShadowRealm;
        villages[activeVillageNumber - 1].GetComponent<Village>().TheShadowRealm = 0;
        bonfirePopulation -= (int)commonCollect.value;
        collect.SetActive(false);
        allocation.SetActive(true);
        commonCollect.value = 0;
        privateCollect.value = 0;
        if(roundNumber ==1)
        {
            investment.enabled = false;
            
        }
        else
        {
            investment.enabled = true;
        }
    }


    /* 
    Validate the values in each input field
    If they add up properly, add each value to its respective place
    Check if the village dies (can't sustain all members) and assign accordingly
    Finally, tell the coroutine to continue, hide the allocation panel, and show the initial panel
    */ 

    public void EndTurn()
    {
        int invest;
        int consume = System.Convert.ToInt32(consumeWood.text);
        int privateRack = System.Convert.ToInt32(addToPrivate.text);
        if (roundNumber == 1)
        {
            invest = 0;
        }
        else
        {
            invest = System.Convert.ToInt32(investToPrivate.text);
        }
        
        if(privateRack + villages[activeVillageNumber - 1].GetComponent<Village>().WoodRackStock >
            (int)Mathf.Pow((villages[activeVillageNumber - 1].GetComponent<Village>().WoodRackInvestment + invest)/ 2, 1.5f))
        {
            errorMessageAllocation.text = "Your wood rack cannot hold " + privateRack + " more wood. Please adjust allocation.";
        }
        else if (consume + privateRack + invest == totalWoodToAllocate)
        {
            if((logsToLive * villages[activeVillageNumber - 1].GetComponent<Village>().NumberOfVillagers) <= consume)
            {
                villages[activeVillageNumber - 1].GetComponent<Village>().TotalLogsConsumed += consume;
                villages[activeVillageNumber - 1].GetComponent<Village>().AddWoodToRack(privateRack);
                villages[activeVillageNumber - 1].GetComponent<Village>().WoodRackInvestment += invest;
                allocation.SetActive(false);
                summary.SetActive(true);
                errorMessageAllocation.text = "";
                turnEnd = true;

            }
            else
            {
                villages[activeVillageNumber - 1].GetComponent<Village>().IsDead = true;
                allocation.SetActive(false);
                summary.SetActive(true);
                errorMessageAllocation.text = "";
                turnEnd = true;
            }

        }
        else if(totalWoodToAllocate == 0)
        {
            villages[activeVillageNumber - 1].GetComponent<Village>().IsDead = true;
            allocation.SetActive(false);
            summary.SetActive(true);
            errorMessageAllocation.text = "";
            turnEnd = true;
        }
        else
        {
            errorMessageAllocation.text = "You have not used all of the collected wood. You have " + totalWoodToAllocate + " wood to allocate. Please allocate it all.";
        }
    }

    public void ToTheAssessment()
    {
        SceneManager.LoadScene(2);
    }

    public void Exiting()
    {
        exitGame.SetActive(true);
    }

    public void ContinueGame()
    {
        exitGame.SetActive(false);
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
