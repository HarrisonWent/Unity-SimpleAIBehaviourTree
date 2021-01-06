using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

//Action move to the intel location
public class Action_SeekIntel : Node
{
    private NavMeshAgent MyAgent;
    private NPC_UI MyUI;
    private Vector3 TargetLocation;

    public Action_SeekIntel(NavMeshAgent NewAgent, NPC_UI NewUI,Vector3 IntelLocation)
    {
        MyAgent = NewAgent;
        MyUI = NewUI;
        TargetLocation = IntelLocation;
    }

    public override TaskStatus ActionUpdate()
    {
        //Debug.Log("Action: Seek the intel!");

        MyAgent.SetDestination(TargetLocation);

        Debug.Log(Vector3.Distance(MyAgent.nextPosition, TargetLocation));
        if (Vector3.Distance(MyAgent.nextPosition, TargetLocation) < 1.5f)
        {
            SceneManager.LoadScene(0);
        }

        if (MyAgent.remainingDistance < 1.0f)
        {            
            return TaskStatus.Success;
        } 

        MyUI.SetToVisualState(NPC_UI.States.Patrol);
        return TaskStatus.Ongoing;
    }

    //Override the node type to action
    public override NodeType GetNodeType()
    {
        return NodeType.Action;
    }
}

//Action have i been caught
public class Action_CheckIfCaught : Node
{
    private AISight MySight;
    public Action_CheckIfCaught(AISight m_Sight)
    {
        MySight = m_Sight;
    }

    public override TaskStatus ActionUpdate()
    {
        //If there is a guard within 1.5 meters of me i am pretty caught
        if (MySight.GetMeATarget(1.5f, true, true, false,false))
        {
            //todo game over
            SceneManager.LoadScene(0);
            Debug.LogWarning("SPY CAUGHT");
            return TaskStatus.Failure;
        }

        return TaskStatus.Success;
    }

    //Override the node type to action
    public override NodeType GetNodeType()
    {
        return NodeType.Action;
    }
}

//Action Am i being chased
public class Action_CheckIfBeingChased : Node
{
    private AISight MySight;
    public Action_CheckIfBeingChased(AISight m_Sight)
    {
        MySight = m_Sight;
    }

    public override TaskStatus ActionUpdate()
    {
        //If there is a guard within 6 meters of me i should run
        if (MySight.GetMeATarget(10, true, true, false, false))
        {
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
    }

    //Override the node type to action
    public override NodeType GetNodeType()
    {
        return NodeType.Action;
    }
}

//Action Run to hiding spot
public class Action_RunToHide : Node
{
    private NavMeshAgent MyAgent;
    private NPC_UI MyUI;
    private Transform[] HidingSpots;
    private AISight MySight;

    public Action_RunToHide(NavMeshAgent NewAgent, NPC_UI NewUI, Transform[] NewHidingSpots,AISight m_Sight)
    {
        MyAgent = NewAgent;
        MyUI = NewUI;
        HidingSpots = NewHidingSpots;
        MySight = m_Sight;
    }

    public override TaskStatus ActionUpdate()
    {
        //Get the closeset hiding spot
        Vector3 Closest = Vector3.zero;
        float Distance = 999;

        foreach(Transform v3 in HidingSpots)
        {
            float DistanceTry = Vector3.Distance(v3.position, MyAgent.nextPosition);
            if (DistanceTry < Distance)
            {
                Distance = DistanceTry;
                Closest = v3.position;
            }
        }

        MyAgent.SetDestination(Closest);

        //Fail as no longer needed, return to objective
        if (MyAgent.remainingDistance < 1f)
        {
            MySight.MyVisualState = AISight.VisualState.Hidden;
            return TaskStatus.Failure;
        }
        else
        {
            MySight.MyVisualState = AISight.VisualState.Exposed;
        }

        //still going there
        MyUI.SetToVisualState(NPC_UI.States.Chase);
        return TaskStatus.Ongoing;
    }

    //Override the node type to action
    public override NodeType GetNodeType()
    {
        return NodeType.Action;
    }
}