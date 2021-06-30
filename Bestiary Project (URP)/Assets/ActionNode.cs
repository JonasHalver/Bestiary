using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

[Serializable]
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
    public GameObject attn;
    private SimpleTooltipSpawner attnTooltip;
    private Animator animator;
    public Image icon1, icon2;
    public GameObject crit;
    private GameObject secondaryIcon;
    public TextMeshProUGUI valueDisplay;
    public Sprite questionMark;

    public string nodeName;
    public string nodeDescription;

    public enum NodeType { Context, Output, Shape }
    public NodeType nodeType = NodeType.Context;
    public enum ToggleType { Targeting, TargetGroup, Cooldown }
    public OutputInfo actionOutput;
    public ContextInfo actionContext;
    public Action.Shape actionShape;
    public Action.TargetPriority priority;

    public bool isBig = false;
    public bool requiresEditing;
    public bool hasBeenEdited;
    private bool error;

    public enum WindowType { Edit1, Edit2, Collection, Discard }
    public WindowType windowType;
    private WindowType wt = WindowType.Collection;

    public GameObject contextEditPrefab, outputEditPrefab, shapeEditPrefab;

    public static event System.Action NodeChanged;

    public Color bgShape, bgContext, bgOutput;
    public ActionNode cloneOriginal;
    public List<ActionNode> clones = new List<ActionNode>();
    public bool isTheOriginal = true;

    private Vector3 currentPos, prevPos;
    public float velocity;

    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
    }
    public float blend = 0;

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
        Animator.SetBool("Normal", !requiresEditing);
        currentPos = transform.position;
        prevPos = currentPos;
    }
    public void AddToLists()
    {
        if (!Book.currentEntry.activeAction.nodeParents.ContainsKey(this)) Book.currentEntry.activeAction.nodeParents.Add(this, WindowType.Collection);
        if (!Book.currentEntry.activeAction.nodePositions.ContainsKey(this)) Book.currentEntry.activeAction.nodePositions.Add(this, transform.position);
    }
    private void Start()
    {
        ResetIcons();
    }

    private void ResetIcons()
    {
        if (nodeType == NodeType.Context)
        {
            icon1.sprite = GameManager.instance.currentIconCollection.GetIcon(actionContext.context).icon;
            icon1.color = GameManager.instance.currentIconCollection.GetIcon(actionContext.context).iconColor;
            secondaryIcon = icon2.gameObject;
            icon2.sprite = questionMark;
            icon2.color = Color.white;
        }
        else if (nodeType == NodeType.Shape)
        {
            icon1.sprite = GameManager.instance.currentIconCollection.GetIcon(actionShape).icon;
            icon1.color = GameManager.instance.currentIconCollection.GetIcon(actionShape).iconColor;
            secondaryIcon = icon2.gameObject;
            icon2.sprite = questionMark;
            icon2.color = Color.white;
        }
        else
        {
            icon1.sprite = GameManager.instance.currentIconCollection.GetIcon(actionOutput.output).icon;
            icon1.color = GameManager.instance.currentIconCollection.GetIcon(actionOutput.output).iconColor;
            switch (actionOutput.output)
            {
                case Action.Output.Condition:
                case Action.Output.Healing:
                case Action.Output.Movement:
                    secondaryIcon = valueDisplay.gameObject;
                    valueDisplay.text = "0";
                    break;
                case Action.Output.Damage:
                    secondaryIcon = icon2.gameObject;
                    icon2.sprite = questionMark;
                    icon2.color = Color.white;
                    break;
            }
        }
        if (!isBig)
            secondaryIcon.SetActive(true);
        else
            secondaryIcon.SetActive(requiresEditing);
    }
    // Update is called once per frame
    void Update()
    {
        if (selected) Selected();
        else
        {
            if (transform.parent.name != "Content") transform.parent = originalWindow;
            if (!InEditor)
            {
                blend = 0;
                //LineManager.Instance.SeverConnections(this);
                actionContext.ResetInformation();
                actionOutput.ResetInformation();
                hasBeenEdited = false;
                isBig = false;
                ResetIcons();
            }
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
        if (windowType != wt)
        {

            switch (windowType)
            {
                case WindowType.Collection:
                case WindowType.Discard:
                    StartCoroutine(Expand(false));
                    //secondaryIcon.SetActive(true);
                    break;
                case WindowType.Edit1:
                case WindowType.Edit2:
                    
                    StartCoroutine(Expand(true));
                    break;
            }
        }
        wt = windowType;
        currentPos = transform.position;
        velocity = (((currentPos - prevPos).sqrMagnitude)+velocity)/2;
        SoundManager.Drag(velocity);
        prevPos = currentPos;
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
                if (hit.gameObject.name.Contains("Edit1")) windowType = WindowType.Edit1;
                else if (hit.gameObject.name.Contains("Edit2")) windowType = WindowType.Edit2;
                else if (hit.gameObject.name.Contains("Discard")) windowType = WindowType.Discard;
                else windowType = WindowType.Collection;
                return hit.gameObject.transform.Find("Node Holder").Find("Viewport").Find("Content");
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
        //LineManager.Instance.MovingNode = true;
        //LineManager.Instance.NodeMoving = this;
    }
    private void OnDeselect()
    {
        SoundManager.StopDrag();
        selected = false;
        Relocate(originalWindow, window, transform.position);
        return;

        #region bad solution
        if (nodeType == NodeType.Output || nodeType == NodeType.Shape)
        {
            if (window != null)
            {
                if (window.parent.parent.parent.name.Contains("Edit"))
                {
                    if (originalWindow.parent.parent.parent.name.Contains("Collection"))
                    {
                        if (nodeType == NodeType.Shape)
                        {
                            Book.currentEntry.activeAction.nodeParents[this] = windowType;
                            Book.currentEntry.activeAction.nodePositions[this] = transform.position;
                        }
                        Clone(transform.position, window, nodeType == NodeType.Output ? actionOutput : null);
                        transform.parent = originalWindow;
                        transform.position = originalLocation;
                        blend = 0;
                        secondaryIcon.SetActive(true);                        
                    }
                    else if (originalWindow.parent.parent.parent.name.Contains("Discard"))
                    {
                        transform.parent = ActionEditor.instance.collectionHolder;
                        Clone(transform.position, window, actionOutput);
                        Book.currentEntry.activeAction.nodeParents[this] = windowType;
                        Book.currentEntry.activeAction.nodePositions[this] = transform.position;
                    }
                    else
                    {
                        transform.parent = window;
                        Book.currentEntry.activeAction.nodeParents[this] = windowType;
                        Book.currentEntry.activeAction.nodePositions[this] = transform.position;
                    }
                    if (windowType == WindowType.Edit1 && originalWindow.parent.parent.parent.name.Contains("Edit2"))
                    {
                        switch (nodeType)
                        {
                            case NodeType.Output:
                                Book.currentEntry.activeAction.guessAction.secondaryOutput.Remove(actionOutput);
                                Book.currentEntry.activeAction.guessAction.primaryOutput.Add(actionOutput);
                                break;
                            case NodeType.Shape:
                                Book.currentEntry.activeAction.guessAction.primaryShape = actionShape;
                                break;
                        }
                    }
                    else if (windowType == WindowType.Edit1 && !originalWindow.parent.parent.parent.name.Contains("Edit1"))
                    {
                        switch (nodeType)
                        {
                            case NodeType.Output:
                                Book.currentEntry.activeAction.guessAction.primaryOutput.Add(actionOutput);
                                break;
                            case NodeType.Shape:
                                Book.currentEntry.activeAction.guessAction.primaryShape = actionShape;
                                break;
                        }                        
                    }
                    if (windowType == WindowType.Edit2 && originalWindow.parent.parent.parent.name.Contains("Edit1"))
                    {
                        switch (nodeType)
                        {
                            case NodeType.Output:
                                Book.currentEntry.activeAction.guessAction.secondaryOutput.Add(actionOutput);
                                Book.currentEntry.activeAction.guessAction.primaryOutput.Remove(actionOutput);
                                break;
                            case NodeType.Shape:
                                Book.currentEntry.activeAction.guessAction.secondaryShape = actionShape;
                                break;
                        }
                    }
                    else if (windowType == WindowType.Edit2 && !originalWindow.parent.parent.parent.name.Contains("Edit2"))
                    {
                        switch (nodeType)
                        {
                            case NodeType.Output:
                                Book.currentEntry.activeAction.guessAction.secondaryOutput.Add(actionOutput);
                                break;
                            case NodeType.Shape:
                                Book.currentEntry.activeAction.guessAction.secondaryShape = actionShape;
                                break;
                        }
                    }
                }
                else if (window.parent.parent.parent.name.Contains("Discard"))
                {
                    transform.parent = window;
                    switch (nodeType)
                    {
                        case NodeType.Output:
                            if (originalWindow.parent.parent.parent.name.Contains("Edit1")) Book.currentEntry.activeAction.guessAction.primaryOutput.Remove(actionOutput);
                            else if (originalWindow.parent.parent.parent.name.Contains("Edit2")) Book.currentEntry.activeAction.guessAction.secondaryOutput.Remove(actionOutput);
                            break;
                        case NodeType.Shape:
                            break;
                    }
                    if (cloneOriginal != null)
                    {
                        Book.currentEntry.activeAction.nodeParents[cloneOriginal] = WindowType.Discard;
                        Book.currentEntry.activeAction.nodePositions[cloneOriginal] = transform.position;
                        cloneOriginal.gameObject.transform.parent = ActionEditor.instance.discardHolder;
                        cloneOriginal.transform.position = transform.position;
                        Destroy(gameObject);
                    }
                }
                else if (window.parent.parent.parent.name.Contains("Collection"))
                {
                    switch (nodeType)
                    {
                        case NodeType.Output:
                            if (originalWindow.parent.parent.parent.name.Contains("Edit1")) Book.currentEntry.activeAction.guessAction.primaryOutput.Remove(actionOutput);
                            else if (originalWindow.parent.parent.parent.name.Contains("Edit2")) Book.currentEntry.activeAction.guessAction.secondaryOutput.Remove(actionOutput);
                            break;
                        case NodeType.Shape:
                            break;
                    }
                    if (originalWindow.parent.parent.parent.name.Contains("Edit"))
                    {
                        Book.currentEntry.activeAction.nodeParents[cloneOriginal] = WindowType.Collection;
                        Book.currentEntry.activeAction.nodePositions[cloneOriginal] = transform.position;
                        NodeChanged.Invoke();
                        Destroy(gameObject);
                        return;
                    }
                    else
                    {
                        transform.parent = window;
                        Book.currentEntry.activeAction.nodeParents[this] = windowType;
                        Book.currentEntry.activeAction.nodePositions[this] = transform.position;
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
                if (window.parent.parent.parent.name.Contains("Edit"))
                {
                    switch (nodeType)
                    {
                        case NodeType.Context:
                            if (windowType == WindowType.Edit1 && !originalWindow.parent.parent.parent.name.Contains("Edit1")) Book.currentEntry.activeAction.guessAction.primaryContext.Add(actionContext);
                            break;
                        case NodeType.Shape:
                            if (windowType == WindowType.Edit1) Book.currentEntry.activeAction.guessAction.primaryShape = actionShape;
                            else if (windowType == WindowType.Edit2) Book.currentEntry.activeAction.guessAction.secondaryShape = actionShape;
                            break;
                    }
                }
                else
                {
                    switch (nodeType)
                    {
                        case NodeType.Context:
                            if (originalWindow.parent.parent.parent.name.Contains("Edit1")) Book.currentEntry.activeAction.guessAction.primaryContext.Remove(actionContext);
                            break;
                        case NodeType.Shape:
                            if (originalWindow.parent.parent.parent.name.Contains("Edit1")) Book.currentEntry.activeAction.guessAction.primaryShape = Action.Shape.Melee;
                            else if (originalWindow.parent.parent.parent.name.Contains("Edit2")) Book.currentEntry.activeAction.guessAction.secondaryShape = Action.Shape.Melee;
                            break;
                    }
                }
                transform.parent = window;
                Book.currentEntry.activeAction.nodeParents[this] = windowType;
                Book.currentEntry.activeAction.nodePositions[this] = transform.position;

            }
        }
        GetComponent<Image>().raycastTarget = true;
        
        window = null;
        //LineManager.Instance.MovingNode = false;
        // REWRITE THIS TO UPDATE THE ICON INSTEAD / OTHER JUICE
        // StartCoroutine(MoveConnectors(!InEditor));
        if (!InEditor)
        {
            //LineManager.Instance.SeverConnections(this);
            actionContext.ResetInformation();
            actionOutput.ResetInformation();
            hasBeenEdited = false;
            ResetIcons();
        }
        NodeChanged.Invoke();
        #endregion
    }
    public void Relocate(Transform startWindow, Transform endWindow, Vector3 position)
    {
        if (endWindow == null)
        {
            transform.parent = startWindow;
            transform.position = originalLocation != null ? originalLocation : Vector2.zero;
            GetComponent<Image>().raycastTarget = true;
            if (!InEditor)
            {
                //LineManager.Instance.SeverConnections(this);
                actionContext.ResetInformation();
                actionOutput.ResetInformation();
                hasBeenEdited = false;
                ResetIcons();
            }
            NodeChanged.Invoke();
            return;
        }
        WindowType start = WindowType.Collection, end = WindowType.Collection;
        if (startWindow.parent.parent.parent.name.Contains("Edit1")) start = WindowType.Edit1;
        else if (startWindow.parent.parent.parent.name.Contains("Edit2")) start = WindowType.Edit2;
        else if (startWindow.parent.parent.parent.name.Contains("Collection")) start = WindowType.Collection;
        else start = WindowType.Discard;
        if (endWindow.parent.parent.parent.name.Contains("Edit1")) end = WindowType.Edit1;
        else if (endWindow.parent.parent.parent.name.Contains("Edit2")) end = WindowType.Edit2;
        else if (endWindow.parent.parent.parent.name.Contains("Collection")) end = WindowType.Collection;
        else end = WindowType.Discard;

        switch (start)
        {
            case WindowType.Collection:
                switch (end)
                {
                    case WindowType.Collection:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Shape:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                        }
                        break;
                    case WindowType.Edit1:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                ResetIcons();
                                StartCoroutine(Expand(true));
                                break;
                            case NodeType.Output:
                                if (isTheOriginal)
                                {
                                    transform.parent = ActionEditor.instance.collectionHolder;
                                    Clone(position, endWindow, actionOutput).Relocate(startWindow, endWindow, position);
                                    ResetIcons();
                                }
                                else
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                }
                                break;
                            case NodeType.Shape:
                                if (isTheOriginal)
                                {
                                    transform.parent = ActionEditor.instance.collectionHolder;
                                    Clone(position, endWindow, null).Relocate(startWindow, endWindow, position);
                                    ResetIcons();
                                }
                                else
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                }
                                break;
                        }
                        break;
                    case WindowType.Edit2:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                ResetIcons();
                                StartCoroutine(Expand(true));
                                break;
                            case NodeType.Output:
                                if (isTheOriginal)
                                {
                                    transform.parent = ActionEditor.instance.collectionHolder;
                                    Clone(position, endWindow, actionOutput).Relocate(startWindow, endWindow, position);
                                    ResetIcons();
                                }
                                else
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                }
                                break;
                            case NodeType.Shape:
                                if (isTheOriginal)
                                {
                                    transform.parent = ActionEditor.instance.collectionHolder;
                                    Clone(position, endWindow, actionOutput).Relocate(startWindow, endWindow, position);
                                    ResetIcons();
                                }
                                else
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                }
                                break;
                        }
                        break;
                    case WindowType.Discard:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                if (isTheOriginal)
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                    for (int i = 0; i < clones.Count; i++)
                                    {
                                        Destroy(clones[i].gameObject);
                                        clones.RemoveAt(i);
                                        i--;
                                    }
                                }
                                else
                                {
                                    cloneOriginal.Relocate(startWindow, endWindow, transform.position);
                                }
                                break;
                            case NodeType.Shape:
                                if (isTheOriginal)
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                    for (int i = 0; i < clones.Count; i++)
                                    {
                                        Destroy(clones[i].gameObject);
                                        clones.RemoveAt(i);
                                        i--;
                                    }
                                }
                                else
                                {
                                    cloneOriginal.Relocate(startWindow, endWindow, transform.position);
                                }
                                break;
                        }
                        break;                    
                }
                break;
            case WindowType.Edit1:
                switch (end)
                {
                    case WindowType.Collection:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                cloneOriginal.clones.Remove(this);
                                cloneOriginal.actionContext.ResetInformation();
                                cloneOriginal.actionOutput.ResetInformation();
                                cloneOriginal.hasBeenEdited = false;
                                cloneOriginal.ResetIcons();
                                Destroy(gameObject);
                                break;
                            case NodeType.Shape:
                                cloneOriginal.clones.Remove(this);
                                cloneOriginal.actionContext.ResetInformation();
                                cloneOriginal.actionOutput.ResetInformation();
                                cloneOriginal.hasBeenEdited = false;
                                cloneOriginal.ResetIcons();
                                Destroy(gameObject);
                                break;
                        }
                        break;
                    case WindowType.Edit1:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Shape:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                        }
                        break;
                    case WindowType.Edit2:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Shape:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                        }
                        break;
                    case WindowType.Discard:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                if (isTheOriginal)
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                    for (int i = 0; i < clones.Count; i++)
                                    {
                                        Destroy(clones[i].gameObject);
                                        clones.RemoveAt(i);
                                        i--;
                                    }
                                }
                                else
                                {
                                    cloneOriginal.Relocate(startWindow, endWindow, transform.position);
                                }
                                break;
                            case NodeType.Shape:
                                if (isTheOriginal)
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                    for (int i = 0; i < clones.Count; i++)
                                    {
                                        Destroy(clones[i].gameObject);
                                        clones.RemoveAt(i);
                                        i--;
                                    }
                                }
                                else
                                {
                                    cloneOriginal.Relocate(startWindow, endWindow, transform.position);
                                }
                                break;
                        }
                        break;
                }
                break;
            case WindowType.Edit2:
                switch (end)
                {
                    case WindowType.Collection:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                cloneOriginal.clones.Remove(this);
                                cloneOriginal.actionContext.ResetInformation();
                                cloneOriginal.actionOutput.ResetInformation();
                                cloneOriginal.hasBeenEdited = false;
                                cloneOriginal.ResetIcons();
                                Destroy(gameObject);
                                break;
                            case NodeType.Shape:
                                cloneOriginal.clones.Remove(this);
                                cloneOriginal.actionContext.ResetInformation();
                                cloneOriginal.actionOutput.ResetInformation();
                                cloneOriginal.hasBeenEdited = false;
                                cloneOriginal.ResetIcons();
                                Destroy(gameObject);
                                break;
                        }
                        break;
                    case WindowType.Edit1:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Shape:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                        }
                        break;
                    case WindowType.Edit2:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Shape:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                        }
                        break;
                    case WindowType.Discard:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                if (isTheOriginal)
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                    for (int i = 0; i < clones.Count; i++)
                                    {
                                        Destroy(clones[i].gameObject);
                                        clones.RemoveAt(i);
                                        i--;
                                    }
                                }
                                else
                                {
                                    cloneOriginal.Relocate(startWindow, endWindow, transform.position);
                                }
                                break;
                            case NodeType.Shape:
                                if (isTheOriginal)
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                    for (int i = 0; i < clones.Count; i++)
                                    {
                                        Destroy(clones[i].gameObject);
                                        clones.RemoveAt(i);
                                        i--;
                                    }
                                }
                                else
                                {
                                    cloneOriginal.Relocate(startWindow, endWindow, transform.position);
                                }
                                break;
                        }
                        break;
                }
                break;
            case WindowType.Discard:
                switch (end)
                {
                    case WindowType.Collection:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Shape:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                        }
                        break;
                    case WindowType.Edit1:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                if (isTheOriginal)
                                {
                                    transform.parent = ActionEditor.instance.collectionHolder;
                                    Clone(position, endWindow, actionOutput).Relocate(startWindow, endWindow, position);
                                }
                                else
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                }
                                break;
                            case NodeType.Shape:
                                if (isTheOriginal)
                                {
                                    transform.parent = ActionEditor.instance.collectionHolder;
                                    Clone(position, endWindow, null).Relocate(startWindow, endWindow, position);
                                }
                                else
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                }
                                break;
                        }
                        break;
                    case WindowType.Edit2:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                if (isTheOriginal)
                                {
                                    transform.parent = ActionEditor.instance.collectionHolder;
                                    Clone(position, endWindow, actionOutput).Relocate(startWindow, endWindow, position);
                                }
                                else
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                }
                                break;
                            case NodeType.Shape:
                                if (isTheOriginal)
                                {
                                    transform.parent = ActionEditor.instance.collectionHolder;
                                    Clone(position, endWindow, null).Relocate(startWindow, endWindow, position);
                                }
                                else
                                {
                                    transform.parent = endWindow;
                                    Book.currentEntry.activeAction.nodeParents[this] = end;
                                    Book.currentEntry.activeAction.nodePositions[this] = position;
                                }
                                break;
                        }
                        break;
                    case WindowType.Discard:
                        switch (nodeType)
                        {
                            case NodeType.Context:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Output:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                            case NodeType.Shape:
                                transform.parent = endWindow;
                                Book.currentEntry.activeAction.nodeParents[this] = end;
                                Book.currentEntry.activeAction.nodePositions[this] = position;
                                break;
                        }
                        break;
                }
                break;
        }
        GetComponent<Image>().raycastTarget = true;

        window = null;

        NodeChanged.Invoke();
    }
    public ActionNode Clone(Vector3 position, Transform parent, OutputInfo oi)
    {
        GameObject clone = Instantiate(gameObject, position, Quaternion.identity, parent);
        clone.GetComponent<Image>().raycastTarget = true;
        ActionNode an = clone.GetComponent<ActionNode>();
        an.blend = 1;
        an.isBig = true;
        an.isTheOriginal = false;
        an.cloneOriginal = this;
        clones.Add(an);
        if (oi != null)
        {
            an.actionOutput = new OutputInfo(oi);
            an.DelaySetIcons();
        }
        NodeChanged.Invoke();
        //Book.currentEntry.activeAction.nodeParents.Add(clone.GetComponent<ActionNode>(), parent.parent.parent.parent.name.Contains("Edit1") ? WindowType.Edit1 : WindowType.Edit2);
        //Book.currentEntry.activeAction.nodePositions.Add(clone.GetComponent<ActionNode>(), position);
        return an;
    }
    public void DelaySetIcons()
    {
        StartCoroutine(OneFrameDelay());
    }
    IEnumerator OneFrameDelay()
    {
        yield return null;
        SetIcons();
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
    public void ExternalExpand()
    {
        StartCoroutine(Expand(true));
    }
    IEnumerator Expand(bool expand)
    {
        secondaryIcon.SetActive(requiresEditing && expand);
        float goal = expand ? 1 : 0;
        float t = 0;
        isBig = expand;
        while (t < 1)
        {
            blend = Mathf.Lerp(blend, goal, t);
            t += Time.deltaTime * 2;
            yield return null;
        }
        //Canvas.ForceUpdateCanvases();
        //LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
    private void EditNode()
    {

        GameObject newWindow;
        switch (nodeType)
        {
            case NodeType.Shape:
                newWindow = Instantiate(shapeEditPrefab, canvas);
                ShapeEdit sEdit = newWindow.GetComponent<ShapeEdit>();
                sEdit.node = this;
                sEdit.ShowEditing();
                break;
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

        if (hasBeenEdited)
        {
            SetIcons(); 
            /*
            switch (nodeType)
            {
                case NodeType.Context:
                    for (int i = 0; i < Book.currentEntry.activeAction.guessAction.primaryContext.Count; i++)
                    {
                        if (Book.currentEntry.activeAction.guessAction.primaryContext[i].context == actionContext.context)
                        {
                            Book.currentEntry.activeAction.guessAction.primaryContext[i].damageType = actionContext.damageType;
                            Book.currentEntry.activeAction.guessAction.primaryContext[i].condition = actionContext.condition;
                        }
                    }
                    break;
                case NodeType.Output:
                    if (windowType == WindowType.Edit1)
                    {
                        for (int i = 0; i < Book.currentEntry.activeAction.guessAction.primaryOutput.Count; i++)
                        {
                            if (Book.currentEntry.activeAction.guessAction.primaryOutput[i].output== actionOutput.output)
                            {
                                Book.currentEntry.activeAction.guessAction.primaryOutput[i].damageType = actionOutput.damageType;
                                Book.currentEntry.activeAction.guessAction.primaryOutput[i].condition = actionOutput.condition;
                                Book.currentEntry.activeAction.guessAction.primaryOutput[i].value = actionOutput.value;
                                Book.currentEntry.activeAction.guessAction.primaryOutput[i].towards = actionOutput.towards;
                                Book.currentEntry.activeAction.guessAction.primaryOutput[i].critical = actionOutput.critical;
                            }
                        }
                    }
                    else if (windowType == WindowType.Edit2)
                    {
                        for (int i = 0; i < Book.currentEntry.activeAction.guessAction.secondaryOutput.Count; i++)
                        {
                            if (Book.currentEntry.activeAction.guessAction.secondaryOutput[i].output == actionOutput.output)
                            {
                                Book.currentEntry.activeAction.guessAction.secondaryOutput[i].damageType = actionOutput.damageType;
                                Book.currentEntry.activeAction.guessAction.secondaryOutput[i].condition = actionOutput.condition;
                                Book.currentEntry.activeAction.guessAction.secondaryOutput[i].value = actionOutput.value;
                                Book.currentEntry.activeAction.guessAction.secondaryOutput[i].towards = actionOutput.towards;
                                Book.currentEntry.activeAction.guessAction.secondaryOutput[i].critical = actionOutput.critical;
                            }
                        }
                    }
                    break;
                case NodeType.Shape:                    
                    // Add functionality for target priority
                    break;
            }
            */
        }
        NodeChanged.Invoke();
    }
    public void SetIcons()
    {
        switch (nodeType)
        {
            case NodeType.Context:                
                switch (actionContext.context)
                {
                    default: break;
                    case Action.Context.TookDamageOfType:
                        icon2.sprite = GameManager.instance.currentIconCollection.GetIcon(actionContext.damageType).icon;
                        icon2.color = GameManager.instance.currentIconCollection.GetIcon(actionContext.damageType).iconColor;
                        break;
                    case Action.Context.AllyHasSpecificCondition:
                    case Action.Context.EnemyHasSpecificCondition:
                    case Action.Context.ReceivedSpecificCondition:
                    case Action.Context.SelfHasSpecificCondition:
                        icon2.sprite = GameManager.instance.currentIconCollection.GetIcon(actionContext.condition).icon;
                        icon2.color = GameManager.instance.currentIconCollection.GetIcon(actionContext.condition).iconColor;
                        break;
                }
                break;
            case NodeType.Output:
                switch (actionOutput.output)
                {
                    case Action.Output.Damage:
                        crit.SetActive(actionOutput.critical);
                        icon2.sprite = GameManager.instance.currentIconCollection.GetIcon(actionOutput.damageType).icon;
                        icon2.color = GameManager.instance.currentIconCollection.GetIcon(actionOutput.damageType).iconColor;
                        break;
                    case Action.Output.Healing:
                        valueDisplay.text = actionOutput.value.ToString();
                        break;
                    case Action.Output.Condition:
                        icon1.sprite = GameManager.instance.currentIconCollection.GetIcon(actionOutput.condition).icon;
                        icon1.color = GameManager.instance.currentIconCollection.GetIcon(actionOutput.condition).iconColor;
                        valueDisplay.text = actionOutput.value.ToString();
                        break;
                    case Action.Output.Movement:
                        valueDisplay.text = actionOutput.value.ToString();
                        break;
                }
                break;
            case NodeType.Shape:
                break;
        }
        hasBeenEdited = true;
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
