using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationRandomizer : MonoBehaviour
{
    private List<GameObject> decorations = new List<GameObject>();
    private List<int> indeces = new List<int>();
    public int numberOfDecorationsMin,numberOfDecorationsMax;
    private int number;
    private int maxIterations = 1000;

    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform child in transform)
        {
            decorations.Add(child.gameObject);
        }
        number = Random.Range(numberOfDecorationsMin, numberOfDecorationsMax);
        int count = 0;
        for (int i = 0; i < number; i++)
        {
            int d = Random.Range(0, decorations.Count);
            if (indeces.Contains(d))
            {
                count++;
                if (count >= maxIterations) break;
                i--; continue;
            }
            indeces.Add(d);
        }
        for (int i = 0; i < indeces.Count; i++)
        {
            decorations[indeces[i]].SetActive(true);
        }
    }
}
