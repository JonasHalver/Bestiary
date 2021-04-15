using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionEditor : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI editCount, collectionCount, discardCount;
    [SerializeField] private Transform editHolder, collectionHolder, discardHolder;

    public static GraphicRaycaster graphicRaycaster;

    // Start is called before the first frame update
    void Start()
    {
        graphicRaycaster = GetComponent<GraphicRaycaster>();
    }

    // Update is called once per frame
    void Update()
    {
        editCount.text = editHolder.childCount.ToString();
        collectionCount.text = collectionHolder.childCount.ToString();
        discardCount.text = discardHolder.childCount.ToString();

    }
}
