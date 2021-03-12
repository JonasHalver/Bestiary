using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class AdventurerMovement : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    public bool selected = false;
    private Vector3 pickupOffset;
    private Image img;
    private Character character;

    private List<RaycastResult> hits = new List<RaycastResult>();

    public Node currentNode;
    public Node targetNode, previousNode;
    private Node checkNode;

    public int movementLeft;
    public int movementCost;

    public List<Node> destinationNodes = new List<Node>();
    private bool illegalMove = false;

    public static event System.Action newPosition;

    private float t;
    private bool moving = false;
    public bool canMove = false;
    public static bool unitSelected = false;

    private void Awake()
    {

    }

    private void OnEnable()
    {
        //CombatManager.EndRound += StartOfRound;
        CombatManager.startCombat += StartOfRound;
        CombatManager.StartOfTurn += CharacterCheck;
    }

    private void OnDisable()
    {
        //CombatManager.EndRound -= StartOfRound;
        CombatManager.startCombat -= StartOfRound;
        CombatManager.StartOfTurn -= CharacterCheck;
    }

    private void Start()
    {
        img = GetComponent<Image>();
        currentNode = (currentNode == null) ? CurrentNode() : currentNode;
        character = GetComponent<Character>();
        //currentNode.occupant = character;
        moving = false;
    }   

    private void Update()
    {
        if (currentNode == null)
        {
            currentNode = CurrentNode();
            
        }
        else
        {
            if (currentNode.occupant == null) currentNode.occupant = character;
        }
        if (selected)
        {
            Selected();
        }

        canMove = !moving &&
            character.alive &&
            CombatManager.instance.currentStage == CombatManager.CombatStage.Setup &&
            !GameManager.paused &&
            unitSelected == false &&
            (GameManager.instance.debugMode ? true : character.stats.characterType != CharacterStats.CharacterTypes.NPC);
    }

    IEnumerator MoveToNewLocation(bool ai)
    {
        moving = true;
        if (targetNode != null)
        {
            if (ai)
            {
                if (destinationNodes.Count > 0)
                {
                    foreach (Node n in destinationNodes)
                    {
                        n.MovementHightlight();
                    }
                }
                else currentNode.ErrorHighlight();
                yield return new WaitForSeconds(0.5f);
                targetNode.NodeSelected();
            }
            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 2;
                transform.position = Vector3.Lerp(transform.position, targetNode.tile.transform.position, t);
                yield return null;
            }
            if (previousNode != currentNode) previousNode = currentNode;
            currentNode = targetNode;
            targetNode = null;
        }
        if (previousNode != null && previousNode.occupant == character) previousNode.occupant = null;
        if (currentNode.occupant != character) currentNode.occupant = character;

        CombatGrid.StopHighlight();
        ReportNewNode(currentNode);

        moving = false;
    }

    public void CharacterCheck(Character actor)
    {
        if (actor == character)
        {
            StartOfRound();
        }
    }

    public void StartOfRound()
    {
        if (character.conditions.Contains(Debuff.ControlType.Slow))
        {
            movementLeft = Mathf.Max(1, character.stats.movement - 2);
        }
        else if (character.conditions.Contains(Debuff.ControlType.Root))
        {
            movementLeft = 0;
        }
        else
        {
            movementLeft = character.stats.movement;
        }
        destinationNodes.Clear();
    }

    public void Selected()
    {
        if (destinationNodes.Count == 0) destinationNodes = GenerateDestinationList(movementLeft);
        currentNode.NodeSelected();
        transform.position = Input.mousePosition + pickupOffset;
        Node c = CurrentNode();
        if (c != null)
        {
            if (c != targetNode)
            {
                targetNode = c;
                movementCost = CombatGrid.Vector2ToDistance(currentNode.coordinate, targetNode.coordinate);
                ReportNewNode(c);
            }
        }
        if (!destinationNodes.Contains(c) || c.occupant != null && c.occupant != character)
        {
            illegalMove = true;
            foreach(Node n in destinationNodes)
            {
                n.ErrorHighlight();
            }
        }
        else if (destinationNodes.Count == 0)
        {
            illegalMove = true;
        }
        else
        {
            illegalMove = false;
            CombatGrid.MovementHighlight(destinationNodes);
        }

        CombatGrid.HighlightNodeStatic(targetNode);
    }
    
    public Node CurrentNode()
    {
        Node output = null;
        PointerEventData ped = new PointerEventData(EventSystem.current);
        ped.position = transform.position;
        CombatGrid.gRaycaster.Raycast(ped, hits);
        foreach (RaycastResult hit in hits)
        {
            if (hit.gameObject.CompareTag("Tile"))
            {
                output = CombatGrid.IdentifyNodeFromTile(hit.gameObject);
            }
        }

        return output;
    }

    public List<Node> GenerateDestinationList(int moves)
    {
        List<Node> output = new List<Node>();

        foreach(Node node in CombatGrid.grid)
        {
            if (CombatGrid.Vector2ToDistance(currentNode.coordinate, node.coordinate) <= moves) output.Add(node);
        }

        return output;
    } 

    public void AIMovement()
    {
        List<Node> destinations = new List<Node>();
        if (!character.conditions.Contains(Debuff.ControlType.Root))
            destinations = GenerateDestinationList(movementLeft);
        if (destinations.Count == 0) targetNode = currentNode;
        
        else 
        {
            for (int i = 0; i < destinations.Count; i++)
            {
                if (destinations[i].occupant != null)
                {
                    destinations.RemoveAt(i);
                    i--;
                }
            }
        }
        Node destination = destinations.Count > 0 ? destinations[UnityEngine.Random.Range(0, destinations.Count)] : currentNode;
        destinationNodes = destinations;
        targetNode = destination;

        StartCoroutine(MoveToNewLocation(true));
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (canMove)
        {
            if (selected)
            {
                if (eventData.pointerId == -1)
                    OnDeselect(false);
                else
                    OnDeselect(true);
            }
            else
            {
                if (eventData.pointerId == -1) OnSelect();
            }
        }
        else if (selected)
            if (eventData.pointerId == -1)
                OnDeselect(false);
            else
                OnDeselect(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canMove) selected = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerId == -1)
        {
            OnDeselect(false);
        }
        else
        {
            OnDeselect(true);
        }
    }    

    private void OnSelect()
    {
        pickupOffset = transform.position - Input.mousePosition;
        selected = true;
        unitSelected = true;
    }

    private void OnDeselect(bool canceled)
    {
        selected = false;
        unitSelected = false;
        if (!canceled && targetNode != null && !illegalMove)
        {
            StartCoroutine(MoveToNewLocation(false));
            previousNode = currentNode;
        }
        else if (!canceled && illegalMove)
        {
            targetNode = currentNode;
            StartCoroutine(MoveToNewLocation(false));
        }
        else  if (canceled)
        {
            targetNode = currentNode;
            StartCoroutine(MoveToNewLocation(false));
        }
    }

    public void ReportNewNode(Node n)
    {
        if (checkNode != n)
        {
            newPosition.Invoke();
            checkNode = currentNode;
        }
    }
}

