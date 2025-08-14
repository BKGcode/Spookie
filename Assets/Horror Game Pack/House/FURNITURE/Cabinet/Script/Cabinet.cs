using UnityEngine;

public class Cabinet : MonoBehaviour
{
    public Animator CabinetAnimator;
    public int ID;
    public Transform transformholder;

    public bool CanInteract()
    {
        return true;
    }

    public int GetID()
    {
        return ID;
    }

    public void StartAnimation()
    {
        CabinetAnimator.Play("Cabinet", 0, 0.0f);
    }

    public void ExitAnimation()
    {
        CabinetAnimator.Play("CabinetClose", 0, 0.0f);
    }
    public Transform GetTransformHolder()
    {
        return transformholder;
    }
}
