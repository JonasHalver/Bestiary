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
    public Transform editHolder, secondEditHolder, collectionHolder, discardHolder;

    [SerializeField] private TMP_InputField nameInput;
    public GameObject actionDescription;
    private List<OutputInfo> outputGuessesPrimary = new List<OutputInfo>();
    private List<ContextInfo> contextGuessesPrimary = new List<ContextInfo>();
    private List<OutputInfo> outputGuessesSecondary = new List<OutputInfo>();
    private List<Action.Shape> primaryShapes = new List<Action.Shape>();
    private List<Action.Shape> secondaryShapes = new List<Action.Shape>();
    public static GraphicRaycaster graphicRaycaster;
    [SerializeField] private TextMeshProUGUI logTest;

    private Action.TargetGroup primaryTargetGroup = Action.TargetGroup.Enemies;
    private Action.TargetGroup secondaryTargetGroup = Action.TargetGroup.Allies;
    public static event System.Action ActionEditorOpened;
    public static event System.Action ActionEditorClosed;
    public CustomToggle ptg, pt, c, stg, st;
    public TextMeshProUGUI buttonText;

    public HorizontalLayoutGroup hlg1, hlg2, hlg3;
    private bool firstTenFrames = false;
    private int framecount = 0;

    private Dictionary<Action.Context, ActionNode> contextNodes = new Dictionary<Action.Context, ActionNode>();
    private void Awake()
    {
        instance = this;
        ActionEditorOpened.Invoke();
        //guessAction = ScriptableObject.CreateInstance<Action>();
    }
    private void OnEnable()
    {
        ActionNode.NodeChanged += CompareActionInformation;
    }
    private void OnDisable()
    {
        ActionNode.NodeChanged -= CompareActionInformation;
        ActionEditorClosed.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {
        graphicRaycaster = GetComponent<GraphicRaycaster>();
        ActionNode[] n = transform.GetComponentsInChildren<ActionNode>();
        Dictionary<ActionNode, ActionNode> temp = new Dictionary<ActionNode, ActionNode>();
        ActionNode condition = null;
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
                    if (nan.nodeType == ActionNode.NodeType.Output && nan.actionOutput.output == Action.Output.Condition) condition = nan;
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
            if (n[i].nodeType == ActionNode.NodeType.Context)
            {
                contextNodes.Add(n[i].actionContext.context, n[i]);
            }
        }
        LoadPreviousSetup(n);
        LogBuilder();
        #region Old solution
        /*foreach(ActionNode an in n)
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
                            an.Clone(Book.currentEntry.activeAction.nodePositions[an], editHolder, null);
                            //an.transform.parent = editHolder;
                            //an.transform.position = Book.currentEntry.activeAction.nodePositions[an];
                            break;
                        case ActionNode.NodeType.Output:
                            OutputInfo oi = null;
                            for (int i = 0; i < Book.currentEntry.activeAction.guessAction.primaryOutput.Count; i++)
                            {
                                if (Book.currentEntry.activeAction.guessAction.primaryOutput[i].output == an.actionOutput.output) oi = Book.currentEntry.activeAction.guessAction.primaryOutput[i];
                            }
                            an.transform.parent = editHolder;
                            an.transform.position = Book.currentEntry.activeAction.nodePositions[an];
                            //an.Clone(Book.currentEntry.activeAction.nodePositions[an], editHolder, oi);
                            break;
                    }
                    StartCoroutine(OneFrameDelay(an.gameObject, "SetIcons"));
                    break;
                case ActionNode.WindowType.Edit2:
                    switch (an.nodeType)
                    {
                        case ActionNode.NodeType.Context:
                        case ActionNode.NodeType.Shape:
                           // an.Clone(Book.currentEntry.activeAction.nodePositions[an], secondEditHolder, null);
                            an.transform.parent = secondEditHolder;
                            an.transform.position = Book.currentEntry.activeAction.nodePositions[an];
                            break;
                        case ActionNode.NodeType.Output:
                            OutputInfo oi = null;
                            for (int i = 0; i < Book.currentEntry.activeAction.guessAction.secondaryOutput.Count; i++)
                            {
                                if (Book.currentEntry.activeAction.guessAction.secondaryOutput[i].output == an.actionOutput.output) oi = Book.currentEntry.activeAction.guessAction.secondaryOutput[i];
                            }
                            //an.Clone(Book.currentEntry.activeAction.nodePositions[an], secondEditHolder, oi);
                            an.transform.parent = secondEditHolder;
                            an.transform.position = Book.currentEntry.activeAction.nodePositions[an];
                            break;
                    }
                    StartCoroutine(OneFrameDelay(an.gameObject, "SetIcons"));
                    break;
            }
            
        }*/
        #endregion
        ActionCheck ac = Book.currentEntry.activeAction;
        if (ac.guessAction != null)
        {
            if (ac.guessAction.primaryOutput.Count > 0)
            {
                ptg.activeIndex = (int)ac.guessAction.primaryOutput[0].affectedGroup;
            }
            pt.activeIndex = (int)guessAction.primaryTargeting;
            c.activeIndex = guessAction.cooldown;
            if (guessAction.secondaryOutput.Count > 0)
            {
                stg.activeIndex = (int)ac.guessAction.secondaryOutput[0].affectedGroup;
                st.activeIndex = (int)ac.guessAction.secondaryTargeting;
            }
        }
        /*
        for (int i = 0; i < ac.guessAction.primaryOutput.Count; i++)
        {
            switch (ac.guessAction.primaryOutput[i].output)
            {
                case Action.Output.Condition:
                    condition.Clone(Vector3.zero, editHolder, ac.guessAction.primaryOutput[i]);
                    break;
                default: break;
            }
        }
        for (int i = 0; i < ac.guessAction.secondaryOutput.Count; i++)
        {
            switch (ac.guessAction.secondaryOutput[i].output)
            {
                case Action.Output.Condition:
                    condition.Clone(Vector3.zero, secondEditHolder, ac.guessAction.secondaryOutput[i]);
                    break;
                default: break;
            }
        }*/
        nameInput.text = Book.currentEntry.activeAction.guessAction.actionName;
        CompareActionInformation();
        StartCoroutine(DelayLayoutDisable());
    }
    
    public void WarningMovement()
    {
        actionDescription.SetActive(!actionDescription.activeSelf);
    }

    private void LoadPreviousSetup(ActionNode[] nodes)
    {
        for (int i = 0; i < guessAction.primaryContext.Count; i++)
        {
            foreach(ActionNode n in nodes)
            {
                if (n.nodeType == ActionNode.NodeType.Context)
                {
                    if (n.actionContext.context == guessAction.primaryContext[i].context)
                    {
                        n.actionContext = guessAction.primaryContext[i];
                        n.Relocate(collectionHolder, editHolder, Vector3.zero);
                        break;
                    }
                }
            }
        }
        if (guessAction.primaryShape != Action.Shape.None)
        {
            foreach(ActionNode n in nodes)
            {
                if (n.nodeType == ActionNode.NodeType.Shape)
                {
                    if (n.actionShape == guessAction.primaryShape)
                    {
                        n.Relocate(collectionHolder, editHolder, Vector3.zero);
                        break;
                    }
                }
            }
        }
        if (guessAction.primaryOutput.Count > 0)
        {
            primaryTargetGroup = guessAction.primaryOutput[0].affectedGroup;
        }
        for (int i = 0; i < guessAction.primaryOutput.Count; i++)
        {
            foreach(ActionNode n in nodes)
            {
                if (n.nodeType == ActionNode.NodeType.Output)
                {
                    if (n.actionOutput.output == guessAction.primaryOutput[i].output)
                    {
                        n.actionOutput = new OutputInfo(guessAction.primaryOutput[i]);
                        n.Relocate(collectionHolder, editHolder, Vector3.zero);
                        break;
                    }
                }
            }
        }
        if (guessAction.secondaryShape != Action.Shape.None)
        {
            foreach (ActionNode n in nodes)
            {
                if (n.nodeType == ActionNode.NodeType.Shape)
                {
                    if (n.actionShape == guessAction.secondaryShape)
                    {
                        n.Relocate(collectionHolder, secondEditHolder, Vector3.zero);
                        break;
                    }
                }
            }
        }
        if (guessAction.secondaryOutput.Count > 0)
        {
            secondaryTargetGroup = guessAction.secondaryOutput[0].affectedGroup;
        }
        for (int i = 0; i < guessAction.secondaryOutput.Count; i++)
        {
            foreach (ActionNode n in nodes)
            {
                if (n.nodeType == ActionNode.NodeType.Output)
                {
                    if (n.actionOutput.output == guessAction.secondaryOutput[i].output)
                    {
                        n.actionOutput = new OutputInfo(guessAction.secondaryOutput[i]);
                        n.Relocate(collectionHolder, secondEditHolder, Vector3.zero);
                        break;
                    }
                }
            }
        }
        for (int i = 0; i < nodes.Length; i++)
        {
            if (Book.currentEntry.activeAction.nodeParents.ContainsKey(nodes[i]))
            {
                if (Book.currentEntry.activeAction.nodeParents[nodes[i]] == ActionNode.WindowType.Discard)
                {
                    nodes[i].Relocate(collectionHolder, discardHolder, Book.currentEntry.activeAction.nodePositions[nodes[i]]);
                }
            }
        }
    }

    IEnumerator DelayLayoutDisable()
    {
        yield return null;
        hlg1.enabled = false;
        hlg2.enabled = false;
        hlg3.enabled = false;
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
        if (!firstTenFrames)
        {
            framecount++;
            if (framecount >= 10) firstTenFrames = true;
        }
        if (nameInput.text != null) Book.currentEntry.activeAction.guessAction.actionName = nameInput.text;
        else Book.currentEntry.activeAction.guessAction.actionName = "Unnamed Action";
        if (Book.currentEntry.activeAction.guessAction.descriptionSet)
        {
            buttonText.text = $"{(Book.currentEntry.guess.characterName != null ? Book.currentEntry.guess.characterName : "The Monster")} " +
                $"{Book.instance.descriptionsList.GetList(Book.currentEntry.character.stats.bodyType)[Book.currentEntry.activeAction.guessAction.descriptionIndex]}";
        }
        else
        {
            buttonText.text = "Warning Movement";
        }
        if (action == null)
        {
            if (Book.currentEntry.activeAction.originalAction != null) action = Book.currentEntry.activeAction.originalAction;
        }
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
        if (!firstTenFrames) return;
        outputGuessesPrimary.Clear();
        contextGuessesPrimary.Clear();
        primaryShapes.Clear();

        // Clear Guessaction information
        guessAction.primaryContext.Clear();
        guessAction.primaryShape = Action.Shape.None;
        guessAction.primaryOutput.Clear();
        guessAction.secondaryShape = Action.Shape.None;
        guessAction.secondaryOutput.Clear();
        int outputCorrectCount = 0;
        int contextCorrectCount = 0;
        ActionNode node = null;
        bool primaryShapeFound = false;
        bool secondaryShapeFound = false;
        for (int i = 0; i < editHolder.childCount; i++)
        {
            node = editHolder.GetChild(i).GetComponent<ActionNode>();
            if (node.nodeType == ActionNode.NodeType.Context)
            {
                contextGuessesPrimary.Add(node.actionContext);
                guessAction.primaryContext.Add(node.actionContext);
            }
            else if (node.nodeType == ActionNode.NodeType.Shape)
            {
                primaryShapeFound = true;
                primaryShapes.Add(node.actionShape);
                guessAction.primaryShape = node.actionShape;
                if (primaryShapes.Count > 1) node.Error("Each effect can only have one shape.");
            }
            else
            {
                node.actionOutput.affectedGroup = primaryTargetGroup;
                outputGuessesPrimary.Add(node.actionOutput);
                bool damage = false, healing = false;
                for (int j = 0; j < outputGuessesPrimary.Count - 1; j++)
                {
                    if (outputGuessesPrimary[j].output == node.actionOutput.output && outputGuessesPrimary[j].output != Action.Output.Condition)
                    {
                        node.Error("Each effect can only have one of each type of output, except for conditions.");
                    }
                    damage = outputGuessesPrimary[j].output == Action.Output.Damage;
                    healing = outputGuessesPrimary[j].output == Action.Output.Healing;

                    if (healing && damage)
                    {
                        if (outputGuessesPrimary[j].output == Action.Output.Healing || outputGuessesPrimary[j].output == Action.Output.Damage)
                        {
                            node.Error("An effect cannot heal and deal damage. Consider using splitting them into the primary and secondary effect.");
                        }
                    }
                    
                }
                guessAction.primaryOutput.Add(outputGuessesPrimary[outputGuessesPrimary.Count - 1]);
            }
        }
        if (!primaryShapeFound) guessAction.primaryShape = Action.Shape.None;
        //if (primaryShapes.Count == 1 && primaryShapes[0] == action.primaryShape) print("Primary shape correct");
        CheckIncompatibility(contextGuessesPrimary);

        // Secondary effect
        //if (Book.currentEntry.activeAction.guessAction.secondaryOutput.Count != 0)
        //{
        outputGuessesSecondary.Clear();
        secondaryShapes.Clear();
        for (int i = 0; i < secondEditHolder.childCount; i++)
        {
            node = secondEditHolder.GetChild(i).GetComponent<ActionNode>();
            if (node.nodeType == ActionNode.NodeType.Context) node.Error("The secondary effect uses the context of the primary effect. Only place Shape and Output nodes here.");
            else if (node.nodeType == ActionNode.NodeType.Shape)
            {
                secondaryShapeFound = true;
                secondaryShapes.Add(node.actionShape);
                guessAction.secondaryShape = node.actionShape;
                if (secondaryShapes.Count > 1)
                {
                    node.Error("Each effect can only have one shape.");
                }
            }
            else
            {
                bool damage = false, healing = false;
                node.actionOutput.affectedGroup = secondaryTargetGroup;
                outputGuessesSecondary.Add(node.actionOutput);
                for (int j = 0; j < outputGuessesSecondary.Count - 1; j++)
                {
                    if (outputGuessesSecondary[j].output == node.actionOutput.output && outputGuessesSecondary[j].output != Action.Output.Condition)
                    {
                        node.Error("Each effect can only have one of each type of output, except for conditions.");
                    }
                    damage = outputGuessesSecondary[j].output == Action.Output.Damage;
                    healing = outputGuessesSecondary[j].output == Action.Output.Healing;

                    if (healing && damage)
                    {
                        if (outputGuessesSecondary[j].output == Action.Output.Healing || outputGuessesSecondary[j].output == Action.Output.Damage)
                        {
                            node.Error("An effect cannot both heal and deal damage. Consider using splitting them into the primary and secondary effect.");
                        }
                    }
                }
                guessAction.secondaryOutput.Add(outputGuessesSecondary[outputGuessesSecondary.Count - 1]);
            }
        }
        if (!secondaryShapeFound) guessAction.secondaryShape = Action.Shape.None;
        //}

        LogBuilder();

        // Comparison
        if (action == null) return;
        print("running comparison");
        for (int i = 0; i < guessAction.primaryContext.Count; i++)
        {
            for (int j = 0; j < action.primaryContext.Count; j++)
            {
                if (guessAction.primaryContext[i].Match(action.primaryContext[j])) contextCorrectCount++;
            }
        }
        for (int i = 0; i < guessAction.primaryOutput.Count; i++)
        {
            for (int j = 0; j < action.primaryOutput.Count; j++)
            {
                if (guessAction.primaryOutput[i].Match(action.primaryOutput[j])) outputCorrectCount++;
            }
        }
        bool primaryNodesCorrect = false;
        if (contextCorrectCount == action.primaryContext.Count && outputCorrectCount == action.primaryOutput.Count) primaryNodesCorrect = true;
        print($"{guessAction.primaryContext.Count} contexts on guess, {action.primaryContext.Count} on actual");
        print($"{guessAction.primaryOutput.Count} outputs on guess, {action.primaryOutput.Count} on actual");
        print($"{primaryNodesCorrect}, with {contextCorrectCount}/{action.primaryContext.Count} contexts, and {outputCorrectCount}/{action.primaryOutput.Count} outputs.");

        outputCorrectCount = 0;

        for (int i = 0; i < guessAction.secondaryOutput.Count; i++)
        {
            for (int j = 0; j < action.secondaryOutput.Count; j++)
            {
                if (guessAction.secondaryOutput[i].Match(action.secondaryOutput[j])) outputCorrectCount++;
            }
        }
        bool secondaryNodesCorrect = false;
        if (outputCorrectCount == action.secondaryOutput.Count) secondaryNodesCorrect = true;

        // Check primary
        if (guessAction.descriptionIndex != action.descriptionIndex) return;
        if (primaryShapes.Count != 1 || guessAction.primaryShape != action.primaryShape) return;
        if (!primaryNodesCorrect) return;
        /*
        if (guessAction.primaryOutput.Count > 0)
        {
            if (guessAction.primaryOutput[0].affectedGroup != action.primaryOutput[0].affectedGroup)
            {
                return;
            }
            print("affected group correct");
        }
        else return;
        if (guessAction.primaryTargeting != action.primaryTargeting) return;
        if (guessAction.cooldown != action.cooldown) return;*/

        // Check secondary
        if (action.secondaryOutput.Count > 0)
        {
            if (!secondaryNodesCorrect) return;
            if (secondaryShapes.Count != 1 || guessAction.secondaryShape != action.secondaryShape) return;
            /*if (guessAction.secondaryTargeting != action.secondaryTargeting) return;
            if (guessAction.secondaryOutput[0].affectedGroup != action.secondaryOutput[0].affectedGroup) return;*/
        }
        Book.currentEntry.activeAction.informationCorrect = true;
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
            if (temp.Count > 0)
            {
                contextNodes[list[i].context].Incompatible(temp);
                print("Display error");
            }

        }
    }
    private void LogBuilder()
    {
        StringBuilder log = new StringBuilder();
        OutputInfo primaryMove = null, secondaryMove = null;
        bool flag1 = false, flag2 = false;
        Log elements = GameManager.instance.logElementCollection;

        if (guessAction.primaryOutput.Count == 0 || guessAction.primaryContext.Count == 0 || guessAction.primaryShape == Action.Shape.None || guessAction.descriptionIndex == -1)
        {
            guessAction.targetingSet = false;
            log.Append($"Add at least 1 <b><color={(guessAction.primaryContext.Count > 0 ? "#015400" : "red")}>Context</color></b>, 1 " +
                $"<b><color={(guessAction.primaryShape != Action.Shape.None ? "#015400" : "red")}>Shape</color></b>, " +
                $"and 1 <b><color={(guessAction.primaryOutput.Count > 0 ? "#015400" : "red")}>Output</color></b>, " +
                $"and set the <b><color={(guessAction.descriptionIndex > -1 ? "#015400" : "red")}>Warning Movement</color></b> for this action to be valid.");
            logTest.text = log.ToString();
            Book.currentEntry.activeAction.guessAction.description = "This action lacks information. Click here to add it.";
            return;
        }
        else guessAction.targetingSet = true;

        log.Append("If the monster ");
        if (guessAction.primaryOutput.Count > 0)
        {
            guessAction.primaryOutput.Sort((g1, g2) => g1.output.CompareTo(g2.output));
        }
        if (guessAction.secondaryOutput.Count>0)
        {
            guessAction.secondaryOutput.Sort((g1, g2) => g1.output.CompareTo(g2.output));
        }
        if (guessAction.primaryContext.Count > 0)
        {
            for (int i = 0; i < guessAction.primaryContext.Count; i++)
            {
                if (i != 0 && i == guessAction.primaryContext.Count - 1) log.Append("and ");
                switch (guessAction.primaryContext[i].context)
                {
                    default:
                        log.Append(elements.GetString(guessAction.primaryContext[i].context));
                        break;
                    case Action.Context.AllyHasSpecificCondition:
                    case Action.Context.EnemyHasSpecificCondition:
                    case Action.Context.SelfHasSpecificCondition:
                        log.Append(elements.GetString(guessAction.primaryContext[i].context, guessAction.primaryContext[i].condition));
                        break;
                    case Action.Context.TookDamageOfType:
                        log.Append(elements.GetString(guessAction.primaryContext[i].context, guessAction.primaryContext[i].damageType));
                        break;
                }
                log.Append(", ");
            }
        }
        log.Append("the monster ").Append(guessAction.actionDescription).Append(", showing that it will ");
        if (guessAction.primaryOutput.Count > 0)
        {
            if (guessAction.primaryOutput.Count == 1 && guessAction.primaryOutput[0].output == Action.Output.Movement)
            {
                log.Append("target ");
                flag1 = true;
            }
            else if (guessAction.primaryOutput.Count > 1 && guessAction.primaryOutput[guessAction.primaryOutput.Count-1].output == Action.Output.Movement)
            {
                flag1 = true;
            }
            for (int i = 0; i < guessAction.primaryOutput.Count; i++)
            {
                if (!flag1)
                {
                    if (i != 0 && i == guessAction.primaryOutput.Count - 1) log.Append("and ");
                }
                else
                {
                    if (i != 0 && i == guessAction.primaryOutput.Count - 2) log.Append("and ");
                }
                switch (guessAction.primaryOutput[i].output)
                {
                    case Action.Output.Damage:
                        log.Append(elements.GetString(guessAction.primaryOutput[i].output, guessAction.primaryOutput[i].damageType, guessAction.primaryOutput[i].critical));
                        break;
                    case Action.Output.Healing:
                        log.Append(elements.GetString(guessAction.primaryOutput[i].output, guessAction.primaryOutput[i].value));
                        break;
                    case Action.Output.Condition:
                        log.Append(elements.GetString(guessAction.primaryOutput[i].output, guessAction.primaryOutput[i].condition, guessAction.primaryOutput[i].value));
                        break;
                    case Action.Output.Movement:
                        primaryMove = guessAction.primaryOutput[i];                        
                        break;
                }
                if (!flag1)
                {
                    if (i != guessAction.primaryOutput.Count - 1)
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
                    if (i != guessAction.primaryOutput.Count - 2 && i != guessAction.primaryOutput.Count-1)
                    {
                        log.Append(", ");
                    }
                    else if (i != guessAction.primaryOutput.Count-1)
                    {
                        log.Append(" to ");
                    }
                }
            }
        }
        if (guessAction.primaryShape != Action.Shape.None)
        {
            log.Append(elements.GetString(guessAction.primaryShape, primaryTargetGroup, guessAction.primaryTargeting));
        }
        if (primaryMove != null)
        {
            log.Append(" ");

            log.Append(elements.GetString(primaryMove.output, primaryMove.value, primaryMove.towards));
        }
        log.Append(".");
        if (guessAction.secondaryOutput.Count > 0)
        {
            log.Append(" Then it will ");
            if (guessAction.secondaryOutput.Count == 1 && guessAction.secondaryOutput[0].output == Action.Output.Movement)
            {
                log.Append("target ");
                flag2 = true;
            }
            else if (guessAction.secondaryOutput.Count > 1 && guessAction.secondaryOutput[guessAction.secondaryOutput.Count - 1].output == Action.Output.Movement)
            {
                flag2 = true;
            }
            for (int i = 0; i < guessAction.secondaryOutput.Count; i++)
            {
                if (!flag2)
                {
                    if (i != 0 && i == guessAction.secondaryOutput.Count - 1) log.Append("and ");
                }
                else
                {
                    if (i != 0 && i == guessAction.secondaryOutput.Count - 2) log.Append("and ");
                }
                switch (guessAction.secondaryOutput[i].output)
                {
                    case Action.Output.Damage:
                        log.Append(elements.GetString(guessAction.secondaryOutput[i].output, guessAction.secondaryOutput[i].damageType, guessAction.secondaryOutput[i].critical));
                        break;
                    case Action.Output.Healing:
                        log.Append(elements.GetString(guessAction.secondaryOutput[i].output, guessAction.secondaryOutput[i].value));
                        break;
                    case Action.Output.Condition:
                        log.Append(elements.GetString(guessAction.secondaryOutput[i].output, guessAction.secondaryOutput[i].condition, guessAction.secondaryOutput[i].value));
                        break;
                    case Action.Output.Movement:
                        secondaryMove = guessAction.secondaryOutput[i];
                        break;
                }
                if (!flag2)
                {
                    if (i != guessAction.secondaryOutput.Count - 1)
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
                    if (i != guessAction.secondaryOutput.Count - 2 && i != guessAction.secondaryOutput.Count - 1)
                    {
                        log.Append(", ");
                    }
                    else if (i != guessAction.secondaryOutput.Count - 1)
                    {
                        log.Append(" to ");
                    }
                }
            }
            if (guessAction.secondaryShape != Action.Shape.None)
            {
                log.Append(elements.GetString(guessAction.secondaryShape, secondaryTargetGroup, guessAction.secondaryTargeting));
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
