using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsEditor : MonoBehaviour
{
    public GameObject dtiPrefab;

    [Header("Hit Points")]
    public HitPointDisplay hitPointDisplay;
    public float hitPointValue = 0;
    public float displayedValue = 0;
    [Header("Speed")]
    public TextMeshProUGUI speedText;

    [Header("Movement")]
    public Transform movementGrid;
    public List<Image> tiles = new List<Image>();
    public Color tileValid, tileInvalid;

    [Header("Resistances")]
    public Transform resistancesGrid;
    public List<GameObject> resistanceIcons = new List<GameObject>();

    [Header("Weaknesses")]
    public Transform weaknessesGrid;
    public List<GameObject> weaknessIcons = new List<GameObject>();

    private bool editable;
     
    // Start is called before the first frame update
    void Start()
    {        
        for (int i = 1; i < movementGrid.childCount; i++)
        {
            tiles.Add(movementGrid.GetChild(i).GetComponent<Image>());
        }
        UpdateDisplays();
    }
    private void Update()
    {
        editable = !Book.currentEntry.isMerc;
        hitPointValue = Book.currentEntry.guess.hitPoints;
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
        hitPointDisplay.value = 1;
        hitPointDisplay.ClearHearts();

        if (!Book.currentEntry.isMerc)
            hitPointDisplay.value = (int)Book.currentEntry.guess.hitPoints;
        else
            hitPointDisplay.value = (int)Book.currentEntry.origin.hitPoints;
        displayedValue = hitPointDisplay.value;
    }
    public void DisplaySpeed()
    {
        int speed = Book.currentEntry.isMerc ? Book.currentEntry.origin.speed : Book.currentEntry.guess.speed;
        string line = speed.ToString();
        switch (speed)
        {
            case 1:
            case 2:
            case 3:
                line += " (Low speed)";
                break;
            case 4:
            case 5:
            case 6:
            case 7:
                line += " (Average speed)";
                break;
            case 8:
            case 9:
            case 10:
                line += " (High speed)";
                break;
        }
        speedText.text = line;
    }
    public void DisplayMovement()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (i < (Book.currentEntry.isMerc ? Book.currentEntry.origin.movement : Book.currentEntry.guess.movement))
                tiles[i].color = tileValid;
            else
                tiles[i].color = tileInvalid;
        }
    }
    public void DisplayResistances()
    {
        for (int i = 0; i < resistanceIcons.Count; i++)
        {
            RemoveDamageTypeIcon(resistanceIcons[i], true);
            i--;
        }
        if (!Book.currentEntry.isMerc)
        {
            for (int i = 0; i < Book.currentEntry.guess.resistances.Count; i++)
            {
                AddDamageTypeIcon(Book.currentEntry.guess.resistances[i], true);
            }
        }
        else
        {
            for (int i = 0; i < Book.currentEntry.origin.resistances.Count; i++)
            {
                AddDamageTypeIcon(Book.currentEntry.origin.resistances[i], true);
            }
        }
        if (resistanceIcons.Count == 0)
        {
            CreateNone(true);
        }

    }
    public void DisplayWeaknesses()
    {
        for (int i = 0; i < weaknessIcons.Count; i++)
        {
            RemoveDamageTypeIcon(weaknessIcons[i], false);
            i--;
        }
        if (!Book.currentEntry.isMerc)
        {
            for (int i = 0; i < Book.currentEntry.guess.weaknesses.Count; i++)
            {
                AddDamageTypeIcon(Book.currentEntry.guess.weaknesses[i], false);
            }
        }
        else
        {
            for (int i = 0; i < Book.currentEntry.origin.weaknesses.Count; i++)
            {
                AddDamageTypeIcon(Book.currentEntry.origin.weaknesses[i], false);
            }
        }
        if (weaknessIcons.Count == 0)
        {
            CreateNone(false);
        }
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

        Book.OpenStatEditing((Entry.StatEntries)stat);
    }
}
