using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarFlash : MonoBehaviour
{
    private Image img;
    public AnimationCurve curve;
    private void Awake()
    {
        img = GetComponent<Image>();
    }
    private void OnEnable()
    {
        CombatAction.ActionInformationConfirmed += ShowFlash;
    }
    private void OnDisable()
    {
        CombatAction.ActionInformationConfirmed -= ShowFlash;
    }

    public void ShowFlash(ActionCheck ac)
    {
        StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        float t = 0;
        float f = 0;
        while (t < 1)
        {
            f = curve.Evaluate(t);
            t += Time.deltaTime;
            img.color = new Color(1, 1, 1, f);
            yield return null;
        }
    }
}
