using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
/// <summary>
/// Used by Sequences, Selectors and actions
/// </summary>
public class Node
{
    public enum TaskStatus
    {
        Success,
        Failure,
        Ongoing
    }
    public Node()
    {
    }

    public Node(NodeType NewNodeType,Node[] SubNodes)
    {
        BaseType = NewNodeType;
        MySubNodes = SubNodes;
    }

    //Used to execute the corresponding functions
    public enum NodeType
    {
        Action,
        Selector,
        Sequence,
        NotSet
    }NodeType BaseType = NodeType.NotSet;

    private Node[] MySubNodes;

    /// <summary>
    /// Update the required SubNodes for the type of node this is
    /// </summary>
    /// <returns></returns>
    public TaskStatus OnUpdate()
    {
        TaskStatus CurrentStatus = TaskStatus.Failure;
        switch (GetNodeType())
        {
            case NodeType.Action:
                CurrentStatus = ActionUpdate();
                break;
            case NodeType.Selector:
                CurrentStatus = SelectorUpdate();
                break;
            case NodeType.Sequence:
                CurrentStatus = SequenceUpdate();
                break;
            case NodeType.NotSet:
                Debug.LogWarning("The node type has not been set!");
                break;
        }
        return CurrentStatus;        
    }

    /// <summary>
    /// Carrys out an action, this is the one custom actions are put into
    /// </summary>
    /// <returns></returns>
    public virtual TaskStatus ActionUpdate()
    {
        Debug.LogWarning("Action hasn't been overriden");
        return TaskStatus.Failure;
    }

    /// <summary>
    /// Executes all sub actions sequentially, stops if an action fails and returns failure
    /// </summary>
    /// <returns></returns>
    public TaskStatus SequenceUpdate()
    {
        //Debug.Log("***Sequence Check***");
        foreach (Node n in MySubNodes)
        {            
            TaskStatus SubStatus = n.OnUpdate();
            if(SubStatus != TaskStatus.Success) 
            { 
                //Debug.Log("Sequence ended with fail/ongoing"); 
                return SubStatus; 
            }
        }
        //Debug.Log("Sequence ended with Success");
        return TaskStatus.Success;
    }

    public virtual NodeType GetNodeType() { return BaseType; }
    
    /// <summary>
    /// Executes all sub actions sequentially, stops after the first success
    /// </summary>
    /// <returns></returns>
    public TaskStatus SelectorUpdate()
    {
        //Debug.Log("***Selector***");
        foreach (Node N in MySubNodes)
        {
            TaskStatus SubStatus = N.OnUpdate();
            if (SubStatus == TaskStatus.Success) 
            { 
                //Debug.Log("Selector ended with success"); 
                return SubStatus; 
            }

            if(SubStatus == TaskStatus.Ongoing) 
            { 
                //Debug.Log("Selector ended with ongoing"); 
                return SubStatus; 
            }
        }
        //Debug.Log("***Selector ended with fail***");
        return TaskStatus.Failure;
    }
}
