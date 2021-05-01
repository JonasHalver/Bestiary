using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;
    public TutorialTextBox tutorialTextBox;
    public TutorialMask tutorialMask;

    public int tutorialIndex = 0;
    public List<string> tutorialParts = new List<string>();
    public static bool allowBestiary = false;
    public static bool allowCombat = false;

    public static event System.Action ShowGrid;
    public static event System.Action ShowAlly;
    public static event System.Action ShowEnemy;
    public static event System.Action ShowLogAndInitiative;
    public static event System.Action ShowBestiary;
    public static event System.Action StartCombat;

    public GameObject clickBlocker;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        if (GameManager.tutorial)
            TutorialStateMachine();
        else StartCoroutine(ShowEverything());
    }
    IEnumerator ShowEverything()
    {
        yield return null;
        ShowGrid.Invoke();
        yield return new WaitForSeconds(0.5f);
        ShowAlly.Invoke();
        yield return new WaitForSeconds(0.1f);
        ShowEnemy.Invoke();
        yield return new WaitForSeconds(0.1f);
        StartCombat.Invoke();
        yield return new WaitForSeconds(0.5f);
        ShowBestiary.Invoke();
        yield return new WaitForSeconds(0.5f);
        ShowLogAndInitiative.Invoke();
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        InputManager.Escape += Continue;
    }
    private void OnDisable()
    {
        InputManager.Escape -= Continue;
    }
    private void Update()
    {
        if (TutorialMask.active && (!TutorialTextBox.mouseOverText && !TutorialMask.mouseOverMask)) clickBlocker.SetActive(true);
        else clickBlocker.SetActive(false);
    }

    public void Continue()
    {
        switch (tutorialIndex)
        {
            default:
                tutorialTextBox.interrupt = true;
                tutorialMask.interrupt = true;
                tutorialIndex++;
                TutorialStateMachine();
                break;
            case 3:
            case 4:
            case 10:
            case 15:
            case 23:
            case 24:
                break;
        }        
    }
    public void ForceContinue(bool interrupt)
    {
        tutorialTextBox.interrupt = interrupt;
        tutorialMask.interrupt = interrupt;
        tutorialIndex++;
        TutorialStateMachine();
    }

    public void TutorialStateMachine()
    {
        switch (tutorialIndex)
        {
            case 0:
                tutorialTextBox.Fade(true);
                NextTutorial(tutorialParts[tutorialIndex], false, true);
                break;
            case 1:
                NextTutorial(tutorialParts[tutorialIndex], false, true);
                break;
            case 2:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 3:
                HideTutorial();
                ShowGrid.Invoke();
                StartCoroutine(Delay(1));
                break;
            case 4:
                ShowAlly.Invoke();
                StartCoroutine(Delay(1));
                break;
            case 5:
                tutorialTextBox.Fade(true);
                NextTutorial(tutorialParts[tutorialIndex], false, true);
                break;
            case 6:
                NextTutorial(tutorialParts[tutorialIndex], false, true);
                break;
            case 7:
                HideTutorial();
                ShowEnemy.Invoke();
                StartCoroutine(Delay(1));
                break;
            case 8:
                tutorialTextBox.Fade(true);
                NextTutorial(tutorialParts[tutorialIndex], false, true);
                break;
            case 9:
                NextTutorial(tutorialParts[tutorialIndex], false, true);
                break;
            case 10:
                StartCombat.Invoke();
                allowCombat = true;
                NextTutorial(tutorialParts[tutorialIndex], false, false);
                break;
            case 11:
                NextTutorial(tutorialParts[tutorialIndex], false, true);
                ShowLogAndInitiative.Invoke();
                break;
            case 12:
                GameManager.ChangeState(GameManager.GameState.PauseCombat);
                break;
            case 13:
                NextTutorial(tutorialParts[tutorialIndex], true, false);                
                break;
            case 14:                
                NextTutorial(tutorialParts[tutorialIndex], false, true);
                allowBestiary = true;
                break;
            case 15:
                NextTutorial(tutorialParts[tutorialIndex], true, false);
                allowBestiary = true;
                ShowBestiary.Invoke();
                break;
                // Bestiary
            case 16:
                NextTutorial(tutorialParts[tutorialIndex], false, true);
                break;
            case 17:
                NextTutorial(tutorialParts[tutorialIndex], false, true);
                break;
            case 18:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 19:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 20:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 21:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 22:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 23:
                NextTutorial(tutorialParts[tutorialIndex], true, false);
                break;
            case 24:
                NextTutorial(tutorialParts[tutorialIndex], true, false);
                break;
                // Action Editor
            case 25:
                NextTutorial(tutorialParts[tutorialIndex], false, true);
                break;
            case 26:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 27:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 28:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 29:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 30:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 31:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 32:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 33:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 34:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 35:
                NextTutorial(tutorialParts[tutorialIndex], true, true);
                break;
            case 36:
                NextTutorial(tutorialParts[tutorialIndex], false, false);
                break;
        }
    }

    private void NextTutorial(string part, bool highlight, bool allowPlayerToContinue)
    {
        tutorialTextBox.MoveBox(tutorialParts[tutorialIndex], allowPlayerToContinue);
        if (highlight)
        {
            tutorialMask.Fade(true);
            tutorialMask.MoveMask(tutorialParts[tutorialIndex]);
        }
        else
        {
            tutorialMask.Fade(false);
        }
    }
    private void HideTutorial()
    {
        tutorialTextBox.Fade(false);
        tutorialMask.Fade(false);
    }
    IEnumerator Delay(float amount)
    {
        float t = 0;
        while (t < amount)
        {
            t += Time.deltaTime;
            yield return null;
        }
        tutorialIndex++;
        TutorialStateMachine();
    }
}
