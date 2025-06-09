using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NinthArt;

public class CarMesh : MonoBehaviour
{
    public SkinnedMeshRenderer mesh;
    public GameObject arrow;
    public GameObject question;
    public Vector3 modelOffset;

    public List<PosPassCar> posPassCars = new List<PosPassCar>();
    public Animator animator;

    public int colorMatIndex = 0;

    [SerializeField] Transform moveVfx;
    private void Start()
    {
        VFXManager.PlayVFX(Vfx.MOVE, moveVfx.position, moveVfx.rotation, moveVfx);
    }
    public void OpenHoob()
    {
        mesh.SetBlendShapeWeight(0, 100.0f);
        arrow.SetActive(false);
    }
    public void CloseHoob()
    {
        mesh.SetBlendShapeWeight(0, 0.0f);
        foreach(PosPassCar posPassCar in posPassCars)
        {
            posPassCar.gameObject.SetActive(false);
        }
    }
    public void PassengerGoUpAnim()
    {
        animator.SetTrigger("passSeat");
    }
    public void ImpactAnim()
    {
        animator.SetTrigger("impact");
    }
    public void ActiveMoveVfx(bool active)
    {
        Debug.Log("active " + active + " move vfx " + transform.parent.name);
        moveVfx.gameObject.SetActive(active);
    }
    Color curColor;
    public void SetColor(ColorConfig colorConfig, bool isHidden, ColorCar colorCar)
    {
        if(Gameplay.Instance != null)
            curColor = colorConfig.colors[Gameplay.Instance.Level.shuffleColors[(int)colorCar]];
        else
            curColor = colorConfig.colors[(int)colorCar];

        mesh.materials[colorMatIndex].color = isHidden ? colorConfig.hiddenColor : curColor;
        if(isHidden)
        {
            arrow.SetActive(false);
            question.SetActive(true);
        } 
        else
        {
            arrow.SetActive(true);
            question.SetActive(false);
        }    
    }
    public void UnlockHiddenColor()
    {
        if (arrow == null || question == null || mesh == null)
            return;

        arrow.SetActive(true);
        question.SetActive(false);
        mesh.materials[colorMatIndex].color = curColor;
    }
}
