using UnityEngine;

public class HideCamera : MonoBehaviour
{
    public Camera mainCamera;
    public Camera childCamera;
    public Animator hideCameraAnimator;
    public int ID;

    public int GetID()
    {
        return ID;
    }

    public void StartAnimation()
    {
        hideCameraAnimator.Play("HideAnimation", 0, 0.0f);
    }

    public void ExitAnimation()
    {
        hideCameraAnimator.Play("ExitHideAnimation", 0, 0.0f);
    }

    public void IdleMode()
    {
        hideCameraAnimator.Play("Idle", 0, 0.0f);
    }

    public void SetMainCameraEnabled(bool enabled)
    {
        mainCamera.enabled = enabled;
    }

    public void SetChildCameraEnabled(bool enabled)
    {
        childCamera.enabled = enabled;
    }

    public Vector3 GetChildCameraPosition()
    {
        return childCamera.transform.position;
    }

    public Quaternion GetChildCameraRotation()
    {
        return childCamera.transform.rotation;
    }

    public Vector3 GetMainCameraPosition()
    {
        return mainCamera.transform.position;
    }

    public Quaternion GetMainCameraRotation()
    {
        return mainCamera.transform.rotation;
    }

    public void SetMainCameraPosition(Vector3 position)
    {
        mainCamera.transform.position = position;
    }

    public void SetMainCameraRotation(Quaternion rotation)
    {
        mainCamera.transform.rotation = rotation;
    }
}
