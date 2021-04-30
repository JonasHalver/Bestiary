using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BookRelocation : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector2 basePos;
    private Vector2 clickOffset;
    private void OnEnable()
    {
        basePos = new Vector2(Screen.width / 2, Screen.height / 2);
        transform.position = basePos;
    }
    private void Update()
    {
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        clickOffset = eventData.position - (Vector2)transform.position;
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position - clickOffset;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
    }
}
