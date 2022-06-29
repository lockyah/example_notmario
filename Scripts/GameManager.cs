using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /*
     * Handles all persistent data between levels as a singleton.
     * 
     * The in-game UI (score, coins, etc.) and level-start UI (lives, world number) are both attached to and controlled by this object.
     * Handles playing sounds, loading and setting up each level, and updates the UI.
     */

    public static GameManager GM;

    int Lives = 3; //Max of 128
    int Coins = 0; //99 max, 100 coins adds a life and resets to 0

    //Indexed to allow combos to modify which score is added
    List<int> PointOptions = new List<int> {50, 100, 200, 400, 500, 800, 1000, 2000, 4000, 5000, 8000, 0};
    int Points = 0; //Limit of 999,999

    public bool TimerActive = false;
    public float Timer = 160f;

    bool PanicMode = false; //Is there less than 30 seconds remaining?
    
    [SerializeField] int CurrentLevel = 0; //Build index. Scene names are used to track the "WORLD 1-1" layout.
    int SavedPowerUp = 0;

    TMP_Text ScoreText, CoinText, WorldText, TimeText, FlashWorldText, FlashLives;
    Animator Ani;
    [SerializeField] int SFXSources = 5;
    [SerializeField] AudioSource Music;
    List<AudioSource> AudioSources = new List<AudioSource>();


    void Start()
    {
        if(GM == null)
        {
            GM = this;
            DontDestroyOnLoad(gameObject);

            Ani = GetComponent<Animator>();

            Transform CanvasObject = transform.GetChild(0);
            ScoreText = CanvasObject.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
            CoinText = CanvasObject.GetChild(1).GetComponent<TMP_Text>();
            WorldText = CanvasObject.GetChild(2).GetChild(0).GetComponent<TMP_Text>(); //Only the "1-1" part of "WORLD 1-1". Determined by Scene name.
            TimeText = CanvasObject.GetChild(3).GetChild(0).GetComponent<TMP_Text>();

            FlashWorldText = transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>(); //Full "WORLD 1-1" title
            FlashLives = transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>(); //Only the counter for lives

            for (int i = 0; i < SFXSources; i++)
            {
                AudioSources.Add(gameObject.AddComponent<AudioSource>());
                AudioSources[i].playOnAwake = false;
            }

        } else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if(Timer > 0 && TimerActive)
        {
            Timer -= Time.deltaTime;

            if(Timer <= 0)
            {
                //Game over!
                //Kill Mario with the same animation as if he had run into an enemy.
                GameObject.FindGameObjectWithTag("Player").GetComponent<MarioControl>().InstantDeath(false);
            } else if(Timer <= 30 && !PanicMode)
            {
                //Panic!
                StartCoroutine(PanicStart());
            }
        }

        ScoreText.text = Points.ToString("000000");
        CoinText.text = "x" + Coins.ToString("00");
        TimeText.text = Mathf.Round(Timer).ToString("000");
    }

    public void LoadLevel(int index)
    {
        PlaySound("", true); //Stop music

        Time.timeScale = index == 0 ? 1 : 0; //Only title screen starts at ts 1

        //Most other scripts can't see the scene index, so -1 indicates "use the next level in order."
        //-2 is for "Reload the same level."
        switch (index)
        {
            case -1:
                CurrentLevel++;
                break;
            case -2:
                //Do not change index
                break;
            default:
                CurrentLevel = index;
                break;
        }

        StartCoroutine(WaitForLoad(CurrentLevel));
    }

    IEnumerator WaitForLoad(int index)
    {
        if(index != 0)
        {
            Ani.SetTrigger("PrepScreen"); //Turn screen black so that the first frame of the new level isn't seen.
        }

        SceneManager.LoadScene(CurrentLevel);

        while(SceneManager.GetActiveScene().buildIndex != CurrentLevel)
        {
            //Delay until scene is loaded.
            yield return new WaitForEndOfFrame();
        }

        if (index != 0)
        {
            WorldText.text = SceneManager.GetActiveScene().name;
            FlashWorldText.text = "WORLD " + WorldText.text;
            FlashLives.text = Lives.ToString();
            Timer = 160f;

            Ani.ResetTrigger("PrepScreen");
            Ani.SetTrigger(Lives < 0 ? "GameOver" : "LevelStart");

            if (Lives < 0)
            {
                PlaySound("Game Over", true);
            }
            else
            {
                //Pass on Mario's current Powerup to the next iteration of him
                GameObject.FindGameObjectWithTag("Player").GetComponent<MarioControl>().GivePowerUp(SavedPowerUp, true);
            }
        }
        else
        {
            //Go directly to title screen
            WorldText.text = "1-1";

            Lives = 3;
            Points = 0;
            Timer = 999;
            Coins = 0;
            SavedPowerUp = 0;

            SetTimerActive(false);

            PlaySound("Overworld", true);
        }
        
    }

    public void StartLevel()
    {
        //Called via Animation Event after the lives counter is shown.

        PlaySound(SceneManager.GetActiveScene().buildIndex == 4 ? "Bowser" : "Overworld", true); //Set level music. Only 1-4 starts with different music.
        Time.timeScale = 1;
        SetTimerActive(true);
    }

    public void SetTimerActive(bool active)
    {
        TimerActive = active;
    }

    IEnumerator PanicStart()
    {
        string currentMusic = Music.clip.name; //Save which song is currently playing

        PlaySound("Hurry Up", true);
        PanicMode = true;
        yield return new WaitForSeconds(2.8f);

        //Any song played with Panic Mode on plays the fast version instead.
        PlaySound(currentMusic, true);
    }

    public int GetFireworksBonus()
    {
        //Returns the last digit of the timer to determine if any fireworks should go off at the flagpole.
        //Having reached the end of the level, also disables Panic Mode if it was active and saves Mario's powerup.
        PanicMode = false;
        SavedPowerUp = (int)GameObject.FindGameObjectWithTag("Player").GetComponent<MarioControl>().GetPowerUp(); //Save current Powerup to pass on

        return (int)char.GetNumericValue(TimeText.text[2]);
    }

    public IEnumerator CastleFinish()
    {
        //After the time bonus in a castle, set the high score and animate fading to black.
        if(Points > PlayerPrefs.GetInt("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore", Points);
        }

        yield return new WaitForSeconds(3f);

        //Fade to black
        Ani.SetTrigger("CastleEnd");
        yield return new WaitForSeconds(3f);

        LoadLevel(0);

    }

    public IEnumerator TimeBonus()
    {
        while(Timer > 0)
        {
            Timer = Mathf.Clamp(Timer - 1, 0, 999);

            if(Timer > 0)
            {
                AddPoints(0); //Give 50 points per second remaining.
                PlaySound("Beep", false);
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    public void ChangeBackground(string name)
    {
        //Changes the background colour and effects based on the setting.

        if (name == "Underground" || name == "Bowser")
        {
            Camera.main.backgroundColor = Color.black;
            Camera.main.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            Camera.main.backgroundColor = new Color(0.58f, 0.58f, 1f);
            Camera.main.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    public void PlaySound(string name, bool isMusic)
    {
        AudioSource a = null;

        if (isMusic)
        {
            a = Music;

            //If currently in Panic Mode, load the faster music unless it's a Star, which doesn't have one.
            a.clip = Resources.Load<AudioClip>("Sounds/Music/" + name + (PanicMode && name != "Star" ? " Hurry" : ""));

            //Only the level themes should loop - short themes like the Star or Level Ending should not.
            a.loop = name == "Overworld" || name == "Underground" || name == "Bowser" || name == "Underwater";

        } else
        {
            //Search through each AudioSource to find one that's empty.
            foreach(AudioSource source in AudioSources)
            {
                if (!source.isPlaying)
                {
                    a = source;
                    break;
                }
            }

            if(a == null)
            {
                //If all sources are currently playing, give the first as a failsafe
                a = AudioSources[0];
            }
            a.clip = Resources.Load<AudioClip>("Sounds/SFX/" + name);
        }

        if (a.isPlaying)
        {
            a.Stop();
        }
        a.Play();
    }

    public void AddLife()
    {
        Lives++;
        Mathf.Clamp(Lives, 0, 128);

        PlaySound("1up", false);
    }

    public void LoseLife()
    {
        Lives--; //Does not need to be clamped. Negatives trigger the game over sequence.
        SavedPowerUp = 0; //Prevent Mario from restarting with a saved Powerup

        LoadLevel(CurrentLevel);
    }

    public void AddCoin()
    {
        //Only one coin is added at a time
        Coins++;

        if(Coins >= 100)
        {
            AddLife();
            Coins = 0;
        } else
        {
            PlaySound("Coin", false);
        }
    }

    public void AddPoints(int pointIndex)
    {
        if(pointIndex == 11)
        {
            AddLife();
        }

        Points += PointOptions[pointIndex];

        Mathf.Clamp(Points, 0, 999999);
    }

    //Alternate AddPoints that creates a marker of the points earned.
    public void AddPoints(int pointIndex, Vector2 markerPos)
    {
        if (markerPos != null)
        {
            //Instantiate a Points Object at Mario's position, and add points to counter.
            Instantiate(Resources.Load<GameObject>("Effects/Points"), markerPos, Quaternion.identity).GetComponent<Points>().Setup(pointIndex -1);
        }

        AddPoints(pointIndex);
    }
}
