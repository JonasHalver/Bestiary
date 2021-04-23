using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{/*
    private static LineManager instance;
    public static LineManager Instance
    {
        get
        {
            return instance;
        }
    }
    [SerializeField] private GameObject linePrefab;
    private Dictionary<ActionNode, NodeConnections> connectionLookup = new Dictionary<ActionNode, NodeConnections>();

    public bool MovingNode { get; set; }
    public ActionNode NodeMoving { get; set; }

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Update()
    {
        if (MovingNode)
        {
            FollowNode(NodeMoving);
        }
        else
        {
            NodeMoving = null;
        }
    }

    public void NewConnection(Vector2 origin, ActionNode node)
    {
        GameObject newLine = Instantiate(linePrefab, transform);
        UILineRenderer line = newLine.GetComponent<UILineRenderer>();
        line.points[0] = origin - new Vector2(Screen.width / 2, Screen.height / 2);

        NodeConnections newConnection;
        if (connectionLookup.ContainsKey(node))
        {
            newConnection = connectionLookup[node];
        }
        else
        {
            newConnection = new NodeConnections(node);
        }

        StartCoroutine(FollowMouse(line, newConnection));
    }

    public void CompleteConnection(UILineRenderer line, NodeConnections connection, ActionNode finish)
    {
        connection.lines.Add(finish, new NodeConnections.Info(false, line));
        connection.connections.Add(finish);
        if (!connectionLookup.ContainsKey(connection.origin))
        {
            connectionLookup.Add(connection.origin, connection);
        }
        
        if (connectionLookup.ContainsKey(finish))
        {
            connectionLookup[finish].connections.Add(connection.origin);
            connectionLookup[finish].lines.Add(connection.origin, new NodeConnections.Info(true, line));
        }
        else
        {
            NodeConnections newConnection = new NodeConnections(finish);
            newConnection.connections.Add(connection.origin);
            newConnection.lines.Add(connection.origin, new NodeConnections.Info(true, line));
            connectionLookup.Add(finish, newConnection);
        }
    }

    IEnumerator FollowMouse(UILineRenderer line, NodeConnections connection)
    {
        yield return null;
        while (true)
        {
            line.points[1] = Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2);
            line.SetVerticesDirty();
            if (Input.GetMouseButtonDown(0))
            {
                if (connection.origin.Connection != null)
                {
                    ActionNode finish = connection.origin.Connection.transform.parent.parent.GetComponent<ActionNode>();
                    if (connection.connections.Contains(finish))
                    {
                        Destroy(line.gameObject);
                        break;
                    }
                    CompleteConnection(line, connection, finish);
                    line.points[1] = connection.origin.Connection.position - new Vector3(Screen.width / 2, Screen.height / 2);
                    
                    line.SetVerticesDirty();
                }
                else
                {
                    Destroy(line.gameObject);
                }
                break;
            }
            yield return null;
        }
    }

    private void FollowNode(ActionNode node)
    {
        NodeConnections current = null;
        if(connectionLookup.ContainsKey(node)) current = connectionLookup[node];
        if (current != null)
        {
            foreach(KeyValuePair<ActionNode, NodeConnections.Info> kvp in connectionLookup[node].lines)
            {
                if (kvp.Value.onLeft)
                {
                    kvp.Value.line.points[1] = node.nodeConnectLeft.position - new Vector3(Screen.width / 2, Screen.height / 2);
                }
                else
                {
                    kvp.Value.line.points[0] = node.nodeConnectRight.position - new Vector3(Screen.width / 2, Screen.height / 2);
                }
                    kvp.Value.line.SetVerticesDirty();
            }
        }
    }
    public void SeverConnections(ActionNode node)
    {
        if (connectionLookup.ContainsKey(node))
        {
            List<GameObject> tempLines = new List<GameObject>();
            foreach(KeyValuePair<ActionNode, NodeConnections.Info> kvp in connectionLookup[node].lines)
            {
                if (connectionLookup.ContainsKey(kvp.Key))
                {
                    connectionLookup[kvp.Key].connections.Remove(node);
                    connectionLookup[kvp.Key].lines.Remove(node);
                }
                tempLines.Add(kvp.Value.line.gameObject);
            }
            connectionLookup[node].connections.Clear();
            for (int i = 0; i < tempLines.Count; i++)
            {
                Destroy(tempLines[i]);
                tempLines.RemoveAt(i);
                i--;
            }
            connectionLookup.Remove(node);
        }
    }*/
}
