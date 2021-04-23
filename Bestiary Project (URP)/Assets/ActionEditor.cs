using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class ActionEditor : MonoBehaviour
{
    public static ActionEditor instance;

    public Action action, guessAction;
    [SerializeField] private TextMeshProUGUI editCount, collectionCount, discardCount;
    [SerializeField] private Transform editHolder, secondEditHolder, collectionHolder, discardHolder;

    private List<OutputInfo> outputGuessesPrimary = new List<OutputInfo>();
    private List<ContextInfo> contextGuessesPrimary = new List<ContextInfo>();
    private List<OutputInfo> outputGuessesSecondary = new List<OutputInfo>();
    private List<Action.Shape> primaryShapes = new List<Action.Shape>();
    private List<Action.Shape> secondaryShapes = new List<Action.Shape>();
    public static GraphicRaycaster graphicRaycaster;
    [SerializeField] private TextMeshProUGUI logTest;


    private Dictionary<Action.Context, ActionNode> contextNodes = new Dictionary<Action.Context, ActionNode>();
    private void Awake()
    {
        instance = this;
        //guessAction = ScriptableObject.CreateInstance<Action>();
    }
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
    }
    public void SetTargetGroup(int index, bool primary)
    {
        Action.TargetGroup tg = Action.TargetGroup.Enemies;
        switch (index)
        {
            case 0:
                tg = Action.TargetGroup.Enemies;
                break;
            case 1:
                tg = Action.TargetGroup.Allies;
                break;
            case 2:
                tg = Action.TargetGroup.All;
                break;
        }
        if (primary) guessAction.primaryTargetGroup = tg;
        else guessAction.secondaryTargetGroup = tg;
        CompareActionInformation();
    }
    public void SetTargeting(int index, bool primary)
    {
        Action.Targeting t = Action.Targeting.Character;
        if (index == 1) t = Action.Targeting.Ground;
        if (primary) guessAction.primaryTargeting = t;
        else guessAction.secondaryTargeting = t;
        CompareActionInformation();
    }
    public void SetCooldown(int cd)
    {
        guessAction.cooldown = cd;
        CompareActionInformation();
    }

    private void CompareActionInformation()
    {
        //if (guessAction.primaryTargetGroup == action.primaryTargetGroup) print("Primary target group correct");
        //if (guessAction.primaryTargeting == action.primaryTargeting) print("Primary targeting correct");
        //if (guessAction.cooldown == action.cooldown) print("Cooldown correct");
        outputGuessesPrimary.Clear();
        contextGuessesPrimary.Clear();
        primaryShapes.Clear();
        int outputCorrectCount = 0;
        int contextCorrectCount = 0;
        for (int i = 0; i < editHolder.childCount; i++)
        {
            ActionNode node = editHolder.GetChild(i).GetComponent<ActionNode>();
            if (node.nodeType == ActionNode.NodeType.Context) contextGuessesPrimary.Add(node.actionContext);
            else if (node.nodeType == ActionNode.NodeType.Shape)
            {
                primaryShapes.Add(node.actionShape);
                if (primaryShapes.Count > 1) node.Error("Each effect can only have one shape.");
            }
            else outputGuessesPrimary.Add(node.actionOutput);
        }
        //if (primaryShapes.Count == 1 && primaryShapes[0] == action.primaryShape) print("Primary shape correct");
        CheckIncompatibility(contextGuessesPrimary);
        LogBuilder();

        if (action == null) return;
        for (int i = 0; i < contextGuessesPrimary.Count; i++)
        {
            for (int j = 0; j < action.primaryContext.Count; j++)
            {
                if (contextGuessesPrimary[i].Match(action.primaryContext[j])) contextCorrectCount++;
            }
        }
        for (int i = 0; i < outputGuessesPrimary.Count; i++)
        {
            for (int j = 0; j < action.primaryOutput.Count; j++)
            {
                if (outputGuessesPrimary[i].Match(action.primaryOutput[j])) outputCorrectCount++;
            }
        }
        if (contextCorrectCount == action.primaryContext.Count && outputCorrectCount == action.primaryOutput.Count)
        {
            print("Primary effect is correct");
        }

        // Secondary effect
        if (action.secondaryOutput.Count == 0) return;
        outputGuessesSecondary.Clear();
        secondaryShapes.Clear();
        outputCorrectCount = 0;
        //if (guessAction.secondaryTargetGroup == action.secondaryTargetGroup) print("Secondary target group correct");
        //if (guessAction.secondaryTargeting == action.secondaryTargeting) print("Secondary targeting correct");
        
        for (int i = 0; i < secondEditHolder.childCount; i++)
        {
            ActionNode node = secondEditHolder.GetChild(i).GetComponent<ActionNode>();
            if (node.nodeType == ActionNode.NodeType.Context) node.Error("The secondary effect uses the context of the primary effect. Only place Shape and Output nodes here.");
            else if (node.nodeType == ActionNode.NodeType.Shape)
            {
                secondaryShapes.Add(node.actionShape);
                if (secondaryShapes.Count > 1)
                {
                    node.Error("Each effect can only have one shape.");
                }
            }
            else outputGuessesSecondary.Add(node.actionOutput);
        }
        //if (secondaryShapes.Count == 1 && secondaryShapes[0] == action.secondaryShape) print("Secondary shape correct");

        for (int i = 0; i < outputGuessesSecondary.Count; i++)
        {
            for (int j = 0; j < action.secondaryOutput.Count; j++)
            {
                if (outputGuessesSecondary[i].Match(action.secondaryOutput[j])) outputCorrectCount++;
            }
        }
        //if (outputCorrectCount == action.secondaryOutput.Count)
        //{
        //    print("Secondary effect is correct");
        //}
        
    }
    private void CheckIncompatibility(List<ContextInfo> list)
    {
        foreach(KeyValuePair<Action.Context, ActionNode> kvp in contextNodes)
        {
            kvp.Value.EndError();
        }
        List<Action.Context> temp = new List<Action.Context>();
        for (int i = 0; i < list.Count; i++)
        {
            temp.Clear();
            if (Action.InvalidContextPairs.ContainsKey(list[i].context))
            {
                for (int j = 0; j < Action.InvalidContextPairs[list[i].context].Count; j++)
                {
                    for (int k = 0; k < list.Count; k++)
                    {
                        if (list[k].context == Action.InvalidContextPairs[list[i].context][j])
                        {
                            temp.Add(list[k].context);
                        }
                    }
                }
            }
            if (temp.Count > 0) contextNodes[list[i].context].Incompatible(temp);
        }
    }
    private void LogBuilder()
    {
        StringBuilder log = new StringBuilder();
        Log elements = GameManager.instance.logElementCollection;
        log.Append("If the monster ");
        if (contextGuessesPrimary.Count > 0)
        {
            for (int i = 0; i < contextGuessesPrimary.Count; i++)
            {
                if (i != 0 && i == contextGuessesPrimary.Count - 1) log.Append("and ");
                switch (contextGuessesPrimary[i].context)
                {
                    default:
                        log.Append(elements.GetString(contextGuessesPrimary[i].context));
                        break;
                    case Action.Context.AllyHasSpecificCondition:
                    case Action.Context.EnemyHasSpecificCondition:
                    case Action.Context.SelfHasSpecificCondition:
                        log.Append(elements.GetString(contextGuessesPrimary[i].context, contextGuessesPrimary[i].condition));
                        break;
                    case Action.Context.TookDamageOfType:
                        log.Append(elements.GetString(contextGuessesPrimary[i].context, contextGuessesPrimary[i].damageType));
                        break;
                }
                log.Append(", ");
            }
        }
        log.Append("the monster ").Append(guessAction.actionDescription).Append(", showing that it will ");
        if (outputGuessesPrimary.Count > 0)
        {
            for (int i = 0; i < outputGuessesPrimary.Count; i++)
            {
                if (i != 0 && i == outputGuessesPrimary.Count - 1) log.Append("and ");
                switch (outputGuessesPrimary[i].output)
                {
                    case Action.Output.Damage:
                        log.Append(elements.GetString(outputGuessesPrimary[i].output, outputGuessesPrimary[i].damageType, outputGuessesPrimary[i].critical));
                        break;
                    case Action.Output.Healing:
                        log.Append(elements.GetString(outputGuessesPrimary[i].output, outputGuessesPrimary[i].value));
                        break;
                    case Action.Output.Condition:
                        log.Append(elements.GetString(outputGuessesPrimary[i].output, outputGuessesPrimary[i].condition, outputGuessesPrimary[i].value));
                        break;
                    case Action.Output.Movement:
                        log.Append(elements.GetString(outputGuessesPrimary[i].output, outputGuessesPrimary[i].value, outputGuessesPrimary[i].towards));
                        break;
                }
                if (i != outputGuessesPrimary.Count - 1) log.Append(", ");
                else log.Append(" ");
            }
        }
        if (primaryShapes.Count == 1)
        {
            log.Append(elements.GetString(primaryShapes[0], guessAction.primaryTargetGroup, guessAction.primaryTargeting));
        }
        log.Append(".");
        if (outputGuessesSecondary.Count > 0)
        {
            log.Append(" Then it will ");
            for (int i = 0; i < outputGuessesSecondary.Count; i++)
            {
                if (i != 0 && i == outputGuessesSecondary.Count - 1) log.Append("and ");
                switch (outputGuessesSecondary[i].output)
                {
                    case Action.Output.Damage:
                        log.Append(elements.GetString(outputGuessesSecondary[i].output, outputGuessesSecondary[i].damageType, outputGuessesSecondary[i].critical));
                        break;
                    case Action.Output.Healing:
                        log.Append(elements.GetString(outputGuessesSecondary[i].output, outputGuessesSecondary[i].value));
                        break;
                    case Action.Output.Condition:
                        log.Append(elements.GetString(outputGuessesSecondary[i].output, outputGuessesSecondary[i].condition, outputGuessesSecondary[i].value));
                        break;
                    case Action.Output.Movement:
                        log.Append(elements.GetString(outputGuessesSecondary[i].output, outputGuessesSecondary[i].value, outputGuessesSecondary[i].towards));
                        break;
                }
                if (i != outputGuessesSecondary.Count-1) log.Append(", ");
                else log.Append(" ");
            }
            if (secondaryShapes.Count == 1)
            {
                log.Append(elements.GetString(secondaryShapes[0], guessAction.secondaryTargetGroup, guessAction.secondaryTargeting));
            }
            log.Append(".");
        }
        
        logTest.text = log.ToString();
    }
}
