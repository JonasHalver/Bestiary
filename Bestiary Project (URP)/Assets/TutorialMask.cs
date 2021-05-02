using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TutorialMask : MonoBehaviour
{
    public Transform rectHolder;
    public Image mask;
    public List<RectTransform> windowLocations = new List<RectTransform>();
    private Dictionary<string, RectTransform> locations = new Dictionary<string, RectTransform>();
    private RectTransform rt;
    private int locationIndex = 0;
    public bool interrupt = false;
    public static bool mouseOverMask = false;
    public static bool active; 

    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform child in rectHolder)
        {
            windowLocations.Add(child.GetComponent<RectTransform>());
        }
        rt = GetComponent<RectTransform>();
        for (int i = 0; i < windowLocations.Count; i++)
        {
            locations.Add(windowLocations[i].gameObject.name, windowLocations[i]);
        }
    }
    private void Update()
    {
        Vector2 boxMin, boxMax;
        boxMin = (Vector2)transform.position - rt.sizeDelta / 2;
        boxMax = (Vector2)transform.position + rt.sizeDelta / 2;
        mouseOverMask = (Input.mousePosition.x > boxMin.x && Input.mousePosition.x < boxMax.x) && (Input.mousePosition.y > boxMin.y && Input.mousePosition.y < boxMax.y);
    }
    public void MoveMask(string destination)
    {
        interrupt = true;
        StartCoroutine(InterpolateBetweenPositions(locations[destination]));
    }
    public void Fade(bool fadeIn)
    {
        interrupt = true;
        StartCoroutine(FadeLerp(fadeIn));
        active = fadeIn;
    }
    IEnumerator InterpolateBetweenPositions(RectTransform destination)
    {
        yield return null;
        interrupt = false;
        float t = 0;
        while (t < 1)
        {
            rt.sizeDelta = Vector2.Lerp(rt.sizeDelta, destination.sizeDelta, t);
            rt.localPosition = Vector3.Lerp(rt.localPosition, destination.localPosition, t);
            if (interrupt) break;
            t += Time.deltaTime;
            yield return null;
        }
    }
    IEnumerator FadeLerp(bool fadeIn)
    {
        yield return null;
        float t = 0;
        interrupt = false;
        Color baseColor = mask.color;
        float a = baseColor.a;
        while (t < 1)
        {
            a = Mathf.Lerp(a, fadeIn ? 0.4f : 0, t);
            mask.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            t += Time.deltaTime;
            if (interrupt) break;
            yield return null;
        }
    }
}

