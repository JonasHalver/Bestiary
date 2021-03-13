using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public enum ElementType { Dropdown, Text, Button }

    private void Awake()
    {
        instance = this;
    }
}
