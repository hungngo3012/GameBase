using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NinthArt;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private ColorCar colorCar;
    public ColorCar ColorCar
    {
       get => colorCar;
       set
       {
            colorCar = value;
       }
    }

    [SerializeField] private Transform slotPar;
    [SerializeField] private List<PosPassCar> listPosPass;

    [SerializeField] internal ColorConfig colorConfig;

    private int carCountSlot;
    public int CarCountSlotPassenger => carCountSlot;
    
    private int countPassenger;

    public CarNumSlotConfig carNumSlotConfig;
    [SerializeField] internal CarType carType;
    [SerializeField] List<CarModel> carsModel = new List<CarModel>();

    private PointStop pointStop;
    private CarSlot carSlot;
    private Level level;
    private bool isCarMovingCrash;

    private List<Vector3> listMove;
    private List<RayPoint> listRayPoint;
    public bool CarIsMoving => isCarMovingCrash;
    internal int CountPassenger => countPassenger;

    public bool HiddenColorCar;

    // Start is called before the first frame update
    public void Init(CarSlot c, BoxCollider b, CarType type = CarType.Small)
    {
        b.center = carNumSlotConfig.carSlotConfig[(int)carType].boxCenter;
        b.size = carNumSlotConfig.carSlotConfig[(int)carType].boxSize;

        UpdateCarModel();
        if (Gameplay.Instance != null)
            level = Gameplay.Instance.Level;

        listRayPoint = GetComponentsInChildren<RayPoint>().ToList();
        carSlot = c;
        foreach (var r in listRayPoint)
        {
            r.InitBox(b);
        }

        listPosPass = new List<PosPassCar>(GetCarMesh().posPassCars);
        for (int i = 0; i < listPosPass.Count; i++)
        {
            listPosPass[i].Init(i);
        }

        carCountSlot = listPosPass.Count;
        if (HiddenColorCar)
            EventManager.Subscribe(NinthArt.EventType.CarGoToStop, CheckUnlockHiddenColor);
    }
    private void OnDisable()
    {
        if (HiddenColorCar)
            EventManager.Unsubscribe(NinthArt.EventType.CarGoToStop, CheckUnlockHiddenColor);
    }
    private void OnDestroy()
    {
        if (HiddenColorCar)
            EventManager.Unsubscribe(NinthArt.EventType.CarGoToStop, CheckUnlockHiddenColor);
    }
    internal void CheckUnlockHiddenColor(object o = null)
    {
        if (listRayPoint.Any(x => x != null && x.IsCar()))
            return;

        GetCarMesh().UnlockHiddenColor();
        HiddenColorCar = false;
    }    
    internal bool CarCanMove()
    {
        return !(listRayPoint.Any(x => x != null && x.IsCar()));
    }
    internal int GetSlotCount()
    {
        return carNumSlotConfig.carSlotConfig[(int)carType].slots;
    }
    internal CarMesh GetCarMesh()
    {
        return carsModel[(int)carType].carMesh;
    }    
    internal void UpdateCarModel()
    {
        if ((int)carType >= carsModel.Count)
            Debug.LogError("carType > carModel: " + transform.parent.name);
        else
        {
            carsModel[(int)carType].gameObject.SetActive(true);
            UpdateCarColor();
        }
    }   
    internal void UpdateCarColor()
    {
        GetCarMesh().SetColor(colorConfig, HiddenColorCar, colorCar);   
    }    
    internal IEnumerator ShuffleColorCar()
    {
        // Tạo một danh sách các màu có sẵn
        if(!HiddenColorCar)
        {
            List<Color> availableColors = colorConfig.colors.ToList();

            // Thời gian mỗi lần đổi màu
            float shuffleInterval = GlobalDefine.shuffleDuration; // 0.5 giây                                                    
            int shuffleCount = 10;// Số lần đổi màu trước khi dừng

            for (int i = 0; i < shuffleCount; i++)
            {
                Color randomColor = availableColors[Random.Range(0, availableColors.Count)];
                CarMesh tmpCarMesh = GetCarMesh();
                tmpCarMesh.mesh.materials[tmpCarMesh.colorMatIndex].color = randomColor;
                yield return new WaitForSeconds(shuffleInterval);
            }
        }

        UpdateCarColor();
        if(curCarBelt != null)
        {
            curCarBelt.UpdateCarColorInHiddenList(carSlot);
        }
    }    
    internal Vector3 StopOffset()
    {
        return -GetCarMesh().modelOffset;
    }
    internal bool IsParking()
    {
        return carSlot.isParking;
    }    
    private float _timeDelayAnim = 0.3f;

    private Tweener _moveT;
    private RayPoint _trueRay;
    private Vector3 _startP;
    internal CarBelt curCarBelt;
    public void CarMove(bool vipMove = false, bool forceInit = false)
    {
        CarMesh curCarMesh = GetCarMesh();
        if (!forceInit && !vipMove && listRayPoint.Any(x => x != null && x.IsCar()))
        {
            if (curCarBelt != null)
            {
                Gameplay.Instance.Level.StopRollCarBelts();
            }

            var l1 = listRayPoint.FindAll(x => x != null && x.IsCar());
            foreach (var s in l1)
            {
                s.PointToMove(transform);
            }
            var l = l1.OrderBy(x=>x.Distance).ToList();
            _trueRay = l[0];
            _startP = l[0].PointToMove(transform);

            CarMesh crashCarMesh = l[0].GetCarMesh();
            isCarMovingCrash = true;

            if(_trueRay.Distance > 3.0f)
                curCarMesh.ActiveMoveVfx(true);

            _moveT = transform.DOLocalMoveZ(_trueRay.Distance, _trueRay.Distance / 10f).SetEase(Ease.Linear).OnComplete(() =>
            {
                if (crashCarMesh != null)
                    crashCarMesh.ImpactAnim();

                Utils.VibrateHeavy();
                VFXManager.PlayVFX(Vfx.CRASH, _startP + new Vector3(0.0f, 0.5f, 0.0f), transform.rotation, transform);
                SoundManager.PlaySfx("crash");
                StartCoroutine(MoveCrash());
            });
        }
        else
        { 
            pointStop = (vipMove)? level.VipPointStop : level.GetPointStopFree();
            if (pointStop == null)
            {
                return;
            }
            pointStop.SetBusy();
            carSlot.SetFree(forceInit);
            var startp = listRayPoint[1].PointToMove(transform);
            listMove = level.FindShortestPath(startp, pointStop, this);
            var ar = listMove.ToArray();

            if (vipMove && Gameplay.Instance != null)
            {
                if(HiddenColorCar)
                {
                    curCarMesh.UnlockHiddenColor();
                    HiddenColorCar = false;
                }    

                Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Normal);
                if(!forceInit)
                    Gameplay.Instance.Level.SaveGameProgress();
            }    

            level.inTransit++;
            level.carMoving++;

            if(!forceInit)
            {
                curCarMesh.ActiveMoveVfx(true);
                SoundManager.PlaySfx("carGoToStop", 1.0f);
                transform.DOScale(GlobalDefine.carScaleWhenStop, GlobalDefine.carMoveDuration).SetEase(Ease.Linear).SetSpeedBased(true);
                transform.DOPath(ar, GlobalDefine.carMoveDuration).SetLookAt(0).SetEase(Ease.Linear).SetSpeedBased(true).OnComplete(() =>
                {
                    if (curCarBelt != null)
                    {
                        curCarBelt.FreeCar(carSlot);
                        curCarBelt = null;
                    }
                    curCarMesh.ActiveMoveVfx(false);
                    curCarMesh.OpenHoob();
                    pointStop.SetCar(this);
                    Quaternion currentRotation = transform.rotation;
                    transform.rotation = Quaternion.Euler(currentRotation.eulerAngles.x, currentRotation.eulerAngles.y, 0);
                    level.carMoving--;
                    StartCoroutine(level.PassToCar());
                    //level.PassToCar();
                });
            }
            else
            {
                if (curCarBelt != null)
                {
                    curCarBelt.FreeCar(carSlot);
                    curCarBelt = null;
                }
                transform.localScale = Vector3.one * GlobalDefine.carScaleWhenStop;
                transform.position = ar[ar.Length - 1];
                curCarMesh.OpenHoob();
                pointStop.SetCar(this);
                Quaternion currentRotation = transform.rotation;
                transform.rotation = pointStop.transform.rotation;
                level.carMoving--;
                StartCoroutine(level.PassToCar());
                //level.PassToCar();
            }    
        }
    }
    IEnumerator MoveCrash()
    {
        while (!isCarMovingCrash)
            yield return null;
        // Wait until the distance condition is met
        yield return new WaitUntil(() => _trueRay.CheckDistance(_startP) <= 0.1f);

        // Stop the movement tween
        _moveT?.Kill();

        // Wait for the delay to finish
        while (_timeDelayAnim > 0)
        {
            _timeDelayAnim -= Time.deltaTime;
            yield return null;
        }

        GetCarMesh().ActiveMoveVfx(false);
        // Move the car and reset the state
        transform.DOLocalMoveZ(0, transform.localPosition.z / 10f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                isCarMovingCrash = false;
                if (curCarBelt != null)
                    Gameplay.Instance.Level.ContinueRollCarBelts();
            });

        _timeDelayAnim = 0.3f;  // Reset the delay timer
    }
    public bool CheckFullCar()
    {
        return countPassenger >= carCountSlot;
    }
    public int GetNumEmptySlot()
    {
        return (carCountSlot - countPassenger);
    }    

    public void PassSeatAddCount()
    {
        countPassenger++;
    }
    public PosPassCar GetFreeSlot()
    {
        var slot = listPosPass.FirstOrDefault(x => !x.HasPass);
        return slot;
    }
    public PosPassCar GetRndBusySlot()
    {
        List<PosPassCar> tmp = new List<PosPassCar>();
        foreach(PosPassCar posPassCar in listPosPass)
        {
            if (!posPassCar.HasPass)
                continue;
            tmp.Add(posPassCar);
        }

        if (tmp.Count <= 0)
            return null;

        return tmp[Random.Range(0, tmp.Count)];
    }
    public void CarFullAndGo(bool init = false)
    {
        pointStop.SetFree();
        listMove.Clear();
        listMove.Add(pointStop.RStop());
        listMove.Add(level.EndRoad.transform.position);
        if(!init)
            Gameplay.Instance.Level.SaveGameProgress();
        StartCoroutine(WaitPassMove());
    }
    IEnumerator WaitPassMove()
    {
        var ar = listMove.ToArray();
        yield return new WaitUntil(()=>listPosPass.All(x => x.passSitted));

        SoundManager.PlaySfx("carGo", 0.3f);
        Utils.VibrateHeavy();
        GetCarMesh().CloseHoob();
        transform.DOPath(ar, 30f).SetLookAt(0).SetEase(Ease.Linear).SetSpeedBased(true).OnComplete(() =>
        {
            gameObject.SetActive(false);
            carSlot.gameObject.SetActive(false);
        });
    }
}
public enum CarType
{
    Small = 0,
    Medium = 1,
    Big = 2,
    Bus = 3,
}