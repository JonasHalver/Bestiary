using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    public GameObject attn;
    private SimpleTooltipSpawner attnTooltip;
    private Animator animator;

    public string nodeName;
    public string nodeDescription;

    public enum NodeType { Context, Output, Shape }
    public NodeType nodeType = NodeType.Context;
    public enum ToggleType { Targeting, TargetGroup, Cooldown }
    public OutputInfo actionOutput;
    public ContextInfo actionContext;
    public Action.Shape actionShape;

    public bool requiresEditing;
    public bool hasBeenEdited;
    private bool error;

    public GameObject contextEditPrefab, outputEditPrefab;

    public static event System.Action NodeChanged;

    public Color bgShape, bgContext, bgOutput;

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
            Transform output = null;
            if (!selected) output = transform.parent.parent.parent.parent;
            else if (originalWindow) return originalWindow.parent.parent.name.Contains("Edit");
            if (output) return output.name.Contains("Edit");
            else if (originalWindow) return originalWindow.parent.parent.name.Contains("Edit");
            else return false;
        }
    }

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>().transform;
        Color c = Color.white;
        switch (nodeType)
        {
            case NodeType.Context:
                c = bgContext;
                break;
            case NodeType.Shape:
                c = bgShape;
                break;
            case NodeType.Output:
                c = bgOutput;
                break;
        }
        transform.Find("Background").GetComponent<Image>().color = c;
        attnTooltip = attn.GetComponent<SimpleTooltipSpawner>();
    }    

    // Update is called once per frame
    void Update()
    {
        if (selected) Selected();
        else
        {
            if (transform.parent.name != "Content") transform.parent = originalWindow;
        }
        Animator.SetFloat("Blend", blend);
        if (InEditor && !error)
        {
            attn.SetActive(requiresEditing && !hasBeenEdited);
        }
        else if (InEditor && error) attn.SetActive(true);
        else attn.SetActive(false);
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
                return hit.gameObject.transform.Find("Node Holder").Find("Viewport").Find("Content");
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
        GetComponent<Image>().raycastTarget = false;
        originalLocation = transform.position;
        originalWindow = transform.parent;
        transform.parent = canvas;
        offset = transform.position - Input.mousePosition;
        LineManager.Instance.MovingNode = true;
        LineManager.Instance.NodeMoving = this;
    }
    private void OnDeselect()
    {
        if (nodeType == NodeType.Output || nodeType == NodeType.Shape)
        {
            if (window != null)
            {
                if (window.parent.parent.parent.name.Contains("Edit"))
                {
                    if (originalWindow.parent.parent.parent.name.Contains("Collection"))
                    {
                        GameObject clone = Instantiate(gameObject, transform.position, Quaternion.identity, window);
                        clone.GetComponent<Image>().raycastTarget = true;
                        transform.parent = originalWindow;
                        transform.position = originalLocation;
                    }
                    else if (originalWindow.parent.parent.parent.name.Contains("Discard"))
                    {
                        transform.parent = window;
                    }
                    else
                    {
                        transform.parent = window;
                    }
                }
                else if (window.parent.parent.parent.name.Contains("Discard"))
                {
                    transform.parent = window;
                }
                else if (window.parent.parent.parent.name.Contains("Collection"))
                {
                    if (originalWindow.parent.parent.parent.name.Contains("Edit"))
                    {
                        NodeChanged.Invoke();
                        Destroy(gameObject);
                        return;
                    }
                    else
                    {
                        transform.parent = window;
                    }
                }
            }
            else
            {
                transform.parent = originalWindow;
                transform.position = originalLocation;
            }
        }
        else
        {
            if (window == null)
            {
                transform.parent = originalWindow;
                transform.position = originalLocation;
            }
            else
            {
                transform.parent = window;
            }
        }
        selected = false;
        GetComponent<Image>().raycastTarget = true;
        
        window = null;
        LineManager.Instance.MovingNode = false;
        // REWRITE THIS TO UPDATE THE ICON INSTEAD / OTHER JUICE
        // StartCoroutine(MoveConnectors(!InEditor));
        if (!InEditor)
        {
            LineManager.Instance.SeverConnections(this);
            actionContext.ResetInformation();
            actionOutput.ResetInformation();
            hasBeenEdited = false;
        }
        NodeChanged.Invoke();
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
        GameObject newWindow;
        switch (nodeType)
        {
            case NodeType.Shape:
            case NodeType.Context:
                newWindow = Instantiate(contextEditPrefab, canvas);
                ContextEdit cEdit = newWindow.GetComponent<ContextEdit>();
                cEdit.node = this;
                cEdit.ShowEditing();
                break;
            case NodeType.Output:
                newWindow = Instantiate(outputEditPrefab, canvas);
                OutputEdit oEdit = newWindow.GetComponent<OutputEdit>();
                oEdit.node = this;
                oEdit.ShowEditing();
                break;
        }
    }
    public void EndEdit(bool changesMade)
    {
        if (!hasBeenEdited)
            hasBeenEdited = changesMade;
        NodeChanged.Invoke();
    }

    public void Incompatible(List<Action.Context> contexts)
    {
        error = true;
        attnTooltip.type = Tooltips.TooltipType.Custom;
        attnTooltip.tooltipString = $"This node is in conflict with: ";
        for (int i = 0; i < contexts.Count; i++)
        {
            if (contexts.Count == 1)
            {
                attnTooltip.tooltipString += $"{contexts[i]}.";
                break;
            }
            if (i == contexts.Count - 1)
            {
                attnTooltip.tooltipString += $"and {contexts[i]}.";
            }
            else
            {
                attnTooltip.tooltipString += $"{contexts[i]}, ";
            }
        }
    }
    public void Error (string errorMessage)
    {
        error = true;
        attnTooltip.type = Tooltips.TooltipType.Custom;
        attnTooltip.tooltipString = $"ERROR: {errorMessage}";

    }
    public void EndError()
    {
        error = false;
        attnTooltip.tooltipString = "Click to add more information to this node.";
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
