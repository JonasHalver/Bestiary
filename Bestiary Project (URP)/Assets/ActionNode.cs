using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActionNode : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    private bool selected = false;
    private Vector2 offset = Vector2.zero;
    private Transform canvas;
    private List<RaycastResult> hits = new List<RaycastResult>();
    private Vector2 originalLocation;
    private Transform originalWindow;
    private Transform window;
    private bool dragging = false;
    public Transform nodeConnectRight, nodeConnectLeft;
    private Animator animator;

    public enum NodeType { Context, Output }
    public NodeType nodeType = NodeType.Context;
    public OutputInfo actionOutput;
    public ContextInfo actionContext;

    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
    }
    private float blend = 0;

    public Transform Connection
    {
        get => NodeConnection();
    }
    public bool InEditor
    {
        get
        {
            return transform.parent.parent.name.Contains("Edit");
        }
    }

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>().transform;
        
        
    }    

    // Update is called once per frame
    void Update()
    {
        if (selected) Selected();
        Animator.SetFloat("Blend", blend);
    }

    private void Selected()
    {
        //transform.position = (Vector2)Input.mousePosition + offset;
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

    private Transform NodeConnection()
    {
        PointerEventData ped = new PointerEventData(EventSystem.current);
        ped.position = Input.mousePosition;
        hits.Clear();
        ActionEditor.graphicRaycaster.Raycast(ped, hits);
        foreach(RaycastResult hit in hits)
        {
            if (hit.gameObject.CompareTag("Node"))
            {
                if (hit.gameObject.GetComponent<ActionNode>())
                {
                    if (hit.gameObject.GetComponent<ActionNode>().InEditor)
                    {
                        return hit.gameObject.GetComponent<ActionNode>().nodeConnectLeft;
                    }
                }
                else if (hit.gameObject.transform.parent.GetComponent<ActionNode>().InEditor)
                {
                    return hit.gameObject.transform.parent.GetComponent<ActionNode>().nodeConnectLeft;
                }
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
        LineManager.Instance.MovingNode = true;
        LineManager.Instance.NodeMoving = this;
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
        LineManager.Instance.MovingNode = false;
        StartCoroutine(MoveConnectors(!InEditor));
        if (!InEditor) LineManager.Instance.SeverConnections(this);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        OnSelect();
        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!selected) OnSelect();
        dragging = true;
        transform.position = eventData.position + offset;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        OnDeselect();
        dragging = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartCoroutine(ClickDelay());
    }
    IEnumerator ClickDelay()
    {
        while (!dragging)
        {
            if (Input.GetMouseButtonUp(0))
            {
                if(InEditor)
                    EditNode();
                break;
            }
            yield return null;
        }
    }

    public void AddLine()
    {
        if (InEditor)
            LineManager.Instance.NewConnection(nodeConnectRight.position, this);
    }

    IEnumerator MoveConnectors(bool retract)
    {
        float goal = retract ? 0 : 1;
        float t = 0;
        while (t < 1)
        {
            blend = Mathf.Lerp(blend, goal, t);
            t += Time.deltaTime;
            yield return null;
        }
    }

    private void EditNode()
    {
        print("Open editing");
    }
}

public class NodeConnections
{
    public ActionNode origin;
    public List<ActionNode> connections = new List<ActionNode>();
    public Dictionary<ActionNode, Info> lines = new Dictionary<ActionNode, Info>();

    public NodeConnections(ActionNode _origin)
    {
        origin = _origin;
    }
    public class Info
    {
        public bool onLeft;
        public UILineRenderer line;
        public Info(bool _onLeft, UILineRenderer _line)
        {
            onLeft = _onLeft;
            line = _line;
        }
    }
}
