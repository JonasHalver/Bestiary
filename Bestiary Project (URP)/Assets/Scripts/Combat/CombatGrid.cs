using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
                grid[x, y] = new Node(transform.GetChild(index).gameObject, x, y);
                index++;
            }
        }
        foreach(Node n in grid)
        {
            n.FindNeighbors();
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
            for (int i = 0; i < ca.primaryTarget.AffectedNodes.Count; i++)
            {
                ca.primaryTarget.AffectedNodes[i].ActionHighlight();
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
            if (isMonster) return grid[2, 3];
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
        if (ca.action.isPass)
        {
            output.Add(ca.origin.movement.currentNode);
            return output;
        }

        /* Outdated
        switch (ca.action.shape)
        {
            case Action.Shape.Arc:
                output = GenerateArc(ca.targetNode.coordinate, ca.origin.position);
                break;
            case Action.Shape.Cone:
                output = GenerateCone(ca.targetNode.coordinate, ca.origin.position);
                break;
            case Action.Shape.Single:
                output.Add(ca.targetNode);
                if (ca.action.alwaysHitsSelf) output.Add(ca.origin.movement.currentNode);
                break;
            case Action.Shape.Area:
                output = GenerateThreeByThree(ca.targetNode);
                break;
            case Action.Shape.Line:
                output = GenerateLine(ca.targetNode.coordinate, ca.origin.position);
                break;
            case Action.Shape.All:
                foreach (Character actor in CombatManager.actors)
                {
                    if (actor.alive)
                    {
                        switch (ca.action.targetGroup)
                        {
                            case Action.TargetGroup.Allies:
                                if (actor != ca.origin && Character.AllyOrEnemy(ca.origin, actor))
                                {
                                    output.Add(actor.movement.currentNode);
                                }
                                else if (actor == ca.origin && ca.action.canHitSelf)
                                {
                                    output.Add(actor.movement.currentNode);
                                }
                                break;
                            case Action.TargetGroup.Enemies:
                                if (!Character.AllyOrEnemy(ca.origin, actor))
                                {
                                    output.Add(actor.movement.currentNode);
                                }
                                break;
                            case Action.TargetGroup.All:
                                output.Add(actor.movement.currentNode);
                                break;
                        }
                    }
                }
                break;
        }
        */
        output = ca.affectedNodes;
        return output;
    }

    public void SelectNode(Node node)
    {
        //foreach (Node n in grid)
        //{
        //    n.NodeDeselected();
        //}
        if (selectedNode != null) selectedNode.NodeDeselected();
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

    /* Outdated
    public List<Node> Arc(CombatAction ca)
    {
        List<Node> output = new List<Node>();
        List<Node> baseline = GenerateArc(ca.target.position, ca.origin.position);
        int hitcount = 0, hitmax = 0;
        if (ca.action.targetGroup == Action.TargetGroup.Enemies && ca.action.targetPriority == Action.TargetPriority.Closest)
        {
            if (ca.bpi.enemiesInMelee.Count > 1)
            {
                foreach(Node node in ca.target.movement.currentNode.neighbors)
                {
                    if (node != null)
                    {
                        hitcount = 0;
                        List<Node> targetTest = GenerateArc(node.coordinate, ca.origin.position);
                        foreach (Node n in targetTest)
                        {
                            if (n.occupant != null && !Character.AllyOrEnemy(ca.origin, n.occupant))
                            {
                                hitcount++;
                            }
                            if (hitcount > hitmax)
                            {
                                output = targetTest;
                                hitmax = hitcount;
                            }
                        }
                    }
                }
            }
        }
        if (hitmax > 1)
        {
            return output;
        }
        else
        {
            return baseline;
        }
    }
    */

    public static ShapeTest ArcTest(ShapeTest test)
    {
        bool valid = false;
        Node bestTarget = null;

        Node originNode = test.origin.movement.currentNode;
        Vector2 originV2 = originNode.coordinate;

        int hitCount = 0;
        bool validTarget = false;
        ShapeTest output = new ShapeTest(test.action, test.origin);
        foreach (Node node in originNode.neighbors)
        {
            if (node != null)
            {
                hitCount = 0;
                List<Node> targetTest = GenerateArc(node.coordinate, originV2);
                foreach(Node n in targetTest)
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
    public Node bestTargetNode;
    public Character bestTargetCharacter;
    public List<TargetInfo> potentialTargets = new List<TargetInfo>();
    public bool valid;

    public ShapeTest(Action _action, Character _origin, int _hitMinimum, bool _hitEnemyOnly)
    {
        action = _action;
        origin = _origin;
        hitMinimum = _hitMinimum;
        hitEnemyOnly = _hitEnemyOnly;
    }

    public ShapeTest(Action _action, Character _origin, int _hitMinimum, Action.TargetGroup _targetGroup, Action.Targeting _targeting)
    {
        action = _action;
        origin = _origin;
        hitMinimum = _hitMinimum;
        targetGroup = _targetGroup;
        targeting = _targeting;
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
}
