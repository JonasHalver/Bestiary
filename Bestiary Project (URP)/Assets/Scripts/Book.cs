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
    public int pageNumber = 0;
    public ActionDescription descriptionsList;
    public static GraphicRaycaster GR;
    public GameObject pagePrefab;
    public List<Page> monsterPages = new List<Page>();
    public List<Page> mercPages = new List<Page>();
    public GameObject actionCanvas;
    public GameObject statCanvas;

    public static event System.Action StatsUpdated;
    public static bool openOnMerc = false;

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
                GameObject newPage = Instantiate(pagePrefab, monsterBinding);
                Page p = newPage.GetComponent<Page>();
                monsterPages.Add(p);
                p.entry = monsterEntries[i];
                monsterEntries[i].page = p;
                monsterEntries[i].origin.pageNumber = i;
                newPage.SetActive(false);
                p.entry.CreateChecks();
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

    public void OpenPages(bool open, int page)
    {
        binding.gameObject.SetActive(open);
        mercBinding.SetActive(openOnMerc);
        monsterBinding.gameObject.SetActive(!openOnMerc);
        pageNumber = page;
        PageChange();
    }

    private void Update()
    {
        if (!GameManager.bookFilled) return;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { pageNumber--; PageChange(); }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { pageNumber++; PageChange(); }
    }

    public static void OpenActionEditing()
    {
        for (int i = 0; i < instance.actionCanvas.transform.childCount; i++)
        {
            instance.actionCanvas.transform.GetChild(i).gameObject.SetActive(false);
        }
        instance.actionCanvas.SetActive(true);
        instance.actionCanvas.transform.GetChild(0).gameObject.SetActive(true);
        instance.actionCanvas.transform.GetChild(1).gameObject.SetActive(true);
        GameManager.openWindows.Add(instance.actionCanvas);
        GameManager.focusedWindow = instance.actionCanvas;
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
    public void EditingText(bool editing)
    {
        GameManager.textInput = editing;
    }
    public void PageChange()
    {
        Page openPage = null;
        if (pageNumber < 0) pageNumber = (openOnMerc ? mercPages.Count : monsterPages.Count) - 1;
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
        if (openPage != null) openPage.UpdateName();
    }
    public void OpenMercs(bool check)
    {
        openOnMerc = check;
        mercBinding.SetActive(openOnMerc);
        monsterBinding.gameObject.SetActive(!openOnMerc);
        PageChange();
    }
}
