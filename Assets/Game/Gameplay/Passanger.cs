using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NinthArt;
using UnityEngine;

public class Passanger : MonoBehaviour
{
    [SerializeField] private ColorCar colorCar;
    [SerializeField] internal SkinConfig skinConfig;
    public ColorCar ColorCar => colorCar;

    internal PassengerMesh passMesh;
    internal int curIndex;
    void InitModel()
    {
        GameObject pass = Gameplay.levelSkin.passenger;
        GameObject model = Instantiate(pass, transform);

        passMesh = model.GetComponentInChildren<PassengerMesh>();
    }
    public void InitColor(ColorCar c)
    {
        if (passMesh == null)
            InitModel();

        ColorConfig colorConfig = Gameplay.Instance.colorConfig;
        colorCar = c;

        passMesh.mesh.materials[passMesh.colorMatIndex].color = colorConfig.colors[Gameplay.Instance.Level.shuffleColors[(int)c]];
    }
    internal IEnumerator ShuffleColorPassenger()
    {
        // Tạo một danh sách các màu có sẵn
        List<Color> availableColors = Gameplay.Instance.colorConfig.colors.ToList();

        // Thời gian mỗi lần đổi màu
        float shuffleInterval = GlobalDefine.shuffleDuration; // 0.5 giây
                                                              // Số lần đổi màu trước khi dừng
        int shuffleCount = 10;

        for (int i = 0; i < shuffleCount; i++)
        {
            Color randomColor = availableColors[UnityEngine.Random.Range(0, availableColors.Count)];
            passMesh.mesh.materials[passMesh.colorMatIndex].color = randomColor;
            yield return new WaitForSeconds(shuffleInterval);
        }
    }
    public void GotoCar(CarController c)
    {
        c.PassSeatAddCount();

        PosPassCar slot = c.GetFreeSlot();
        slot.PassGoPos(this);

        var posMove = slot.transform.position;

        Vector3 dir = posMove - transform.position;
        dir.Normalize();
        dir.y = 0;

        Quaternion rotation = Quaternion.LookRotation(dir);
        passMesh.RunAnim();
        transform.rotation = rotation;//Nhìn về phía ô 

        float duration = Vector3.Distance(transform.position, posMove) / GlobalDefine.passengerRunSpeed;
        transform.DOScale(Vector3.one * GlobalDefine.passengerScaleWhenSeat, duration).SetEase(Ease.Linear);
        transform.DOMove(posMove, duration).SetEase(Ease.Linear).OnComplete(
            () =>
            {
                c.GetCarMesh().PassengerGoUpAnim();
                passMesh.SitAnim();

                SoundManager.PlaySfx("pop", 0.1f);
                transform.parent = slot.transform;
                transform.localPosition = Vector3.zero;
                transform.localEulerAngles = Vector3.zero;

                slot.passSitted = true;
            });
    }    
    int walkingThread = 0;
    public void Walk(List<Vector3> listMove)
    {
        if(walkingThread <= 0)
            passMesh.WalkAnim();

        walkingThread++;
        var ar = listMove.ToArray();
        transform.DOLocalPath(ar, GlobalDefine.passengerWalkDuration).SetLookAt(0).SetEase(Ease.Linear).SetSpeedBased(true).OnComplete(() =>
        {
            walkingThread--;
            if (walkingThread <= 0)
                passMesh.IdleAnim();
        });
    }
}
