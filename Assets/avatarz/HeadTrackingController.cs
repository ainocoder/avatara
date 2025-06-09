using UnityEngine;
using Convai.Scripts.Runtime.Core;  // ConvaiHeadTracking이 있는 네임스페이스
using Convai.Scripts.Runtime.Features;

public class HeadTrackingController : StateMachineBehaviour
{
    private ConvaiHeadTracking headTracking;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (headTracking == null)
            headTracking = animator.GetComponent<ConvaiHeadTracking>();

        if (headTracking != null)
        {
            headTracking.enabled = false;
            Debug.Log("Head Tracking Disabled");
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (headTracking != null)
        {
            headTracking.enabled = true;
            Debug.Log("Head Tracking Enabled");
        }
    }
}
