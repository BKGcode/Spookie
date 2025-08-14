using UnityEngine;
using System.Collections.Generic;

public class FuseBox : MonoBehaviour
{

    public enum FuseboxType
    {
        FuseboxDoor,
        FuseboxHandle
    }

    public FuseboxType fuseboxType;

    [SerializeField] public InspectController inspectController;
    [SerializeField] public string ItemName;

    public AudioSource Open;
    public AudioSource Close;

    public bool FuseboxDoorOpen = false;

    public Animator FuseboxAnimator;
    [SerializeField] private string openAnimation = "FuseBoxDoorOpen";
    [SerializeField] private string closeAnimation = "FuseBoxDoorClose";



    public Animator FuseboxLeverAnimator;
    [SerializeField] private string pullupAnimation = "FuseBoxDoorLeverUp";
    [SerializeField] private string pulldownAnimation = "FuseBoxDoorLeverDown";
    public bool FuseboxLeverPulled = false;

    public AudioSource PullDown;
    public AudioSource PullUp;
    public List<Switcher> Switchers = new List<Switcher>();
    public List<Television> television = new List<Television>();
    public List<BrokenSwitcher> brokenSwitchers = new List<BrokenSwitcher>();
    public List<Lamp> Lamps = new List<Lamp>();

    private void Start()
    {
        if(!FuseboxLeverPulled)
        {
            foreach (var switcher in Switchers)
            {
                switcher.IsGeneratorOn = true;
            }
            foreach (var televisions in television)
            {
                televisions.isGeneratorOn = true;
            }
            foreach (var brokenswitcher in brokenSwitchers)
            {
                brokenswitcher.isGeneratorOn = true;
            }
            foreach (var lamps in Lamps)
            {
                lamps.isGeneratorOn = true;
            }
        }
        else
        {
            foreach (var switcher in Switchers)
            {
                switcher.IsGeneratorOn = false;
            }
            foreach (var televisions in television)
            {
                televisions.isGeneratorOn = false;
            }
            foreach (var brokenswitcher in brokenSwitchers)
            {
                brokenswitcher.isGeneratorOn = false;
            }
            foreach (var lamps in Lamps)
            {
                lamps.isGeneratorOn = false;
            }
        }
    }


    public void FuseboxOpen()
    {
        if (fuseboxType == FuseboxType.FuseboxDoor)
        {
            if (!FuseboxDoorOpen)
            {
                FuseboxAnimator.Play(openAnimation, 0, 0.0f);
                Open.Play();
                FuseboxDoorOpen = true;
            }
            else
            {
                FuseboxAnimator.Play(closeAnimation, 0, 0.0f);
                Close.Play();
                FuseboxDoorOpen = false;
            }
        }
        else if (fuseboxType == FuseboxType.FuseboxHandle)
        {
            if(FuseboxLeverPulled)
            {
                FuseboxLeverAnimator.Play(pullupAnimation, 0, 0.0f);
                PullUp.Play();
                foreach (var switcher in Switchers)
                {
                    switcher.IsGeneratorOn = true;
                }
                foreach(var televisions in television)
                {
                    televisions.isGeneratorOn = true;
                }
                foreach (var brokenswitcher in brokenSwitchers)
                {
                    brokenswitcher.isGeneratorOn = true;
                }
                foreach(var lamps in Lamps)
                {
                    lamps.isGeneratorOn = true;
                }
                FuseboxLeverPulled = false;
            }
            else
            {
                PullDown.Play();
                FuseboxLeverAnimator.Play(pulldownAnimation, 0, 0.0f);
                foreach (var switcher in Switchers)
                {
                    switcher.IsGeneratorOn = false;
                }
                foreach (var televisions in television)
                {
                    televisions.isGeneratorOn = false;
                }
                foreach (var brokenswitcher in brokenSwitchers)
                {
                    brokenswitcher.isGeneratorOn = false;
                }
                foreach (var lamps in Lamps)
                {
                    lamps.isGeneratorOn = false;
                }
                FuseboxLeverPulled = true;
            }
        }
    }

    public void ShowObjectName()
    {
        inspectController.ShowName(ItemName);
    }

    public void HideObjectName()
    {
        inspectController.HideName();
    }

}
