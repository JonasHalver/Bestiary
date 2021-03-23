using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CombatLogCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject topLine, botLine;
    public TextMeshProUGUI userName, affected, victimNameText, xCharacters, effectText, andText, buffText;
    public Image userIcon, victimIcon, effectIcon, buffIcon;

    public CombatAction ca;
    private Entry entry, victimEntry;
    public Color enemy, ally;
    private Character user, victim;

    public static bool displayingPast = false;
    private int t = 0;
    private void Update()
    {
        if (t > 10) UpdateCanvas();
        t++;
    }

    public void CreateCard() 
    {
        user = ca.origin;
        victim = ca.affectedCharacters.Count == 1 ? ca.affectedCharacters[0] : null;
        string username;
        if (user.stats.characterType == CharacterStats.CharacterTypes.NPC)
        {
            GetComponent<Image>().color = enemy;
            for (int i = 0; i < Book.monsterEntries.Count; i++)
            {
                if (Book.monsterEntries[i].origin.characterCode.Equals(user.stats.characterCode))
                {
                    entry = Book.monsterEntries[i];
                    break;
                }
            }
            username = entry.guess.characterName;
            userName.text = username;
            userIcon.sprite = user.stats.characterIcon;
            userIcon.color = user.stats.characterIconColor;
        }
        else
        {
            GetComponent<Image>().color = ally;
            username = user.stats.characterName;
            userName.text = username;
            userIcon.sprite = user.stats.characterIcon;
            userIcon.color = user.stats.characterIconColor;
        }
        switch (ca.action.actionType)
        {
            case Action.ActionType.Attack:
                affected.text = "hit";
                andText.gameObject.SetActive(false);
                buffIcon.gameObject.SetActive(false);
                buffText.gameObject.SetActive(false);

                effectIcon.sprite = GameManager.Icon(ca.action.damageType);
                effectText.text = ca.action.damageType.ToString() + " damage";
                break;
            case Action.ActionType.AttackDebuff:
                affected.text = "hit";
                buffIcon.sprite = ca.action.debuff.icon;
                buffIcon.color = ca.action.debuff.iconColor;
                buffText.text = ca.action.debuff.debuffName;

                effectIcon.sprite = GameManager.Icon(ca.action.damageType);
                effectText.text = ca.action.damageType.ToString() + " damage";
                break;
            case Action.ActionType.Buff:
                affected.text = "affected";
                andText.gameObject.SetActive(false);
                buffIcon.sprite = ca.action.buff.icon;
                buffIcon.color = ca.action.buff.iconColor;
                buffText.text = ca.action.buff.buffName;

                effectIcon.gameObject.SetActive(false);
                effectText.gameObject.SetActive(false);
                break;
            case Action.ActionType.Debuff:
                affected.text = "affected";
                andText.gameObject.SetActive(false);
                buffIcon.sprite = ca.action.debuff.icon;
                buffIcon.color = ca.action.debuff.iconColor;
                buffText.text = ca.action.debuff.debuffName;

                effectIcon.gameObject.SetActive(false);
                effectText.gameObject.SetActive(false);
                break;
            case Action.ActionType.Healing:
                affected.text = "affected";
                andText.gameObject.SetActive(false);
                buffIcon.gameObject.SetActive(false);
                buffText.gameObject.SetActive(false);

                effectIcon.sprite = GameManager.Icon(Character.DamageTypes.Healing);
                effectText.text = "healing";
                break;
            case Action.ActionType.HealingBuff:
                affected.text = "affected";
                buffIcon.sprite = ca.action.buff.icon;
                buffIcon.color = ca.action.buff.iconColor;
                buffText.text = ca.action.buff.buffName;

                effectIcon.sprite = GameManager.Icon(Character.DamageTypes.Healing);
                effectText.text = "healing";
                break;
        }

        if (victim == null)
        {
            
            victimIcon.gameObject.SetActive(false);
            victimNameText.gameObject.SetActive(false);
            xCharacters.text = $"{ca.affectedCharacters.Count.ToString()} characters";
        }
        else
        {
            string victimName;
            if (victim.stats.characterType == CharacterStats.CharacterTypes.Adventurer)
            {
                victimName = victim.stats.characterName;
            }
            else
            {
                for (int i = 0; i < Book.monsterEntries.Count; i++)
                {
                    if (Book.monsterEntries[i].origin.characterCode.Equals(victim.stats.characterCode))
                    {
                        victimEntry = Book.monsterEntries[i];
                        break;
                    }
                }
                victimName = victimEntry.guess.characterName;
            }
            victimNameText.text = victimName;
            victimIcon.sprite = victim.stats.characterIcon;
            victimIcon.color = victim.stats.characterIconColor;
            xCharacters.gameObject.SetActive(false);
        }
        UpdateCanvas();
    }

    public void UpdateCanvas()
    {
        t = 0;
        Canvas.ForceUpdateCanvases();
        topLine.SetActive(true);
        topLine.GetComponent<HorizontalLayoutGroup>().enabled = false;
        topLine.GetComponent<HorizontalLayoutGroup>().enabled = true;

        botLine.SetActive(true);
        botLine.GetComponent<HorizontalLayoutGroup>().enabled = false;
        botLine.GetComponent<HorizontalLayoutGroup>().enabled = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        
        displayingPast = false;
        StartCoroutine(ExitDelay());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CombatManager.instance.currentStage != CombatManager.CombatStage.Setup) return;
        displayingPast = true;
        CombatGrid.ShowPreviousPositions(ca.bpi, user, ca.affectedCharacters);
    }

    IEnumerator ExitDelay()
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            if (displayingPast)
            {
                break;
            }
            yield return null;
        }
        if (!displayingPast)
        {
            CombatGrid.ShowCurrentPositions();

        }
    }
}
