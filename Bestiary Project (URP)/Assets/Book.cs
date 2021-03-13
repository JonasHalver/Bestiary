using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Book : MonoBehaviour
{
    public static Book instance;
    public static List<Entry> monsterEntries;
    public static Entry currentEntry;
    public int pageNumber = 0;
    public ActionDescription descriptionsList;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            // Replace this part when a save/load system works
            monsterEntries = new List<Entry>();

            OnLoadActions(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        else if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
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

    private void Update()
    {
        if (monsterEntries.Count < 0) currentEntry = monsterEntries[pageNumber];
    }
}
