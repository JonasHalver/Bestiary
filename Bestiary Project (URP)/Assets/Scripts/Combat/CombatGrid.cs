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

    private void Awake()
    {
        instance = this;
        gRaycaster = canvas.GetComponent<GraphicRaycaster>();
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
                        SelectNode(n);
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
        if (ca != null && ca.affectedNodes.Count > 0)
        {
            ca.highlighted = true;
            foreach (Node node in ca.affectedNodes)
            {
                node.ActionHighlight();
            }
        }       
    }

    public static Node CharacterSpawn(bool isMonster)
    {
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
            case Action.Shape.ThreeByThree:
                output = GenerateThreeByThree(ca.targetNode);
                break;
            case Action.Shape.Line:
                output = GenerateLine(ca.targetNode.coordinate, ca.origin.position);
                break;
        }

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

    public static ShapeTest ArcTest(ShapeTest test)
    {
        bool valid = false;
        Node bestTarget = null;

        Node originNode = test.origin.movement.currentNode;
        Vector2 originV2 = originNode.coordinate;

        int hitCount = 0, hitCountMax = 0;
        bool validTarget = false;
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
                        validTarget = (test.hitEnemyOnly && !Character.AllyOrEnemy(test.origin, n.occupant)) || (!test.hitEnemyOnly && Character.AllyOrEnemy(test.origin, n.occupant));
                        if (validTarget && n.occupant.alive)
                            hitCount++;
                    }
                    
                }
                if (hitCount > hitCountMax && hitCount >= test.hitMinimum)
                {
                    bestTarget = node;
                    valid = true;
                    hitCountMax = hitCount;
                }
            }
        }
        return new ShapeTest(valid, bestTarget);
    }

    public static ShapeTest ConeTest(ShapeTest test)
    {
        bool valid = false;
        bool validTarget = false;
        Node bestTarget = null;

        Node originNode = test.origin.movement.currentNode;
        Vector2 originV2 = originNode.coordinate;

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
                        validTarget = (test.hitEnemyOnly && !Character.AllyOrEnemy(test.origin, n.occupant)) || (!test.hitEnemyOnly && Character.AllyOrEnemy(test.origin, n.occupant));
                        if (validTarget && n.occupant.alive) hitCount++;
                    }
                    
                }
                if (hitCount > hitCountMax && hitCount >= test.hitMinimum)
                {
                    bestTarget = node;
                    valid = true;
                    hitCountMax = hitCount;
                }
            }
        }
        return new ShapeTest(valid, bestTarget);
    }

    public static ShapeTest LineTest(ShapeTest test)
    {
        bool valid = false;
        bool validTarget = false;
        Node bestTarget = null;

        Node originNode = test.origin.movement.currentNode;
        Vector2 originV2 = originNode.coordinate;

        int hitCount = 0, hitCountMax = 0;

        foreach (Node node in originNode.neighbors)
        {
            if (node != null)
            {
                hitCount = 0;
                List<Node> targetTest = GenerateLine(node.coordinate, originV2);
                foreach(Node n in targetTest)
                {
                    if (n.occupant != null)
                    {
                        validTarget = (test.hitEnemyOnly && !Character.AllyOrEnemy(test.origin, n.occupant)) || (!test.hitEnemyOnly && Character.AllyOrEnemy(test.origin, n.occupant));
                        if (validTarget && n.occupant.alive) hitCount++;
                    }
                }
                if (hitCount > hitCountMax && hitCount >= test.hitMinimum)
                {
                    bestTarget = node;
                    valid = true;
                    hitCountMax = hitCount;
                }
            }
        }
        return new ShapeTest(valid, bestTarget);
    }

    public static ShapeTest PointBlankAoETest(ShapeTest test)
    {
        bool valid = false, validTarget = false;
        Node bestTarget = null;

        Node originNode = test.origin.movement.currentNode;

        int hitCount = 0;
        foreach (Node node in originNode.neighbors)
        {
            if (node != null)
            {
                if (node.occupant != null)
                {
                    validTarget = (test.hitEnemyOnly && !Character.AllyOrEnemy(test.origin, node.occupant))||(!test.hitEnemyOnly && Character.AllyOrEnemy(test.origin, node.occupant));
                    if (validTarget && node.occupant.alive) hitCount++;
                }
            }
        }
        if (hitCount >= test.hitMinimum)
        {
            bestTarget = originNode;
            valid = true;
        }

        return new ShapeTest(valid, bestTarget);
    }

    public static ShapeTest ThreeByThreeTest(ShapeTest test)
    {
        bool valid = false;
        bool validTarget = false;
        Node bestTarget = null;

        int hitCount = 0, hitCountMax = 0;

        foreach (Node node in grid)
        {
            hitCount = 0;
            List<Node> targetTest = GenerateThreeByThree(node);
            foreach (Node target in targetTest)
            {
                if (target.occupant != null)
                {
                    validTarget = test.hitEnemyOnly && !Character.AllyOrEnemy(test.origin, target.occupant);
                    if (validTarget && target.occupant.alive) hitCount++;
                }                
            }
            if (hitCount > hitCountMax && hitCount >= test.hitMinimum)
            {
                bestTarget = node;
                valid = true;
                hitCountMax = hitCount;
            }
        }
        return new ShapeTest(valid, bestTarget);
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
        originalColor = img.color;
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

    public Node targetNode;
    public bool valid;

    public ShapeTest(Action _action, Character _origin, int _hitMinimum, bool _hitEnemyOnly)
    {
        action = _action;
        origin = _origin;
        hitMinimum = _hitMinimum;
        hitEnemyOnly = _hitEnemyOnly;
    }

    public ShapeTest(bool _valid, Node _targetNode)
    {
        valid = _valid;
        targetNode = _targetNode;
    }
}
