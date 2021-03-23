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
    public Transform effectsDisplay;
    public List<HealthBarEffects> effects = new List<HealthBarEffects>();
    bool displayingDamageOrHeal = false;
    public HitPointDisplay hpd;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CombatManager.instance.currentStage == CombatManager.CombatStage.Setup)
        {
            character.HighlightAction();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CombatManager.instance.currentStage == CombatManager.CombatStage.Setup)
            CombatGrid.StopHighlight();
    }

    private void Start()
    {
        maxWidth = bar.GetComponent<RectTransform>().rect.width;
        //nameText.text = character.stats.characterName;
        character.AcquiredDebuff += DisplayEffect;
        character.AcquiredBuff += DisplayEffect;
        character.LostDebuff += RemoveEffect;
        character.LostBuff += RemoveEffect;
        character.Healed += Healed;
        character.TookDamage += TookDamage;
    }
    private void OnDisable()
    {
        character.AcquiredDebuff -= DisplayEffect;
        character.LostDebuff -= RemoveEffect;
    }

    void Update()
    {
        t = Mathf.InverseLerp(0, character.stats.hitPoints, character.currentHitpoints);

        bar.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(0, maxWidth, t), bar.rectTransform.sizeDelta.y);
        bar.color = healthbarGradient.Evaluate(t);

        hitPoints.text = character.currentHitpoints + "/" + character.stats.hitPoints;

        
        nameText.text = character.stats.characterType== CharacterStats.CharacterTypes.Adventurer ? character.stats.characterName : (character.entry.guess.characterName != null ? character.entry.guess.characterName: "Unnamed Monster");
        //hpd.value = character.currentHitpoints;
        DisplayHitPoints();
        foreach(HealthBarEffects effect in effects)
        {
            effect.durationText.text = effect.debuff != null ?
                effect.debuff.durationRemaining.ToString()
                : (effect.buff.durationRemaining).ToString();
        }
    }
    
    public void DisplayHitPoints()
    {
        if (character.stats.characterType == CharacterStats.CharacterTypes.Adventurer)
        {
            hpd.value = character.currentHitpoints;
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
            hpd.value = guess.hitPoints - character.damageTaken;
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
        background.color = Color.white;
        displayingDamageOrHeal = false;
    }

    IEnumerator DisplayDamage()
    {
        displayingDamageOrHeal = true;
        background.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        background.color = Color.white;
        displayingDamageOrHeal = false;
    }

    public void DisplayEffect(Buff buff)
    {
        GameObject newEffect = Instantiate(effectPrefab, effectsDisplay);
        Image img = newEffect.GetComponent<Image>();
        img.sprite = buff.icon;
        img.color = buff.iconColor;
        newEffect.GetComponent<SimpleTooltipSpawner>().tooltipString = buff.tooltipString;
        newEffect.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (buff.durationRemaining+1).ToString();
        effects.Add(new HealthBarEffects(newEffect, newEffect.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), buff));
    }

    public void DisplayEffect(Debuff debuff)
    {
        GameObject newEffect = Instantiate(effectPrefab, effectsDisplay);
        Image img = newEffect.GetComponent<Image>();
        img.sprite = debuff.icon;
        img.color = debuff.iconColor;
        newEffect.GetComponent<SimpleTooltipSpawner>().tooltipString = debuff.tooltipString;

        newEffect.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            debuff.debuffType == Debuff.DebuffType.DamageOverTime ? debuff.durationRemaining.ToString() : (debuff.durationRemaining + 1).ToString();
        effects.Add(new HealthBarEffects(newEffect, newEffect.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), debuff));
    }

    public void RemoveEffect(Debuff debuff)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].debuff == debuff)
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
            CharacterSheet.ShowSheet(character);
        }
    }
}

public class HealthBarEffects
{
    public GameObject effect;
    public TextMeshProUGUI durationText;
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
}
