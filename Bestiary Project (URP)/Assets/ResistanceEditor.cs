using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResistanceEditor : MonoBehaviour
{
    public bool isResistance;
    public TMP_Dropdown dropdown;
    public Transform grid1, grid2;
    public List<GameObject> icons = new List<GameObject>();
    public List<Character.DamageTypes> currentTypes = new List<Character.DamageTypes>();
    public GameObject iconPrefab;
    public Button accept;

    // Start is called before the first frame update
    void Start()
    {
        CreateNone();
    }

    private void OnEnable()
    {
        CharacterStats stats = Book.currentEntry.guess;
        currentTypes = isResistance ? stats.resistances : stats.weaknesses;
        //if (currentTypes.Count == 0) CreateNone();
        //else
        //{
            for (int i = 0; i < currentTypes.Count; i++)
            {
                AddDamageTypeIcon(currentTypes[i]);
            }
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTypes.Contains((Character.DamageTypes)dropdown.value)) accept.interactable = false;
        else accept.interactable = true;

        if (icons.Count == 0)
        {
            CreateNone();
        }
        if (grid1.childCount < 3 && grid2.childCount > 0)
        {
            for (int i = 0; i < grid2.childCount; i++)
            {
                GameObject c = grid2.GetChild(i).gameObject;
                c.transform.parent = grid1;
                i--;
                if (grid1.childCount == 3)
                {
                    break;
                }
            }
        }
    }
    public void CreateNone()
    {
        GameObject newIcon = Instantiate(iconPrefab, grid1);
        DamageTypeIcon dti = newIcon.GetComponent<DamageTypeIcon>();
        dti.isEditable = false;
        dti.isNone = true;
        icons.Add(newIcon);
    }

    public void Added()
    {        
        Character.DamageTypes type = (Character.DamageTypes)dropdown.value;
        currentTypes.Add(type);
        AddDamageTypeIcon(type);
        UpdateStats();
    }

    public void AddDamageTypeIcon(Character.DamageTypes type)
    {
        if (icons.Count == 1)
        {
            if (icons[0].GetComponent<DamageTypeIcon>().isNone)
            {
                Destroy(icons[0]);
                icons.RemoveAt(0);
            }
        }
        GameObject newIcon = Instantiate(iconPrefab, icons.Count < 3 ? grid1 : grid2);
        DamageTypeIcon dti = newIcon.GetComponent<DamageTypeIcon>();
        dti.icon.color = Color.white;
        dti.isEditable = true;
        dti.isNone = false;
        dti.isResistance = isResistance;
        dti.manager = gameObject;
        icons.Add(newIcon);
        dti.damageType = type;
    }

    public void RemoveDamageTypeIcon(DamageTypeIcon icon)
    {
        currentTypes.Remove(icon.damageType);
        icons.Remove(icon.gameObject);
        Destroy(icon.gameObject);
        UpdateStats();
    }

    public void UpdateStats()
    {
        if (isResistance)
            Book.currentEntry.guess.resistances = currentTypes;
        else
            Book.currentEntry.guess.weaknesses = currentTypes;

        Book.UpdateStats();
    }
}
