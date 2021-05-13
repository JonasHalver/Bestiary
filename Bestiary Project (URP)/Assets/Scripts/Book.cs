using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Book : MonoBehaviour
{
    public Transform binding;
    public GameObject mercBinding;
    public Transform monsterBinding;
    public static Book instance;
    public static List<Entry> monsterEntries;
    public static List<Entry> mercEntries;
    public static Entry currentEntry;
    public int pageNumberTOC = 0;
    public int pageNumberMonsters = 0;
    public int pageNumberMercs = 0;
    public int pageNumberGlossary = 0;
    public int pageNumberCurrent = 0;
    public int pageNumberSettings = 0;
    public ActionDescription descriptionsList;
    public static GraphicRaycaster GR;
    public GameObject pagePrefab;
    public List<Page> monsterPages = new List<Page>();
    public List<Page> mercPages = new List<Page>();
    public GameObject actionCanvas;
    public GameObject statCanvas;
    public GameObject editWindowPrefab;

    public GameObject toc, current, monsters, mercenaries, glossary, settings;

    public static event System.Action StatsUpdated;
    public static bool openOnMerc = false;
    public static event System.Action EntryShown;

    public enum Chapter { TableOfContents, Current, Monsters, Mercenaries, Glossary, Settings }
    public Chapter currentChapter = Chapter.TableOfContents;
    public static event System.Action ChapterChanged;

    public int ActivePageNumber
    {
        get
        {
            switch (currentChapter)
            {
                default: return 0;
                case Chapter.TableOfContents:
                    return pageNumberTOC;
                case Chapter.Current:
                    return pageNumberCurrent;
                case Chapter.Monsters:
                    return pageNumberMonsters;
                case Chapter.Mercenaries:
                    return pageNumberMercs;
                case Chapter.Glossary:
                    return pageNumberGlossary;
                case Chapter.Settings:
                    return pageNumberSettings;
            }
        }
        set
        {
            switch (currentChapter)
            {
                case Chapter.TableOfContents:
                    pageNumberTOC = value;
                    break;
                case Chapter.Current:
                    pageNumberCurrent=value;
                    break;
                case Chapter.Monsters:
                    pageNumberMonsters = value;
                    break;
                case Chapter.Mercenaries:
                    pageNumberMercs = value;
                    break;
                case Chapter.Glossary:
                    pageNumberGlossary = value;
                    break;
                case Chapter.Settings:
                    pageNumberSettings = value;
                    break;
            }
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            GR = GetComponent<GraphicRaycaster>();
            // Replace this part when a save/load system works
            monsterEntries = new List<Entry>();
            mercEntries = new List<Entry>();
            OnLoadActions(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        else if (instance != null && instance != this)
        {
            Destroy(transform.parent.gameObject);
        }
        DontDestroyOnLoad(transform.parent.gameObject);
    }
    private void Start()
    {
        
    }
    public void FillBook()
    {
        for (int i = 0; i < monsterEntries.Count; i++)
        {
            if (monsterEntries[i].page == null)
            {
                // For Monsters chapter
                GameObject newPage = Instantiate(pagePrefab, monsterBinding);
                Page p = newPage.GetComponent<Page>();
                monsterPages.Add(p);
                p.entry = monsterEntries[i];
                p.chapter = Chapter.Monsters;
                p.pageNumber = i;
                monsterEntries[i].page = p;
                monsterEntries[i].origin.pageNumber = i;
                newPage.SetActive(false);

                // For Current chapter
               /* GameObject newPage1 = Instantiate(pagePrefab, current.transform);
                Page p1 = newPage1.GetComponent<Page>();
                monsterPages.Add(p1);
                p1.entry = monsterEntries[i];
                p1.chapter = Chapter.Current;
                p1.entry.origin.pageNumberCurrent = current.transform.childCount - 1;
                newPage1.SetActive(false);
                p1.pageNumber = current.transform.childCount-1;*/
                
                p.entry.CreateChecks();
            }
            else
            {
                monsterEntries[i].page.entry = monsterEntries[i];
            }
        }
        for (int i = 0; i < mercEntries.Count; i++)
        {
            if(mercEntries[i].page == null)
            {
                GameObject newPage = Instantiate(pagePrefab, mercBinding.transform);
                Page p = newPage.GetComponent<Page>();
                mercPages.Add(p);
                newPage.SetActive(false);
                p.entry = mercEntries[i];
                mercEntries[i].page = p;
                mercEntries[i].origin.pageNumber = i;
                // update the stats here
                p.entry.CreateChecksMerc();
            }
        }
        GameManager.bookFilled = true;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLoadActions;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLoadActions;
    }

    public void OnLoadActions(Scene scene, LoadSceneMode mode)
    {
    }

    public static void UpdateStats()
    {
        StatsUpdated.Invoke();
    }

    public void OpenBook()
    {        
        binding.gameObject.SetActive(true);
        ChapterChange((int)currentChapter);
        PageChange();
    }

    public void OpenPages(bool open, int page)
    {
        binding.gameObject.SetActive(open);
        mercBinding.SetActive(openOnMerc);
        monsterBinding.gameObject.SetActive(!openOnMerc);
        //pageNumber = page;
        PageChange();
    }

    private void Update()
    {
        if (!GameManager.bookFilled) return;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { ActivePageNumber--; PageChange(); }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { ActivePageNumber++; PageChange(); }
    }

    public static void OpenActionEditing()
    {
        //for (int i = 0; i < instance.actionCanvas.transform.childCount; i++)
        //{
        //    instance.actionCanvas.transform.GetChild(i).gameObject.SetActive(false);
        //}
        //instance.actionCanvas.SetActive(true);
        //instance.actionCanvas.transform.GetChild(0).gameObject.SetActive(true);
        //instance.actionCanvas.transform.GetChild(1).gameObject.SetActive(true);

        GameObject newEditWindow = Instantiate(instance.editWindowPrefab);
        ActionEditor ae = newEditWindow.GetComponent<ActionEditor>();
        ae.action = currentEntry.activeAction.originalAction;
        ae.guessAction = currentEntry.activeAction.guessAction;

        GameManager.openWindows.Add(newEditWindow);
        GameManager.focusedWindow = newEditWindow;
    }
    public static void OpenStatEditing(Entry.StatEntries stat)
    {
        GameObject canvas = instance.statCanvas;
        GameObject s = canvas.transform.GetChild((int)stat).gameObject;
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            canvas.transform.GetChild(i).gameObject.SetActive(false);
        }
        s.SetActive(true);
        canvas.SetActive(true);
        GameManager.openWindows.Add(canvas);
        GameManager.openWindows.Add(s);
        GameManager.focusedWindow = s;
    }
    public void CloseEditingWindow()
    {
        statCanvas.SetActive(false);
        TutorialManager.instance.EndStandalone();
    }
    public void EditingText(bool editing)
    {
        GameManager.textInput = editing;
    }

    public void ChapterChange(int chapter)
    {
        currentChapter = (Chapter)chapter;
        if (GameManager.tutorial && !TutorialManager.showedBestiary && (currentChapter == Chapter.Current || currentChapter == Chapter.Monsters)) EntryShown.Invoke();
        switch (currentChapter)
        {
            case Chapter.TableOfContents:
                toc.SetActive(true);
                current.SetActive(false);
                monsters.SetActive(false);
                mercenaries.SetActive(false);
                glossary.SetActive(false);
                settings.SetActive(false);
                break;
            case Chapter.Current:
                toc.SetActive(false);
                current.SetActive(true);
                monsters.SetActive(false);
                mercenaries.SetActive(false);
                glossary.SetActive(false);
                settings.SetActive(false);
                break;
            case Chapter.Monsters:
                toc.SetActive(false);
                current.SetActive(false);
                monsters.SetActive(true);
                mercenaries.SetActive(false);
                glossary.SetActive(false);
                settings.SetActive(false);
                break;
            case Chapter.Mercenaries:
                toc.SetActive(false);
                current.SetActive(false);
                monsters.SetActive(false);
                mercenaries.SetActive(true);
                glossary.SetActive(false);
                settings.SetActive(false);
                break;
            case Chapter.Glossary:
                toc.SetActive(false);
                current.SetActive(false);
                monsters.SetActive(false);
                mercenaries.SetActive(false);
                glossary.SetActive(true);
                settings.SetActive(false);
                break;
            case Chapter.Settings:
                toc.SetActive(false);
                current.SetActive(false);
                monsters.SetActive(false);
                mercenaries.SetActive(false);
                glossary.SetActive(false);
                settings.SetActive(true);
                break;
        }
        PageChange();
        ChapterChanged.Invoke();
    }
    public void FlipPage(bool forward)
    {
        ActivePageNumber += forward ? 1 : -1;
        PageChange();
    }
    public void PageChange()
    {
        Page openPage = null;

        switch (currentChapter)
        {
            case Chapter.TableOfContents:
                if (ActivePageNumber > 0)
                {
                    ActivePageNumber = 0;
                    currentChapter = Chapter.Monsters;
                    ActivePageNumber = 0;
                    ChapterChange((int)currentChapter);
                    return;
                }
                else if (ActivePageNumber < 0)
                {
                    ActivePageNumber = 0;
                    currentChapter = Chapter.Settings;
                    ActivePageNumber = 0;
                    ChapterChange((int)currentChapter);
                    return;
                }
                break;
            case Chapter.Current:
                if (ActivePageNumber >= current.transform.childCount)
                {
                    ActivePageNumber = 0;
                    currentChapter = Chapter.Monsters;
                    ActivePageNumber = 0;
                    ChapterChange((int)currentChapter);
                    return;
                }
                else if (ActivePageNumber < 0)
                {
                    ActivePageNumber = 0;
                    currentChapter = Chapter.TableOfContents;
                    ActivePageNumber = 0;
                    ChapterChange((int)currentChapter);
                    return;
                }
                foreach (Transform child in current.transform)
                {
                    child.gameObject.SetActive(false);
                }
                current.transform.GetChild(ActivePageNumber).gameObject.SetActive(true);
                break;
            case Chapter.Monsters:
                if (ActivePageNumber >= monsters.transform.childCount)
                {
                    ActivePageNumber = 0;
                    currentChapter = Chapter.Mercenaries;
                    ActivePageNumber = 0;
                    ChapterChange((int)currentChapter);
                    return;
                }
                else if (ActivePageNumber < 0)
                {
                    ActivePageNumber = 0;
                    currentChapter = Chapter.TableOfContents;
                    ActivePageNumber = 0;
                    ChapterChange((int)currentChapter);
                    return;
                }
                foreach (Transform child in monsters.transform)
                {
                    child.gameObject.SetActive(false);
                }
                monsters.transform.GetChild(ActivePageNumber).gameObject.SetActive(true);
                break;
            case Chapter.Mercenaries:
                if (ActivePageNumber >= mercenaries.transform.childCount)
                {
                    ActivePageNumber = 0;
                    currentChapter = Chapter.Glossary;
                    ActivePageNumber = 0;
                    ChapterChange((int)currentChapter);
                    return;
                }
                else if (ActivePageNumber < 0)
                {
                    ActivePageNumber = 0;
                    currentChapter = Chapter.Monsters;
                    ActivePageNumber = monsters.transform.childCount - 1;
                    ChapterChange((int)currentChapter);
                    return;
                }
                foreach (Transform child in mercenaries.transform)
                {
                    child.gameObject.SetActive(false);
                }
                mercenaries.transform.GetChild(ActivePageNumber).gameObject.SetActive(true);
                break;
            case Chapter.Glossary:
                if (ActivePageNumber >= glossary.transform.childCount)
                {
                    ActivePageNumber = 0;
                    currentChapter = Chapter.Settings;
                    ActivePageNumber = 0;
                    ChapterChange((int)currentChapter);
                    return;
                }
                else if (ActivePageNumber < 0)
                {
                    ActivePageNumber = 0;
                    currentChapter = Chapter.Mercenaries;
                    ActivePageNumber = mercenaries.transform.childCount - 1;
                    ChapterChange((int)currentChapter);
                    return;
                }
                foreach (Transform child in glossary.transform)
                {
                    child.gameObject.SetActive(false);
                }
                glossary.transform.GetChild(ActivePageNumber).gameObject.SetActive(true);
                break;
            case Chapter.Settings:
                if (ActivePageNumber > 0)
                {
                    ActivePageNumber = 0;
                    currentChapter = Chapter.TableOfContents;
                    ActivePageNumber = 0;
                    ChapterChange((int)currentChapter);
                    return;
                }
                else if (ActivePageNumber < 0)
                {
                    ActivePageNumber = 0;
                    currentChapter = Chapter.Glossary;
                    ActivePageNumber = glossary.transform.childCount - 1;
                    ChapterChange((int)currentChapter);
                    return;
                }
                break;
        }

        /*if (pageNumber < 0) pageNumber = (openOnMerc ? mercPages.Count : monsterPages.Count) - 1;
        else if (pageNumber >= (openOnMerc ? mercPages.Count : monsterPages.Count)) pageNumber = 0;
        if (!openOnMerc)
        {
            if (monsterEntries.Count > 0)
            {
                currentEntry = monsterEntries[pageNumber];
                for (int i = 0; i < monsterPages.Count; i++)
                {
                    if (i != pageNumber) monsterPages[i].gameObject.SetActive(false);
                    else { monsterPages[i].gameObject.SetActive(true); openPage = monsterPages[i]; }
                }
            }
        }
        else
        {
            if (mercEntries.Count > 0)
            {
                currentEntry = mercEntries[pageNumber];
                for (int i = 0; i < mercPages.Count; i++)
                {
                    if (i != pageNumber) mercPages[i].gameObject.SetActive(false);
                    else { mercPages[i].gameObject.SetActive(true); openPage = mercPages[i]; }
                }
            }
        }
        if (openPage != null) openPage.UpdateName();*/
    }
    public void OpenMercs(bool check)
    {
        openOnMerc = check;
        mercBinding.SetActive(openOnMerc);
        monsterBinding.gameObject.SetActive(!openOnMerc);
        PageChange();
    }
    public void Restart()
    {
        GameManager.instance.Restart();
    }
    public void Exit()
    {
        GameManager.instance.Exit();
    }
    public void CloseBook()
    {
        binding.gameObject.SetActive(false);
        if (GameManager.gameState == GameManager.GameState.Bestiary)
            GameManager.ChangeState(GameManager.GameState.Normal);
    }
}
