using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SecondaryWindowExpansion : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    private LayoutElement le;
    [SerializeField] private float minHeight = 50f, maxSmallHeight = 75f, maxFullHeight = 600, minWidth = 50f, maxSmallWidth = 75f, maxFullWidth = 3000f;
    private bool fullyExpanded = false;
    public bool height = true;
    public bool onClick = true;
    private bool stop = false;
    public List<GameObject> objectsToActivateOnExpand = new List<GameObject>();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!onClick) return;
        if (!fullyExpanded)
        {
            if (height) StartCoroutine(FullExpandHeight(true));
            else StartCoroutine(FullExpandWidth(true));
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!fullyExpanded)
        {
            if (height)
                StartCoroutine(SmallExpandHeight(true));
            else StartCoroutine(SmallExpandWidth(true));            
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!fullyExpanded)
        {
            if (height) StartCoroutine(SmallExpandHeight(false));
            else StartCoroutine(SmallExpandWidth(false));
        }
    }

    private void Awake()
    {
        le = GetComponent<LayoutElement>();
    }

    IEnumerator SmallExpandHeight(bool expand)
    {
        stop = true;
        yield return null;
        stop = false;
        float t = 0;
        while (t < 1)
        {
            if (expand) le.preferredHeight = Mathf.Lerp(le.preferredHeight, maxSmallHeight, t);
            else le.preferredHeight = Mathf.Lerp(le.preferredHeight, minHeight, t);
            t += Time.deltaTime * 2;
            if (stop) break;
            yield return null;
        }
    }
    IEnumerator FullExpandHeight(bool expand)
    {
        stop = true;
        for (int i = 0; i < objectsToActivateOnExpand.Count; i++)
        {
            objectsToActivateOnExpand[i].SetActive(expand);
        }
        yield return null;
        stop = false;
        fullyExpanded = expand;
        float t = 0;
        while (t < 1)
        {
            if (expand) le.preferredHeight = Mathf.Lerp(le.preferredHeight, maxFullHeight, t);
            else le.preferredHeight = Mathf.Lerp(le.preferredHeight, minHeight, t);
            t += Time.deltaTime * 2;
            yield return null;
        }
    }
    IEnumerator SmallExpandWidth(bool expand)
    {
        stop = true;
        yield return null;
        stop = false;
        float t = 0;
        while (t < 1)
        {
            if (expand) le.preferredWidth = Mathf.Lerp(le.preferredWidth, maxSmallWidth, t);
            else le.preferredWidth = Mathf.Lerp(le.preferredWidth, minWidth, t);
            t += Time.deltaTime * 2;
            if (stop) break;
            yield return null;
        }
    }
    IEnumerator FullExpandWidth(bool expand)
    {
        stop = true;
        for (int i = 0; i < objectsToActivateOnExpand.Count; i++)
        {
            objectsToActivateOnExpand[i].SetActive(expand);
        }
        yield return null;
        stop = false;
        fullyExpanded = expand;
        float t = 0;
        while (t < 1)
        {
            if (expand) le.preferredWidth = Mathf.Lerp(le.preferredWidth, maxFullWidth, t);
            else le.preferredWidth = Mathf.Lerp(le.preferredWidth, minWidth, t);
            t += Time.deltaTime * 2;
            yield return null;
        }
    }
    public void Maximize()
    {
        if (height) StartCoroutine(FullExpandHeight(true));
        else StartCoroutine(FullExpandWidth(true));
    }
    public void Minimize()
    {
        if (height) StartCoroutine(FullExpandHeight(false));
        else StartCoroutine(FullExpandWidth(false));
    }
}
