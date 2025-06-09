using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerMesh : MonoBehaviour
{
    public SkinnedMeshRenderer mesh;
    public int colorMatIndex = 0;
    [SerializeField] Animator animator;

    internal void IdleAnim ()
    {
        animator.SetBool("walk", false);
    }
    internal void RunAnim()
    {
        animator.SetTrigger("run");
    }
    internal void WalkAnim()
    {
        animator.SetBool("walk", true);
    }
    internal void SitAnim()
    {
        animator.SetTrigger("sit");
    }
}
