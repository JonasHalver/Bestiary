using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject menuCanvas;

    public GameObject gameOverPanel;
    public GameObject pauseMenu;
    public GameObject glossary;

    public bool enemiesWon, alliesWon;
    public static bool paused;

    public static event System.Action GamePaused;
    public bool debugMode = false;

    public List<Character> enemies = new List<Character>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        for (int i = 0; i < enemies.Count; i++)
        {
            bool flag = false;
            foreach(Entry entry in Book.monsterEntries)
            {
                if (entry.origin == enemies[i]) flag = true;
            }
            if (!flag) Book.monsterEntries.Add(new Entry(enemies[i]));
        }

        FirstActions();
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnNewSceneLoad;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnNewSceneLoad;
    }
    private void OnNewSceneLoad(Scene scene, LoadSceneMode mode)
    {
        FirstActions();
    }
    private void FirstActions()
    {
    }

    private void Update()
    {
        // Game State Check
        if (!debugMode)GameStateCheck();
        if (enemiesWon || alliesWon) GameOver();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Pause();
        }
    }

    public void GameStateCheck()
    {
        int alliesAlive = 0;
        int enemiesAlive = 0;
        foreach(Character actor in CombatManager.actors)
        {
            if (actor.alive)
            {
                switch (actor.stats.characterType)
                {
                    case CharacterStats.CharacterTypes.NPC:
                        enemiesAlive++;
                        break;
                    case CharacterStats.CharacterTypes.Adventurer:
                        alliesAlive++;
                        break;
                }
            }
        }
        if (alliesAlive > 0 && enemiesAlive == 0) alliesWon = true;
        else if (enemiesAlive > 0 && alliesAlive == 0) enemiesWon = true;
    }

    public void Pause()
    {
        glossary.SetActive(false);
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        paused = pauseMenu.activeSelf;
        GamePaused.Invoke();
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Glossary()
    {
        glossary.SetActive(!glossary.activeSelf);
    }

    public void GameOver()
    {
        TextMeshProUGUI winText = gameOverPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        winText.text = alliesWon ? "You Win!" : "You Lose!";
        gameOverPanel.SetActive(true);
    }

    public void Restart()
    {
        paused = false;
        CombatManager.instance.StopAllCoroutines();
        SceneManager.LoadScene(0);
    }
}
