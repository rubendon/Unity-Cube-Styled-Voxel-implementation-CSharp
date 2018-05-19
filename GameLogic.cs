using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour {

    int toolDelay = 0; //for delayAmounting destroying/placing voxels
    public int delayAmount = 50;
    public int soundDelay = 0;

    public Text timerText; //UI elements
    public Text scoreText; 

    public Text gameOverText;
    public Text scoreDisplay;
    public GameObject highScores;

    public Image highlight;
    public Image xRayBorder;
    public Image pointer;

    public List <AudioClip> soundEffects;
    public Slider volumeSlider;
    public Slider speedSlider;

    public GameObject instructions;
    public GameObject keys;
    public GameObject types;
    public GameObject toolInstructions;
    public GameObject menu;

    public bool test = true;

    public Text iForInstructions;

    public List<GameObject> subInstructions;

    public List<Image> icons;
    public List<GameObject> tools;
    public List<Image> arrow;
    public int toolSelected = 2;

    private float timer = 60*5;
    private int t = 5;
    int score = 0;

    int count = 0;
    bool negative;
    bool gameOver = false;
    bool end = false;

    bool xray = false;
    bool started = false;
    bool playing = false;
    VoxelRenderer tooth;

    bool pause = false;
        
    private void Awake()
    {
        GameObject.Find("Main Camera").GetComponent<SmoothMouseLook>().enabled = false;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 40;
        DisableUI();
        delayAmount = 30;
        soundDelay = 0;
        GetComponent<AudioSource>().volume = 0.5f;
        toolSelected = 0;
        toolDelay = 0;
        menu.SetActive(false);
    }

    public void Start()
    {
        instructions.SetActive(true);
        NextInstruction(0);
    }

    public void NextInstruction(int next)
    {
        for(int i = 0; i < subInstructions.Count; i++)
        {
            if (i == next)
                subInstructions[i].SetActive(true);
            else
                subInstructions[i].SetActive(false);
        }
    }

    public void StartGame()
    {
        if (started == false)
        {
            this.GetComponent<Testing>().start = true;
            Cursor.lockState = CursorLockMode.Locked;
            tooth = new VoxelRenderer();
            GameObject.Find("Main Camera").GetComponent<SmoothMouseLook>().enabled = true;
            GameObject.Find("Main Camera").GetComponent<SmoothMouseLook>().SetClamp(tooth.X, tooth.Y, tooth.Z);
            GameObject.Find("Main Camera").GetComponent<SmoothMouseLook>().SetStartPosition(tooth.X, tooth.Y, tooth.Z);
            EnableUI();
            playing = true;
            SetActiveTools();
            started = true;
            gameObject.GetComponent<AudioSource>().Play();
        }
        else
        {
            InstructionsToggle();
        }
    }

    void ShowOptions(bool show)
    {
        GameObject.Find("Main Camera").GetComponent<SmoothMouseLook>().enabled = !show;
        menu.SetActive(show);
        if (show) {
            Cursor.lockState = CursorLockMode.None;
            DisableUI();
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            EnableUI();
        }
    }

    void DisableUI()
    {
        timerText.enabled = false;
        scoreText.enabled = false;
        gameOverText.enabled = false;
        scoreDisplay.enabled = false;
        pointer.enabled = false;
        xRayBorder.enabled = false;
        for (int i = 0; i < icons.Count; i++)
        {
            tools[i].SetActive(false);
            icons[i].enabled = false;
        }
        highlight.enabled = false;
        arrow[0].enabled = false;
        arrow[1].enabled = false;
        highScores.SetActive(false);
        iForInstructions.enabled = false;

    }

    void EnableUI()
    {
        timerText.enabled = true;
        scoreText.enabled = true;
        pointer.enabled = true;
        gameOverText.enabled = false;
        scoreDisplay.enabled = false;
        instructions.SetActive(false);
        highlight.enabled = true;
        for(int i = 0; i < icons.Count; i++)
        {
            icons[i].enabled = true;
        }
        arrow[0].enabled = true;
        arrow[1].enabled = true;
        iForInstructions.enabled = true;
    }

    void SetActiveTools()
    {
        for (int i = 0; i < tools.Count; i++)
        {
            if (i != toolSelected)
            {
                tools[i].SetActive(false);
            }
            else
            {
                highlight.transform.position = icons[i].transform.position;
                tools[i].SetActive(true);
                toolSelected = i;
            }
        }
    }

    void SwitchTool(int tool)
    {
        if (toolSelected != tool)
        {
            toolSelected = tool;
            SetActiveTools();
        }
    }

    void AddScore()
    {
        Database db = new Database();
        db.AddScore(PlayerPrefs.GetString("username"), score, PlayerPrefs.GetInt("gameMode"));
    }

    void EndGame()
    {
        gameOver = true;
        playing = false;
        GetComponent<AudioSource>().Pause();
        GetComponent<AudioSource>().PlayOneShot(soundEffects[3]);

        tools[toolSelected].SetActive(false);
        PlayerPrefs.SetInt("score", score);
        GameObject.Find("Main Camera").GetComponent<SmoothMouseLook>().enabled = false;
        gameOverText.enabled = true;
        scoreDisplay.enabled = true;
        gameOverText.GetComponent<Animator>().Play("gameOverIn");
        highScores.SetActive(true);
        for(int i = 0; i < icons.Count; i++)
        {
            icons[i].enabled = false;
        }
        scoreText.enabled = false;
        timerText.enabled = false;
        AddScore();
        Cursor.lockState = CursorLockMode.None;
    }

    public void ChangeVolume()
    {
        this.gameObject.GetComponent<AudioSource>().volume = (volumeSlider.value - 1) * 0.25f;
    }

    public void ChangeSensitivity()
    {
        delayAmount = (5 - (int)speedSlider.value)*10;

    }

    public void switchScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    void MouseClicked()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2,Screen.height/2,0));

        if (pause != true && toolDelay==0)
        {
            //0 - drill
            if (toolSelected == 0)
            {
                if (Physics.Raycast(ray, out hit, 1000.0f))
                {
                    if (hit.transform.gameObject.tag == "tooth") //checks if tooth is toolDelay
                    {
                        pause = true;
                        int result = tooth.RemoveVoxel(hit, 0);
                        score += result;
                        if (soundDelay < 0)
                        {
                            this.GetComponent<AudioSource>().PlayOneShot(soundEffects[0]);
                            soundDelay = 6;
                        }

                        if (result == -1000)
                            t = 0;
                        pause = false;
                        toolDelay += delayAmount;
                    }
                }
            }
            //1 - polish
            else if (toolSelected == 1)
            {
                if (Physics.Raycast(ray, out hit, 1000.0f))
                {
                    if (hit.transform.gameObject.tag == "tooth") //checks if tooth is toolDelay
                    {
                        int result = tooth.RemoveVoxel(hit, 1); //removes voxel
                        score += result;
                        toolDelay += delayAmount;
                        if (result != 0)
                        {
                            if (soundDelay < 0)
                            {
                                this.GetComponent<AudioSource>().PlayOneShot(soundEffects[1]);
                                soundDelay = 6;
                            }
                        }
                    }
                }
            }
            //2 - filler
            else if (toolSelected == 2)
            {
                if (Physics.Raycast(ray, out hit, 1000.0f))
                {
                    if (hit.transform.gameObject.tag == "tooth") //checks if tooth is toolDelay
                    {
                        score += tooth.AddVoxel(hit); //adds voxel
                        toolDelay += delayAmount;
                        if (delayAmount == 0)
                        {
                            toolDelay += 5;
                        }
                        if (soundDelay < 0)
                        {
                            this.GetComponent<AudioSource>().PlayOneShot(soundEffects[2]);
                            soundDelay = 6;
                        }
                    }
                }
            }
        }
    }

    public void Menu()
    {
        playing = true;
        ShowOptions(false);
        EnableUI();
        SetActiveTools();
        GameObject.Find("Main Camera").GetComponent<SmoothMouseLook>().enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void InstructionsToggle()
    {
        instructionsOn = false;
        GameObject.Find("Main Camera").GetComponent<SmoothMouseLook>().enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        instructions.SetActive(false);
        NextInstruction(0);
        playing = true;
    }

    public bool instructionsOn;

    // Update is called once per frame
    void Update()
    {
        if (t == 0) //end screen out
        {
            scoreDisplay.text = score.ToString();
            playing = false;
            t = -1;
            DisableUI();
            EndGame();
        }else if (playing == true) //while playing game
        {
            soundDelay--;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                playing = false;
                ShowOptions(true);
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                tooth.XRayMode(true);
                xRayBorder.enabled = true;
                pointer.enabled = false;
                playing = false;
                xray = true;
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                instructionsOn = true;
                GameObject.Find("Main Camera").GetComponent<SmoothMouseLook>().enabled = false;
                Cursor.lockState = CursorLockMode.None;
                instructions.SetActive(true);
                NextInstruction(0);
                playing = false;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && playing == true)
            {
                if (toolSelected != icons.Count-1)
                {
                    SwitchTool(toolSelected + 1);
                }
                else
                {
                    SwitchTool(0);
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && playing == true)
            {
                if (toolSelected != 0)
                {
                    SwitchTool(toolSelected -1);
                }
                else
                {
                    SwitchTool(icons.Count-1);
                }
            }

            timer -= Time.deltaTime;
            t = ((int)timer);

            timerText.text = "TIMER: " + (t / 60) + ":" + (t % 60);
            scoreText.text = "SCORE: " + score;

            if (toolDelay == 0)
            {
                if (Input.GetMouseButton(0)) //left click is pressed
                {
                    MouseClicked();
                }
            }
            else
            {
                toolDelay--;
            }
        }
        else
        {
            soundDelay--;
            if (Input.GetKeyDown(KeyCode.Escape) && started == true && xray == false && instructionsOn==false && gameOver==false)
            {
                Menu();
            }
            if (Input.GetKeyDown(KeyCode.X) && started == true && xray == true && instructionsOn == false && gameOver == false)
            {
                tooth.XRayMode(false);
                xRayBorder.enabled = false;
                xray = false;
                playing = true;
                pointer.enabled = true;
                EnableUI();
            }
            else if (Input.GetKeyDown(KeyCode.I) && instructionsOn == true)
            {
                InstructionsToggle();
            }
        }
    }
}
