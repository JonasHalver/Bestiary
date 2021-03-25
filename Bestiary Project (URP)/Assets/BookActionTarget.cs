using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BookActionTarget : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TMP_Dropdown position, nearCount, shape, minimumHits, targetStatus, targetPriority;

    private void OnEnable()
    {

        SetPosition();
        SetNearCount();
        SetShape();
        SetMinimumHits();
        SetTargetStatus();
        SetTargetPriority();
        Book.currentEntry.activeAction.guessAction.targetingSet = true;
    }
    private void Update()
    {
        string titleText = title.text;
        string replacement = Book.currentEntry.guess.characterName != null ? Book.currentEntry.guess.characterName : "the monster";
        titleText = titleText.Replace("*", replacement);
        title.text = titleText;

    }
    public void ValueChanged()
    {
        SetPosition();
        SetNearCount();
        SetShape();
        SetMinimumHits();
        SetTargetStatus();
        SetTargetPriority();
        Book.currentEntry.activeAction.CalculateValidity();
        Book.currentEntry.activeAction.guessAction.targetingSet = true;
        BookActionCard.CardUpdate();
    }

    public void SetPosition()
    {
        Action action = Book.currentEntry.activeAction.guessAction;

        switch (position.value)
        {
            case 0:
                action.position = Action.Position.Irrelevant;
                break;
            case 1:
                action.position = Action.Position.Alone;
                break;
            case 2:
                action.position = Action.Position.NearAlly;
                break;
            case 3:
                action.position = Action.Position.NearEnemy;
                break;
            case 4:
                action.position = Action.Position.NotNearAlly;
                break;
            case 5:
                action.position = Action.Position.NotNearEnemy;
                break;
        }
        if (action.position == Action.Position.Alone || action.position == Action.Position.NotNearAlly || action.position == Action.Position.NotNearEnemy || action.position == Action.Position.Irrelevant)
        {
            nearCount.interactable = false;
            nearCount.value = 0;
        }
        else nearCount.interactable = true;
    }
    public void SetNearCount()
    {
        Action action = Book.currentEntry.activeAction.guessAction;
        action.nearTargetCount = nearCount.value + 1;
    }
    public void SetShape()
    {
        Action action = Book.currentEntry.activeAction.guessAction;
        switch (shape.value)
        {
            case 0:
                action.shape = Action.Shape.Single;
                action.canHitSelf = true;
                break;
            case 1:
                action.shape = Action.Shape.Single;
                action.canHitSelf = false;
                break;
            case 2:
                action.shape = Action.Shape.Arc;
                action.canHitSelf = false;
                break;
            case 3:
                action.shape = Action.Shape.Cone;
                action.canHitSelf = false;
                break;
            case 4:
                action.shape = Action.Shape.ThreeByThree;
                action.canHitSelf = false;
                break;
            case 5:
                action.shape = Action.Shape.ThreeByThree;
                action.canHitSelf = true;
                break;
            case 6:
                action.shape = Action.Shape.Line;
                action.canHitSelf = false;
                break;
        }
        if (action.shape == Action.Shape.Single)
        {
            minimumHits.interactable = false;
            minimumHits.value = 0;
            action.minimumHits = 1;
            targetStatus.interactable = true;
        }
        else
        {
            minimumHits.interactable = true;
            targetStatus.interactable = false;
            targetStatus.value = 0;
        }
        if (action.canHitSelf || shape.value != 1)
        {
            targetPriority.interactable = false;
            targetPriority.value = targetPriority.options.Count-1;
        }
        else
        {
            targetPriority.interactable = true;
        }
    }
    public void SetMinimumHits()
    {
        Action action = Book.currentEntry.activeAction.guessAction;
        action.minimumHits = minimumHits.value + 1;
    }
    public void SetTargetStatus()
    {

        Action action = Book.currentEntry.activeAction.guessAction;
        action.targetConditions.Clear();
        switch (targetStatus.value)
        {
            case 0:
                action.targetConditions.Add(Action.Status.Irrelevant);
                break;
            case 1:
                action.targetConditions.Add(Action.Status.Below50);
                break;
            case 2:
                action.targetConditions.Add(Action.Status.Above50);
                break;
            case 3:
                action.targetConditions.Add(Action.Status.InMelee);
                break;
            case 4:
                action.targetConditions.Add(Action.Status.NotInMelee);
                break;
        }
    }
    public void SetTargetPriority()
    {
        Action action = Book.currentEntry.activeAction.guessAction;
        switch (targetPriority.value)
        {
            case 0:
                action.targetPriority = Action.TargetPriority.LowestHPCurrent;
                break;
            case 1:
                action.targetPriority = Action.TargetPriority.lowestHPPercent;
                break;
            case 2:
                action.targetPriority = Action.TargetPriority.HighestHPCurrent;
                break;
            case 3:
                action.targetPriority = Action.TargetPriority.HighestHPPercent;
                break;
            case 4:
                action.targetPriority = Action.TargetPriority.Closest;
                break;
            case 5:
                action.targetPriority = Action.TargetPriority.Farthest;
                break;
            case 6:
                action.targetPriority = Action.TargetPriority.HasSameDebuff;
                break;
            case 7:
                action.targetPriority = Action.TargetPriority.DoesntHaveSameDebuff;
                break;
            case 8:
                action.targetPriority = Action.TargetPriority.None;
                break;
        }
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
