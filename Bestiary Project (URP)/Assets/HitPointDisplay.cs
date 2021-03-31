using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPointDisplay : MonoBehaviour
{
    [Range(0,50)]
    public float value = 1;
    private float previousValue = 0;
    [Range(0, 50)]
    public float damageTaken = 0;
    private float previousDamageTaken = 0;
    public bool displayHalf;
    [Header ("Prefabs")]
    public GameObject hitPointPrefab, halfHitPrefab, emptyHitPointPrefab, hitPointHolder, compacterPrefab, emptyPrefab;
    [Header("Spawned Objects")]
    public List<HitPoint> hearts = new List<HitPoint>();
    public List<GameObject> emptyHearts = new List<GameObject>();
    private GameObject newHeart, newCompact, newEmpty;
    public List<GameObject> fullHearts = new List<GameObject>();
    public List<GameObject> displayedHearts = new List<GameObject>();
    public List<GameObject> compactFull = new List<GameObject>();
    public List<GameObject> compactEmpty = new List<GameObject>();
    public float displayedValue =0;
    public bool shouldBeUpdatingHearts = false;
    private List<GameObject> temp = new List<GameObject>();
    [SerializeField] private List<GameObject> emptiesE = new List<GameObject>();
    [SerializeField] private List<GameObject> emptiesF = new List<GameObject>();
    private int fullHeartsCount, emptyHeartsCount, halfHeartCount;
    private bool spawningHearts = false;
    private int runCount = 0;
    public bool editing = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        value = Mathf.Clamp(value, 1, 50);
        if (!editing)
        {
            if (!spawningHearts)
            {
                if (previousValue != value)
                {
                    StartCoroutine(HeartsCoroutine());
                    previousValue = value;
                }
                else if (previousDamageTaken != damageTaken)
                {
                    StartCoroutine(HeartsCoroutine());
                    previousDamageTaken = damageTaken;
                }
            }
        }
    }

    public void SpawnHearts()
    {
        // Spawning hearts
        if (hearts.Count < value)
        {
            newHeart = Instantiate(hitPointPrefab, hitPointHolder.transform);
            hearts.Add(newHeart.GetComponent<HitPoint>());
            displayedHearts.Add(newHeart);
            displayedValue++;
        }
        if (hearts.Count > value)
        {
            ClearHearts();
            return;
        }
        for (int i = 0; i < hearts.Count; i++)
        {
            if (hearts[i].isFull) fullHearts.Add(hearts[i].gameObject);
        }
        if (displayedValue != value - damageTaken && (value - damageTaken) > -1)
        {
            shouldBeUpdatingHearts = true;
            emptyHearts.Clear();
            for (int i = 0; i < hearts.Count; i++)
            {
                hearts[i].current = hearts[i].full;
                hearts[i].isFull = true; hearts[i].isHalf = false; hearts[i].isEmpty = false;
                if (!fullHearts.Contains(hearts[i].gameObject)) fullHearts.Add(hearts[i].gameObject);
            }
            int doubleDamageTaken = Mathf.RoundToInt(damageTaken * 2);
            int count = hearts.Count - 1;
            for (int i = 0; i < doubleDamageTaken; i++)
            {
                if (count > -1)
                {
                    if (i % 2 == 0)
                    {
                        hearts[count].current = hearts[count].half;
                        hearts[count].isFull = false; hearts[count].isHalf = true; hearts[count].isEmpty = false;
                        if (fullHearts.Contains(hearts[count].gameObject)) fullHearts.Remove(hearts[count].gameObject);
                    }
                    else
                    {
                        hearts[count].current = hearts[count].empty;
                        hearts[count].isFull = false; hearts[count].isHalf = false; hearts[count].isEmpty = true;
                        if (fullHearts.Contains(hearts[count].gameObject)) fullHearts.Remove(hearts[count].gameObject);
                        emptyHearts.Add(hearts[count].gameObject);
                        count--;
                    }
                    displayedValue -= 0.5f;
                }
            }
        }
        else shouldBeUpdatingHearts = false;
        displayedValue = Mathf.Clamp(displayedValue, 0, 50);

        // Compacting
        if (displayedHearts.Count > 5)
        {
            int emptyCount = 0, fullcount = 0;
            for (int i = 0; i < displayedHearts.Count; i++)
            {
                if (displayedHearts[i].GetComponent<HitPoint>().isFull) fullcount++;
                else if (displayedHearts[i].GetComponent<HitPoint>().isEmpty) emptyCount++;
            }
            if (fullcount > 5)
            {

                int orderIndex = displayedHearts[0].transform.GetSiblingIndex();
                newCompact = Instantiate(compacterPrefab, hitPointHolder.transform);
                compactFull.Add(newCompact);
                newCompact.transform.SetSiblingIndex(orderIndex);
                int fcount = 0;
                for (int i = 0; i < 5; i++)
                {
                    displayedHearts[i].transform.parent = newCompact.transform;
                    displayedHearts.RemoveAt(i);
                    i--;
                    fcount++;
                    if (fcount == 5) break;
                }
            }
            if (emptyCount > 5)
            {
                int orderindex = displayedHearts[displayedHearts.Count - 1].transform.GetSiblingIndex();
                newCompact = Instantiate(compacterPrefab, hitPointHolder.transform);
                compactEmpty.Add(newCompact);
                int ecount = 0;
                for (int i = displayedHearts.Count - 1; i > 0; i--)
                {
                    displayedHearts[i].transform.parent = newCompact.transform;
                    displayedHearts.RemoveAt(i);
                    ecount++;
                    if (ecount == 5) break;
                }
                newCompact.transform.SetSiblingIndex(orderindex + 1);

            }
        }
        int displayedFull = 0, displayedEmpty = 0;
        if (displayedHearts.Count > 0)
        {
            for (int i = 0; i < displayedHearts.Count; i++)
            {
                if (displayedHearts[i].GetComponent<HitPoint>().isFull) displayedFull++;
                else if (displayedHearts[i].GetComponent<HitPoint>().isEmpty) displayedEmpty++;
            }
        }
        if (displayedFull == 0)
        {
            if (compactFull.Count > 0)
            {
                temp.Clear();
                int index = compactFull[compactFull.Count - 1].transform.GetSiblingIndex();
                for (int i = 0; i < compactFull[compactFull.Count - 1].transform.childCount; i++)
                {
                    temp.Add(compactFull[compactFull.Count - 1].transform.GetChild(i).gameObject);
                }
                for (int i = compactFull[compactFull.Count - 1].transform.childCount - 1; i > -1; i--)
                {
                    Transform h = compactFull[compactFull.Count - 1].transform.GetChild(i);
                    h.parent = hitPointHolder.transform;
                    h.SetSiblingIndex(index);
                    //displayedHearts.Add(h.gameObject);
                    //temp.Add(h.gameObject);
                }
                for (int i = 0; i < displayedHearts.Count; i++)
                {
                    temp.Add(displayedHearts[i]);
                }
                displayedHearts.Clear();
                for (int i = 0; i < temp.Count; i++)
                {
                    displayedHearts.Add(temp[i]);
                }
                Destroy(compactFull[compactFull.Count - 1]);
                compactFull.RemoveAt(compactFull.Count - 1);
            }
        }
        if (displayedEmpty == 0)
        {
            if (compactEmpty.Count > 0)
            {
                temp.Clear();

                int index = compactEmpty[compactEmpty.Count - 1].transform.GetSiblingIndex();

                for (int i = 0; i < compactEmpty[compactEmpty.Count - 1].transform.childCount; i++)
                {
                    displayedHearts.Add(compactEmpty[compactEmpty.Count - 1].transform.GetChild(i).gameObject);
                }
                for (int i = compactEmpty[compactEmpty.Count - 1].transform.childCount - 1; i > -1; i--)
                {
                    Transform h = compactEmpty[compactEmpty.Count - 1].transform.GetChild(i);
                    h.parent = hitPointHolder.transform;
                    h.SetSiblingIndex(index);
                    //displayedHearts.Add(h.gameObject);
                }
                //for (int i = 0; i < displayedHearts.Count; i++)
                //{
                //    temp.Add(displayedHearts[i]);
                //}
                //displayedHearts.Clear();
                //for (int i = 0; i < temp.Count; i++)
                //{
                //    displayedHearts.Add(temp[i]);
                //}
                Destroy(compactEmpty[compactEmpty.Count - 1]);
                compactEmpty.RemoveAt(compactEmpty.Count - 1);
            }
        }
        if (value == 1 && transform.childCount > 1) ClearHearts();
    }
    IEnumerator Delay()
    {
        yield return null;
        StartCoroutine(HeartsCoroutine());
    }
    IEnumerator HeartsCoroutine()
    {

        spawningHearts = true;
        // Spawning hearts
        while (hearts.Count != value)
        {
            if (hearts.Count < value)
            {
                newHeart = Instantiate(hitPointPrefab, hitPointHolder.transform);
                hearts.Add(newHeart.GetComponent<HitPoint>());
                displayedHearts.Add(newHeart);
                displayedValue++;
            }
            if (hearts.Count > value)
            {
                ClearHearts();
                StartCoroutine(Delay());
                yield break;
            }
            yield return null;
        }
        emptyHeartsCount = 0; fullHeartsCount = 0; halfHeartCount = 0;
        for (int i = 0; i < hearts.Count; i++)
        {
            if (hearts[i].isFull) { fullHearts.Add(hearts[i].gameObject); fullHeartsCount++; }
            else if (hearts[i].isEmpty) { emptyHeartsCount++; }
            else halfHeartCount++;
        }
        displayedValue = fullHeartsCount + ((float)halfHeartCount / 2);
        if (displayedValue != value - damageTaken && (value - damageTaken) > -1)
        {
            emptyHearts.Clear();
            for (int i = 0; i < hearts.Count; i++)
            {
                hearts[i].current = hearts[i].full;
                hearts[i].isFull = true; hearts[i].isHalf = false; hearts[i].isEmpty = false;
                if (!fullHearts.Contains(hearts[i].gameObject)) fullHearts.Add(hearts[i].gameObject);
            }
            int doubleDamageTaken = Mathf.RoundToInt(damageTaken * 2);
            int count = hearts.Count - 1;
            for (int i = 0; i < doubleDamageTaken; i++)
            {
                if (count > -1)
                {
                    if (i % 2 == 0)
                    {
                        hearts[count].current = hearts[count].half;
                        hearts[count].isFull = false; hearts[count].isHalf = true; hearts[count].isEmpty = false;
                        if (fullHearts.Contains(hearts[count].gameObject)) fullHearts.Remove(hearts[count].gameObject);
                        displayHalf = true;
                    }
                    else
                    {
                        hearts[count].current = hearts[count].empty;
                        hearts[count].isFull = false; hearts[count].isHalf = false; hearts[count].isEmpty = true;
                        if (fullHearts.Contains(hearts[count].gameObject)) fullHearts.Remove(hearts[count].gameObject);
                        emptyHearts.Add(hearts[count].gameObject);
                        displayHalf = false;
                        count--;
                    }
                    displayedValue -= 0.5f;
                    if (displayedValue <= 0) break;
                    yield return null;
                }
            }
        }
        displayedValue = Mathf.Clamp(displayedValue, 0, 50);

        // Compacting
        if (displayedHearts.Count > 0)
        {
            int emptyCount = 0, fullcount = 0;
            for (int i = 0; i < displayedHearts.Count; i++)
            {
                if (displayedHearts[i].GetComponent<HitPoint>().isFull) fullcount++;
                else if (displayedHearts[i].GetComponent<HitPoint>().isEmpty) emptyCount++;
            }
            while (fullcount > 5)
            {
                int orderIndex = displayedHearts[0].transform.GetSiblingIndex();
                newCompact = Instantiate(compacterPrefab, hitPointHolder.transform);
                compactFull.Add(newCompact);
                int fcount = 0;
                for (int i = 0; i < 5; i++)
                {
                    displayedHearts[i].transform.parent = newCompact.transform;
                    displayedHearts.RemoveAt(i);
                    fullcount--;
                    i--;
                    fcount++;
                    yield return null;
                    if (fcount == 5) break;
                }                
                newCompact.transform.SetSiblingIndex(orderIndex);

                newEmpty = Instantiate(emptyPrefab, hitPointHolder.transform);
                newEmpty.transform.SetSiblingIndex(newCompact.transform.GetSiblingIndex() + 1);
                emptiesF.Add(newEmpty);
                
                yield return null;
            }
            while (emptyCount > 5)
            {
                int orderindex = displayedHearts[displayedHearts.Count - 1].transform.GetSiblingIndex();
                newCompact = Instantiate(compacterPrefab, hitPointHolder.transform);
                compactEmpty.Add(newCompact);
                int ecount = 0;
                for (int i = displayedHearts.Count - 1; i > 0; i--)
                {
                    displayedHearts[i].transform.parent = newCompact.transform;
                    displayedHearts.RemoveAt(i);
                    emptyCount--;
                    ecount++;
                    yield return null;
                    if (ecount == 5) break;
                }
                newCompact.transform.SetSiblingIndex(orderindex);

                newEmpty = Instantiate(emptyPrefab, hitPointHolder.transform);
                newEmpty.transform.SetSiblingIndex(newCompact.transform.GetSiblingIndex());
                emptiesE.Add(newEmpty);
                
                yield return null;
            }
            
        }
        int displayedFull = 0, displayedEmpty = 0;
        if (displayedHearts.Count > 0)
        {
            for (int i = 0; i < displayedHearts.Count; i++)
            {
                if (displayedHearts[i].GetComponent<HitPoint>().isFull) displayedFull++;
                else if (displayedHearts[i].GetComponent<HitPoint>().isEmpty) displayedEmpty++;
            }
        }
        while (displayedFull == 0)
        {
            if (compactFull.Count > 0)
            {
                temp.Clear();
                int index = compactFull[compactFull.Count - 1].transform.GetSiblingIndex()+1;
                for (int i = 0; i < compactFull[compactFull.Count - 1].transform.childCount; i++)
                {
                    temp.Add(compactFull[compactFull.Count - 1].transform.GetChild(i).gameObject);
                }
                for (int i = compactFull[compactFull.Count - 1].transform.childCount - 1; i > -1; i--)
                {
                    Transform h = compactFull[compactFull.Count - 1].transform.GetChild(i);
                    h.parent = hitPointHolder.transform;
                    h.SetSiblingIndex(index);
                    displayedFull++;
                    yield return null;
                    //displayedHearts.Add(h.gameObject);
                    //temp.Add(h.gameObject);
                }
                for (int i = 0; i < displayedHearts.Count; i++)
                {
                    temp.Add(displayedHearts[i]);
                }
                displayedHearts.Clear();
                for (int i = 0; i < temp.Count; i++)
                {
                    displayedHearts.Add(temp[i]);
                }
                int f = compactFull.Count - 1;
                Destroy(compactFull[f]);
                compactFull.RemoveAt(f);
                Destroy(emptiesF[f]);
                emptiesF.RemoveAt(f);
            }
            yield return null;
        }
        while (displayedEmpty == 0)
        {
            if (compactEmpty.Count > 0)
            {
                temp.Clear();

                int index = compactEmpty[compactEmpty.Count - 1].transform.GetSiblingIndex()+1;
                for (int i = 0; i < compactEmpty[compactEmpty.Count - 1].transform.childCount; i++)
                {
                    displayedHearts.Add(compactEmpty[compactEmpty.Count - 1].transform.GetChild(i).gameObject);
                }
                for (int i = 0; i < compactEmpty[compactEmpty.Count-1].transform.childCount; i++)
                {
                    temp.Add(compactEmpty[compactEmpty.Count - 1].transform.GetChild(i).gameObject);
                }
                for (int i = 0; i < temp.Count; i++)
                {
                    Transform h = temp[i].transform;
                    h.parent = hitPointHolder.transform;
                    h.SetSiblingIndex(index);
                    displayedEmpty++;
                    yield return null;
                }
                
                int e = compactEmpty.Count - 1;
                Destroy(compactEmpty[e]);
                compactEmpty.RemoveAt(e);

                Destroy(emptiesE[e]);
                emptiesE.RemoveAt(e);
            }
            else break;
            yield return null;
        }
        //Sort displayed hearts
        int compactedCount = Mathf.Max(0,(compactFull.Count*5));
        int siblingIndex = compactFull.Count*2;
        temp.Clear();
        for (int i = compactedCount; i < hearts.Count; i++)
        {
            for (int j = 0; j < displayedHearts.Count; j++)
            {
                if (displayedHearts[j].GetComponent<HitPoint>() == hearts[i])
                {
                    temp.Add(displayedHearts[j]);
                    displayedHearts[j].transform.SetSiblingIndex(siblingIndex);
                    siblingIndex++;
                    break;
                }
            }
        }
        displayedHearts.Clear();
        for (int i = 0; i < temp.Count; i++)
        {
            displayedHearts.Add(temp[i]);
        }
        if (value == 1 && transform.childCount > 1) ClearHearts();
        spawningHearts = false;
        if (runCount == 0)
        {
            runCount++;
            StartCoroutine(Delay());
        }
        else runCount = 0;
    }

    public void ClearHearts()
    {
        displayedValue = 0;
        for (int i = 0; i < hearts.Count; i++)
        {
            Destroy(hearts[i].gameObject);
            hearts.RemoveAt(i);
            i--;
        }
        emptyHearts.Clear();
        fullHearts.Clear();
        displayedHearts.Clear();

        for (int i = 0; i < compactEmpty.Count; i++)
        {
            Destroy(compactEmpty[i]);
            compactEmpty.RemoveAt(i);
            i--;
        }

        for (int i = 0; i < compactFull.Count; i++)
        {
            Destroy(compactFull[i]);
            compactFull.RemoveAt(i);
            i--;
        }
        for (int i = 0; i < emptiesE.Count; i++)
        {
            Destroy(emptiesE[i]);
            emptiesE.RemoveAt(i);
            i--;
        }
        for (int i = 0; i < emptiesF.Count; i++)
        {
            Destroy(emptiesF[i]);
            emptiesF.RemoveAt(i);
            i--;
        }
    }
}
