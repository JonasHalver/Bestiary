using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollOverride : MonoBehaviour
{
    private Scrollbar sr;
    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<Scrollbar>();
    }

    // Update is called once per frame
    void Update()
    {
        if (CombatManager.instance.currentStage != CombatManager.CombatStage.Setup)
        {
            sr.value = 0;
        }
    }
}
