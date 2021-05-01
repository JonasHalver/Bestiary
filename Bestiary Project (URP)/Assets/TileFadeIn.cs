using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileFadeIn : MonoBehaviour
{
    private Image img;

    private void Start()
    {
        img = GetComponent<Image>();
    }
    public void FadeIn()
    {
        StartCoroutine(Fade());
    }
    IEnumerator Fade()
    {
        Color baseColor = img.color;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            img.color = new Color(baseColor.r, baseColor.g, baseColor.b, t);
            yield return null;
        }
        img.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1);
    }
}
