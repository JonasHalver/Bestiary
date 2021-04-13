using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitPoint : MonoBehaviour
{
    public Image image;
    public Sprite full, half, empty, current;
    public Color healthyColor, poisonedColor, armoredColor;
    public bool isFull = true, isHalf, isEmpty;

    private void Update()
    {
        image.sprite = current;
        image.color = healthyColor;
    }
}
