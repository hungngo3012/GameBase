using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorController : MonoBehaviour
{
    private Animator animator;

    [System.Serializable]
    public class AnimationEventEntry
    {
        public string animationName;
        public UnityEvent onAnimationEvent;
    }

    [Header("Sự kiện animation tùy chỉnh")]
    public List<AnimationEventEntry> animationEvents = new List<AnimationEventEntry>();

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }
    }

    // === Các hàm quản lý animation ===

    public void PlayTrigger(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }

    public void SetBool(string boolName, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(boolName, value);
        }
    }

    public bool IsPlaying(string animationName)
    {
        if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            return animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f;
        }
        return false;
    }

    public void SetAnimationSpeed(float speed)
    {
        if (animator != null)
        {
            animator.speed = speed;
        }
    }

    public void ResetAllTriggers()
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(param.name);
            }
        }
    }

    // === Hàm kích hoạt UnityEvent khi animation đạt điểm cụ thể ===

    // Gọi từ Animation Event (trong Animator)
    public void OnAnimationEvent(string animationName)
    {
        foreach (var entry in animationEvents)
        {
            if (entry.animationName == animationName)
            {
                entry.onAnimationEvent?.Invoke();
                break;
            }
        }
    }

    // === Ví dụ: gọi khi animation kết thúc ===
    IEnumerator WaitForAnimationEnd(string animationName)
    {
        while (IsPlaying(animationName))
        {
            yield return null;
        }

        OnAnimationEvent(animationName); // Gọi UnityEvent sau khi animation kết thúc
    }

    // Gọi từ script khác để chờ animation và kích hoạt event
    public void PlayAndWaitEvent(string triggerName, string animationName)
    {
        PlayTrigger(triggerName);
        StartCoroutine(WaitForAnimationEnd(animationName));
    }
}
