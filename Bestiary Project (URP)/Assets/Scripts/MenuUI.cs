using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    public static MenuUI instance;
    private Animator uiAnim;
    public Animator UIAnim
    {
        get 
        { 
            if (uiAnim == null) uiAnim = GetComponent<Animator>(); 
            return uiAnim; 
        }
    }

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject glossary;
    [SerializeField] private GameObject gameover;
    [SerializeField] private GameObject generalUI;

    [SerializeField] private Button bestiaryButton;
    [SerializeField] private Button glossaryButton;
    [SerializeField] private Button settingsButton;

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
    private void OnEnable()
    {
        TutorialManager.ShowBestiary += ShowBestiary;
        //TutorialManager.ShowLogAndInitiative += LowerTopBar;
    }
    private void OnDisable()
    {
        TutorialManager.ShowBestiary -= ShowBestiary;
        //TutorialManager.ShowLogAndInitiative -= LowerTopBar; 
    }

    public void OpenGlossary()
    {
        GameManager.ChangeState(GameManager.GameState.Glossary);
    }
    public void OpenPauseMenu()
    {
        GameManager.ChangeState(GameManager.GameState.PauseMenu);
    }
    public void OpenJournal()
    {
        GameManager.ChangeState(GameManager.GameState.Bestiary);
    }

    public void Exit()
    {
        GameManager.instance.Exit();
    }

    private void LowerTopBar()
    {
        UIAnim.SetTrigger("FadeIn");
    }

    private void ShowBestiary()
    {
        bestiaryButton.enabled = true;
        UIAnim.SetTrigger("FadeIn");
    }
}
