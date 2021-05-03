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

    //public GameObject gameOverPanel;
    //public GameObject pauseMenu;
    //public GameObject glossary;

    public bool enemiesWon, alliesWon;
    public static bool paused;

    public static event System.Action GamePaused;
    public bool debugMode = false;

    public List<CharacterStats> enemies = new List<CharacterStats>();
    public List<CharacterStats> activeMercenaries = new List<CharacterStats>();
    public List<CharacterStats> mercenaries = new List<CharacterStats>();
    public List<CombatEncounter> combatEncounters = new List<CombatEncounter>();
    public List<CombatEncounter> tutorialEncounters = new List<CombatEncounter>();
    public int tutorialProgress = 0;
    public Icons currentIconCollection;
    public Tooltips currentTooltipCollection;
    public Log logElementCollection;

    public static List<int> seed;

    public enum GameState { Normal, PauseMenu, PauseCombat, Bestiary, Glossary}
    public static GameState gameState = GameState.Normal;
    public GameState gamestateDisplay;
    public static bool combatPaused = false;
    public GameObject journal;
    private bool flag = false;
    public bool combatStartSequence = false;
    public static bool bookFilled, actorsSpawned;

    public static GameObject focusedWindow;
    public static List<GameObject> openWindows = new List<GameObject>();

    public static bool textInput;
    public static bool tutorial = true;

    public bool startInTutorial = true;
    public static event System.Action OpenedBestiary;
    public static event System.Action ClosedBestiary;
    public static event System.Action Victory;
    public static event System.Action Defeat;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        seed = new List<int>();
        for (int i = 0; i < 1000; i++)
        {
            seed.Add(Random.Range(0, 10));
        }

        //FirstActions();
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnNewSceneLoad;
        InputManager.Escape += Escape;
        InputManager.OpenGlossary += OpenGlossary;
        InputManager.OpenJournal += OpenBestiary;
        InputManager.Commit += PauseCombat;
        TutorialManager.StartCombat += CombatStartSequence;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnNewSceneLoad;
        InputManager.Escape -= Escape;
        InputManager.OpenGlossary -= OpenGlossary;
        InputManager.OpenJournal -= OpenBestiary;
        InputManager.Commit -= PauseCombat;
        TutorialManager.StartCombat -= CombatStartSequence;
    }
    private void OnNewSceneLoad(Scene scene, LoadSceneMode mode)
    {
        gameState = GameState.Normal;
        alliesWon = false; enemiesWon = false;
        combatStartSequence = false;
        actorsSpawned = false;
        FirstActions();
    }
    public void CombatStartSequence()
    {

        if (!bookFilled)
        {
              
        }
        else
        {
            if (!actorsSpawned)
            {
                CombatManager.instance.SpawnAllies();
                CombatManager.instance.SpawnEnemies();
                CombatManager.instance.SetInitiative();
            }
            else
                combatStartSequence = true;
        }
    }
    private void FirstActions()
    {
        tutorial = startInTutorial;

        if (tutorial)
        {
            activeMercenaries.Add(mercenaries[tutorialProgress]);
            for (int i = 0; i < tutorialEncounters[tutorialProgress].enemies.Count; i++)
            {
                enemies.Add(tutorialEncounters[tutorialProgress].enemies[i]);
            }
            tutorialProgress++;
        }
        else
        {
            if (activeMercenaries.Count < 4)
            {
                activeMercenaries = mercenaries;
            }
            for (int i = 0; i < combatEncounters[0].enemies.Count; i++)
            {
                enemies.Add(combatEncounters[0].enemies[i]);
            }
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            bool flag = false;
            foreach (Entry entry in Book.monsterEntries)
            {
                if (entry.origin == enemies[i]) flag = true;
            }
            if (!flag)
            {
                Entry n = new Entry(enemies[i]);
                Book.monsterEntries.Add(n);
                enemies[i].entry = n;
            }
        }
        for (int i = 0; i < activeMercenaries.Count; i++)
        {
            bool flag = false;
            foreach (Entry entry in Book.mercEntries)
            {
                if (entry.origin == activeMercenaries[i]) flag = true;
            }
            if (!flag)
            {
                Entry n = new Entry(activeMercenaries[i]);
                Book.mercEntries.Add(n);

                activeMercenaries[i].entry = n;
                n.isMerc = true;
            }
        }
        Book.instance.FillBook();
    }

    private void Update()
    {
        if (combatStartSequence)
        {
            if (!debugMode) GameStateCheck();
            if (enemiesWon || alliesWon) GameOver();
            GameStateMachine();
        }
        gamestateDisplay = gameState;
        UpdateWindowsList();
    }

    public void UpdateWindowsList()
    {
        for (int i = 0; i < openWindows.Count; i++)
        {
            if (!openWindows[i].activeSelf)
            {
                openWindows.RemoveAt(i);
                i--;
            }
        }
        if (focusedWindow != null)
        {
            if (!focusedWindow.activeSelf)
            {
                if (openWindows.Count > 0)
                {
                    focusedWindow = openWindows[openWindows.Count - 1];
                }
                else focusedWindow = null;
            }
        }
    }
    public void PauseCombat()
    {
        if (tutorial) return;
        if (CombatManager.instance.currentStage != CombatManager.CombatStage.Setup)
        {
            if (gameState == GameState.Normal)
            {
                combatPaused = true;
                ChangeState(GameState.PauseCombat);
            }
            else if (gameState == GameState.PauseCombat)
            {
                combatPaused = false;
                ChangeState(GameState.Normal);
            }
        }
    }
    public static void ChangeState(GameState newState)
    {
        instance.flag = false;
        if (newState == GameState.Normal && combatPaused) newState = GameState.PauseCombat;
        //if (CombatManager.instance.currentStage != CombatManager.CombatStage.Setup && newState != GameState.Normal) combatPaused = true;
        if (tutorial && gameState == GameState.Bestiary)
        {
            ClosedBestiary.Invoke();
        }
        gameState = newState;
        if (tutorial && TutorialManager.allowBestiary && newState == GameState.Bestiary)
        {
            //TutorialManager.instance.ForceContinue(true);
            instance.GameStateMachine();
        }
        
    }

    public void GameStateMachine()
    {
        switch (gameState)
        {
            case GameState.Normal:
                if (!flag)
                {
                    Book.instance.OpenPages(false, Book.instance.pageNumber);
                    MenuUI.PauseMenu.SetActive(false);
                    MenuUI.Glossary.SetActive(false);
                    flag = true;
                }
                break;
            case GameState.PauseMenu:
                if (!flag)
                {
                    Book.instance.OpenPages(false, Book.instance.pageNumber);
                    MenuUI.PauseMenu.SetActive(true);
                    MenuUI.Glossary.SetActive(false);
                    flag = true;
                }
                break;
            case GameState.PauseCombat:
                if (!flag)
                {
                    Book.instance.OpenPages(false, Book.instance.pageNumber);
                    MenuUI.PauseMenu.SetActive(false);
                    MenuUI.Glossary.SetActive(false);
                    flag = true;
                }
                break;
            case GameState.Bestiary:
                if (!flag)
                {
                    OpenedBestiary.Invoke();
                    Book.instance.OpenPages(true, Book.instance.pageNumber);
                    MenuUI.PauseMenu.SetActive(false);
                    MenuUI.Glossary.SetActive(false);
                    flag = true;
                }
                break;
            case GameState.Glossary:
                if (!flag)
                {
                    Book.instance.OpenPages(false, Book.instance.pageNumber);
                    MenuUI.PauseMenu.SetActive(false);
                    MenuUI.Glossary.SetActive(true);
                    flag = true;
                }
                break;
        }
    }

    public void Escape()
    {
        if (tutorial) return;
        if(focusedWindow == null)
        {
            if (gameState != GameState.Normal)
            {
                ChangeState(GameState.Normal);
            }
            else
                OpenPauseMenu();
        }
        else
        {
            CloseFocusedWindow();
        }
    }

    public void CloseFocusedWindow()
    {
        focusedWindow.SendMessage("CloseWindow");
        focusedWindow.SetActive(false);
        openWindows.Remove(focusedWindow);
        if (openWindows.Count > 0) focusedWindow = openWindows[openWindows.Count-1];
        else focusedWindow = null;
    }

    public void CloseAllWindows()
    {
        if (focusedWindow != null)
        {
            focusedWindow.SetActive(false);
            for (int i = 0; i < openWindows.Count; i++)
            {
                openWindows[i].SetActive(false);
                openWindows.RemoveAt(i);
                i--;
            }
        }
    }
    private void OpenPauseMenu()
    {

        if (gameState != GameState.PauseMenu)
        {
            ChangeState(GameState.PauseMenu);
        }
        else
        {
            ChangeState(GameState.Normal);
        }
        CloseAllWindows();

    }
    private void OpenBestiary()
    {
        if (gameState != GameState.Bestiary)
        {
            ChangeState(GameState.Bestiary);
        }
        else
        {
            ChangeState(GameState.Normal);
        }
        CloseAllWindows();
    }
    private void OpenGlossary()
    {
        if (gameState != GameState.Glossary) ChangeState(GameState.Glossary);
        else ChangeState(GameState.Normal);
        CloseAllWindows();

    }
    public void ExitGame()
    {
        Application.Quit();
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

    private void Pause()
    {
        if (gameState != GameState.PauseMenu)
            ChangeState(GameState.PauseMenu);
        else ChangeState(GameState.Normal);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void GameOver()
    {
        TextMeshProUGUI winText = MenuUI.GameOver.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        winText.text = alliesWon ? "You Win!" : "You Lose!";
        MenuUI.GameOver.SetActive(true);
        if (alliesWon) Victory.Invoke();
        else Defeat.Invoke();
    }

    public void Restart()
    {
        CombatManager.instance.StopAllCoroutines();
        SceneManager.LoadScene(0);
    }

    public static Sprite Icon(Character.DamageTypes damageType)
    {
        Sprite icon = null;

        icon = instance.currentIconCollection.GetIcon(damageType).icon;

        return icon;
    }
}
