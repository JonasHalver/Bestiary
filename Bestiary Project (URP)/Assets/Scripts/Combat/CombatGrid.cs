using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class CombatGrid : MonoBehaviour, IPointerDownHandler
{
    public static CombatGrid instance;

    public static Node[,] grid = new Node[5, 5];
    public TextMeshProUGUI debug;
    private string debugText;

    public GameObject canvas;
    public static GraphicRaycaster gRaycaster;

    public Color actionHighlight, characterHighlight, movementHighlight, errorHighlight;

    private Node selectedNode;
    private Dictionary<Character, Vector2> savedPositions = new Dictionary<Character, Vector2>();
    public static bool displayingThePast;

    public Gradient heatmap;

    public static Dictionary<Node, int> gridRows = new Dictionary<Node, int>();
    public List<Transform> rows = new List<Transform>();


    private void Awake()
    {
        instance = this;
        gRaycaster = canvas.GetComponent<GraphicRaycaster>();
    }
    private void OnEnable()
    {
        TutorialManager.ShowGrid += ShowGrid;
    }
    private void OnDisable()
    {
        TutorialManager.ShowGrid -= ShowGrid;
    }
    public void ShowGrid()
    {
        StartCoroutine(SequentialFadeIn());
    }
    IEnumerator SequentialFadeIn()
    {
        foreach (Node n in grid)
        {
            n.tile.GetComponent<TileFadeIn>().FadeIn();
            yield return new WaitForSeconds(1 / 25);
        }
    }
    void Start()
    {
        int index = 0;
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                Node newNode = new Node(transform.GetChild(index).gameObject, x, y);
                grid[x, y] = newNode;
                AssignToRow(newNode);
                index++;
            }
        }
        foreach(Node n in grid)
        {
            n.FindNeighbors();
        }        
    }
    private void AssignToRow(Node n)
    {
        switch (n.x)
        {
            case 0:
                switch (n.y)
                {
                    case 0:
                        gridRows.Add(n, 5);
                        break;
                    case 1:
                        gridRows.Add(n, 6);
                        break;
                    case 2:
                        gridRows.Add(n, 7);
                        break;
                    case 3:
                        gridRows.Add(n, 8);
                        break;
                    case 4:
                        gridRows.Add(n, 9);
                        break;
                }
                break;
            case 1:
                switch (n.y)
                {
                    case 0:
                        gridRows.Add(n, 4);
                        break;
                    case 1:
                        gridRows.Add(n, 5);
                        break;
                    case 2:
                        gridRows.Add(n, 6);
                        break;
                    case 3:
                        gridRows.Add(n, 7);
                        break;
                    case 4:
                        gridRows.Add(n, 8);
                        break;
                }
                break;
            case 2:
                switch (n.y)
                {
                    case 0:
                        gridRows.Add(n, 3);
                        break;
                    case 1:
                        gridRows.Add(n, 4);
                        break;
                    case 2:
                        gridRows.Add(n, 5);
                        break;
                    case 3:
                        gridRows.Add(n, 6);
                        break;
                    case 4:
                        gridRows.Add(n, 7);
                        break;
                }
                break;
            case 3:
                switch (n.y)
                {
                    case 0:
                        gridRows.Add(n, 2);
                        break;
                    case 1:
                        gridRows.Add(n, 3);
                        break;
                    case 2:
                        gridRows.Add(n, 4);
                        break;
                    case 3:
                        gridRows.Add(n, 5);
                        break;
                    case 4:
                        gridRows.Add(n, 6);
                        break;
                }
                break;
            case 4:
                switch (n.y)
                {
                    case 0:
                        gridRows.Add(n, 1);
                        break;
                    case 1:
                        gridRows.Add(n, 2);
                        break;
                    case 2:
                        gridRows.Add(n, 3);
                        break;
                    case 3:
                        gridRows.Add(n, 4);
                        break;
                    case 4:
                        gridRows.Add(n, 5);
                        break;
                }
                break;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerEnter)
        {
            if (eventData.pointerId == -1)
            {
                foreach(Node n in grid)
                {
                    if (n.tile == eventData.pointerEnter)
                    {
                        //SelectNode(n);
                    }
                }
            }
            else if (eventData.pointerId == -2)
            {
                foreach(Node n in grid)
                {
                    if (n.tile == eventData.pointerEnter)
                    {
                        //HighlightNeighbors(n);
                    }
                }
            }
        }
    }

    void Update()
    {

    }

    public static void HighlightNodeStatic(Node node)
    {
        if (node == null) return;
        foreach(Node n in grid)
        {
            if (n == node)
            {
                instance.SelectNode(n);
            }
        }
    }

    public static void MovementHighlight(List<Node> nodes)
    {
        foreach(Node n in grid)
        {
            if (nodes.Contains(n))
            {
                n.MovementHightlight();
            }
            else n.NodeDeselected();
        }
    }
    
    public void HighlighAction(CombatAction ca)
    {
        foreach (CombatAction c in CombatManager.combatActions) if (c != ca) c.highlighted = false;
        //StopHighlight();
        if (ca != null && ca.action.actionName != "Pass") 
        {
            if (ca.primaryTarget != null)
            {
                for (int i = 0; i < ca.primaryTarget.AffectedNodes.Count; i++)
                {
                    ca.primaryTarget.AffectedNodes[i].ActionHighlight();
                }
            }
            if (ca.secondaryTarget != null)
            {
                for (int i = 0; i < ca.secondaryTarget.AffectedNodes.Count; i++)
                {
                    ca.secondaryTarget.AffectedNodes[i].ActionHighlight();
                }
            }
            ca.highlighted = true;
        }       
    }

    public static Node CharacterSpawn(bool isMonster)
    {
        if (GameManager.tutorial && TutorialManager.instance.currentSequence == TutorialManager.TutorialSequence.Main1)
        {
            if (isMonster) return grid[2, 4];
            else return grid[2, 1];
        }
        List<Node> possibleSpawns = new List<Node>();
        foreach(Node n in grid)
        {
            if (isMonster)
            { 
                if (n.coordinate.y >= 3 && n.occupant == null) possibleSpawns.Add(n); 
            }
            else
            {
                if (n.coordinate.y <= 1 && n.occupant == null) possibleSpawns.Add(n);
            }

        }
        return possibleSpawns[Random.Range(0, possibleSpawns.Count)];
    }

    public static List<Node> NodesAffectedByAction(CombatAction ca)
    {
        List<Node> output = new List<Node>();
        if (ca.action.isPass  || ca.primaryTarget == null)
        {
            output.Add(ca.origin.movement.currentNode);
            return output;
        }

        output.AddRange(ca.primaryTarget.AffectedNodes);
        if (ca.secondaryTarget != null)
        {
            output.AddRange(ca.secondaryTarget.AffectedNodes);
        }        
        return output;
    }

    public void SelectNode(Node node)
    {
        //foreach (Node n in grid)
        //{
        //    n.NodeDeselected();
        //}
        //if (selectedNode != null) selectedNode.NodeDeselected();
        selectedNode = node;
        node.NodeSelected();
    }

    public void DeselectNode()
    {
        if (selectedNode != null) selectedNode.NodeDeselected();
    }

    public static void StopHighlight()
    {
        foreach (Node n in grid)
        {
            n.NodeDeselected();
        }
    }

    public static ShapeTest ShapeTest(ShapeTest test, Action.Shape shape)
    {
        Node originNode = test.origin.movement.currentNode;
        Vector2 originV2 = originNode.coordinate;
        List<TargetingTest> tests = new List<TargetingTest>();
        List<Node> targetTest = new List<Node>();

        if (shape == Action.Shape.Area)
        {
            foreach (Node node in grid)
            {
                if (node != null)
                {
                    targetTest = GenerateThreeByThree(node);
                    tests.Add(new TargetingTest(targetTest, node));
                }

            }
        }
        else if (shape == Action.Shape.Pulse)
        {
            List<Node> nodelist = new List<Node>();
            foreach (Node node in originNode.neighbors)
            {
                if (node != null) nodelist.Add(node);
            }
            tests.Add(new TargetingTest(nodelist, originNode));
        }
        else
        {
            foreach (Node node in originNode.neighbors)
            {
                if (node != null)
                {
                    switch (shape)
                    {
                        case Action.Shape.Arc:
                            targetTest = GenerateArc(node.coordinate, originV2);
                            break;
                        case Action.Shape.Cone:
                            targetTest = GenerateCone(node.coordinate, originV2);
                            break;
                        case Action.Shape.Line:
                            targetTest = GenerateLine(node.coordinate, originV2);
                            break;
                    }
                    tests.Add(new TargetingTest(targetTest, node));
                }
            }
        }
        if (tests.Count > 0)
        {
            test.tests.AddRange(tests);
            test.Validation();
        }
        return test;
    }

    public static ShapeTest ArcTest(ShapeTest test)
    {
        bool valid = false;

        Node originNode = test.origin.movement.currentNode;
        Vector2 originV2 = originNode.coordinate;

        int hitCount = 0, hitBest = 0;
        bool validTarget = false;
        Node bestTarget = null;
        List<TargetingTest> tests = new List<TargetingTest>();
        ShapeTest output = new ShapeTest(test.action, test.origin);
        foreach (Node node in originNode.neighbors)
        {
            if (node != null)
            {
                hitCount = 0;
                List<Node> targetTest = GenerateArc(node.coordinate, originV2);
                tests.Add(new TargetingTest(targetTest, node));
                
                /*foreach(Node n in targetTest)
                {
                    if (n.occupant != null)
                    {
                        switch (test.targetGroup)
                        {
                            case Action.TargetGroup.Enemies:
                                validTarget = !Character.AllyOrEnemy(test.origin, n.occupant);
                                break;
                            case Action.TargetGroup.Allies:
                                validTarget = Character.AllyOrEnemy(test.origin, n.occupant);
                                break;
                            case Action.TargetGroup.All:
                                validTarget = true;
                                break;
                        }
                        if (validTarget && n.occupant.alive)
                            hitCount++;
                    }                    
                }
                if (hitCount >= test.hitMinimum)
                {
                    output.potentialTargets.Add(new TargetInfo(node, targetTest));
                }*/                
            }
        }
        if (tests.Count > 0)
        {
            output.tests.AddRange(tests);
            output.valid = true;
        }
        return output;
    }

    public static ShapeTest ConeTest(ShapeTest test)
    {
        bool valid = false;
        bool validTarget = false;
        Node bestTarget = null;

        Node originNode = test.origin.movement.currentNode;
        Vector2 originV2 = originNode.coordinate;
        ShapeTest output = new ShapeTest(test.action, test.origin);

        int hitCount = 0, hitCountMax = 0;
        foreach (Node node in originNode.neighbors)
        {
            if (node != null)
            {
                hitCount = 0;
                List<Node> targetTest = GenerateCone(node.coordinate, originV2);
                foreach (Node n in targetTest)
                {
                    if (n.occupant != null)
                    {
                        switch (test.targetGroup)
                        {
                            case Action.TargetGroup.Enemies:
                                validTarget = !Character.AllyOrEnemy(test.origin, n.occupant);
                                break;
                            case Action.TargetGroup.Allies:
                                validTarget = Character.AllyOrEnemy(test.origin, n.occupant);
                                break;
                            case Action.TargetGroup.All:
                                validTarget = true;
                                break;
                        }
                        if (validTarget && n.occupant.alive)
                            hitCount++;
                    }

                }
                if (hitCount >= test.hitMinimum)
                {
                    output.potentialTargets.Add(new TargetInfo(node, targetTest));
                }
            }
        }
        if (output.potentialTargets.Count > 0) output.valid = true;

        return output;
    }

    public static ShapeTest LineTest(ShapeTest test)
    {
        bool valid = false;
        bool validTarget = false;
        Node bestTarget = null;

        Node originNode = test.origin.movement.currentNode;
        Vector2 originV2 = originNode.coordinate;
        ShapeTest output = new ShapeTest(test.action, test.origin);

        int hitCount = 0, hitCountMax = 0;

        foreach (Node node in originNode.neighbors)
        {
            if (node != null)
            {
                hitCount = 0;
                List<Node> targetTest = GenerateLine(node.coordinate, originV2);
                foreach (Node n in targetTest)
                {
                    if (n.occupant != null)
                    {
                        switch (test.targetGroup)
                        {
                            case Action.TargetGroup.Enemies:
                                validTarget = !Character.AllyOrEnemy(test.origin, n.occupant);
                                break;
                            case Action.TargetGroup.Allies:
                                validTarget = Character.AllyOrEnemy(test.origin, n.occupant);
                                break;
                            case Action.TargetGroup.All:
                                validTarget = true;
                                break;
                        }
                        if (validTarget && n.occupant.alive)
                            hitCount++;
                    }

                }
                if (hitCount >= test.hitMinimum)
                {
                    output.potentialTargets.Add(new TargetInfo(node, targetTest));
                }
            }
        }
        if (output.potentialTargets.Count > 0) output.valid = true;

        return output;
    }

    public static ShapeTest PulseTest(ShapeTest test)
    {
        bool valid = false, validTarget = false;
        Node bestTarget = null;
        ShapeTest output = new ShapeTest(test.action, test.origin);

        Node originNode = test.origin.movement.currentNode;
        List<Node> nodeList = new List<Node>();
        int hitCount = 0;
        foreach (Node node in originNode.neighbors)
        {
            if (node != null)
            {
                nodeList.Add(node);
                if (node.occupant != null)
                {
                    switch (test.targetGroup)
                    {
                        case Action.TargetGroup.Enemies:
                            validTarget = !Character.AllyOrEnemy(test.origin, node.occupant);
                            break;
                        case Action.TargetGroup.Allies:
                            validTarget = Character.AllyOrEnemy(test.origin, node.occupant);
                            break;
                        case Action.TargetGroup.All:
                            validTarget = true;
                            break;
                    }
                    if (validTarget && node.occupant.alive)
                        hitCount++;
                }
            }
        }
        if (hitCount >= test.hitMinimum)
        {
            output.potentialTargets.Add(new TargetInfo(originNode, nodeList));
        }
        if (output.potentialTargets.Count > 0) output.valid = true;

        return output;
    }

    public static ShapeTest AreaTest(ShapeTest test)
    {
        bool valid = false;
        bool validTarget = false;
        Node bestTarget = null;
        ShapeTest output = new ShapeTest(test.action, test.origin);

        int hitCount = 0, hitCountMax = 0;

        foreach (Node node in grid)
        {
            hitCount = 0;
            List<Node> targetTest = GenerateThreeByThree(node);
            foreach (Node n in targetTest)
            {
                if (n.occupant != null)
                {
                    switch (test.targetGroup)
                    {
                        case Action.TargetGroup.Enemies:
                            validTarget = !Character.AllyOrEnemy(test.origin, n.occupant);
                            break;
                        case Action.TargetGroup.Allies:
                            validTarget = Character.AllyOrEnemy(test.origin, n.occupant);
                            break;
                        case Action.TargetGroup.All:
                            validTarget = true;
                            break;
                    }
                    if (validTarget && n.occupant.alive)
                        hitCount++;
                }

            }
            if (hitCount >= test.hitMinimum)
            {
                output.potentialTargets.Add(new TargetInfo(node, targetTest));
            }
        }
    
        if (output.potentialTargets.Count > 0) output.valid = true;

        return output;
    }

    public TargetingTest TargetEvaluation(List<TargetingTest> targets, Character actor, Action action, bool primary)
    {
        bool characterIsMonster = actor.stats.characterType == CharacterStats.CharacterTypes.NPC;
        TargetingTest output = null;
        float hp = 100;
        int hits = 100;
        Action.TargetGroup tg = primary ? action.primaryTargetGroup : action.secondaryTargetGroup;
        switch (tg)
        {
            case Action.TargetGroup.Allies:
                if (characterIsMonster) targets.Sort((t2, t1) => t1.monstersHit.Count.CompareTo(t2.monstersHit.Count));
                else targets.Sort((t2, t1) => t1.mercsHit.Count.CompareTo(t2.mercsHit.Count));
                break;
            case Action.TargetGroup.Enemies:
                if (!characterIsMonster) targets.Sort((t2, t1) => t1.monstersHit.Count.CompareTo(t2.monstersHit.Count));
                else targets.Sort((t2, t1) => t1.mercsHit.Count.CompareTo(t2.mercsHit.Count));
                break;
            case Action.TargetGroup.All:
                targets.Sort((t2, t1) => t1.totalHits.CompareTo(t2.totalHits));
                break;
        }
        switch (action.targetPriority)
        {
            case Action.TargetPriority.MostHits:
                output = targets[0];
                break;
            case Action.TargetPriority.MostHurt:
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].HPOfMostHurtCharacter(tg, actor) < hp)
                    {
                        hp = targets[i].HPOfMostHurtCharacter(tg, actor);
                        output = targets[i];
                    }
                }
                break;
            case Action.TargetPriority.HasSpecificCondition:
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].TargetHasCondition(tg, actor, action.priorityConditionComparison))
                    {
                        output = targets[i];
                        break;
                    }
                }
                break;
            case Action.TargetPriority.DoesntHaveSpecificCondition:
                for (int i = 0; i < targets.Count; i++)
                {
                    if (!targets[i].TargetHasCondition(tg, actor, action.priorityConditionComparison))
                    {
                        output = targets[i];
                        break;
                    }
                }
                break;
            case Action.TargetPriority.NotHurtingAlly:
                if (actor.stats.characterType == CharacterStats.CharacterTypes.Adventurer)
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i].mercsHit.Count < hits)
                        {
                            hits = targets[i].mercsHit.Count;
                            output = targets[i];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (targets[i].monstersHit.Count < hits)
                        {
                            hits = targets[i].monstersHit.Count;
                            output = targets[i];
                        }
                    }
                }
                break;
        }
        if (output == null && targets.Count > 0)
        {
            output = targets[0];
            //Debug.Log($"{actor.stats.characterName} tried to target {action.targetPriority}, but failed to find a good target amongst {targets.Count} targets");
        }
        return output;
    }

    public static List<Node> GenerateArc(Vector2 target, Vector2 origin)
    {
        List<Node> output = new List<Node>();
        output.Add(grid[(int)target.x, (int)target.y]);
        Vector2 dir = target - origin;

        if (dir.x == 0)
        {
            if (dir.y == 1)
            {
                if (target.x != 4) output.Add(grid[(int)target.x + 1, (int)target.y]);
                if (target.x != 0) output.Add(grid[(int)target.x - 1, (int)target.y]);
            }
            else if (dir.y == -1)
            {
                if (target.x != 4) output.Add(grid[(int)target.x + 1, (int)target.y]);
                if (target.x != 0) output.Add(grid[(int)target.x - 1, (int)target.y]);
            }
        }
        else if (dir.y == 0)
        {
            if (dir.x == 1)
            {
                if (target.y != 4) output.Add(grid[(int)target.x, (int)target.y + 1]);
                if (target.y != 0) output.Add(grid[(int)target.x, (int)target.y - 1]);
            }
            else if (dir.x == -1)
            {
                if (target.y != 4) output.Add(grid[(int)target.x, (int)target.y + 1]);
                if (target.y != 0) output.Add(grid[(int)target.x, (int)target.y - 1]);
            }
        }
        else if (Mathf.Abs(dir.y) == 1 && Mathf.Abs(dir.x) == 1)
        {
            if (dir.x < 0)
            {
                if (dir.y < 0)
                {
                    output.Add(grid[(int)target.x + 1, (int)target.y]);
                    output.Add(grid[(int)target.x, (int)target.y + 1]);
                }
                else
                {
                    output.Add(grid[(int)target.x, (int)target.y - 1]);
                    output.Add(grid[(int)target.x + 1, (int)target.y]);
                }
            }
            else
            {
                if (dir.y < 0)
                {
                    output.Add(grid[(int)target.x, (int)target.y + 1]);
                    output.Add(grid[(int)target.x - 1, (int)target.y]);
                }
                else
                {
                    output.Add(grid[(int)target.x, (int)target.y - 1]);
                    output.Add(grid[(int)target.x - 1, (int)target.y]);
                }
            }
        }
        return output;
    }

    public static List<Node> GenerateCone(Vector2 target, Vector2 origin)
    {
        List<Node> output = new List<Node>();

        Vector2 dir = target - origin;
        Vector2 test;
        // Cardinal directions
        if (dir.x == 0)
        {
            if (dir.y == 1)
            {
                test = origin + new Vector2(0, 1);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(0, 2);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(0, 3);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(1, 2);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(-1, 2);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(1, 3);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(-1, 3);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
            }
            if (dir.y == -1)
            {
                test = origin + new Vector2(0, -1);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(0, -2);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(0, -3);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(1, -2);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(-1, -2);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(1, -3);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(-1, -3);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
            }
        }
        if (dir.y == 0)
        {
            if (dir.x == 1)
            {
                test = origin + new Vector2(1, 0);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(2, 0);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(3, 0);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(2, 1);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(2, -1);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(3, 1);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(3, -1);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
            }
            if (dir.x == -1)
            {
                test = origin + new Vector2(-1, 0);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(-2, 0);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(-3, 0);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(-2, 1);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(-2, -1);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(-3, 1);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                test = origin + new Vector2(-3, -1);
                if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
            }
        }

        // Corners
        if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
        {
            if (dir.x == 1)
            {
                if (dir.y == 1)
                {
                    test = origin + new Vector2(1, 1);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(1, 2);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(1, 3);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(2, 1);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(2, 2);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(3, 1);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                }
                else if (dir.y == -1)
                {
                    test = origin + new Vector2(1, -1);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(1, -2);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(1, -3);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(2, -1);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(2, -2);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(3, -1);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                }
            }
            else if (dir.x == -1)
            {
                if (dir.y == 1)
                {
                    test = origin + new Vector2(-1, 1);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(-1, 2);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(-1, 3);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(-2, 1);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(-2, 2);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(-3, 1);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                }
                else if (dir.y == -1)
                {
                    test = origin + new Vector2(-1, -1);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(-1, -2);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(-1, -3);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(-2, -1);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(-2, -2);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                    test = origin + new Vector2(-3, -1);
                    if (NodeIsOnGrid(test)) output.Add(grid[(int)test.x, (int)test.y]);
                }
            }
        }

        return output;
    }

    public static List<Node> GenerateLine(Vector2 target, Vector2 origin)
    {
        List<Node> output = new List<Node>();
        Vector2 dir = target - origin;
        Vector2 test = origin + dir;

        for (int i = 0; i < 5; i++)
        {
            if (NodeIsOnGrid(test))
            {
                output.Add(grid[(int)test.x, (int)test.y]);
                test += dir;
            }
            else break;
        } 
        return output;
    }

    public static List<Node> GenerateThreeByThree(Node center)
    {
        List<Node> output = new List<Node>();
        output.Add(center);

        foreach(Node n in center.neighbors)
        {
            if (n != null) output.Add(n);
        }

        return output;
    }

    public static Node IdentifyNodeFromTile(GameObject tile)
    {
        Node output = null;
        foreach(Node n in grid)
        {
            if (n.tile == tile)
            {
                output = n;
            }
        }
        return output;
    }

    public static bool NodeIsOnGrid(Vector2 coordinate)
    {
        bool output = true;

        if (coordinate.x < 0 || coordinate.x > 4) output = false;
        if (coordinate.y < 0 || coordinate.y > 4) output = false;

        return output;
    }

    public void HighlightNeighbors(Node node)
    {
        foreach (Node n in grid)
        {
            n.NodeDeselected();
        }

        node.NodeNeighborHighlight();
    }

    public static int Vector2ToDistance(Vector2 pos1, Vector2 pos2)
    {
        Vector2 dir = pos1 - pos2;
        int dist = Mathf.Max(Mathf.Abs(Mathf.RoundToInt(dir.x)), Mathf.Abs(Mathf.RoundToInt(dir.y)));
        return dist;
    }
    public static int NodeToDistance(Node n1, Node n2)
    {
        Vector2 dir = n1.coordinate - n2.coordinate;
        int dist = Mathf.Max(Mathf.Abs(Mathf.RoundToInt(dir.x)), Mathf.Abs(Mathf.RoundToInt(dir.y)));
        return dist;
    }
    public static Node NodeFromPosition(Vector2 position)
    {
        int x, y;
        x = (int)position.x;
        y = (int)position.y;
        Node output = null;
        if (x > -1 && x < 5)
        {
            if (y > -1 && y < 5)
            {
                output = grid[x, y];
            }
        }
        return output;
    }

    public static void ShowPreviousPositions(BattlefieldPositionInfo bpi, Character user, List<Character> victims)
    {
        StopHighlight();
        instance.savedPositions = CombatManager.characterPositions;
        foreach(KeyValuePair<Character,Vector2> kvp in bpi.characterPositions)
        {
            Node n = grid[(int)kvp.Value.x, (int)kvp.Value.y];
            kvp.Key.movement.DisplayPastPosition(n);

            if (kvp.Key == user) n.NodeSelected();
            else if (victims.Contains(kvp.Key)) n.ActionHighlight();
        }
        displayingThePast = true;
    }
    public static void ShowCurrentPositions()
    {
        StopHighlight();
        foreach (KeyValuePair<Character, Vector2> kvp in instance.savedPositions)
        {
            kvp.Key.movement.DisplayPastPosition(grid[(int)kvp.Value.x, (int)kvp.Value.y]);
        }
        displayingThePast = false;
    }
    public class TargetingTest
    {
        public List<Node> targetNodes = new List<Node>();
        public Node targetNode;
        public List<Character> mercsHit = new List<Character>();
        public List<Character> monstersHit = new List<Character>();
        public int totalHits;
        public TargetingTest(List<Node> l, Node t)
        {
            targetNodes = l;
            targetNode = t;
            for (int i = 0; i < l.Count; i++)
            {
                if (l[i].occupant != null)
                {
                    if (l[i].occupant.alive)
                    {
                        if (l[i].occupant.stats.characterType == CharacterStats.CharacterTypes.Adventurer) mercsHit.Add(l[i].occupant);
                        else monstersHit.Add(l[i].occupant);
                    }
                }
            }
            totalHits = mercsHit.Count + monstersHit.Count;
        }
        public float HPOfMostHurtCharacter(Action.TargetGroup targetGroup, Character actor)
        {
            float lowestMonsterHP = 100;
            float lowestMercHP = 100;

            for (int i = 0; i < targetNodes.Count; i++)
            {
                if (targetNodes[i].occupant != null)
                {
                    Character c = targetNodes[i].occupant;
                    switch (c.stats.characterType)
                    {
                        case CharacterStats.CharacterTypes.Adventurer:
                            if (c.currentHitpoints < lowestMercHP) lowestMercHP = c.currentHitpoints;
                            break;
                        case CharacterStats.CharacterTypes.NPC:
                            if (c.currentHitpoints < lowestMonsterHP) lowestMonsterHP = c.currentHitpoints;
                            break;
                    }
                }
            }
            switch (targetGroup)
            {
                default: return 100;
                case Action.TargetGroup.All:
                    return Mathf.Min(lowestMercHP, lowestMonsterHP);
                case Action.TargetGroup.Allies:
                    return actor.stats.characterType == CharacterStats.CharacterTypes.NPC ? lowestMonsterHP : lowestMercHP;
                case Action.TargetGroup.Enemies:
                    return actor.stats.characterType != CharacterStats.CharacterTypes.NPC ? lowestMonsterHP : lowestMercHP;                    
            }
        }
        public bool TargetHasCondition(Action.TargetGroup tg, Character actor, Action.Condition condition)
        {
            bool output = false;
            switch (tg)
            {
                case Action.TargetGroup.All:
                    for (int i = 0; i < monstersHit.Count; i++)
                    {
                        if (monstersHit[i].Conditions.ContainsKey(condition))
                        {
                            output = true;
                            break;
                        }
                    }
                    if (!output)
                    {
                        for (int i = 0; i < mercsHit.Count; i++)
                        {
                            if (mercsHit[i].Conditions.ContainsKey(condition))
                            {
                                output = true;
                                break;
                            }
                        }
                    }
                    break;
                case Action.TargetGroup.Allies:
                    if (actor.stats.characterType == CharacterStats.CharacterTypes.Adventurer)
                    {
                        for (int i = 0; i < mercsHit.Count; i++)
                        {
                            if (mercsHit[i].Conditions.ContainsKey(condition))
                            {
                                output = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < monstersHit.Count; i++)
                        {
                            if (monstersHit[i].Conditions.ContainsKey(condition))
                            {
                                output = true;
                                break;
                            }
                        }
                    }
                    break;
                case Action.TargetGroup.Enemies:
                    if (actor.stats.characterType != CharacterStats.CharacterTypes.Adventurer)
                    {
                        for (int i = 0; i < mercsHit.Count; i++)
                        {
                            if (mercsHit[i].Conditions.ContainsKey(condition))
                            {
                                output = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < monstersHit.Count; i++)
                        {
                            if (monstersHit[i].Conditions.ContainsKey(condition))
                            {
                                output = true;
                                break;
                            }
                            else
                            {
                                //print($"{monstersHit[i].stats.characterName} doesn't have {condition}");
                            }
                        }
                    }
                    break;
            }
            return output;
        }
    }
}

public class TargetInfo
{
    public Character targetCharacter;
    public Node targetNode;
    public List<Node> affectedNodes = new List<Node>();
    public List<Character> affectedCharacters = new List<Character>();

    public TargetInfo(Character target, List<Node> nodesHit)
    {
        targetCharacter = target;
        targetNode = target.movement.currentNode;
        affectedNodes = nodesHit;
        for (int i = 0; i < affectedNodes.Count; i++)
        {
            if (affectedNodes[i].occupant != null)
                affectedCharacters.Add(affectedNodes[i].occupant);
        }
    }
    public TargetInfo(Node target, List<Node> nodesHit)
    {
        targetNode = target;
        targetCharacter = target.occupant;
        affectedNodes = nodesHit;
        for (int i = 0; i < affectedNodes.Count; i++)
        {
            if (affectedNodes[i].occupant != null)
                affectedCharacters.Add(affectedNodes[i].occupant);
        }
    }
    public TargetInfo(Character target, List<Character> targetsHit)
    {
        targetNode = target.movement.currentNode;
        targetCharacter = target;
        for (int i = 0; i < targetsHit.Count; i++)
        {
            affectedNodes.Add(targetsHit[i].movement.currentNode);
        }
        affectedCharacters = targetsHit;
    }
}

public class Node
{
    public GameObject tile;
    private Color originalColor;
    private Image img;
    public int x;
    public int y;
    public Vector2 coordinate;
    public Node[] neighbors = new Node[8];
    public Character occupant;

    public Node(GameObject _tile, int _x, int _y)
    {
        tile = _tile;
        x = _x;
        y = _y;
        coordinate = new Vector2(x, y);
        img = tile.GetComponent<Image>();
        originalColor = new Color(img.color.r, img.color.g, img.color.b, 1);
    }

    public void FindNeighbors()
    {
        int _x, _y;

        _x = x - 1; _y = y; neighbors[0] = _x >= 0 ? CombatGrid.grid[_x, _y] : null;
        _x = x + 1; _y = y; neighbors[1] = _x < 5 ? CombatGrid.grid[_x, _y] : null;
        _x = x; _y = y - 1; neighbors[2] = _y >= 0 ? CombatGrid.grid[_x, _y] : null;
        _x = x; _y = y + 1; neighbors[3] = _y < 5 ? CombatGrid.grid[_x, _y] : null;
        _x = x - 1; _y = y - 1; neighbors[4] = _x < 0 || _y < 0 ? null : CombatGrid.grid[_x, _y];
        _x = x - 1; _y = y + 1; neighbors[5] = _x < 0 || _y >= 5 ? null : CombatGrid.grid[_x, _y];
        _x = x + 1; _y = y - 1; neighbors[6] = _x >= 5 || _y < 0 ? null : CombatGrid.grid[_x, _y];
        _x = x + 1; _y = y + 1; neighbors[7] = _x >= 5 || _y >= 5 ? null : CombatGrid.grid[_x, _y];
    }

    public void NodeDeselected()
    {
        img.color = originalColor;
    }

    public void NodeSelected()
    {
        img.color = CombatGrid.instance.characterHighlight;
    }

    public void SetNodeColor(Color c)
    {
        img.color = c;
    }
    
    public void NodeNeighborHighlight()
    {
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (neighbors[i] != null)
            {
                neighbors[i].ActionHighlight();
            }
        }
    }

    public void MovementHightlight()
    {
        img.color = CombatGrid.instance.movementHighlight;
    }

    public void ActionHighlight()
    {
        img.color = CombatGrid.instance.actionHighlight;
    }

    public void ErrorHighlight()
    {
        img.color = CombatGrid.instance.errorHighlight;
    }
}

public class ShapeTest
{
    public Action action;
    public Character origin;
    public int hitMinimum;
    public bool hitEnemyOnly;
    public Action.TargetGroup targetGroup;
    public Action.Targeting targeting;
    public Action.TargetPriority targetPriority;
    public Node bestTargetNode;
    public Character bestTargetCharacter;
    public List<TargetInfo> potentialTargets = new List<TargetInfo>();
    public List<CombatGrid.TargetingTest> tests = new List<CombatGrid.TargetingTest>();
    public bool valid;

    public ShapeTest(Action _action, Character _origin, int _hitMinimum, bool _hitEnemyOnly)
    {
        action = _action;
        origin = _origin;
        hitMinimum = _hitMinimum;
        hitEnemyOnly = _hitEnemyOnly;
    }

    public ShapeTest(Action _action, Character _origin, int _hitMinimum, Action.TargetGroup _targetGroup, Action.Targeting _targeting, Action.TargetPriority _priority)
    {
        action = _action;
        origin = _origin;
        hitMinimum = _hitMinimum;
        targetGroup = _targetGroup;
        targeting = _targeting;
        targetPriority = _priority;
    }

    public ShapeTest(bool _valid, Node _targetNode)
    {
        valid = _valid;
        bestTargetNode = _targetNode;
    }
    public ShapeTest(bool _valid, Character _targetCharacter)
    {
        valid = _valid;
        bestTargetCharacter = _targetCharacter;
    }

    public ShapeTest(Action _action, Character _origin)
    {
        valid = true;
        action = _action;
        origin = _origin;
    }
    public void Validation()
    {
        bool flag = false;
        for (int i = 0; i < tests.Count; i++)
        {
            switch (targetGroup)
            {
                case Action.TargetGroup.All:
                    if (tests[i].totalHits >= hitMinimum)
                    {
                        flag = true;
                        goto Done;
                    }
                    break;
                case Action.TargetGroup.Allies:
                    if (action.Actor.stats.characterType == CharacterStats.CharacterTypes.Adventurer)
                    {
                        if (tests[i].mercsHit.Count >= hitMinimum)
                        {
                            flag = true;
                            goto Done;
                        }
                    }
                    else
                    {
                        if (tests[i].monstersHit.Count >= hitMinimum)
                        {
                            flag = true;
                            goto Done;
                        }
                    }
                    break;
                case Action.TargetGroup.Enemies:
                    if (action.Actor.stats.characterType != CharacterStats.CharacterTypes.Adventurer)
                    {
                        if (tests[i].mercsHit.Count >= hitMinimum)
                        {
                            flag = true;
                            goto Done;
                        }
                    }
                    else
                    {
                        if (tests[i].monstersHit.Count >= hitMinimum)
                        {
                            flag = true;
                            goto Done;
                        }
                    }
                    break;
            }
        }
        Done:
        valid = flag;
    }
}
