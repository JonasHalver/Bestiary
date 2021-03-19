using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPointDisplay : MonoBehaviour
{
    [Range(0,50)]
    public int value = 1;
    public GameObject hitPointPrefab, hitPointHolder, compacterPrefab;
    public List<GameObject> hitPoints = new List<GameObject>();
    public List<GameObject> compactHitPoints = new List<GameObject>();

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
        if (hitPoints.Count + compactHitPoints.Count < value)
        {
            GameObject newHP = Instantiate(hitPointPrefab, hitPointHolder.transform);
            hitPoints.Add(newHP);
        }
        else if (hitPoints.Count + compactHitPoints.Count > value)
        {
            int count = hitPoints.Count;
            for (int i = count - 1; i > -1; i--)
            {
                Destroy(hitPoints[i].gameObject);
                hitPoints.RemoveAt(i);
                if (hitPoints.Count <= value) break;
            }
        }
    }
}
