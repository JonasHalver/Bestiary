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
    public Entry entry;
    public ActionCheck actionCheck;
    private bool dragging = false;
    public int priorityIndex = 0;

    public Transform iconGrid;
    public GameObject iconPrefab;
    public List<GameObject> icons = new List<GameObject>();

    public Image panel;
    public List<Color> panelColors = new List<Color>();
    public Color panelColor;

    public Color confirmedColor;
    public Image background;

    public TextMeshProUGUI description;
    public TextMeshProUGUI actionName;
    private bool editable = true;
    public GameObject delete;

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
        //BookActionCard.CardUpdated += UpdateIcons;
        UpdateIcons(false);
    }
    private void OnDisable()
    {
       // BookActionCard.CardUpdated -= UpdateIcons;
    }

    private void Update()
    {
        if (Book.currentEntry.isMerc)
        {
            editable = false;
            delete.SetActive(false);
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
    public void RemoveCard()
    {
        page.RemoveCard(this);
    }
    public void UpdateIcons(bool merc)
    {
        if(!merc)
            if (Book.currentEntry.activeAction != actionCheck) return;
        if (actionCheck != null)
            description.text = actionCheck.guessAction.description;
        else return;
        actionName.text = actionCheck.guessAction.actionName != null ? actionCheck.guessAction.actionName : "Unnamed Action";
        for (int i = 0; i < icons.Count; i++)
        {
            Destroy(icons[i]);
            icons.RemoveAt(i);
            i--;
        }
        if (actionCheck.informationConfirmed) background.color = confirmedColor;
        else background.color = Color.white;
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
