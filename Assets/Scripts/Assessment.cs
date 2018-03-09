using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Assessment : MonoBehaviour {
    [SerializeField]
    Dropdown Q1Dropdown, Q2Dropdown, Q3Dropdown;
    [SerializeField]
    Text Q1Answer, Q2Answer, Q3Answer;

	
	// Update is called once per frame
	void Update ()
    {
		if(Q1Dropdown.value == 2)
        {
            Q1Answer.text = "Correct!";
        }
        else
        {
            Q1Answer.text = "";
        }

        if(Q2Dropdown.value == 1)
        {
            Q2Answer.text = "Correct!";
        }
        else
        {
            Q2Answer.text = "";
        }

        if(Q3Dropdown.value == 1)
        {
            Q3Answer.text = "Correct!";
        }
        else
        {
            Q3Answer.text = "";
        }
	}

    public void ReturnToTitle()
    {
        SceneManager.LoadScene(0);
    }
}
