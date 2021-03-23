using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPointDisplay : MonoBehaviour
{
    [Range(0,50)]
    public float value = 1;
    public bool displayHalf;
    public GameObject hitPointPrefab, halfHitPrefab, hitPointHolder, compacterPrefab;
    public List<GameObject> hitPoints = new List<GameObject>();
    public List<GameObject> compactHitPoints = new List<GameObject>();
    public float displayedValue;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hitPoints.Count >= 6)
        {
            int count = 0;
            for (int i = 0; i < 5; i++)
            {
                Destroy(hitPoints[i].gameObject);
                hitPoints.RemoveAt(i);
                i--;
                count++;
                if (count == 5) break;
            }
            GameObject newCompact = Instantiate(compacterPrefab, transform);
            newCompact.transform.SetAsFirstSibling();
            for (int i = 0; i < 5; i++)
            {
                GameObject newHP = Instantiate(hitPointPrefab, newCompact.transform);
                compactHitPoints.Add(newHP);
            }
        }
        if (hitPoints.Count == 0 && compactHitPoints.Count > 0)
        {
            GameObject compacter = compactHitPoints[0].transform.parent.gameObject;
            int count = 0;
            for (int i = 0; i < compactHitPoints.Count; i++)
            {
                compactHitPoints.RemoveAt(i);
                i--;
                count++;
                if (count == 5) break;
            }
            Destroy(compacter);
        }

        // Spawning hearts
        if (displayedValue < value)
        {
            float difference = value - displayedValue;
            if (difference > 0.5f)
            {
                GameObject newHP = Instantiate(hitPointPrefab, hitPointHolder.transform);
                hitPoints.Add(newHP);
            }
            else
            {
                GameObject newHalf = Instantiate(halfHitPrefab, hitPointHolder.transform);
                hitPoints.Add(newHalf);
                displayHalf = true;
            }
        }
        else if (displayedValue > value)
        {
            float difference = displayedValue - value;
            if (difference < 1f)
            {
                if (hitPoints.Count > 0)
                {
                    if (displayHalf)
                    {
                        Destroy(hitPoints[hitPoints.Count - 1].gameObject);
                        hitPoints.RemoveAt(hitPoints.Count - 1);
                        displayHalf = false;
                    }
                    else
                    {
                        Destroy(hitPoints[hitPoints.Count - 1].gameObject);
                        hitPoints.RemoveAt(hitPoints.Count - 1);
                        GameObject newHalf = Instantiate(halfHitPrefab, hitPointHolder.transform);
                        hitPoints.Add(newHalf);
                        displayHalf = true;
                    }
                }
            }
            else
            {
                if (hitPoints.Count > 0)
                {
                    Destroy(hitPoints[0].gameObject);
                    hitPoints.RemoveAt(0);
                }
            }
        }

        displayedValue = hitPoints.Count + compactHitPoints.Count - (displayHalf ? 0.5f : 0);
    }
}
