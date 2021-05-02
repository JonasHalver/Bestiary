using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUI : MonoBehaviour
{
    public Canvas canvas;
    private Animator canvasAnim;
    public static CombatUI instance;
    public TextMeshProUGUI stageDisplay, stageInfo, combatLog, roundDisplay;

    public Button commitButton;

    private void Awake()
    {
        instance = this;
        canvas = transform.parent.GetComponent<Canvas>();
        canvasAnim = canvas.GetComponent<Animator>();
    }

    private void OnEnable()
    {
        //TutorialManager.ShowGrid += ShowGrid;
        TutorialManager.ShowLogAndInitiative += ShowLog;
        TutorialManager.StartCombat += ShowCommit;
    }
    private void OnDisable()
    {
       // TutorialManager.ShowGrid -= ShowGrid;
        TutorialManager.ShowLogAndInitiative -= ShowLog;
        TutorialManager.StartCombat -= ShowCommit;
    }
    public void OpenInitiative()
    {
        if (GameManager.tutorial)
        {
            //TutorialManager.instance.Continue();
        }
    }
    public void OpenCombatLog()
    {
        if (GameManager.tutorial)
        {
            TutorialManager.instance.ForceContinue(true);
        }
    }

    public void Commit()
    {
        CombatManager.instance.Commit();
    }
    public void ShowGrid()
    {
        canvasAnim.SetTrigger("GridMoveIn");
    }
    public void ShowLog()
    {
        canvasAnim.SetTrigger("MoveIn");
    }
    public void ShowCommit()
    {
        canvasAnim.SetTrigger("FadeIn");
        commitButton.enabled = true;
    }
}
