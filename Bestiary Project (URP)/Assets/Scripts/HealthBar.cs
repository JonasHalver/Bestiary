using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class HealthBar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Character character;
    public Image bar, background;
    public TextMeshProUGUI nameText, hitPoints;
    public Gradient healthbarGradient;
    private float maxWidth;
    private float t;
    public GameObject effectPrefab;
    public Sprite effectPositive, effectNegative;
    public Transform effectsDisplay;
    public List<HealthBarEffects> effects = new List<HealthBarEffects>();
    bool displayingDamageOrHeal = false;
    public HitPointDisplay hpd;
    private Color bgColor;
    public Color b, d;

    private void DeadRemoval(CombatManager.CombatTiming timing)
    {
        if (!character.alive && timing == CombatManager.CombatTiming.StartOfCombatStage)
        {
            Destroy(gameObject);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CombatManager.instance.currentStage == CombatManager.CombatStage.Setup)
        {
            character.HighlightAction();
        }
        CharacterSheet.ShowSheet(character);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CombatManager.instance.currentStage == CombatManager.CombatStage.Setup)
            CombatGrid.StopHighlight();
    }

    private void Start()
    {
        bgColor = background.color;
        maxWidth = bar.GetComponent<RectTransform>().rect.width;
        //nameText.text = character.stats.characterName;
        character.conditionManager.GainedCondition += DisplayEffect;
        character.conditionManager.LostCondition += RemoveEffect;
        character.Healed += Healed;
        character.TookDamage += TookDamage;
        CombatManager.RoundPhases += DeadRemoval;
    }
    private void OnDisable()
    {
        character.conditionManager.GainedCondition -= DisplayEffect;
        character.conditionManager.LostCondition -= RemoveEffect;
        character.Healed -= Healed;
        character.TookDamage -= TookDamage;
        CombatManager.RoundPhases -= DeadRemoval;
    }

    void Update()
    {
        t = Mathf.InverseLerp(0, character.stats.hitPoints, character.currentHitpoints);

        bar.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(0, maxWidth, t), bar.rectTransform.sizeDelta.y);
        bar.color = healthbarGradient.Evaluate(t);

        hitPoints.text = character.currentHitpoints + "/" + character.stats.hitPoints;

        string namestring = character.stats.characterType == CharacterStats.CharacterTypes.Adventurer ? character.stats.characterName : (character.entry.guess.characterName ?? "Unnamed Monster");
        nameText.text = namestring != "" ? namestring : "Unnamed Monster";
        //hpd.value = character.currentHitpoints;
        DisplayHitPoints();
        foreach(HealthBarEffects effect in effects)
        {
            effect.durationText.text = character.Conditions[effect.condition].ToString();
        }
    }
    
    public void DisplayHitPoints()
    {
        if (character.stats.characterType == CharacterStats.CharacterTypes.Adventurer)
        {
            hpd.value = character.stats.hitPoints;
            hpd.damageTaken = character.damageTaken;
        }
        else
        {
            CharacterStats guess = null;
            for (int i = 0; i < Book.monsterEntries.Count; i++)
            {
                if (Book.monsterEntries[i].origin.characterCode.Equals(character.stats.characterCode))
                {
                    guess = Book.monsterEntries[i].guess;
                }
            }
            hpd.value = guess.hitPoints;
            hpd.damageTaken = character.damageTaken;
        }
        
    }

    public void Healed()
    {
        if (!displayingDamageOrHeal)
            StartCoroutine(DisplayHeal());
    }

    public void TookDamage()
    {
        if (!displayingDamageOrHeal) StartCoroutine(DisplayDamage());
    }

    IEnumerator DisplayHeal()
    {
        displayingDamageOrHeal = true;
        background.color = Color.green;
        yield return new WaitForSeconds(0.5f);
        background.color = bgColor;
        displayingDamageOrHeal = false;
    }

    IEnumerator DisplayDamage()
    {
        displayingDamageOrHeal = true;
        background.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        background.color = bgColor;
        displayingDamageOrHeal = false;
    }

    public void DisplayEffect(Buff buff)
    {
        GameObject newEffect = Instantiate(effectPrefab, effectsDisplay);
        Image img = newEffect.transform.GetChild(0).GetComponent<Image>();
        img.sprite = buff.icon;
        img.color = buff.iconColor;
        Image bg = newEffect.GetComponent<Image>();
        bg.color = b;
        newEffect.GetComponent<SimpleTooltipSpawner>().tooltipString = buff.tooltipString;
        newEffect.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = (buff.durationRemaining+1).ToString();
        effects.Add(new HealthBarEffects(newEffect, newEffect.transform.GetChild(1).GetComponent<TextMeshProUGUI>(), buff));
    }

    public void DisplayEffect(Action.Condition condition)
    {
        GameObject newEffect = Instantiate(effectPrefab, effectsDisplay);
        Image img = newEffect.transform.GetChild(0).GetComponent<Image>();
        Image bg = newEffect.GetComponent<Image>();
        Icons.Properties icons = GameManager.instance.currentIconCollection.GetIcon(condition);
        string tooltip = GameManager.instance.currentTooltipCollection.GetString(condition);
        img.sprite = icons.icon;
        img.color = icons.iconColor;
        bg.sprite = Action.ConditionIsBuff[condition] ? effectPositive : effectNegative;
        //bg.color = d;
        bg.color = Action.ConditionIsBuff[condition] ? b : d;
        newEffect.GetComponent<SimpleTooltipSpawner>().tooltipString = tooltip;

        newEffect.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = character.Conditions[condition].ToString();
        effects.Add(new HealthBarEffects(newEffect, newEffect.transform.GetChild(1).GetComponent<TextMeshProUGUI>(), condition));
    }

    public void RemoveEffect(Action.Condition condition)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].condition == condition)
            {
                Destroy(effects[i].effect);
                effects.RemoveAt(i);
                i--;
            }
        }
    }

    public void RemoveEffect(Buff buff)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].buff == buff)
            {
                Destroy(effects[i].effect);
                effects.RemoveAt(i);
                i--;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerId == -1)
        {
            CharacterSheet.instance.ShowEntry(character);
        }
    }
}

public class HealthBarEffects
{
    public GameObject effect;
    public TextMeshProUGUI durationText;
    public Action.Condition condition;
    public Buff buff;
    public Debuff debuff;

    public HealthBarEffects(GameObject _effect, TextMeshProUGUI _text, Buff _buff)
    {
        effect = _effect;
        durationText = _text;
        buff = _buff;
    }
    public HealthBarEffects(GameObject _effect, TextMeshProUGUI _text, Debuff _debuff)
    {
        effect = _effect;
        durationText = _text;
        debuff = _debuff;
    }
    public HealthBarEffects(GameObject _effect, TextMeshProUGUI _text, Action.Condition _condition)
    {
        effect = _effect;
        durationText = _text;
        condition = _condition;
    }
}
