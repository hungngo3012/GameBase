using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosPassCar : MonoBehaviour
{
    [SerializeField] private int index;
    [SerializeField] private bool hasPass;
    internal bool passSitted;
    internal Passanger sittingPass;
    public bool HasPass => hasPass;
    public void Init(int i)
    {
        index = i;
    }

    public void PassGoPos(Passanger pass)
    {
        hasPass = true;
        sittingPass = pass;
    }
}
