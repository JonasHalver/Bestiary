using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Linq;

public class ActionEditor : MonoBehaviour
{
    public static ActionEditor instance;

    public Action action, guessAction;
    [SerializeField] private TextMeshProUGUI editCount, collectionCount, discardCount;
    [SerializeField] private Transform editHolder, secondEditHolder, collectionHolder, discardHolder;

    [SerializeField] private TMP_InputField nameInput;

    private List<OutputInfo> outputGuessesPrimary = new List<OutputInfo>();
    private List<ContextInfo> contextGuessesPrimary = new List<ContextInfo>();
    private List<OutputInfo> outputGuessesSecondary = new List<OutputInfo>();
    private List<Action.Shape> primaryShapes = new List<Action.Shape>();
    private List<Action.Shape> secondaryShapes = new List<Action.Shape>();
    public static GraphicRaycaster graphicRaycaster;
    [SerializeField] private TextMeshProUGUI logTest;

    private Action.TargetGroup primaryTargetGroup = Action.TargetGroup.Enemies;
    private Action.TargetGroup secondaryTargetGroup = Action.TargetGroup.Allies;

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
        Dictionary<ActionNode, ActionNode> temp = new Dictionary<ActionNode, ActionNode>();
        if (Book.currentEntry.activeAction.nodeParents.Count > 0)
        {
            foreach (KeyValuePair<ActionNode, ActionNode.WindowType> oan in Book.currentEntry.activeAction.nodeParents)
            {
                foreach (ActionNode nan in n)
                {
                    if (SameNode(oan.Key, nan))
                    {
                        temp.Add(oan.Key, nan);
                    }
                }
            }
            foreach (KeyValuePair<ActionNode, ActionNode> aa in temp)
            {
                if (aa.Key == aa.Value) continue;
                if (!Book.currentEntry.activeAction.nodeParents.ContainsKey(aa.Value))
                    Book.currentEntry.activeAction.nodeParents.Add(aa.Value, Book.currentEntry.activeAction.nodeParents[aa.Key]);
                if (!Book.currentEntry.activeAction.nodePositions.ContainsKey(aa.Value))
                    Book.currentEntry.activeAction.nodePositions.Add(aa.Value, Book.currentEntry.activeAction.nodePositions[aa.Key]);

                Book.currentEntry.activeAction.nodePositions.Remove(aa.Key);
                Book.currentEntry.activeAction.nodeParents.Remove(aa.Key);
            }
        }
        for (int i = 0; i < n.Length; i++)
        {
            n[i].AddToLists();
        }
        foreach(ActionNode an in n)
        {            
            switch (Book.currentEntry.activeAction.nodeParents[an])
            {
                case ActionNode.WindowType.Collection:
                    break;
                case ActionNode.WindowType.Discard:
                    an.transform.parent = discardHolder;
                    an.transform.position = Book.currentEntry.activeAction.nodePositions[an];                    
                    break;
                case ActionNode.WindowType.Edit1:
                    switch (an.nodeType)
                    {
                        case ActionNode.NodeType.Context:
                            for (int i = 0; i < Book.currentEntry.activeAction.guessAction.primaryContext.Count; i++)
                            {
                                if (Book.currentEntry.activeAction.guessAction.primaryContext[i].context == an.actionContext.context)
                                {
                                    an.actionContext.damageType = Book.currentEntry.activeAction.guessAction.primaryContext[i].damageType;
                                    an.actionContext.condition = Book.currentEntry.activeAction.guessAction.primaryContext[i].condition;
                                }
                            }
                            an.transform.parent = editHolder;
                            
                            an.transform.position = Book.currentEntry.activeAction.nodePositions[an];
                            StartCoroutine(OneFrameDelay(an.gameObject, "ExternalExpand"));
                            break;
                        case ActionNode.NodeType.Shape:
                            an.transform.parent = editHolder;
                            an.transform.position = Book.currentEntry.activeAction.nodePositions[an];
                            StartCoroutine(OneFrameDelay(an.gameObject, "ExternalExpand"));
                            break;
                        case ActionNode.NodeType.Output:
                            OutputInfo oi = null;
                            for (int i = 0; i < Book.currentEntry.activeAction.guessAction.primaryOutput.Count; i++)
                            {
                                if (Book.currentEntry.activeAction.guessAction.primaryOutput[i].output == an.actionOutput.output) oi = Book.currentEntry.activeAction.guessAction.primaryOutput[i];
                            }
                            an.Clone(Book.currentEntry.activeAction.nodePositions[an], editHolder, oi);
                            break;
                    }
                    StartCoroutine(OneFrameDelay(an.gameObject, "SetIcons"));
                    break;
                case ActionNode.WindowType.Edit2:
                    switch (an.nodeType)
                    {
                        case ActionNode.NodeType.Context:
                        case ActionNode.NodeType.Shape:
                            an.transform.parent = secondEditHolder;
                            an.transform.position = Book.currentEntry.activeAction.nodePositions[an];
                            an.blend = 1;
                            break;
                        case ActionNode.NodeType.Output:
                            OutputInfo oi = null;
                            for (int i = 0; i < Book.currentEntry.activeAction.guessAction.secondaryOutput.Count; i++)
                            {
                                if (Book.currentEntry.activeAction.guessAction.secondaryOutput[i].output == an.actionOutput.output) oi = Book.currentEntry.activeAction.guessAction.secondaryOutput[i];
                            }
                            an.Clone(Book.currentEntry.activeAction.nodePositions[an], secondEditHolder, oi);
                            break;
                    }
                    StartCoroutine(OneFrameDelay(an.gameObject, "SetIcons"));
                    break;
            }
            if (an.nodeType == ActionNode.NodeType.Context)
            {
                contextNodes.Add(an.actionContext.context, an);
            }
        }
        CompareActionInformation();
    }

    private bool SameNode(ActionNode originalNode, ActionNode newNode)
    {
        bool flag = false;

        if (originalNode.nodeType == newNode.nodeType)
        {
            switch (originalNode.nodeType)
            {
                case ActionNode.NodeType.Context:
                    if (originalNode.actionContext.context == newNode.actionContext.context) flag = true;
                    break;
                case ActionNode.NodeType.Output:
                    if (originalNode.actionOutput.output == newNode.actionOutput.output) flag = true;
                    break;
                case ActionNode.NodeType.Shape:
                    if (originalNode.actionShape == newNode.actionShape) flag = true;
                    break;
            }
        }

        return flag;
    }

    // Update is called once per frame
    void Update()
    {
        if (nameInput.text != null) Book.currentEntry.activeAction.guessAction.actionName = nameInput.text;
        else Book.currentEntry.activeAction.guessAction.actionName = "Unnamed Action";
    }
    public void EditingName(bool editing)
    {
        Book.instance.EditingText(editing);
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
        if (primary) primaryTargetGroup = tg;
        else secondaryTargetGroup = tg;
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

    public void CompareActionInformation()
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
            else
            {
                outputGuessesPrimary.Add(node.actionOutput);
                for (int j = 0; j < outputGuessesPrimary.Count-1; j++)
                {
                    if (outputGuessesPrimary[j].output == node.actionOutput.output && outputGuessesPrimary[j].output != Action.Output.Condition)
                    {
                        node.Error("Each effect can only have one of each type of output, except for conditions.");
                    }
                    outputGuessesPrimary[j].affectedGroup = primaryTargetGroup;
                }
            }
        }
        //if (primaryShapes.Count == 1 && primaryShapes[0] == action.primaryShape) print("Primary shape correct");
        CheckIncompatibility(contextGuessesPrimary);

        // Secondary effect
        //if (Book.currentEntry.activeAction.guessAction.secondaryOutput.Count != 0)
        //{
        outputGuessesSecondary.Clear();
        secondaryShapes.Clear();
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
            else
            {
                outputGuessesSecondary.Add(node.actionOutput);
                for (int j = 0; j < outputGuessesSecondary.Count - 1; j++)
                {
                    if (outputGuessesSecondary[j].output == node.actionOutput.output && outputGuessesSecondary[j].output != Action.Output.Condition)
                    {
                        node.Error("Each effect can only have one of each type of output, except for conditions.");
                    }
                    outputGuessesSecondary[j].affectedGroup = secondaryTargetGroup;
                }
            }
        }
        //}

        LogBuilder();

        // Comparison
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

        
        outputCorrectCount = 0;
        //if (guessAction.secondaryTargetGroup == action.secondaryTargetGroup) print("Secondary target group correct");
        //if (guessAction.secondaryTargeting == action.secondaryTargeting) print("Secondary targeting correct");
        
        
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
        OutputInfo primaryMove = null, secondaryMove = null;
        bool flag1 = false, flag2 = false;
        Log elements = GameManager.instance.logElementCollection;

        if (outputGuessesPrimary.Count == 0 || contextGuessesPrimary.Count == 0 || primaryShapes.Count == 0 || guessAction.descriptionIndex == -1)
        {
            log.Append($"Add at least 1 <b><color={(contextGuessesPrimary.Count > 0 ? "#015400" : "red")}>Context</color></b>, 1 " +
                $"<b><color={(primaryShapes.Count > 0 ? "#015400" : "red")}>Shape</color></b>, " +
                $"and 1 <b><color={(outputGuessesPrimary.Count > 0 ? "#015400" : "red")}>Output</color></b>, " +
                $"and set the <b><color={(guessAction.descriptionIndex > -1 ? "#015400" : "red")}>Warning Movement</color></b> for this action to be valid.");
            logTest.text = log.ToString();
            Book.currentEntry.activeAction.guessAction.description = "This action lacks information. Click here to add it.";
            return;
        }

        log.Append("If the monster ");
        if (outputGuessesPrimary.Count> 0)
        {
            outputGuessesPrimary.Sort((g1, g2) => g1.output.CompareTo(g2.output));
        }
        if (outputGuessesSecondary.Count>0)
        {
            outputGuessesSecondary.Sort((g1, g2) => g1.output.CompareTo(g2.output));
        }
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
            if (outputGuessesPrimary.Count == 1 && outputGuessesPrimary[0].output == Action.Output.Movement)
            {
                log.Append("target ");
                flag1 = true;
            }
            else if (outputGuessesPrimary.Count > 1 && outputGuessesPrimary[outputGuessesPrimary.Count-1].output == Action.Output.Movement)
            {
                flag1 = true;
            }
            for (int i = 0; i < outputGuessesPrimary.Count; i++)
            {
                if (!flag1)
                {
                    if (i != 0 && i == outputGuessesPrimary.Count - 1) log.Append("and ");
                }
                else
                {
                    if (i != 0 && i == outputGuessesPrimary.Count - 2) log.Append("and ");
                }
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
                        primaryMove = outputGuessesPrimary[i];                        
                        break;
                }
                if (!flag1)
                {
                    if (i != outputGuessesPrimary.Count - 1)
                    {
                        log.Append(", ");
                    }
                    else
                    {
                        log.Append(" to ");
                    }
                }
                else
                {
                    if (i != outputGuessesPrimary.Count - 2 && i != outputGuessesPrimary.Count-1)
                    {
                        log.Append(", ");
                    }
                    else if (i != outputGuessesPrimary.Count-1)
                    {
                        log.Append(" to ");
                    }
                }
            }
        }
        if (primaryShapes.Count == 1)
        {
            log.Append(elements.GetString(primaryShapes[0], guessAction.primaryTargetGroup, guessAction.primaryTargeting));
        }
        if (primaryMove != null)
        {
            log.Append(" ");

            log.Append(elements.GetString(primaryMove.output, primaryMove.value, primaryMove.towards));
        }
        log.Append(".");
        if (outputGuessesSecondary.Count > 0)
        {
            log.Append(" Then it will ");
            if (outputGuessesSecondary.Count == 1 && outputGuessesSecondary[0].output == Action.Output.Movement)
            {
                log.Append("target ");
                flag2 = true;
            }
            else if (outputGuessesSecondary.Count > 1 && outputGuessesSecondary[outputGuessesSecondary.Count - 1].output == Action.Output.Movement)
            {
                flag2 = true;
            }
            for (int i = 0; i < outputGuessesSecondary.Count; i++)
            {
                if (!flag2)
                {
                    if (i != 0 && i == outputGuessesSecondary.Count - 1) log.Append("and ");
                }
                else
                {
                    if (i != 0 && i == outputGuessesSecondary.Count - 2) log.Append("and ");
                }
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
                        secondaryMove = outputGuessesSecondary[i];
                        break;
                }
                if (!flag2)
                {
                    if (i != outputGuessesSecondary.Count - 1)
                    {
                        log.Append(", ");
                    }
                    else
                    {
                        log.Append(" to ");
                    }
                }
                else
                {
                    if (i != outputGuessesSecondary.Count - 2 && i != outputGuessesSecondary.Count - 1)
                    {
                        log.Append(", ");
                    }
                    else if (i != outputGuessesSecondary.Count - 1)
                    {
                        log.Append(" to ");
                    }
                }
            }
            if (secondaryShapes.Count == 1)
            {
                log.Append(elements.GetString(secondaryShapes[0], guessAction.secondaryTargetGroup, guessAction.secondaryTargeting));
            }
            if (secondaryMove != null)
            {
                log.Append(" ");

                log.Append(elements.GetString(secondaryMove.output, secondaryMove.value, secondaryMove.towards));
            }
            log.Append(".");
        }
        
        logTest.text = log.ToString();
        Book.currentEntry.activeAction.guessAction.description = log.ToString();
    }
    public void CloseWindow()
    {
        GameManager.focusedWindow = null;
        GameManager.openWindows.Remove(gameObject);
        Destroy(gameObject);
    }
    IEnumerator OneFrameDelay(GameObject receiver, string message)
    {
        yield return null;
        receiver.SendMessage(message);
    }
}
