using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NPC_UI))]
public class Guard : MonoBehaviour
{
    protected AISight m_Sight;
    protected NavMeshAgent m_Agent;
    private NPC_UI MyUI;

    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Sight = GetComponent<AISight>();
        Random.InitState((int)Time.time);

        MyUI = GetComponent<NPC_UI>();
        MyUI.SpawnMyPanel();

        int MapSize = 25;
        //Give random starting position
        Random.seed = System.DateTime.Now.Millisecond;
        Vector3 tryPosition = new Vector3(Random.Range(-MapSize, MapSize), 0, Random.Range(-MapSize, MapSize));
        NavMesh.SamplePosition(tryPosition, out NavMeshHit hit, 100, NavMesh.AllAreas);
        m_Agent.Warp(hit.position);

        CreateTree();
    }

    private Node TopOfTheTree;
    void CreateTree()
    {
        //Actions
        Action_SleepForDuration My_Action_SleepForDuration = new Action_SleepForDuration(MyUI);
        Action_SuspicousEvent My_Action_SuspicousEvent = new Action_SuspicousEvent(m_Sight);
        Action_Investigate My_Action_Investigate = new Action_Investigate(m_Sight, m_Agent, MyUI);
        Action_Chase My_Action_Chase = new Action_Chase(m_Sight, m_Agent, MyUI);
        Action_Patrol My_Action_Patrol = new Action_Patrol(My_Action_SleepForDuration, m_Agent, MyUI);

        //Node setup, from the bottom of the tree to the top (in that order)

        //The lowest sequence, investiagtes then chases or fails if investigation fails
        List<Node> Sequence_FirstSubNodes = new List<Node>();

        //look for enemy in sight succeed/fail
        Sequence_FirstSubNodes.Add(My_Action_SuspicousEvent);

        //investigate, move to the place where they enemy was seen (ongoing), on seeing for 3 seconds succeed, on arrival fail
        Sequence_FirstSubNodes.Add(My_Action_Investigate);

        //Chase the enemy (ongoing)
        Sequence_FirstSubNodes.Add(My_Action_Chase);

        Node Sequence_First = new Node(Node.NodeType.Sequence, Sequence_FirstSubNodes.ToArray());

        //Next up the selector which checks the above sequence, if that fails it starts a patrol
        List<Node> Selector_SecondSubNodes = new List<Node>();

        //above sequence
        Selector_SecondSubNodes.Add(Sequence_First);

        //patrol to random areas (ongoing), fail if tired
        Selector_SecondSubNodes.Add(My_Action_Patrol);
        Node Selector_Second = new Node(Node.NodeType.Selector, Selector_SecondSubNodes.ToArray());
        
        //Next up the top selector which checks if the above selector failed, if so then it sleeps
        List<Node> Selector_FirstSubNodes = new List<Node>();

        //above selector
        Selector_FirstSubNodes.Add(Selector_Second);

        //fall asleep
        Selector_FirstSubNodes.Add(My_Action_SleepForDuration);
        Node Selector_First = new Node(Node.NodeType.Selector, Selector_FirstSubNodes.ToArray());

        //The top selector to be updated in the tree
        TopOfTheTree = Selector_First;
    }

    void FixedUpdate()
    {
        TopOfTheTree.OnUpdate();
    }
}
