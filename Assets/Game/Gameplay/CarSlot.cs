using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NinthArt;
using UnityEngine;

public class CarSlot : MonoBehaviour
{
    [SerializeField] internal CarController carController;
    [SerializeField] private Slot slot;
    [SerializeField] private BoxCollider boxCollider;
    internal bool isParking = false;
    internal Basement curBasement;
    public ColorCar ColorCar => carController.ColorCar;

    public int CountPassenger => carController.CarCountSlotPassenger;
    // Start is called before the first frame update
    internal void Init()
    {
        carController.Init(this, boxCollider);
    }
    [ContextMenu("CarModel")]
    public void UpdateCarModel()
    {
        carController.UpdateCarModel();
    }

    public void ItemGrabbed(bool vipMode = false)
    {
        if(CarIsMoving())
            return;
        carController.CarMove(vipMode);
    }
    public void OnSelectVipCar()
    {
        if (CarIsMoving())
            return;
        carController.CarMove(true);
    }    
    public void SetFree(bool forceInit = false)
    {
        boxCollider.enabled = false;
        slot.gameObject.SetActive(false);
        isParking = true;

        if (curBasement != null && !forceInit)
            curBasement.SpawnCar();

        EventManager.Annouce(NinthArt.EventType.CarGoToStop);
    }

    public bool CarIsMoving()
    {
        return carController.CarIsMoving;
    }
    public void CarMoveOutFromBasement(Vector3 target)
    {
        StartCoroutine(MoveAndScale(target));
    }
    private IEnumerator MoveAndScale(Vector3 target)
    {
        float duration = GlobalDefine.carMoveOutDuration; // thời gian di chuyển và scale
        float elapsedTime = 0f;

        Vector3 startPosition = transform.position;
        Vector3 startScale = Vector3.zero; // bắt đầu từ scale 0
        Vector3 endScale = Vector3.one; // scale 1 là kích thước gốc

        while (elapsedTime < duration)
        {
            // Di chuyển đến vị trí target
            transform.position = Vector3.Lerp(startPosition, target, elapsedTime / duration);

            // Thay đổi scale từ 0 -> 1
            transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Đảm bảo đạt vị trí và scale cuối cùng
        transform.position = target;
        transform.localScale = endScale;
    }
}
