using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuButtons : MonoBehaviour {
    [SerializeField]
    GameObject menu, players, credits;
    [SerializeField]
    Slider playerAmount;

    public void CreateAGame()
    {
        menu.SetActive(false);
        players.SetActive(true);
    }

    public void StartGame()
    {
        NumberOfPlayers.numberOfPlayers = (int)playerAmount.value;
        SceneManager.LoadScene(1);
    }

    public void Credits()
    {
        menu.SetActive(false);
        credits.SetActive(true);
    }

    public void Back()
    {
        credits.SetActive(false);
        menu.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
