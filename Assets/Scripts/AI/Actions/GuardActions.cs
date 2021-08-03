using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Check if can see enemy
/// </summary>
public class Action_SuspicousEvent : Node
{
    int EyeSightRange = 10;
    private AISight MySight;

    public Action_SuspicousEvent(AISight NewSight)
    {
        MySight = NewSight;
    }

    // Updates the state of the task
    public override TaskStatus ActionUpdate()
    {
        //Debug.Log("Action: Check to see enemy!");
        GameObject Sighted = MySight.GetMeATarget(EyeSightRange, true, true, false,false);
        if (Sighted)
        {     
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Failure;
        }

        //todo add more suspicous events
    }

    //Override the node type to action
    public override NodeType GetNodeType()
    {
        return NodeType.Action;
    }
}

/// <summary>
/// Investigate enemies last known position
/// </summary>
public class Action_Investigate : Node
{
    float TimeToSpot = 1.5f;
    private AISight MySight;
    private NavMeshAgent MyAgent;
    private float Timer = 0.0f;
    public bool HeadingToSuspicious = false;
    private NPC_UI MyUI;

    public Action_Investigate(AISight NewSight, NavMeshAgent NewAgent, NPC_UI NewUI)
    {
        MySight = NewSight;
        MyAgent = NewAgent;
        MyUI = NewUI;
    }

    public override TaskStatus ActionUpdate()
    {
        //Debug.Log("Action: Investigate the last known position!");
               
        if (!HeadingToSuspicious)
        {
            Timer = 0.0f;
            Vector3 LastKnownPosition = MySight.GetLastKnownPosition();
            MyAgent.SetDestination(LastKnownPosition);
            HeadingToSuspicious = true;
        }

        //if we see em for 3 straight seconds succeed
        if (MySight.GetMeATarget(10, true, true, false,true))
        {
            Timer += Time.fixedDeltaTime;
            //Debug.Log(Timer);
            if (Timer >= TimeToSpot)
            {
                //Debug.Log("Start chase");                
                return TaskStatus.Success;
            }
        }
        else
        {
            Timer = 0.0f;
        }

        //if reached where we saw them and no longer see em mission failed
        if (MyAgent.remainingDistance <= 1)
        {
            Timer = 0.0f;
            MyAgent.ResetPath();
            HeadingToSuspicious = false;
            return TaskStatus.Failure;
        }
        //Continue moving to where we saw em
        else
        {
            MyUI.SetToVisualState(NPC_UI.States.Investigate);
            return TaskStatus.Ongoing;
        }
    }

    //Override the node type to action
    public override NodeType GetNodeType()
    {
        return NodeType.Action;
    }
}

/// <summary>
/// Sleep for a duration
/// </summary>
public class Action_SleepForDuration : Node
{
    public float SleepRemaining = 10;
    private NPC_UI MyUI;
    private float Timer = 0.0f, TimeToRecover = 3;
    private bool Sleeping = false;
    public Action_SleepForDuration(NPC_UI NewUI)
    {
        Random.InitState(Random.Range(0, 5000));
        SleepRemaining = Random.Range(4,12);
        MyUI = NewUI;
    }

    public override TaskStatus ActionUpdate()
    {
        //Debug.Log("Action: Sleep!");
        if (!Sleeping)
        {
            Timer = 0.0f;
            Sleeping = true;
        }
        Timer += Time.fixedDeltaTime;
        if(Timer >= TimeToRecover)
        {
            SleepRemaining = Random.Range(5, 12);
            Sleeping = false;
        }

        MyUI.SetToVisualState(NPC_UI.States.Sleep);
        return TaskStatus.Ongoing;
    }

    //Override the node type to action
    public override NodeType GetNodeType()
    {
        return NodeType.Action;
    }
}

/// <summary>
/// Chases the sighted enemy
/// </summary>
public class Action_Chase : Node
{
    int MaxDistance = 10;
    private AISight MySight;
    private NavMeshAgent MyAgent;
    private NPC_UI MyUI;

    public Action_Chase(AISight NewSight, NavMeshAgent NewAgent, NPC_UI NewUI)
    {
        MySight = NewSight;
        MyAgent = NewAgent;
        MyUI = NewUI;
    }

    public override TaskStatus ActionUpdate()
    {
        //Debug.Log("Action: Chase em!");

        Vector3 LastKnownPosition = MySight.GetLastKnownPosition();

        if (!MyAgent.hasPath)
        {
            MyAgent.SetDestination(LastKnownPosition);
        }

        //if we see em chase em, also forgets angle as we can assume where they are going/hear them, still uses rays for walls though
        if (MySight.GetMeATarget(MaxDistance, true, true, false,false))
        {
            MyUI.SetToVisualState(NPC_UI.States.Chase);
            MyAgent.SetDestination(LastKnownPosition);
            return TaskStatus.Ongoing;
        }
        else
        {
            MyAgent.ResetPath();
            return TaskStatus.Failure;
        }
    }

    //Override the node type to action
    public override NodeType GetNodeType()
    {
        return NodeType.Action;
    }
}

/// <summary>
/// Patrol random areas
/// </summary>
public class Action_Patrol : Node
{
    private Action_SleepForDuration MySleep;
    private NavMeshAgent MyAgent;
    int MapSize = 25;
    private NPC_UI MyUI;

    public Action_Patrol(Action_SleepForDuration NewSleepSchedule, NavMeshAgent NewAgent, NPC_UI NewUI)
    {
        MySleep = NewSleepSchedule;
        MyAgent = NewAgent;
        MyUI = NewUI;
    }

    public override TaskStatus ActionUpdate()
    {
        //Debug.Log("Action: Patrol!");
        if (MySleep.SleepRemaining <= 0) { MyAgent.ResetPath(); return TaskStatus.Failure; }

        if (!MyAgent.hasPath || MyAgent.remainingDistance < 1.0f)
        {
            Random.InitState(Random.Range(0,5000));
            Vector3 tryPosition = new Vector3(Random.Range(-MapSize, MapSize), 0, Random.Range(-MapSize, MapSize));
            NavMesh.SamplePosition(tryPosition, out NavMeshHit hit, 100, NavMesh.AllAreas);
            MyAgent.SetDestination(hit.position);
        }

        MySleep.SleepRemaining -= Time.fixedDeltaTime;

        MyUI.SetToVisualState(NPC_UI.States.Patrol);
        return TaskStatus.Ongoing;
    }

    //Override the node type to action
    public override NodeType GetNodeType()
    {
        return NodeType.Action;
    }
}