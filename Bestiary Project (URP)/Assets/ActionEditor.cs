using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionEditor : MonoBehaviour
{
    public Action action;
    [SerializeField] private TextMeshProUGUI editCount, collectionCount, discardCount;
    [SerializeField] private Transform editHolder, collectionHolder, discardHolder;

    private List<OutputInfo> outputGuesses = new List<OutputInfo>();
    private List<ContextInfo> contextGuesses = new List<ContextInfo>();

    public static GraphicRaycaster graphicRaycaster;

    private Dictionary<Action.Context, ActionNode> contextNodes = new Dictionary<Action.Context, ActionNode>();

    private void OnEnable()
    {
        ActionNode.NodeChanged += CompareActionInformation;
    }
    private void OnDisable()
    {
        ActionNode.NodeChanged -= CompareActionInformation;
    }

    // Start is called before the first frame update
    void Start()
    {
        graphicRaycaster = GetComponent<GraphicRaycaster>();
        ActionNode[] n = transform.GetComponentsInChildren<ActionNode>();
        foreach(ActionNode an in n)
        {
            if (an.nodeType == ActionNode.NodeType.Context)
            {
                contextNodes.Add(an.actionContext.context, an);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        editCount.text = editHolder.childCount.ToString();
        collectionCount.text = collectionHolder.childCount.ToString();
        discardCount.text = discardHolder.childCount.ToString();
    }

    private void CompareActionInformation()
    {
        outputGuesses.Clear();
        contextGuesses.Clear();
        int outputCorrectCount = 0;
        int contextCorrectCount = 0;
        for (int i = 0; i < editHolder.childCount; i++)
        {
            ActionNode node = editHolder.GetChild(i).GetComponent<ActionNode>();
            if (node.nodeType == ActionNode.NodeType.Context) contextGuesses.Add(node.actionContext);
            else outputGuesses.Add(node.actionOutput);
        }
        CheckIncompatibility();
        for (int i = 0; i < contextGuesses.Count; i++)
        {
            for (int j = 0; j < action.primaryContext.Count; j++)
            {
                if (contextGuesses[i].Match(action.primaryContext[j])) contextCorrectCount++;
            }
        }
        for (int i = 0; i < outputGuesses.Count; i++)
        {
            for (int j = 0; j < action.primaryOutput.Count; j++)
            {
                if (outputGuesses[i].Match(action.primaryOutput[j])) outputCorrectCount++;
            }
        }
        if (contextCorrectCount == action.primaryContext.Count && outputCorrectCount == action.primaryOutput.Count)
        {
            print("Correct comparison");
        }
    }
    private void CheckIncompatibility()
    {
        List<Action.Context> temp = new List<Action.Context>();
        for (int i = 0; i < contextGuesses.Count; i++)
        {
            temp.Clear();
            if (Action.InvalidContextPairs.ContainsKey(contextGuesses[i].context))
            {
                for (int j = 0; j < Action.InvalidContextPairs[contextGuesses[i].context].Count; j++)
                {
                    for (int k = 0; k < contextGuesses.Count; k++)
                    {
                        if (contextGuesses[k].context == Action.InvalidContextPairs[contextGuesses[i].context][j])
                        {
                            temp.Add(contextGuesses[k].context);
                        }
                    }
                }
            }
            if (temp.Count > 0) contextNodes[contextGuesses[i].context].Incompatible(temp);
        }
    }
}
