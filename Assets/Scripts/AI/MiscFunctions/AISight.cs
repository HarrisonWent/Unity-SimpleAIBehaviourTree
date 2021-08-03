using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISight : MonoBehaviour
{
    public enum WhoToFind
    {
        Guard,
        Spy
    } public WhoToFind WhoShallIFind = WhoToFind.Guard;

    private Vector3 LastKnownPosition = Vector3.zero;
    public int FieldOfView = 45;
    private int layer_mask;

    public enum VisualState
    {
        Exposed,
        Hidden
    }public VisualState MyVisualState = VisualState.Exposed;

    private void Start()
    {
        layer_mask = 1 << LayerMask.NameToLayer("Environment");

        if (WhoShallIFind == WhoToFind.Guard){
            AIList.Spies.Add(this);}
        else{
            AIList.Guards.Add(this);
        }
    }

    /// <summary>
    /// Last known location of target before losing sight
    /// </summary>
    /// <returns></returns>
    public Vector3 GetLastKnownPosition()
    {
        return LastKnownPosition;
    }

    /// <summary>
    /// Searches for a target
    /// </summary>
    /// <param name="MaxRange">Maximum distance target can be</param>
    /// <param name="UseRay">If true sight will be checked for targets</param>
    /// <param name="useClosest">If true closest enemies will be returned</param>
    /// <param name="NonRayFallback">If true enemies which cannot be seen will be returned if there are none that can be seen</param>
    /// <param name="UseAngle">If true it will require sight within a 180 degree angle forwards</param>
    /// <returns></returns>
    public GameObject GetMeATarget(float MaxRange, bool UseRay, bool useClosest, bool NonRayFallback, bool UseAngle)
    {
        List<AISight> Targets = new List<AISight>(); 

        if(WhoShallIFind == WhoToFind.Guard)
        {
            Targets = AIList.Guards;
        }
        else
        {
            Targets = AIList.Spies;
        }

        List<GameObject> CanSee = new List<GameObject>();
        List<GameObject> CantSee = new List<GameObject>();

        foreach (AISight MatchPlayer in Targets)
        {
            //Skip if hidden
            if(MatchPlayer.MyVisualState == VisualState.Hidden)
            {
                return null;
            }

            //Skip if out of range
            if (Vector3.Distance(MatchPlayer.transform.position, transform.position) > MaxRange)
            {
                continue;
            }

            //if not using raycast add it
            else if (!UseRay)
            {
                CanSee.Add(MatchPlayer.gameObject);
            }
            //if using raycast
            else if (UseRay)
            {
                //if visible add it 
                if (CheckVisibility(MatchPlayer.gameObject,UseAngle))
                {
                    CanSee.Add(MatchPlayer.gameObject);
                }
                //if falling back, add it to in range but cant see
                else if (NonRayFallback)
                {
                    CantSee.Add(MatchPlayer.gameObject);
                }
            }
        }
        if (CanSee.Count == 0)
        {
            //Cant see anyone
            if (UseRay && NonRayFallback && CantSee.Count > 0)
            {
                GameObject Chosen = CantSee[Random.Range(0, CantSee.Count)];
                LastKnownPosition = Chosen.transform.position;
                return Chosen;
            }
            return null;
        }
        else
        {
            //Found someone
            if (useClosest)
            {
                float closest = 0;
                GameObject closestObject = null;
                float test = 0f;

                foreach (GameObject a in CanSee)
                {
                    test = Vector3.Distance(transform.position, a.transform.position);
                    if (test < closest || closestObject == null)
                    {
                        closest = test;
                        closestObject = a;
                    }
                }
                LastKnownPosition = closestObject.transform.position;
                return closestObject;
            }
            else
            {
                GameObject Chosen = CanSee[Random.Range(0, CanSee.Count)];
                LastKnownPosition = Chosen.transform.position;
                return Chosen;
            }
        }
    }

    /// <summary>
    /// Checks if a target can be seen
    /// </summary>
    /// <param name="Target">The target object to check sight with</param>
    /// <param name="UseAngle">If true it will require sight within a 180 degree angle forwards</param>
    /// <returns>If can see</returns>
    public bool CheckVisibility(GameObject Target, bool UseAngle)
    {
        RaycastHit hit;
        Vector3 bPos = Target.transform.position;
        Vector3 rayDirection = bPos - transform.position;

        float range = Vector3.Distance(transform.position, Target.transform.position);
        Ray ray = new Ray(transform.position, rayDirection * 1000);

        if (UseAngle)
        {
            //look forward and check field of view
            Debug.DrawRay(bPos, -rayDirection, Color.red, 0.25f);
            Quaternion Angle = Quaternion.FromToRotation(transform.forward, rayDirection);

            float newRotationValue = Angle.eulerAngles.y;
            while (newRotationValue < Angle.y - 180f) newRotationValue += 360f;
            while (newRotationValue > Angle.y + 180f) newRotationValue -= 360f;

            //Debug.Log(newRotationValue);
            if (newRotationValue < 0)
            {
                if (newRotationValue < -FieldOfView) { return false; }
            }
            else
            {
                if (newRotationValue > FieldOfView) { return false; }
            }
        }

        //check for walls
        if (Physics.Raycast(ray, out hit, range, layer_mask))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
