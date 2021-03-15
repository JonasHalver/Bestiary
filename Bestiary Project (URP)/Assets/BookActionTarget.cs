using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BookActionTarget : MonoBehaviour
{
    public TextMeshProUGUI title;
    public Dropdown position, nearCount, shape, minimumHits, targetStatus, targetPriority;

    private void OnEnable()
    {
        string titleText = title.text;
        string replacement = Book.currentEntry.guess.stats.characterName != "" ? Book.currentEntry.guess.stats.characterName : "the monster";
        titleText.Replace("*", replacement);
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
        }
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
