using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NPC_UI))]
public class Spy : MonoBehaviour
{
    public Transform Objective;
    protected AISight m_Sight;
    protected NavMeshAgent m_Agent;
    private NPC_UI MyUI;
    public Transform[] HidingSpots;
    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Sight = GetComponent<AISight>();
        MyUI = GetComponent<NPC_UI>();
        MyUI.SpawnMyPanel();
        CreateTree();
    }

    private Node TopOfTheTree;
    void CreateTree()
    {
        //Actions
        Action_SeekIntel My_Action_SeekIntel = new Action_SeekIntel(m_Agent, MyUI, Objective.position);
        Action_CheckIfCaught My_Action_CheckIfCaught = new Action_CheckIfCaught(m_Sight);
        Action_CheckIfBeingChased My_Action_CheckIfBeingChased = new Action_CheckIfBeingChased(m_Sight);
        Action_RunToHide My_Action_RunToHide = new Action_RunToHide(m_Agent,MyUI, HidingSpots,m_Sight);

        //Node setup, from the bottom of the tree to the top (in that order)

        //The lowest sequence, manages being caught and running away, failure means carry on getting the intel
        List<Node> Sequence_FirstSubNodes = new List<Node>();

        //Check if the games over
        Sequence_FirstSubNodes.Add(My_Action_CheckIfCaught);

        //Check if any guards are chasing us
        Sequence_FirstSubNodes.Add(My_Action_CheckIfBeingChased);

        //Run and hide
        Sequence_FirstSubNodes.Add(My_Action_RunToHide);

        Node Sequence_First = new Node(Node.NodeType.Sequence, Sequence_FirstSubNodes.ToArray());

        //Next up the top selector which checks if the above selector failed, if so then carry on getting the intel
        List<Node> Selector_FirstSubNodes = new List<Node>();

        //above sequence
        Selector_FirstSubNodes.Add(Sequence_First);

        //go to the objective
        Selector_FirstSubNodes.Add(My_Action_SeekIntel);
        Node Selector_First = new Node(Node.NodeType.Selector, Selector_FirstSubNodes.ToArray());

        //The top selector to be updated in the tree
        TopOfTheTree = Selector_First;
    }

    void Update()
    {
        TopOfTheTree.OnUpdate();
    }
}