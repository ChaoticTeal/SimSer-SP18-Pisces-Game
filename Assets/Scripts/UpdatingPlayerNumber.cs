using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdatingPlayerNumber : MonoBehaviour {

    [SerializeField]
    Slider playerNumber;
    [SerializeField]
    Text numberText;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        numberText.text = "Number of players: " + playerNumber.value.ToString("0");
	}
}
