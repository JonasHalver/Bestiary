using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bookmark : MonoBehaviour
{
    public Button left, right;
    public bool onLeft;
    public bool active;

    private void Update()
    {
        left.gameObject.SetActive(onLeft && !active);
        right.gameObject.SetActive(!onLeft && !active);
    }
}
