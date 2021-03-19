using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedEditor : MonoBehaviour
{
    public int value = 5;
    public float baseSize = 75, largeSize = 100;
    public Transform grid;
    public List<GameObject> tiles = new List<GameObject>();
    public GameObject selectedTile;
    public TextMeshProUGUI speedText;
    private string line;

    private void OnEnable()
    {
        value = Book.currentEntry.guess.speed;
    }
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < grid.childCount; i++)
        {
            tiles.Add(grid.GetChild(i).gameObject);
        }
        selectedTile = tiles[value - 1];
        selectedTile.GetComponent<RectTransform>().sizeDelta = new Vector2(largeSize, largeSize);
    }

    // Update is called once per frame
    void Update()
    {
        line = value.ToString();
        switch (value)
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

    public void TileClicked(int index)
    {
        value = index;
        selectedTile = tiles[value - 1];

        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].GetComponent<RectTransform>().sizeDelta = new Vector2(baseSize, baseSize);
        }
        selectedTile.GetComponent<RectTransform>().sizeDelta = new Vector2(largeSize, largeSize);
        UpdateStats();
    }
    public void UpdateStats()
    {
        Book.currentEntry.guess.speed = value;
        Book.UpdateStats();
    }
}
