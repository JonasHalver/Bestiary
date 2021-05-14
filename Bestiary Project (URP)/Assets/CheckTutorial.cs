using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckTutorial : MonoBehaviour
{
    private void OnEnable()
    {
        if (!GameManager.tutorial) gameObject.SetActive(false);
    }
}
