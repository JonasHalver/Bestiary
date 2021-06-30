using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class StatsEditor : MonoBehaviour
{
    public GameObject dtiPrefab;

    public Image moveCharacterIcon;

    [Header("Hit Points")]
    public GameObject hitPointPrefab;
    public List<GameObject> hearts = new List<GameObject>();
    public float hitPointValue = 0;
    public Transform hitPointHolder;
    [Header("Speed")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI speedDescription;

    [Header("Movement")]
    public Image move1;
    public Image move2;
    public Image move3;
    public Image move4;

    [Header("Resistances")]
    public Transform resistancesGrid;
    public TextMeshProUGUI resDisplay;
    public List<GameObject> resistanceIcons = new List<GameObject>();

    [Header("Weaknesses")]
    public Transform weaknessesGrid;
    public TextMeshProUGUI weakDisplay;
    public List<GameObject> weaknessIcons = new List<GameObject>();

    private bool editable;
     
    // Start is called before the first frame update
    void Start()
    {       
        UpdateDisplays();
    }
    private void Update()
    {
        editable = !Book.currentEntry.isMerc;
        hitPointValue = Book.currentEntry.guess.hitPoints;
       // moveCharacterIcon.sprite = Book.currentEntry.origin.characterIcon;
       // moveCharacterIcon.color = Book.currentEntry.origin.characterColor;
    }

    private void OnEnable()
    {
        Book.StatsUpdated += UpdateDisplays;

    }
    private void OnDisable()
    {
        Book.StatsUpdated -= UpdateDisplays;
    }

    public void UpdateDisplays()
    {
        DisplayHitPoints();
        DisplaySpeed();
        DisplayMovement();
        DisplayResistances();
        DisplayWeaknesses();
    }
    
    public void DisplayHitPoints()
    {
        hitPointValue = 1;
        //hitPointDisplay.ClearHearts();

        if (!Book.currentEntry.isMerc)
            hitPointValue = (int)Book.currentEntry.guess.hitPoints;
        else
            hitPointValue = (int)Book.currentEntry.origin.hitPoints;

        AddRemoveHearts();
    }
    public void AddRemoveHearts()
    {
        if (hearts.Count == hitPointValue) return;
        for (int i = 0; i < hearts.Count; i++)
        {
            Destroy(hearts[i]);
            hearts.RemoveAt(i);
            i--;
        }
        for (int i = 0; i < hitPointValue; i++)
        {
            GameObject newHeart = Instantiate(hitPointPrefab, hitPointHolder);
            hearts.Add(newHeart);
        }
    }
    public void DisplaySpeed()
    {
        int speed = Book.currentEntry.isMerc ? Book.currentEntry.origin.speed : Book.currentEntry.guess.speed;
        speedText.text = speed.ToString();
        switch (speed)
        {
            case 1:
            case 2:
            case 3:
                speedDescription.text = "(Low speed)";
                break;
            case 4:
            case 5:
            case 6:
            case 7:
                speedDescription.text = "(Average speed)";
                break;
            case 8:
            case 9:
            case 10:
                speedDescription.text = "(High speed)";
                break;
        }
    }
    public void DisplayMovement()
    {
        switch(Book.currentEntry.isMerc ? Book.currentEntry.origin.movement : Book.currentEntry.guess.movement)
        {
            case 1:
                move1.enabled = true;
                move2.enabled = false;
                move3.enabled = false;
                move4.enabled = false;
                break;
            case 2:
                move1.enabled = false;
                move2.enabled = true;
                move3.enabled = false;
                move4.enabled = false;
                break;
            case 3:
                move1.enabled = false;
                move2.enabled = false;
                move3.enabled = true;
                move4.enabled = false;
                break;
            case 4:
                move1.enabled = false;
                move2.enabled = false;
                move3.enabled = false;
                move4.enabled = true;
                break;
        }
    }
    public void DisplayResistances()
    {
        StringBuilder sb = new StringBuilder();
        int count = Book.currentEntry.isMerc ? Book.currentEntry.origin.resistances.Count : Book.currentEntry.guess.resistances.Count;
        for (int i = 0; i < resistanceIcons.Count; i++)
        {
            RemoveDamageTypeIcon(resistanceIcons[i], true);
            i--;
        }
        if (!Book.currentEntry.isMerc)
        {
            for (int i = 0; i < Book.currentEntry.guess.resistances.Count; i++)
            {
                //AddDamageTypeIcon(Book.currentEntry.guess.resistances[i], true);
                sb.Append($"- {Book.currentEntry.guess.resistances[i]}").AppendLine();
            }
        }
        else
        {
            for (int i = 0; i < Book.currentEntry.origin.resistances.Count; i++)
            {
                //AddDamageTypeIcon(Book.currentEntry.origin.resistances[i], true);
                sb.Append($"- {Book.currentEntry.origin.resistances[i]}").AppendLine();
            }
        }
        if (count == 0)
        {
            sb.Append("None");
            //CreateNone(true);
        }
        resDisplay.text = sb.ToString();
    }
    public void DisplayWeaknesses()
    {
        StringBuilder sb = new StringBuilder();
        int count = Book.currentEntry.isMerc ? Book.currentEntry.origin.weaknesses.Count : Book.currentEntry.guess.weaknesses.Count;

        for (int i = 0; i < weaknessIcons.Count; i++)
        {
            RemoveDamageTypeIcon(weaknessIcons[i], false);
            i--;
        }
        if (!Book.currentEntry.isMerc)
        {
            for (int i = 0; i < Book.currentEntry.guess.weaknesses.Count; i++)
            {
                //AddDamageTypeIcon(Book.currentEntry.guess.weaknesses[i], false);
                sb.Append($"- {Book.currentEntry.guess.weaknesses[i]}").AppendLine();
            }
        }
        else
        {
            for (int i = 0; i < Book.currentEntry.origin.weaknesses.Count; i++)
            {
                //AddDamageTypeIcon(Book.currentEntry.origin.weaknesses[i], false);
                sb.Append($"- {Book.currentEntry.origin.weaknesses[i]}").AppendLine();
            }
        }
        if (count == 0)
        {
            sb.Append("None");
            //CreateNone(false);
        }
        weakDisplay.text = sb.ToString();
    }
    public void CreateNone(bool isResistance)
    {
        GameObject newIcon = Instantiate(dtiPrefab, isResistance ? resistancesGrid : weaknessesGrid);
        DamageTypeIcon dti = newIcon.GetComponent<DamageTypeIcon>();
        dti.transform.localScale = new Vector3(0.4f, 0.4f, 1);
        dti.isEditable = false;
        dti.isNone = true;
        if (isResistance)
            resistanceIcons.Add(newIcon);
        else
            weaknessIcons.Add(newIcon);
    }
    public void AddDamageTypeIcon(Character.DamageTypes type, bool isResistance)
    {
        if (isResistance)
        {
            if (resistanceIcons.Count == 1)
            {
                if (resistanceIcons[0].GetComponent<DamageTypeIcon>().isNone)
                {
                    Destroy(resistanceIcons[0]);
                    resistanceIcons.RemoveAt(0);
                }
            }
        }
        else
        {
            if (weaknessIcons.Count == 1)
            {
                if (weaknessIcons[0].GetComponent<DamageTypeIcon>().isNone)
                {
                    Destroy(weaknessIcons[0]);
                    weaknessIcons.RemoveAt(0);
                }
            }
        }
        GameObject newIcon = Instantiate(dtiPrefab, isResistance ? resistancesGrid : weaknessesGrid);
        DamageTypeIcon dti = newIcon.GetComponent<DamageTypeIcon>();
        dti.transform.localScale = new Vector3(0.4f, 0.4f, 1);

        dti.icon.color = Color.white;
        dti.isEditable = true;
        dti.isNone = false;
        dti.isResistance = isResistance;
        dti.manager = gameObject;
        if (isResistance)
            resistanceIcons.Add(newIcon);
        else
            weaknessIcons.Add(newIcon);
        dti.damageType = type;
    }

    public void RemoveDamageTypeIcon(GameObject icon, bool isResistance)
    {
        if (isResistance)
            resistanceIcons.Remove(icon);
        else
            weaknessIcons.Remove(icon);
        Destroy(icon);
    }

    public void OpenStatEditor(int stat)
    {
        if (!editable) return;
        SoundManager.OpenEditing();
        Book.OpenStatEditing((Entry.StatEntries)stat);
        TutorialManager.instance.StandaloneTutorial(((Entry.StatEntries)stat).ToString());
    }
}
