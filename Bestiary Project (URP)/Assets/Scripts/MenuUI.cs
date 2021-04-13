using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public static MenuUI instance;

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject glossary;
    [SerializeField] private GameObject gameover;

    public static GameObject PauseMenu
    {
        get
        {
            return instance.pauseMenu;
        }
    }
    public static GameObject Glossary
    {
        get { return instance.glossary; }
    }
    public static GameObject GameOver
    {
        get { return instance.gameover; }
    }
    public void Restart()
    {
        GameManager.instance.Restart();
    }
    private void Awake()
    {
        instance = this;
    }

    public void OpenGlossary()
    {
        GameManager.instance.OpenGlossary();
    }
    public void OpenPauseMenu()
    {
        GameManager.instance.Pause();
    }
    public void OpenJournal()
    {
        GameManager.instance.OpenJournal();
    }

    public void Exit()
    {
        GameManager.instance.Exit();
    }
}
