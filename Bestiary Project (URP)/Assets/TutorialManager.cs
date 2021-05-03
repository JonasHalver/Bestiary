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

    public int tutMain1Index = 0;
    public int tutMain2Index = 0;
    public int tutMain3Index = 0;
    public int tutMain4Index = 0;
    public int tutBestiaryIndex = 0;
    public int tutActionIndex = 0;

    public List<string> tutorialMain1 = new List<string>();
    public List<int> unskippableMain1 = new List<int>();
    public List<string> tutorialMain2 = new List<string>();
    public List<int> unskippableMain2 = new List<int>();
    public List<string> tutorialMain3 = new List<string>();
    public List<int> unskippableMain3 = new List<int>();
    public List<string> tutorialMain4 = new List<string>();
    public List<int> unskippableMain4 = new List<int>();
    public List<string> tutorialBestiary = new List<string>();
    public List<int> unskippableBestiary = new List<int>();
    public List<string> tutorialAction = new List<string>();
    public List<int> unskippableAction = new List<int>();

    public static bool allowBestiary = false;
    public static bool allowCombat = false;

    public static event System.Action ShowGrid;
    public static event System.Action ShowAlly;
    public static event System.Action ShowEnemy;
    public static event System.Action ShowLogAndInitiative;
    public static event System.Action ShowBestiary;
    public static event System.Action StartCombat;
    public enum TutorialSequence { Main1, Main2, Main3, Main4, BestiaryMonster, ActionEditing, Standalone, Done }
    public TutorialSequence currentSequence = TutorialSequence.Main1;
    private TutorialSequence activeSequence = TutorialSequence.Main1;
    private bool bestiaryOpened, actionEditingOpened, bestiaryClosed, actionEditorClosed;
    private List<string> openedStandalones = new List<string>();

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
        //gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        InputManager.Escape += Continue;
        GameManager.OpenedBestiary += OpenedBestiary;
        GameManager.ClosedBestiary += ClosedBestiary;
        ActionEditor.ActionEditorOpened += OpenedActionEditor;
        ActionEditor.ActionEditorClosed += ClosedActionEditor;
    }
    private void OnDisable()
    {
        InputManager.Escape -= Continue;
        GameManager.OpenedBestiary -= OpenedBestiary;
        GameManager.ClosedBestiary -= ClosedBestiary;
        ActionEditor.ActionEditorOpened -= OpenedActionEditor;
        ActionEditor.ActionEditorClosed -= ClosedActionEditor;
    }
    private void Update()
    {
        if (TutorialMask.active && (!TutorialTextBox.mouseOverText && !TutorialMask.mouseOverMask)) clickBlocker.SetActive(true);
        else clickBlocker.SetActive(false);
    }

    public void Continue()
    {
        switch (currentSequence)
        {
            case TutorialSequence.Main1:
                if (!unskippableMain1.Contains(tutMain1Index))
                {
                    print($"skipped {tutMain1Index}");
                    tutorialTextBox.interrupt = true;
                    tutorialMask.interrupt = true;
                    tutMain1Index++;
                }
                else return;
                break;
            case TutorialSequence.Main2:
                if (!unskippableMain2.Contains(tutMain2Index))
                {
                    tutorialTextBox.interrupt = true;
                    tutorialMask.interrupt = true;
                    tutMain2Index++;
                }
                else return;
                break;
            case TutorialSequence.Main3:
                if (!unskippableMain3.Contains(tutMain3Index))
                {
                    tutorialTextBox.interrupt = true;
                    tutorialMask.interrupt = true;
                    tutMain3Index++;
                }
                else return;
                break;
            case TutorialSequence.Main4:
                if (!unskippableMain4.Contains(tutMain4Index))
                {
                    tutorialTextBox.interrupt = true;
                    tutorialMask.interrupt = true;
                    tutMain4Index++;
                }
                else return;
                break;
            case TutorialSequence.BestiaryMonster:
                if (!unskippableBestiary.Contains(tutBestiaryIndex))
                {
                    tutorialTextBox.interrupt = true;
                    tutorialMask.interrupt = true;
                    tutBestiaryIndex++;
                }
                else return;
                break;
            case TutorialSequence.ActionEditing:
                if (!unskippableAction.Contains(tutActionIndex))
                {
                    tutorialTextBox.interrupt = true;
                    tutorialMask.interrupt = true;
                    tutActionIndex++;
                }
                else return;
                break;
            case TutorialSequence.Standalone:
                HideTutorial();
                return;
        }
        TutorialStateMachine();
    }
    public void ForceContinue(bool interrupt)
    {
        tutorialTextBox.interrupt = interrupt;
        tutorialMask.interrupt = interrupt;
        switch (currentSequence)
        {
            case TutorialSequence.Main1:
                tutMain1Index++;
                break;
            case TutorialSequence.Main2:
                tutMain2Index++;
                break;
            case TutorialSequence.Main3:
                tutMain3Index++;
                break;
            case TutorialSequence.Main4:
                tutMain4Index++;
                break;
            case TutorialSequence.BestiaryMonster:
                tutBestiaryIndex++;
                break;
            case TutorialSequence.ActionEditing:
                tutActionIndex++;
                break;
            case TutorialSequence.Standalone:
                currentSequence = activeSequence;
                ForceContinue(interrupt);
                return;
        }
        TutorialStateMachine();
    }
    public void StandaloneTutorial(string key)
    {
        if (openedStandalones.Contains(key))
        {
            HideTutorial();
            return;
        }
        openedStandalones.Add(key);
        currentSequence = TutorialSequence.Standalone;
        NextTutorial(key, false, true);
    }   
    public void EndStandalone()
    { 
        currentSequence = activeSequence;
        ForceContinue(true);
        
    }
    public void OpenedBestiary()
    {
        if (bestiaryOpened) return;
        bestiaryOpened = true;
        currentSequence = TutorialSequence.BestiaryMonster;
        activeSequence = TutorialSequence.BestiaryMonster;
        TutorialStateMachine();
    }
    public void ClosedBestiary()
    {
        if (bestiaryClosed) return;
        bestiaryClosed = true;
        currentSequence = TutorialSequence.Main1;
        activeSequence = TutorialSequence.Main1;
        ForceContinue(true);
    }
    public void OpenedActionEditor()
    {
        if (actionEditingOpened) return;
        actionEditingOpened = true;
        currentSequence = TutorialSequence.ActionEditing;
        activeSequence = TutorialSequence.ActionEditing;
        TutorialStateMachine();
    }
    public void ClosedActionEditor()
    {
        if (actionEditorClosed) return;
        actionEditorClosed = true;
        currentSequence = TutorialSequence.BestiaryMonster;
        activeSequence = TutorialSequence.BestiaryMonster;
        ForceContinue(true);
    }

    public void TutorialStateMachine()
    {
        if (!GameManager.tutorial) return;
        switch (currentSequence)
        {
            case TutorialSequence.Main1:
                Main1Sequence(tutMain1Index);
                break;
            case TutorialSequence.Main2:
                break;
            case TutorialSequence.Main3:
                break;
            case TutorialSequence.Main4:
                break;
            case TutorialSequence.BestiaryMonster:
                BestiarySequence(tutBestiaryIndex);
                break;
            case TutorialSequence.ActionEditing:
                ActionEditingSequence(tutActionIndex);
                break;
        }
        #region old solution
        /*switch (tutorialIndex)
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
        }*/
        #endregion
    }

    public void Main1Sequence(int index)
    {
        switch (index)
        {
            case 0:
                tutorialTextBox.Fade(true);
                NextTutorial(tutorialMain1[index], false, true);
                break;
            case 1:
                NextTutorial(tutorialMain1[index], false, true);
                break;
            case 2:
                HideTutorial();
                ShowGrid.Invoke();
                StartCoroutine(Delay(1));
                break;
            case 3:
                ShowAlly.Invoke();
                StartCoroutine(Delay(1));
                break;
            case 4:
                tutorialTextBox.Fade(true);
                NextTutorial(tutorialMain1[index], false, true);
                break;
            case 5:
                HideTutorial();
                ShowEnemy.Invoke();
                StartCoroutine(Delay(1));
                break;
            case 6:
                tutorialTextBox.Fade(true);
                NextTutorial(tutorialMain1[index], false, true);
                break;
            case 7:
                NextTutorial(tutorialMain1[index], false, true);
                break;
            case 8:
                StartCombat.Invoke();
                allowCombat = true;
                NextTutorial(tutorialMain1[index], false, false);
                break;
            case 9:
                NextTutorial(tutorialMain1[index], false, true);
                ShowLogAndInitiative.Invoke();
                break;
            case 10:
                GameManager.ChangeState(GameManager.GameState.PauseCombat);
                break;
            case 11:
                NextTutorial(tutorialMain1[index], true, false);
                break;
            case 12:
                NextTutorial(tutorialMain1[index], false, true);
                allowBestiary = true;
                break;
            case 13:
                NextTutorial(tutorialMain1[index], true, false);
                allowBestiary = true;
                ShowBestiary.Invoke();
                break;
            case 14:
                GameManager.ChangeState(GameManager.GameState.Normal);
                NextTutorial(tutorialMain1[index], false, false);
                break;
        }
    }
    private void BestiarySequence(int index)
    {
        switch (index)
        {
            case 0:
                NextTutorial(tutorialBestiary[index], false, true);
                break;
            case 1:
                NextTutorial(tutorialBestiary[index], false, true);
                break;
            case 2:
                NextTutorial(tutorialBestiary[index], true, true);
                break;
            case 3:
                NextTutorial(tutorialBestiary[index], true, true);
                break;
            case 4:
                NextTutorial(tutorialBestiary[index], true, true);
                break;
            case 5:
                NextTutorial(tutorialBestiary[index], true, true);
                break;
            case 6:
                NextTutorial(tutorialBestiary[index], true, true);
                break;
            case 7:
                NextTutorial(tutorialBestiary[index], true, false);
                break;
            case 8:
                NextTutorial(tutorialBestiary[index], true, false);
                break;
            case 9:
                NextTutorial(tutorialBestiary[index], false, false);
                break;
        }
    }
    private void ActionEditingSequence(int index)
    {
        switch (index)
        {
            case 0:
                NextTutorial(tutorialAction[index], false, true);
                break;
            case 1:
                NextTutorial(tutorialAction[index], true, true);
                break;
            case 2:
                NextTutorial(tutorialAction[index], true, true);
                break;
            case 3:
                NextTutorial(tutorialAction[index], true, true);
                break;
            case 4:
                NextTutorial(tutorialAction[index], true, true);
                break;
            case 5:
                NextTutorial(tutorialAction[index], true, true);
                break;
            case 6:
                NextTutorial(tutorialAction[index], true, true);
                break;
            case 7:
                NextTutorial(tutorialAction[index], true, true);
                break;
            case 8:
                NextTutorial(tutorialAction[index], true, true);
                break;
            case 9:
                NextTutorial(tutorialAction[index], true, true);
                break;
            case 10:
                NextTutorial(tutorialAction[index], true, true);
                break;
            case 11:
                NextTutorial(tutorialAction[index], false, false);
                break;
        }
    }
    private void NextTutorial(string part, bool highlight, bool allowPlayerToContinue)
    {
        if (!GameManager.tutorial) return;
        if (!tutorialTextBox.active) tutorialTextBox.Fade(true);
        tutorialTextBox.MoveBox(part, allowPlayerToContinue);
        if (highlight)
        {
            tutorialMask.Fade(true);
            tutorialMask.MoveMask(part);
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
        switch (currentSequence)
        {
            case TutorialSequence.Main1:
                tutMain1Index++;
                break;
            case TutorialSequence.Main2:
                tutMain2Index++;
                break;
            case TutorialSequence.Main3:
                tutMain3Index++;
                break;
            case TutorialSequence.Main4:
                tutMain4Index++;
                break;
            case TutorialSequence.BestiaryMonster:
                tutBestiaryIndex++;
                break;
            case TutorialSequence.ActionEditing:
                tutActionIndex++;
                break;
        }
        TutorialStateMachine();
    }
}
