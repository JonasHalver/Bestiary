using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUI : MonoBehaviour
{
    public static CombatUI instance;
    public TextMeshProUGUI stageDisplay, stageInfo, combatLog;

    private void Awake()
    {
        instance = this;
    }

    public void Commit()
    {
        CombatManager.instance.Commit();
    }
}
