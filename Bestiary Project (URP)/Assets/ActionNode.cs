using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActionNode : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    private bool selected = false;
    private Vector2 offset = Vector2.zero;
    private Transform canvas;
    private List<RaycastResult> hits = new List<RaycastResult>();
    private Vector2 originalLocation;
    private Transform originalWindow;
    private Transform window;
    private bool dragging = false;


    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>().transform;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!selected) OnSelect();
        dragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (selected) OnDeselect();
        dragging = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selected) OnDeselect();
        else OnSelect();
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (selected && dragging) OnDeselect();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (selected) Selected();
    }

    private void Selected()
    {
        transform.position = (Vector2)Input.mousePosition + offset;
        window = HoverWindow();
    }

    private Transform HoverWindow()
    {
        PointerEventData ped = new PointerEventData(EventSystem.current);
        ped.position = transform.position;
        hits.Clear();
        ActionEditor.graphicRaycaster.Raycast(ped, hits);
        foreach (RaycastResult hit in hits)
        {
            if (hit.gameObject.CompareTag("Window"))
            {
                return hit.gameObject.transform.Find("Node Holder");
            }
        }
        return null;
    }

    private void OnSelect()
    {
        selected = true;
        originalLocation = transform.position;
        originalWindow = transform.parent;
        transform.parent = canvas;
        offset = transform.position - Input.mousePosition;
    }
    private void OnDeselect()
    {
        selected = false;
        if (window == null)
        {
            transform.parent = originalWindow;
            transform.position = originalLocation;
        }
        else
        {
            transform.parent = window;
        }
        window = null;
    }    
}
