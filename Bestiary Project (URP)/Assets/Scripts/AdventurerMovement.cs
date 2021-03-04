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
    private bool canMove = false;

    private void Awake()
    {
    }

    private void Start()
    {
        img = GetComponent<Image>();
        currentNode = (currentNode == null) ? CurrentNode() : currentNode;
        character = GetComponent<Character>();
        //currentNode.occupant = character;
        CombatManager.EndRound += StartOfRound;
        CombatManager.instance.startCombat += StartOfRound;
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
            (GameManager.instance.debugMode ? true : character.stats.characterType != CharacterStats.CharacterTypes.NPC);
    }

    IEnumerator MoveToNewLocation()
    {
        moving = true;
        if (targetNode != null)
        {
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

    public void StartOfRound()
    {
        movementLeft = Mathf.Max(1, character.stats.movement - (character.conditions.Contains(Debuff.ControlType.Slow) ? 2 : 0));
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
        List<Node> destinations = GenerateDestinationList(movementLeft);
        for (int i = 0; i < destinations.Count; i++)
        {
            if(destinations[i].occupant != null)
            {
                destinations.RemoveAt(i);
                i--;
            }
        }
        Node destination = destinations[UnityEngine.Random.Range(0, destinations.Count)];
        targetNode = destination;
        StartCoroutine(MoveToNewLocation());
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
    }

    private void OnDeselect(bool canceled)
    {
        selected = false;
        if (!canceled && targetNode != null && !illegalMove)
        {
            StartCoroutine(MoveToNewLocation());
            previousNode = currentNode;
        }
        else if (!canceled && illegalMove)
        {
            targetNode = currentNode;
            StartCoroutine(MoveToNewLocation());
        }
        else  if (canceled)
        {
            targetNode = currentNode;
            StartCoroutine(MoveToNewLocation());
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

