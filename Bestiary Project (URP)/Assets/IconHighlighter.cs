using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconHighlighter : MonoBehaviour
{
    private Image img;
    public Color highlighted, normal;

    public static event System.Action<IconHighlighter> Highlight;

    private void Awake()
    {
        img = GetComponent<Image>();
    }
    private void OnEnable()
    {
        Highlight += HighlightButton;
    }
    private void OnDisable()
    {
        Highlight -= HighlightButton;
    }

    public void HighlightButton(IconHighlighter ihl)
    {
        img.color = normal;
        if (ihl == this)
        {
            img.color = highlighted;
        }
    }
    public void Selected()
    {
        Highlight.Invoke(this);
    }
}
