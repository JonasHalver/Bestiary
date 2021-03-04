using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(fileName = "new test", menuName = "tests")]
public class TestScript : ScriptableObject
{
    public void Test()
    {
        Debug.Log("Test");
    }
}
