using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TitleScreen : MonoBehaviour
{
    /*
     * Title Screen behaviour. Shows the high score and starts the game when a button is pressed.
     */

    private void Start()
    {
        //Set High Score text
        transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "TOP - " + PlayerPrefs.GetInt("HighScore").ToString("000000");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            //Reset high score
            PlayerPrefs.SetInt("HighScore", 0);
            GameManager.GM.PlaySound("Warp", false);
            transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "TOP - 000000";
        }
        else if (Input.anyKeyDown)
        {
            GameManager.GM.PlaySound("Pause", false);
            GameManager.GM.LoadLevel(1);
        }
    }
}
