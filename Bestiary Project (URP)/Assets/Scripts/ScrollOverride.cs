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
    private void OnEnable()
    {
        sr = GetComponent<Scrollbar>();
        sr.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (CombatManager.instance.currentStage != CombatManager.CombatStage.Setup && GameManager.gameState == GameManager.GameState.Normal)
        {
            sr.value = 0;
        }
    }
    public void ScrollToBottom()
    {
        sr.value = 0;
    }
}
