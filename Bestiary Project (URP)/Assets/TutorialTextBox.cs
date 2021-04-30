using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialTextBox : MonoBehaviour
{
    public Transform rectHolder;
    public Image backGround;
    public List<RectTransform> windowLocations = new List<RectTransform>();
    private Dictionary<string, RectTransform> locations = new Dictionary<string, RectTransform>();
    private RectTransform rt;

    public List<string> tutorialMessages = new List<string>();
    private Dictionary<string, int> messages = new Dictionary<string, int>();
    public TextMeshProUGUI textBox;
    public TextMeshProUGUI clickToContinue;
    public bool interrupt = false;
    public static bool mouseOverText = false;


    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in rectHolder)
        {
            windowLocations.Add(child.GetComponent<RectTransform>());
        }
        rt = GetComponent<RectTransform>();
        for (int i = 0; i < windowLocations.Count; i++)
        {
            locations.Add(windowLocations[i].gameObject.name, windowLocations[i]);
        }
        for (int i = 0; i < tutorialMessages.Count; i++)
        {
            messages.Add(tutorialMessages[i].Substring(0, tutorialMessages[i].IndexOf(" ")), i);
        }
    }
    private void Update()
    {
        Vector2 boxMin, boxMax;
        boxMin = (Vector2)transform.position - rt.sizeDelta / 2;
        boxMax = (Vector2)transform.position + rt.sizeDelta / 2;
        mouseOverText = (Input.mousePosition.x > boxMin.x && Input.mousePosition.x < boxMax.x) && (Input.mousePosition.y > boxMin.y && Input.mousePosition.y < boxMax.y);
    }
    public void MoveBox(string destination, bool allowContinue)
    {
        interrupt = true;
        textBox.text = "";
        clickToContinue.enabled = false;
        StartCoroutine(InterpolateBetweenPositions(destination, allowContinue));
    }  
    public void Fade(bool fadeIn)
    {
        interrupt = true;
        textBox.text = "";
        clickToContinue.enabled = false;
        StartCoroutine(FadeLerp(fadeIn));
    }
    IEnumerator FadeLerp(bool fadeIn)
    {
        yield return null;
        float t = 0;
        interrupt = false;
        Color baseColor = backGround.color;
        float a = baseColor.a;
        while (t < 1)
        {
            a = Mathf.Lerp(a, fadeIn ? 1f : 0, t);
            backGround.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            t += Time.deltaTime * 2;
            //if (interrupt) break;
            yield return null;
        }
    }
    IEnumerator InterpolateBetweenPositions(string tutorial, bool allowContinue)
    {
        yield return null;
        RectTransform destination = locations[tutorial];
        interrupt = false;
        float t = 0;
        StartCoroutine(DisplayText(tutorial, allowContinue));
        while (t < 1)
        {
            rt.localPosition = Vector3.Lerp(rt.localPosition, destination.localPosition, t);
            rt.sizeDelta = Vector3.Lerp(rt.sizeDelta, destination.sizeDelta, t); 
            if (interrupt) break;
            t += Time.deltaTime;
            yield return null;
        }       
    }

    IEnumerator DisplayText(string tutorial, bool allowContinue)
    {
        yield return null;
        interrupt = false;
        bool fast = false;
        string output = "";
        string m = tutorialMessages[messages[tutorial]].Remove(0, tutorial.Length + 1);
        fast = m.Length > 30;
        for (int i = 0; i < m.Length; i++)
        {
            if (interrupt) break;
            if (m[i] == '<')
            {
                int endIndex = m.IndexOf('>', i);
                endIndex = m.IndexOf('>', endIndex+1);
                output += m.Substring(i, endIndex+1 - i);
                i = endIndex;
                continue;
            }
            if (!fast)
                yield return null;
            if (interrupt) break;
            if (m[i] == '*') output += System.Environment.NewLine;
            else output += m[i];
            yield return null;
            if (interrupt) break;

            textBox.text = output;
        }
        if (!interrupt && allowContinue) clickToContinue.enabled = true;
    }
}
