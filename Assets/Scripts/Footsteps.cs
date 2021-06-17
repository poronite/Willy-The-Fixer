using UnityEngine;
using FMODUnity;

public class Footsteps : MonoBehaviour
{
    private float currentFloorMaterial;

    private FMOD.Studio.EventInstance instance;

    public void ChangeSurfaceType(string tag)
    {
        switch (tag)
        {
            case "WoodFloor":
                currentFloorMaterial = 0;
                break;
            case "MetalFloor":
            case "Strings":
                currentFloorMaterial = 1;
                break;
            default:
                break;
        }
    }

    public void TriggerFootstepSound()
    {
        instance = RuntimeManager.CreateInstance("event:/SFX/Characters/Footsteps");
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        instance.setParameterByName("SurfaceType", currentFloorMaterial);
        instance.start();
        instance.release();
    }
}
