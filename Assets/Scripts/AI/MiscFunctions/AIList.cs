using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIList : MonoBehaviour
{
    public static List<AISight> Guards = new List<AISight>();
    public static List<AISight> Spies = new List<AISight>();

    void Awake()
    {
        Guards.Clear();
        Spies.Clear();
    }
}
