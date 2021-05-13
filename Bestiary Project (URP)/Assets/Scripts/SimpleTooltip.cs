using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SimpleTooltip : MonoBehaviour
{
    public static SimpleTooltip instance;
    public string tooltipString;
    public GameObject tooltip;
    public Vector2 extraOffset, offset, modifier;
    public TextMeshProUGUI tooltipText;
    public Vector2 mousepos;
    public SimpleTooltipSpawner spawn;
    private bool isDisplayed = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            if (instance != this)
            {
                Destroy(instance.gameObject);
                
                instance = this;
            }
        }
    }

    private void Start()
    {
        tooltipText.text = tooltipString;
        offset = ((tooltip.GetComponent<RectTransform>().sizeDelta / 2) + extraOffset) * modifier;

        //tooltip.transform.position = Input.mousePosition+(Vector3)offset;
        StartCoroutine(Delay());
    }
    IEnumerator Delay()
    {
        yield return null;
        tooltip.SetActive(true);
        tooltipText.text = tooltipString;
        yield return null;
        if (Input.mousePosition.x > Screen.width / 2) modifier.x = -1;
        else modifier.x = 1;
        if (Input.mousePosition.y > Screen.height / 2) modifier.y = -1;
        else modifier.y = 1;
        offset = ((tooltip.GetComponent<RectTransform>().sizeDelta / 2) + extraOffset) * modifier;
        tooltip.transform.position = Input.mousePosition + (Vector3)offset;
        yield return null;        
        isDisplayed = true;
    }
    private void Update()
    {
        if (!isDisplayed) return;
        if (Input.mousePosition.x > Screen.width / 2) modifier.x = -1;
        else modifier.x = 1;
        if (Input.mousePosition.y > Screen.height / 2) modifier.y = -1;
        else modifier.y = 1;
        offset = ((tooltip.GetComponent<RectTransform>().sizeDelta / 2) + extraOffset)*modifier;
        tooltip.transform.position = Input.mousePosition + (Vector3)offset;

        if (spawn.gameObject == null) Destroy(gameObject);
    }
}
