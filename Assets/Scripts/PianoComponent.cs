using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoComponent : MonoBehaviour
{
    public bool IsRepaired = false;

    public void RepairComponent()
    {
        IsRepaired = true;
    }

    public void DestroyComponent()
    {
        IsRepaired = false;
    }
}
