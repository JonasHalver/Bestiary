using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BookActionTarget : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TMP_Dropdown position, nearCount, shape, minimumHits, targetStatus, targetPriority;
    public string titleString;
    private bool loading = false;

    private void OnEnable()
    {
        StartCoroutine(CardLoaded());
    }
    public void SetInfoFromGuess()
    {
        Action action = Book.currentEntry.activeAction.guessAction;

        switch (action.position)
        {
            case Action.Position.Alone:
                position.value = 1;
                break;
            case Action.Position.NearAlly:
                position.value = 2;
                break;
            case Action.Position.NotNearAlly:
                position.value = 4;
                break;
            case Action.Position.NearEnemy:
                position.value = 3;
                break;
            case Action.Position.NotNearEnemy:
                position.value = 5;
                break;
            case Action.Position.Irrelevant:
                position.value = 0;
                break;
        }

        nearCount.value = action.nearTargetCount - 1;
        switch (action.shape)
        {
            case Action.Shape.Single:
                if (action.canHitSelf) shape.value = 0;
                else
                {
                    if (action.targetConditions[0] == Action.Status.InMelee) shape.value = 1;
                    else shape.value = 2;
                }
                break;
            case Action.Shape.Arc:
                shape.value = 3;
                break;
            case Action.Shape.Cone:
                shape.value = 4;
                break;
            case Action.Shape.ThreeByThree:
                if (!action.canHitSelf) shape.value = 5;
                else shape.value = 6;
                break;
            case Action.Shape.Line:
                shape.value = 7;
                break;
            case Action.Shape.All:
                shape.value = 8;
                break;
        }
        minimumHits.value = action.minimumHits - 1;
        switch (action.targetConditions[0])
        {
            case Action.Status.Irrelevant:
                targetStatus.value = 0;
                break;
            case Action.Status.Below50:
                targetStatus.value = 1;
                break;
            case Action.Status.Above50:
                targetStatus.value = 2;
                break;
            case Action.Status.InMelee:
                targetStatus.value = 3;
                break;
            case Action.Status.NotInMelee:
                targetStatus.value = 4;
                break;
        }
        switch (action.targetPriority)
        {
            case Action.TargetPriority.LowestHPCurrent:
                targetPriority.value = 0;
                break;
            case Action.TargetPriority.lowestHPPercent:
                targetPriority.value = 1;
                break;
            case Action.TargetPriority.HighestHPCurrent:
                targetPriority.value = 2;
                break;
            case Action.TargetPriority.HighestHPPercent:
                targetPriority.value = 3;
                break;
            case Action.TargetPriority.Closest:
                targetPriority.value = 4;
                break;
            case Action.TargetPriority.Farthest:
                targetPriority.value = 5;
                break;
            case Action.TargetPriority.HasSameDebuff:
                targetPriority.value = 6;
                break;
            case Action.TargetPriority.DoesntHaveSameDebuff:
                targetPriority.value = 7;
                break;
            case Action.TargetPriority.None:
                targetPriority.value = 8;
                break;
        }
    }
    private void Update()
    {
        string titleText = titleString;
        string cname = Book.currentEntry.guess.characterName != null ? Book.currentEntry.guess.characterName : "the monster";

        titleText = titleText.Replace("*", cname);
        title.text = titleText;
    }

    public void ResetValues(bool hard)
    {
        loading = true;
        position.value = 0; nearCount.value = 0; shape.value = 0; minimumHits.value = 0; targetStatus.value = 0; targetPriority.value = 0;
        loading = false;
        if (hard)
        {
            ValueChanged();
            Book.currentEntry.activeAction.guessAction.targetingSet = false;
        }
    }

    IEnumerator CardLoaded()
    {
        loading = true;

        ResetValues(false);
        loading = true;
        yield return null;
        SetInfoFromGuess();
        yield return null;
        SetPosition();
        yield return null;
        SetNearCount();
        yield return null;
        SetShape();
        yield return null;
        SetMinimumHits();
        yield return null;
        SetTargetStatus();
        yield return null;
        SetTargetPriority();
        yield return null;
        Book.currentEntry.activeAction.guessAction.targetingSet = true;
        BookActionCard.CardUpdate();
        loading = false;
    }

    public void ValueChanged()
    {
        if (loading) return;
        SetPosition();
        SetNearCount();
        SetShape();
        SetMinimumHits();
        SetTargetStatus();
        SetTargetPriority();
        //Book.currentEntry.activeAction.CalculateValidity();
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
                action.target = Action.Targeting.Character;
                if (targetStatus.value > 2)
                {
                    targetStatus.value = 0;
                    action.targetConditions[0] = Action.Status.Irrelevant;
                }
                break;
            case 1:
                action.shape = Action.Shape.Single;
                action.canHitSelf = false;
                action.target = Action.Targeting.Character;
                if (action.targetConditions[0] == Action.Status.Irrelevant)
                {
                    targetStatus.value = 3;
                    action.targetConditions[0] = Action.Status.InMelee;
                }
                minimumHits.value = 0;
                break;
            case 2:
                action.shape = Action.Shape.Single;
                action.canHitSelf = false;
                action.target = Action.Targeting.Character;
                if (action.targetConditions[0] == Action.Status.InMelee)
                {
                    targetStatus.value = 0;
                    action.targetConditions[0] = Action.Status.Irrelevant;
                }
                minimumHits.value = 0;
                break;
            case 3:
                action.shape = Action.Shape.Arc;
                action.canHitSelf = false;
                action.target = Action.Targeting.Character;
                break;
            case 4:
                action.shape = Action.Shape.Cone;
                action.canHitSelf = false;
                action.target = Action.Targeting.Character;
                break;
            case 5:
                action.shape = Action.Shape.Area;
                action.canHitSelf = false;
                action.target = Action.Targeting.Character;
                break;
            case 6:
                action.shape = Action.Shape.Pulse;
                action.canHitSelf = true;
                action.target = Action.Targeting.Character;
                break;
            case 7:
                action.shape = Action.Shape.Line;
                action.canHitSelf = false;
                action.target = Action.Targeting.Character;
                break;
            case 8:
                action.shape = Action.Shape.All;
                //action.target = Action.Target.All;
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
        if (minimumHits.value > 1 && shape.value == 1) shape.value = 3;
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
                if (shape.value != 1 && shape.value != 3) shape.value = 1;
                break;
            case 4:
                action.targetConditions.Add(Action.Status.NotInMelee);
                if (shape.value == 1) shape.value = 2;
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
