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

    public List<CharacterStats> enemies = new List<CharacterStats>();
    public List<CharacterStats> mercenaries = new List<CharacterStats>();
    public List<CombatEncounter> combatEncounters = new List<CombatEncounter>();
    public Icons currentIconCollection;

    public enum GameState { Normal, PauseMenu, Journal}
    public static GameState gameState = GameState.Normal;
    public GameObject journal;
    private bool flag = false;
    private bool combatStartSequence = false;
    public static bool bookFilled, actorsSpawned;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        

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
    public void CombatStartSequence()
    {
        if (!bookFilled)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                bool flag = false;
                foreach (Entry entry in Book.monsterEntries)
                {
                    if (entry.origin == enemies[i]) flag = true;
                }
                if (!flag) Book.monsterEntries.Add(new Entry(enemies[i]));
            }
            Book.instance.FillBook();   
        }
        else
        {
            if (!actorsSpawned)
            {
                CombatManager.instance.StartCombat(0);
            }
            else
                combatStartSequence = true;
        }
    }
    private void FirstActions()
    {
    }

    private void Update()
    {
        if (!combatStartSequence) CombatStartSequence();
        else
        {
            if (!debugMode) GameStateCheck();
        }
        if (enemiesWon || alliesWon) GameOver();

        GameStateMachine();

    }

    public static void ChangeState(GameState newState)
    {
        instance.flag = false;
        gameState = newState;
    }

    public void GameStateMachine()
    {
        switch (gameState)
        {
            case GameState.Normal:
                if (!flag)
                {
                    Book.instance.OpenPages(false, Book.instance.pageNumber);
                    pauseMenu.SetActive(false);
                    flag = true;
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    flag = false;
                    gameState = GameState.PauseMenu;
                }
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    flag = false;
                    gameState = GameState.Journal;
                }
                break;
            case GameState.PauseMenu:
                if (!flag)
                {
                    Book.instance.OpenPages(false, Book.instance.pageNumber);
                    pauseMenu.SetActive(true);
                    flag = true;
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    flag = false;
                    gameState = GameState.Normal;
                }
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    flag = false;
                    gameState = GameState.Journal;
                }
                break;
            case GameState.Journal:
                if (!flag)
                {
                    Book.instance.OpenPages(true, Book.instance.pageNumber);
                    pauseMenu.SetActive(false);
                    flag = true;
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    flag = false;
                    gameState = GameState.Normal;
                }
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    flag = false;
                    gameState = GameState.Normal;
                }
                break;
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
    public void Journal()
    {
        journal.SetActive(true);
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

    public static Sprite Icon(Character.DamageTypes damageType)
    {
        Sprite icon = null;
        switch (damageType)
        {
            case Character.DamageTypes.Cutting:
                icon = instance.currentIconCollection.cutting;
                break;
            case Character.DamageTypes.Piercing:
                icon = instance.currentIconCollection.piercing;
                break;
            case Character.DamageTypes.Crushing:
                icon = instance.currentIconCollection.crushing;
                break;
            case Character.DamageTypes.Fire:
                icon = instance.currentIconCollection.fire;
                break;
            case Character.DamageTypes.Cold:
                icon = instance.currentIconCollection.cold;
                break;
            case Character.DamageTypes.Acid:
                icon = instance.currentIconCollection.acid;
                break;
            case Character.DamageTypes.Poison:
                icon = instance.currentIconCollection.cutting;
                break;
        }
        return icon;
    }
}
