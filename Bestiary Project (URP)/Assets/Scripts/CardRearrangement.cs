using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardRearrangement : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform cardHolder;
    public Transform bookCanvas;
    private Vector2 clickOffset;
    public GameObject empty;
    private GameObject e;
    private List<RaycastResult> hits = new List<RaycastResult>();
    public TextMeshProUGUI priorityNumber;
    public Page page;
    public ActionCheck actionCheck;
    private bool dragging = false;
    public int priorityIndex = 0;

    public Transform iconGrid;
    public GameObject iconPrefab;
    public List<GameObject> icons = new List<GameObject>();

    public Image panel;
    public List<Color> panelColors = new List<Color>();
    public Color panelColor;

    public TextMeshProUGUI description;
    public TextMeshProUGUI actionName;
    private bool editable = true;

    int frameCount = 0;

    private void Start()
    {
        cardHolder = transform.parent;
        bookCanvas = cardHolder.parent;
        page.ActionPriorityUpdate();
        panel.color = panelColor;
    }
    private void OnEnable()
    {
        BookActionCard.CardUpdated += UpdateIcons;
        UpdateIcons(false);
    }
    private void OnDisable()
    {
        BookActionCard.CardUpdated -= UpdateIcons;
    }

    private void Update()
    {
        if (Book.currentEntry.isMerc)
        {
            editable = false;
        }
        else editable = true;

        if (frameCount >= 10)
        {
            UpdateIcons(Book.currentEntry.isMerc);
            frameCount = 0;
        }
        frameCount++;

        priorityIndex = dragging ? e.transform.GetSiblingIndex() +1 : transform.GetSiblingIndex() + 1;
        priorityIndex = Mathf.Clamp(priorityIndex, 1,4);
        priorityNumber.text = priorityIndex.ToString();
    }

    public void UpdateIcons(bool merc)
    {
        if(!merc)
            if (Book.currentEntry.activeAction != actionCheck) return;
        description.text = actionCheck.guessAction.description;
        actionName.text = actionCheck.guessAction.actionName != null ? actionCheck.guessAction.actionName : "Unnamed Action";
        for (int i = 0; i < icons.Count; i++)
        {
            Destroy(icons[i]);
            icons.RemoveAt(i);
            i--;
        }
        if (actionCheck.guessAction.targetingSet)
        {
            /* Rewrite as dictionary
            switch (actionCheck.guessAction.primaryShape)
            {
                case Action.Shape.Self:
                    AddIcon(GameManager.instance.currentIconCollection.self);
                    break;
                case Action.Shape.Melee:
                    AddIcon(GameManager.instance.currentIconCollection.meleeVert);
                    AddIcon(GameManager.instance.currentIconCollection.meleeDia);
                    break;
                case Action.Shape.Ranged:
                    AddIcon(GameManager.instance.currentIconCollection.ranged);
                    break;
                case Action.Shape.Line:
                    AddIcon(GameManager.instance.currentIconCollection.lineVert);
                    AddIcon(GameManager.instance.currentIconCollection.lineDia);
                    break;
                case Action.Shape.Arc:
                    AddIcon(GameManager.instance.currentIconCollection.arcVert);
                    AddIcon(GameManager.instance.currentIconCollection.arcDia);
                    break;
                case Action.Shape.Cone:
                    AddIcon(GameManager.instance.currentIconCollection.coneVert);
                    AddIcon(GameManager.instance.currentIconCollection.coneDia);
                    break;
                case Action.Shape.Area:
                    AddIcon(GameManager.instance.currentIconCollection.box);
                    break;
                case Action.Shape.Pulse:
                    AddIcon(GameManager.instance.currentIconCollection.selfBox);
                    break;
            } */
        }
        if (actionCheck.guessAction.outcomeSet)
        {
            /* Rewrite as dictionary
            if (actionCheck.guessAction.actionType == Action.ActionType.Attack || actionCheck.guessAction.actionType == Action.ActionType.AttackDebuff)
            {
                switch (actionCheck.guessAction.damageType)
                {
                    case Character.DamageTypes.Cutting:
                        AddIcon(GameManager.instance.currentIconCollection.cutting);
                        break;
                    case Character.DamageTypes.Crushing:
                        AddIcon(GameManager.instance.currentIconCollection.crushing);
                        break;
                    case Character.DamageTypes.Piercing:
                        AddIcon(GameManager.instance.currentIconCollection.piercing);
                        break;
                    case Character.DamageTypes.Acid:
                        AddIcon(GameManager.instance.currentIconCollection.acid);
                        break;
                    case Character.DamageTypes.Fire:
                        AddIcon(GameManager.instance.currentIconCollection.fire);
                        break;
                    case Character.DamageTypes.Poison:
                        AddIcon(GameManager.instance.currentIconCollection.poison);
                        break;
                    case Character.DamageTypes.Cold:
                        AddIcon(GameManager.instance.currentIconCollection.cold);
                        break;
                }
            }
            else if (actionCheck.guessAction.actionType == Action.ActionType.Healing || actionCheck.guessAction.actionType == Action.ActionType.HealingBuff)
            {
                AddIcon(GameManager.instance.currentIconCollection.healing);
            }
            if (actionCheck.guessAction.actionType == Action.ActionType.AttackDebuff || actionCheck.guessAction.actionType == Action.ActionType.Debuff)
            {
                switch (actionCheck.guessAction.debuff.debuffType)
                {
                    case Debuff.DebuffType.Control:
                        switch (actionCheck.guessAction.debuff.controlType)
                        {
                            case Debuff.ControlType.Blind:
                                AddIcon(GameManager.instance.currentIconCollection.blind);
                                break;
                            case Debuff.ControlType.Root:
                                AddIcon(GameManager.instance.currentIconCollection.root);
                                break;
                            case Debuff.ControlType.Slow:
                                AddIcon(GameManager.instance.currentIconCollection.slow);
                                break;
                        }
                        break;
                    case Debuff.DebuffType.DamageOverTime:
                        switch (actionCheck.guessAction.debuff.damageType)
                        {
                            case Character.DamageTypes.Cutting:
                                AddIcon(GameManager.instance.currentIconCollection.cutting);
                                break;
                            case Character.DamageTypes.Crushing:
                                AddIcon(GameManager.instance.currentIconCollection.crushing);
                                break;
                            case Character.DamageTypes.Piercing:
                                AddIcon(GameManager.instance.currentIconCollection.piercing);
                                break;
                            case Character.DamageTypes.Acid:
                                AddIcon(GameManager.instance.currentIconCollection.acidDebuff);
                                break;
                            case Character.DamageTypes.Fire:
                                AddIcon(GameManager.instance.currentIconCollection.burning);
                                break;
                            case Character.DamageTypes.Poison:
                                AddIcon(GameManager.instance.currentIconCollection.poisonDebuff);
                                break;
                            case Character.DamageTypes.Cold:
                                AddIcon(GameManager.instance.currentIconCollection.cold);
                                break;
                        }
                        break;
                }
            }
            if (actionCheck.guessAction.actionType == Action.ActionType.Buff || actionCheck.guessAction.actionType == Action.ActionType.HealingBuff)
            {
                switch (actionCheck.guessAction.buff.buffType)
                {
                    case Buff.BuffType.Armor:
                        AddIcon(GameManager.instance.currentIconCollection.armor);
                        break;
                    case Buff.BuffType.Dodge:
                        AddIcon(GameManager.instance.currentIconCollection.dodge);
                        break;
                }
            }*/
        }
    }

    public void AddIcon(Sprite sprite)
    {
        GameObject newIcon = Instantiate(iconPrefab, iconGrid);
        newIcon.GetComponent<Image>().sprite = sprite;
        icons.Add(newIcon);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!editable) return;
        clickOffset = eventData.position - (Vector2)transform.position;
        e = Instantiate(empty);
        e.transform.parent = cardHolder;
        e.transform.SetSiblingIndex(transform.GetSiblingIndex());
        transform.parent = bookCanvas;
        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!editable) return;

        transform.position = eventData.position - clickOffset;
        hits.Clear();
        Book.GR.Raycast(eventData, hits);
        foreach(RaycastResult hit in hits)
        {
            if (hit.gameObject.CompareTag("ActionCard"))
            {
                if (hit.gameObject != gameObject)// && hit.gameObject != currentHover)
                {
                    Rearrange(hit);
                }
            }
        }
    }

    public void Rearrange(RaycastResult hit)
    {
        int targetIndex = hit.gameObject.transform.GetSiblingIndex();
        int emptyIndex = e.transform.GetSiblingIndex();

        for (int i = emptyIndex - 1; i > targetIndex - 1; i--)
        {
            cardHolder.GetChild(i).SetSiblingIndex(i + 1);
        }
        for (int i = emptyIndex+1; i < targetIndex+1; i++)
        {
            cardHolder.GetChild(i).SetSiblingIndex(i - 1);
        }
        


        empty.transform.SetSiblingIndex(targetIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!editable) return;

        int index = e.transform.GetSiblingIndex();
        Destroy(e);
        transform.parent = cardHolder;
        transform.SetSiblingIndex(index);
        dragging = false;
        StartCoroutine(Delay(page.gameObject, "ActionPriorityUpdate"));
    }

    IEnumerator Delay(GameObject target, string message)
    {
        yield return null;
        target.SendMessage(message);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!editable) return;

        StartCoroutine(ClickDelay());
    }
    IEnumerator ClickDelay()
    {
        while (!dragging)
        {
            if (Input.GetMouseButtonUp(0))
            {
                Book.currentEntry.activeAction = actionCheck;
                Book.OpenActionEditing();
                break;
            }
            yield return null;
        }
    }
}
