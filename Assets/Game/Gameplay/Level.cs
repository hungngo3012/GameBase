using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace NinthArt
{
    public class Level : MonoBehaviour
    {
        // TODO: Implement level-specific features here
        [SerializeField] private List<PointRoad> listPointRoad;
        [SerializeField] private List<PointStop> listPointStop;
        public List<PointRoad> ListPointRoad => listPointRoad;
        [SerializeField] internal Transform carSlotPar;
        [SerializeField] private List<CarSlot> listCarSlot;
        [SerializeField] private List<ColorAndCount> listColorAndCount;
        [SerializeField] private Transform passengerParent;
        [SerializeField] private Transform passengerStartPoint;
        [SerializeField] private Transform passengerMiddlePoint;
        [SerializeField] private Transform passengerEndPoint;
        [SerializeField] private int countPassShow;
        [SerializeField] private int countPassMiddle;
        [SerializeField] private int countPassBorn;
        [SerializeField] private Passanger passengerPref;
        private Queue<Passanger> _passengerQueue = new Queue<Passanger>();
        [SerializeField] private float distanceX;
        [SerializeField] private float distanceY;
        [SerializeField] private float xPerPass;
        [SerializeField] private float yPerPass;
        [SerializeField] private Transform endRoad;
        public Transform EndRoad => endRoad;
        public CarSlot baseCarSlot;
        public Basement basementPrefab;
        public CarBelt carbeltPrefab;

        public CarInLevelCreator Car4;
        public CarInLevelCreator Car6;
        public CarInLevelCreator Car8;

        public PointStop VipPointStop;

        List<Vector3> listPassengerNode = new List<Vector3>();

        public SkinConfig skinConfig;
        public CarNumSlotConfig carNumSlotConfig;
        public Transform envParent;
        internal int levelNumPass;

        public List<CarBeltData> carBeltDatas = new List<CarBeltData>();
        public LevelConfig levelConfig;
        public IEnumerator Init()
        {
            listCarSlot = carSlotPar.GetComponentsInChildren<CarSlot>().ToList();

            //Load env
            LoadEnvModel();
            distanceX = passengerMiddlePoint.localPosition.x - passengerStartPoint.localPosition.x;
            distanceY = passengerEndPoint.localPosition.z - passengerMiddlePoint.localPosition.z;

            xPerPass = distanceX / countPassMiddle;
            yPerPass = distanceY / (countPassShow - countPassMiddle);

            for (int i = 0; i < countPassShow; i++)
            {
                if (i <= countPassMiddle)
                {
                    listPassengerNode.Add(new Vector3(xPerPass * i + passengerStartPoint.localPosition.x, 0, passengerMiddlePoint.localPosition.z));
                }
                else
                {
                    listPassengerNode.Add(new Vector3(xPerPass * countPassMiddle + passengerStartPoint.localPosition.x, 0, yPerPass * (i - countPassMiddle) + passengerMiddlePoint.localPosition.z));
                }
            }
            levelNumPass = CountAllPass();
            UpdateDisplayCountPass(levelNumPass + countPassBorn);
            yield return StartCoroutine(ShowPassenger(true, true));
        }
        CountPassengers countPassengers;
        internal void UpdateDisplayCountPass(int val)
        {
            if (countPassengers != null)
                countPassengers.UpdateNumText(val);
        }
        [ContextMenu("Load env model")]
        void LoadEnvModel()
        {
            GameObject envModel = SkinConfigService.GetEnvSkinModel(skinConfig, Profile.CurrentSkin);
            GameObject newMap = Instantiate(envModel, envParent);
            countPassengers = newMap.GetComponent<CountPassengers>();
        }
        public PointStop GetPointStopFree()
        {
            var p = listPointStop.FirstOrDefault(x => x.IsUnlock && x.IsBusy == false);
            return p;
        }

        public bool CheckFullBusy()
        {
            var l1 = listPointStop.FindAll(x => x.IsUnlock);
            return l1.All(x => x.IsBusy);
        }
        internal Passanger GetRndInQueuePassenger()
        {
            List<Passanger> passengerList = new List<Passanger>(_passengerQueue);

            // Pick a random index
            int randomIndex = UnityEngine.Random.Range(0, passengerList.Count);

            // Return the randomly selected passenger
            if (randomIndex < passengerList.Count)
                return passengerList[randomIndex];
            else
                return null;
        }
        public List<Vector3> FindShortestPath(Vector3 start, PointStop p, CarController carController = null)
        {
            Vector3 stopPos = (carController != null) ? p.VStop(carController.StopOffset()) : p.VStop();
            var node = new List<Vector3>
            {
                start
            };
            if (CanMoveDirectlyToStop(start, p, listPointRoad))
            {
                node.Add(p.RStop());
                node.Add(stopPos);
                return node;
            }

            var l = CheckNearestPoint(start, listPointRoad, p);
            node.AddRange(l);
            node.Add(p.RStop());
            node.Add(stopPos);
            return node;
        }

        private bool IsClosestToRStop(Vector3 start, PointStop p, List<PointRoad> pointRoads)
        {
            var distanceToRStop = Vector3.Distance(start, p.RStop());

            return pointRoads.Select(point => Vector3.Distance(start, point.transform.position))
                .All(distanceToPoint => !(distanceToRStop >= distanceToPoint));
        }
        private bool CanMoveDirectlyToStop(Vector3 start, PointStop p, List<PointRoad> pointRoads)
        {
            if (IsClosestToRStop(start, p, pointRoads) || (Mathf.Abs(start.z - p.RStop().z) < 0.5f))
                return true;
            return false;
        }

        int totalPass = 0;
        private IEnumerator ShowPassenger(bool walk = true, bool delaycall = false, Action a = null)
        {
            UpdateDisplayCountPass(CountAllPass() + countPassBorn);
            if (listColorAndCount.All(x => x.count <= 0))
            {
                // win
                if (_passengerQueue.Count <= 0)
                {
                    DOVirtual.DelayedCall(1f, () => Gameplay.Instance.Win());
                }
                yield break;
            }

            foreach (var t in listColorAndCount)
            {
                if (t.count <= 0) continue;
                var c = t.count;
                if (countPassBorn >= countPassShow)
                    break;
                for (int i = 0; i < c; i++)
                {
                    if (countPassBorn >= countPassShow)
                        break;
                    var p = Instantiate(passengerPref, passengerParent);
                    p.transform.localPosition = listPassengerNode[listPassengerNode.Count - 1];
                    p.gameObject.name = "Passenger_" + totalPass;
                    p.curIndex = countPassShow - 1;
                    p.InitColor(t.colorCar);

                    if (walk)
                        PassengerMove(p, p.curIndex, countPassBorn);
                    else
                    {
                        p.transform.localPosition = listPassengerNode[countPassBorn];
                        p.curIndex = countPassBorn;
                        if (countPassBorn <= countPassMiddle)
                            p.transform.localEulerAngles = new Vector3(0, -90.0f, 0);
                    }

                    _passengerQueue.Enqueue(p);
                    countPassBorn++;
                    totalPass++;
                    t.count--;

                    if (delaycall)
                        yield return new WaitForSeconds(1.0f / GlobalDefine.passengerWalkDuration);
                }
            }
            a?.Invoke();
            //Debug.Log("num passenger in list: " + CountNumPass(listColorAndCount));
        }
        internal void CreateSittingPass(CarController c)
        {
            c.PassSeatAddCount();
            var pos = c.GetFreeSlot();

            Passanger p = Instantiate(passengerPref, passengerParent);
            p.InitColor(c.ColorCar);
            p.transform.parent = pos.transform;

            var transform1 = p.transform;
            transform1.localPosition = Vector3.zero;
            transform1.localEulerAngles = Vector3.zero;
            transform1.localScale = Vector3.one * GlobalDefine.passengerScaleWhenSeat;
            pos.PassGoPos(p);
            pos.passSitted = true;
            p.passMesh.SitAnim();

            if (c.CheckFullCar())
                c.CarFullAndGo(true);
        }
        internal IEnumerator RefreshPassenger()
        {
            //List<Passanger> passangers = FindObjectsOfType<Passanger>().ToList();
            foreach (Passanger passanger in _passengerQueue)
            {
                StartCoroutine(passanger.ShuffleColorPassenger());
            }
            yield return new WaitForSeconds(GlobalDefine.shuffleDuration * 10.0f);
            foreach (Passanger passanger in _passengerQueue)
            {
                Destroy(passanger.gameObject);
            }

            _passengerQueue.Clear();
            countPassBorn = 0;

            Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Normal);
            StartCoroutine(ShowPassenger(false, false));
            yield return StartCoroutine(PassToCar());

            SaveGameProgress();
        }

        internal int inTransit = 0;
        internal int carMoving = 0;
        internal int numPassToCar = 0;
        internal int passToCarCall = 0;

        public IEnumerator PassToCar()
        {
            while (passToCarCall > 0)
                yield return null;
            passToCarCall++;
            while (true)
            {
                if (_passengerQueue.Count == 0)
                {
                    inTransit--;
                    passToCarCall--;
                    yield break;
                }
                var p = _passengerQueue.Peek();
                while (p.curIndex > 0)
                {
                    //Debug.Log(p.gameObject.name);
                    AllPassMove();
                    yield return StartCoroutine(ShowPassenger());
                    yield return null;
                }
                var cc = CheckColor(p.ColorCar);
                if (cc == null)
                {
                    //Debug.Log("cc = null: " + p.gameObject.name);
                    inTransit--;
                    passToCarCall--;
                    StartCoroutine(CheckLose());
                    yield break;
                }

                while (p.curIndex > 0)
                {
                    //Debug.Log(p.gameObject.name);
                    AllPassMove();
                    yield return StartCoroutine(ShowPassenger());
                    yield return null;
                }

                p.GotoCar(cc);
                countPassBorn--;
                var full = cc.CheckFullCar();
                _passengerQueue.Dequeue();

                if (full)
                {
                    cc.CarFullAndGo();
                    StartCoroutine(ShowPassenger());
                    //yield return null;
                    continue;
                }
                /*
				DOVirtual.DelayedCall(GlobalDefine.passengerWalkDuration * 0.01f, () =>
				{
					AllPassMove();

					StartCoroutine(PassToCar());
					StartCoroutine(ShowPassenger());
				});*/
                yield return new WaitForSeconds(GlobalDefine.passengerWalkDuration * 0.01f);
                AllPassMove();

                StartCoroutine(PassToCar());
                StartCoroutine(ShowPassenger());

                passToCarCall--;
                break;
            }
        }
        void AllPassMove()
        {
            var index = 0;
            foreach (var passenger in _passengerQueue)
            {
                PassengerMove(passenger, passenger.curIndex, index);
                index++;
            }
        }
        void PassengerMove(Passanger passenger, int curIndex, int targetIndex)
        {
            if (curIndex == targetIndex)
                return;

            passenger.curIndex = targetIndex;
            List<Vector3> listMoves = listPassengerNode.GetRange(targetIndex, curIndex - targetIndex);
            listMoves.Reverse();
            PassengerMove(passenger, listMoves);
        }
        void PassengerMove(Passanger passenger, List<Vector3> listMoves)
        {
            passenger.Walk(listMoves);
        }
        private CarController CheckColor(ColorCar c)
        {
            List<CarController> sameColorCars = new List<CarController>();
            foreach (var s in listPointStop)
            {
                var currentColor = s.ColorCarCurrent();
                if (currentColor == c)
                    sameColorCars.Add(s.CarController);
            }

            if (sameColorCars.Count > 0)
            {
                return Gameplay.GetMostPriorityCar(sameColorCars);
            }
            return null;
        }
        private List<Vector3> CheckNearestPoint(Vector3 start, List<PointRoad> pointRoads, PointStop p)
        {
            List<Vector3> listTf = new();
            if (pointRoads == null || pointRoads.Count < 2)
            {
                Debug.LogError("List of point roads is null or has less than 2 points.");
                return null;
            }

            List<(float distance, PointRoad point)> distances = new();
            foreach (var point in pointRoads)
            {
                float distance = Vector3.Distance(start, point.transform.position);
                point.weight = distance;
                distances.Add((distance, point));
            }

            distances.Sort((a, b) => a.distance.CompareTo(b.distance));

            var nearestPoint1 = distances[0].point;
            var nearestPoint2 = distances[1].point;
            if (nearestPoint1.listConnect.Count <= 1)
            {
                listTf.Add(nearestPoint1.tf.position);
                return listTf;
            }

            if (nearestPoint2.listConnect.Count <= 1)
            {
                listTf.Add(nearestPoint2.tf.position);
                return listTf;
            }

            var p1 = nearestPoint1.listConnect.FirstOrDefault(x => x.listConnect.Count == 1);
            if (p1 != null)
            {
                listTf.Add(nearestPoint1.tf.position);
                listTf.Add(p1.tf.position);
                return listTf;
            }
            var p2 = nearestPoint2.listConnect.FirstOrDefault(x => x.listConnect.Count == 1);
            if (p2 == null) return null;
            listTf.Add(nearestPoint2.tf.position);
            listTf.Add(p2.tf.position);
            return listTf;
        }
        [ContextMenu("GenPassengerEasy")]
        public void GenEasyLevelPass()
        {
            GenLevelPass(LevelDifficult.Easy);
        }
        [ContextMenu("GenPassengerNormal")]
        public void GenNormalLevelPass()
        {
            GenLevelPass(LevelDifficult.Normal);
        }
        [ContextMenu("GenPassengerHard")]
        public void GenHardLevelPass()
        {
            GenLevelPass(LevelDifficult.Hard);
        }
        [ContextMenu("GenPassengerHell")]
        public void GenHellLevelPass()
        {
            GenLevelPass(LevelDifficult.Hell);
        }
        public void GenLevelPass(LevelDifficult diff)//with input
        {
            LevelDifficultConfig levelDifficultConfig = LevelConfig.GetDiffConfig(diff, levelConfig);
            AutoGenPassenger(levelDifficultConfig.groupSize, levelDifficultConfig.minSameColorInRow, levelDifficultConfig.maxSameColorInRow);
        }
        [ContextMenu("AutoSortBus")]
        public void AutoSortBus()
        {
            List<CarController> listCars = new List<CarController>();
            List<CarController> sortedCars = new List<CarController>();
            List<int> pos = new List<int>();

            int i = 0;
            foreach (Transform child in carSlotPar)
            {
                CarController carController = child.GetComponentInChildren<CarController>();
                if (carController == null)
                {
                    i++;
                    continue;
                }

                listCars.Add(carController);
                pos.Add(i);
                i++;
            }
            while(listCars.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, listCars.Count);
                if (!listCars[index].CarCanMove())
                    continue;

                sortedCars.Add(listCars[index]);
                listCars.RemoveAt(index);
            }

            int j = 0;
            foreach(CarController car in sortedCars)
            {
                car.transform.SetSiblingIndex(pos[j]);
                j++;
            }
        }
        public void AutoGenPassenger(int groupSize, int minSameColorPassengerInRow, int maxSameColorPassengerInRow)
        {
            // Tạo passenger mặc định theo từng xe trong list
            listColorAndCount.Clear();

            // Danh sách chứa Color và số lượng slot
            List<ColorAndCount> linearList = new List<ColorAndCount>();
            foreach (Transform child in carSlotPar)
            {
                CarController carController = child.GetComponentInChildren<CarController>();
                Basement basement = child.GetComponent<Basement>();

                if (carController != null && carController.curCarBelt == null)
                {
                    ColorAndCount colorAndCount = new ColorAndCount(carController.ColorCar, carController.GetSlotCount());
                    linearList.Add(colorAndCount);
                }
                else if (basement != null)
                {
                    Dictionary<ColorCar, int> colorAndCounts = new Dictionary<ColorCar, int>();
                    foreach (HiddenCar hiddenCar in basement.hiddenCars)
                    {
                        ColorAndCount colorAndCount = new ColorAndCount(hiddenCar.color, carNumSlotConfig.carSlotConfig[(int)hiddenCar.type].slots);
                        linearList.Add(colorAndCount);
                    }
                }
            }

            foreach (CarBeltData carBeltData in carBeltDatas)
            {
                Dictionary<ColorCar, int> colorAndCounts = new Dictionary<ColorCar, int>();
                foreach (HiddenCar hiddenCar in carBeltData.hiddenCars)
                {
                    ColorAndCount colorAndCount = new ColorAndCount(hiddenCar.color, carNumSlotConfig.carSlotConfig[(int)hiddenCar.type].slots);
                    linearList.Add(colorAndCount);
                }
            }

            // Shuffle danh sách theo các nhóm groupSize phần tử
            List<ColorAndCount> finalList = ShuffleLinearList(groupSize, minSameColorPassengerInRow, maxSameColorPassengerInRow, linearList);
            // Gán danh sách cuối cùng vào listColorAndCount
            listColorAndCount = new List<ColorAndCount>(finalList);
        }
        List<ColorAndCount> ShuffleLinearList(int groupSize, int minSameColorPassengerInRow, int maxSameColorPassengerInRow, List<ColorAndCount> linearList)
        {
            List<ColorAndCount> result = new List<ColorAndCount>();
            for (int i = 0; i < linearList.Count; i += groupSize)
            {
                // Lấy 4 phần tử hoặc ít hơn nếu nhóm cuối có ít hơn 4 phần tử
                List<ColorAndCount> subList = linearList.Skip(i).Take(groupSize).ToList();

                // Shuffle danh sách con
                List<ColorAndCount> shuffledSubList = ShuffleListPassenger(subList, minSameColorPassengerInRow, maxSameColorPassengerInRow);

                // Gộp các danh sách lại với nhau
                result.AddRange(shuffledSubList);
            }

            return result;
        }
        public string levelId;
        [ContextMenu("InitLevel")]
        public void InitLevel()
        {
            //Lưu vào file
            LevelData levelData = new LevelData();
            levelData.levelId = levelId;

            int i = 0;
            foreach (Transform child in carSlotPar)
            {
                CarSlot carSlot = child.GetComponent<CarSlot>();
                Basement basement = child.GetComponent<Basement>();

                if (carSlot != null)
                {
                    CarData carData = new CarData();
                    carData.Id = i.ToString();
                    carData.posX = carSlot.transform.position.x;
                    carData.posZ = carSlot.transform.position.z;
                    carData.rotation = carSlot.transform.eulerAngles.y;

                    carData.carType = (int)carSlot.carController.carType;
                    carData.color = (int)carSlot.carController.ColorCar;
                    carData.isHiddenColor = carSlot.carController.HiddenColorCar;

                    levelData.cars.Add(carData);
                }
                else if (basement != null)
                {
                    BasementData basementData = new BasementData();
                    basementData.Id = i.ToString();
                    basementData.posX = basement.transform.position.x;
                    basementData.posZ = basement.transform.position.z;
                    basementData.rotation = basement.transform.eulerAngles.y;

                    basementData.hiddenCars = new List<HiddenCar>(basement.hiddenCars);

                    levelData.basements.Add(basementData);
                }
                i++;
            }
            levelData.passengers = new List<ColorAndCount>(listColorAndCount);
            levelData.carBelts = new List<CarBeltData>(carBeltDatas);

            ExportLevelData(levelData);
        }
        [ContextMenu("Clear")]
        public void ClearLevel() //editor
        {
            listColorAndCount = new List<ColorAndCount>();
            carBeltDatas = new List<CarBeltData>();
            while (carSlotPar.childCount > 0)
            {
                foreach (Transform child in carSlotPar)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            foreach (Transform child in envParent)
            {
                DestroyImmediate(child.gameObject);
            }
            carSlotPar.transform.localPosition = Vector3.zero;
        }
        [ContextMenu("LoadLevel")]
        internal void ReLoadLevel()
        {
            LoadLevelInCreator(levelId);
        }
        internal List<int> shuffleColors = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
        internal List<int> notRecommendColors = new List<int>() { 8, 9 };
        List<CarBelt> carBeltsInLevel = new List<CarBelt>();
        internal IEnumerator LoadLevel(string id, string path = null, bool isShuffleColor = true, bool editor = false)
        {
            shuffleColors = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
            notRecommendColors = new List<int>() { 8, 9 };
            if (isShuffleColor)
            {
                shuffleColors = shuffleColors.OrderBy(x => UnityEngine.Random.value).ToList();
                notRecommendColors = notRecommendColors.OrderBy(x => UnityEngine.Random.value).ToList();
            }
            shuffleColors.AddRange(notRecommendColors);

            LevelData levelData = GetLevelDataFromJson(id, path);

            List<Basement> basements = new List<Basement>();
            carBeltDatas = new List<CarBeltData>(levelData.carBelts);
            for (int j = 0; j < levelData.numSlotUnlock; j++)
            {
                PointStop pointStop = GetNearestLockedPointStop();
                if (pointStop != null)
                    pointStop.UnlockPointStop(false, false);
            }
            if (levelData.unlockVipSlot)
                Gameplay.Instance.UseVipSlot(false);
            foreach (BasementData basementData in levelData.basements)
            {
                Basement newBasement = Instantiate(basementPrefab, carSlotPar);
                newBasement.gameObject.name = "Basement_" + basementData.Id;
                newBasement.transform.position = new Vector3(basementData.posX, GlobalDefine.carPosY, basementData.posZ);
                newBasement.transform.rotation = Quaternion.Euler(new Vector3(0.0f, basementData.rotation, 0.0f));
                newBasement.hiddenCars = new List<HiddenCar>(basementData.hiddenCars);

                basements.Add(newBasement);
            }
            foreach (CarData carData in levelData.cars)
            {
                CarSlot newCarSlot = Instantiate(baseCarSlot, carSlotPar);
                newCarSlot.gameObject.name = "Car_" + carData.Id;
                newCarSlot.transform.position = new Vector3(carData.posX, GlobalDefine.carPosY, carData.posZ);
                newCarSlot.transform.rotation = Quaternion.Euler(new Vector3(0.0f, carData.rotation, 0.0f));
                newCarSlot.carController.carType = (CarType)carData.carType;
                newCarSlot.carController.ColorCar = (ColorCar)carData.color;
                newCarSlot.carController.HiddenColorCar = carData.isHiddenColor;
                if (!string.IsNullOrEmpty(carData.curBasement))
                {
                    newCarSlot.curBasement = GameObject.Find(carData.curBasement).GetComponent<Basement>();
                }

                if (!editor)
                {
                    newCarSlot.Init();

                    if (carData.countPass >= 0)
                    {
                        newCarSlot.carController.CarMove(false, true);
                        if (carData.countPass > 0)
                        {
                            for (int j = 1; j <= carData.countPass; j++)
                            {
                                CreateSittingPass(newCarSlot.carController);
                            }
                        }
                    }
                }

                if (!Config.levelLoaded)
                    yield return null;
            }
            foreach (Basement newBasement in basements)
            {
                string name = newBasement.gameObject.name;
                string index = name.Substring(name.IndexOf("_") + 1);
                newBasement.transform.SetSiblingIndex(int.Parse(index));

                if (!editor)
                    newBasement.Init(!string.IsNullOrEmpty(path));
                if (!Config.levelLoaded)
                    yield return null;
            }
            int i = 0;
            foreach (CarBeltData carBeltData in carBeltDatas)
            {
                CarBelt newCarBelt = Instantiate(carbeltPrefab, carSlotPar);
                newCarBelt.gameObject.name = "CarBelt_" + i.ToString();
                newCarBelt.transform.position = new Vector3(-1.7f, 0.0f, carBeltData.posZ);
                newCarBelt.hiddenCars = new List<HiddenCar>(carBeltData.hiddenCars);
                if (!editor)
                    newCarBelt.Init();
                i++;
                if (!Config.levelLoaded)
                    yield return null;
                carBeltsInLevel.Add(newCarBelt);
            }
            listColorAndCount = new List<ColorAndCount>(levelData.passengers);
            Gameplay.Instance.LevelTimePlayed = levelData.playedTime;
            Gameplay.Instance.UpdateTimeTxt();
        }
        internal void StopRollCarBelts()
        {
            foreach (CarBelt carBelt in carBeltsInLevel)
            {
                carBelt.StopRoll();
            }
        }
        internal void ContinueRollCarBelts()
        {
            foreach (CarBelt carBelt in carBeltsInLevel)
            {
                carBelt.ContinueRoll();
            }
        }
        [ContextMenu("FixColorIndex")]
        public void FixColorIndexInFile()
        {
            for (int i = 1; i <= 60; i++)
            {
                string levelId = i.ToString("D3");
                LevelData levelData = GetLevelDataFromJson(levelId);

                foreach (CarData carData in levelData.cars)
                {
                    if (carData.color == 9)
                        carData.color = 7;
                    else if (carData.color == 7)
                        carData.color = 9;
                }
                foreach (ColorAndCount colorAndCount in levelData.passengers)
                {
                    if (colorAndCount.colorCar == (ColorCar)9)
                        colorAndCount.colorCar = (ColorCar)7;
                    else if (colorAndCount.colorCar == (ColorCar)7)
                        colorAndCount.colorCar = (ColorCar)9;
                }

                ExportLevelData(levelData);
            }
        }
        [ContextMenu("SubFixColorIndex")]
        public void FixBasementColorIndexInFile()
        {
            for (int i = 1; i <= 60; i++)
            {
                string levelId = i.ToString("D3");
                LevelData levelData = GetLevelDataFromJson(levelId);

                foreach (BasementData basementData in levelData.basements)
                {
                    foreach (HiddenCar hiddenCar in basementData.hiddenCars)
                    {
                        if (hiddenCar.color == (ColorCar)9)
                            hiddenCar.color = (ColorCar)7;
                        else if (hiddenCar.color == (ColorCar)7)
                            hiddenCar.color = (ColorCar)9;
                    }
                }
                foreach (CarBeltData carBeltData in levelData.carBelts)
                {
                    foreach (HiddenCar hiddenCar in carBeltData.hiddenCars)
                    {
                        if (hiddenCar.color == (ColorCar)9)
                            hiddenCar.color = (ColorCar)7;
                        else if (hiddenCar.color == (ColorCar)7)
                            hiddenCar.color = (ColorCar)9;
                    }
                }

                ExportLevelData(levelData);
            }
        }
        [ContextMenu("FixId")]
        public void FixAllIdInFile()
        {
            for (int i = 1; i <= 70; i++)
            {
                string levelId = i.ToString("D3");
                LevelData levelData = GetLevelDataFromJson(levelId);

                int j = 0;
                foreach (CarData carData in levelData.cars)
                {
                    carData.Id = j.ToString();
                    j++;
                }
                int k = 0;
                foreach (BasementData basementData in levelData.basements)
                {
                    basementData.Id = (j + k).ToString();
                    k++;
                }

                ExportLevelData(levelData);
            }
        }
        [ContextMenu("ReGenPassAllLevel")]
        public void RegenPassAllInFile()
        {
            for (int i = 1; i <= 70; i++)
            {
                string levelId = i.ToString("D3");
                this.levelId = levelId;

                LoadLevelInCreator(levelId);
                LevelDifficult dif = levelConfig.configs[i - 1].diff;

                if (dif == LevelDifficult.Easy)
                    GenEasyLevelPass();
                else if (dif == LevelDifficult.Normal)
                    GenNormalLevelPass();
                else if (dif == LevelDifficult.Hard)
                    GenHardLevelPass();
                else if (dif == LevelDifficult.Hell)
                    GenHellLevelPass();

                InitLevel();
                ClearLevel();
            }
        }
        internal void LoadLevelInCreator(string id)
        {
            LevelData levelData = GetLevelDataFromJson(id);
            carBeltDatas = new List<CarBeltData>(levelData.carBelts);
            foreach (CarData carData in levelData.cars)
            {
                CarInLevelCreator carInLevelCreator = new CarInLevelCreator();
                if (carData.carType == (int)CarType.Small)
                {
                    carInLevelCreator = Car4;
                }
                else if (carData.carType == (int)CarType.Medium)
                {
                    carInLevelCreator = Car6;
                }
                else if (carData.carType == (int)CarType.Big)
                {
                    carInLevelCreator = Car8;
                }

                CarInLevelCreator newCarSlot = Instantiate(carInLevelCreator, carSlotPar);
                newCarSlot.gameObject.name = "Car_" + carData.Id;
                newCarSlot.transform.position = new Vector3(carData.posX, GlobalDefine.carPosY, carData.posZ);
                newCarSlot.transform.rotation = Quaternion.Euler(new Vector3(0.0f, carData.rotation, 0.0f));
                newCarSlot.color = (ColorCar)carData.color;
                newCarSlot.isHidden = carData.isHiddenColor;

                newCarSlot.SetTestColor();
            }
            foreach (BasementData basementData in levelData.basements)
            {
                Basement newBasement = Instantiate(basementPrefab, carSlotPar);
                newBasement.transform.SetSiblingIndex(int.Parse(basementData.Id));

                newBasement.gameObject.name = "Basement_" + basementData.Id;
                newBasement.transform.position = new Vector3(basementData.posX, GlobalDefine.carPosY, basementData.posZ);
                newBasement.transform.rotation = Quaternion.Euler(new Vector3(0.0f, basementData.rotation, 0.0f));
                newBasement.hiddenCars = new List<HiddenCar>(basementData.hiddenCars);
            }
            listColorAndCount = new List<ColorAndCount>(levelData.passengers);
        }
        internal void ExportLevelData(LevelData levelData, string path = null)
        {
            string data = JsonUtility.ToJson(levelData, true);
            string filePath = Path.Combine(Application.dataPath, "Resources/") + GlobalDefine.LevelDataPath + "levelData_" + levelData.levelId + ".json";
            if (!string.IsNullOrEmpty(path))
                filePath = path;

            File.WriteAllText(filePath, data);
            Debug.Log("write data to: " + filePath);
        }
        internal void DisableProgress()
        {
            string path = Application.persistentDataPath + GlobalDefine.LevelProgressDataPath;
            if (string.IsNullOrEmpty(path))
                return;

            string data = File.ReadAllText(path);
            LevelData levelProgressData = JsonUtility.FromJson<LevelData>(data);
            levelProgressData.levelId = "-1";//đổi id để không có level nào trùng id này -> tắt load tiến trình

            ExportLevelData(levelProgressData, path);
        }
        internal static LevelData GetLevelDataFromJson(string id, string path = null)
        {
            string filePath = GlobalDefine.LevelDataPath + "levelData_" + id;
            if (!string.IsNullOrEmpty(path))
            {
                filePath = path;
                string data = File.ReadAllText(filePath);
                LevelData levelProgressData = JsonUtility.FromJson<LevelData>(data);

                return levelProgressData;
            }

            TextAsset jsonData = Resources.Load<TextAsset>(filePath);
            if (jsonData != null)
            {
                LevelData levelData = JsonUtility.FromJson<LevelData>(jsonData.text);
                Debug.Log("Level data loaded from Resources: " + filePath);
                return levelData;
            }
            else
            {
                Debug.LogError("File not found in Resources: " + filePath);
                return null;
            }
        }
        internal IEnumerator CheckLose()
        {
            //yield return new WaitForSeconds(0.5f);
            if (inTransit > 0 || carMoving > 0 || passToCarCall > 0)
                yield break;

            if (Gameplay.Instance.WasLost())
                yield break;
            var l = listPointStop.FindAll(x => x.IsUnlock);
            if (l.All(x => x.IsBusy))
            {
                Gameplay.Instance.Lose();
            }
        }
        internal PointStop GetNearestLockedPointStop()
        {
            foreach (PointStop pointStop in listPointStop)
            {
                if (pointStop.IsUnlock)
                    continue;
                return pointStop;
            }

            return null;
        }
        internal PointStop GetRandomHavePassPointStop()
        {
            List<PointStop> busyPoints = new List<PointStop>();
            foreach (PointStop pointStop in listPointStop)
            {
                if (!pointStop.IsBusy || pointStop.CarController == null || pointStop.CarController.CountPassenger <= 0)
                    continue;
                busyPoints.Add(pointStop);
            }

            if (busyPoints.Count > 0)
                return busyPoints[UnityEngine.Random.Range(0, busyPoints.Count)];
            return null;
        }
        #region tool
        [ContextMenu("Use tool sort")]
        public void SortTool()
        {
            Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Pause);
            List<ColorAndCount> front = new List<ColorAndCount>();
            List<ColorAndCount> tail = new List<ColorAndCount>();
            listColorAndCount.Clear();

            //add list
            foreach (Transform child in carSlotPar)
            {
                CarSlot carSlot = child.GetComponent<CarSlot>();
                Basement basement = child.GetComponent<Basement>();
                CarBelt carBelt = child.GetComponent<CarBelt>();

                if (carSlot != null)
                {
                    CarController carController = carSlot.GetComponentInChildren<CarController>();
                    if (carController != null)
                    {
                        if (!carController.gameObject.activeSelf)
                            return;
                        if (carController.IsParking())
                            front.Add(new ColorAndCount(carController.ColorCar, carController.GetNumEmptySlot()));
                        else
                            tail.Add(new ColorAndCount(carController.ColorCar, carController.GetNumEmptySlot()));
                    }
                }
                if (basement != null)
                {
                    Dictionary<ColorCar, int> colorAndCounts = new Dictionary<ColorCar, int>();
                    foreach (HiddenCar hiddenCar in basement.hiddenCars)
                    {
                        ColorAndCount colorAndCount = new ColorAndCount(hiddenCar.color, carNumSlotConfig.carSlotConfig[(int)hiddenCar.type].slots);
                        tail.Add(colorAndCount);
                    }
                }
                if (carBelt != null)
                {
                    Dictionary<ColorCar, int> colorAndCounts = new Dictionary<ColorCar, int>();
                    foreach (HiddenCar hiddenCar in carBelt.hiddenCars)
                    {
                        ColorAndCount colorAndCount = new ColorAndCount(hiddenCar.color, carNumSlotConfig.carSlotConfig[(int)hiddenCar.type].slots);
                        tail.Add(colorAndCount);
                    }
                }
            }

            //handle tail
            tail = ShuffleLinearList(4, GlobalDefine.minSameColorPassengerInRow, GlobalDefine.maxSameColorPassengerInRow, tail);
            //final merge
            listColorAndCount.AddRange(front);
            listColorAndCount.AddRange(tail);

            StartCoroutine(RefreshPassenger());
        }
        [ContextMenu("CountNumPassenger")]
        public void TestCountPass()
        {
            Debug.Log(CountNumPass(listColorAndCount));
        }
        public static int CountNumPass(List<ColorAndCount> colorAndCounts)//for testing
        {
            int num = 0;
            int i = 0;
            foreach (ColorAndCount colorAndCount in colorAndCounts)
            {
                if (colorAndCount.count < 0)
                    throw new Exception("có thằng count < 0: " + i);
                num += colorAndCount.count;
                i++;
            }
            return num;
        }
        public int CountAllPass()
        {
            return CountNumPass(listColorAndCount);
        }
        List<ColorAndCount> ShuffleListPassenger(List<ColorAndCount> colorAndCounts, int minSameColor, int maxSameColor)
        {
            List<ColorAndCount> result = new List<ColorAndCount>();
            foreach (ColorAndCount colorAndCount in colorAndCounts)
            {
                int maxRowCount = (maxSameColor < colorAndCount.count) ? maxSameColor : colorAndCount.count;
                List<int> nums = new List<int>();

                if (colorAndCount.count > minSameColor)
                    nums = GeneralCalculate.GenerateRandomNumbersWithSum(colorAndCount.count, minSameColor, maxRowCount);
                else
                    nums.Add(colorAndCounts.Count);

                foreach (int num in nums)
                {
                    ColorAndCount newColorAndCount = new ColorAndCount(colorAndCount.colorCar, num);
                    result.Add(newColorAndCount);
                }
            }

            result = result.OrderBy(x => UnityEngine.Random.value).ToList();
            return result;
        }
        List<ColorAndCount> MergeListPassenger(List<ColorAndCount> colorAndCounts)
        {
            Dictionary<ColorCar, int> mergedDict = new Dictionary<ColorCar, int>();
            foreach (var colorAndCount in colorAndCounts)
            {
                if (mergedDict.ContainsKey(colorAndCount.colorCar))
                {
                    mergedDict[colorAndCount.colorCar] += colorAndCount.count;
                }
                else
                {
                    mergedDict.Add(colorAndCount.colorCar, colorAndCount.count);
                }
            }

            List<ColorAndCount> mergedList = new List<ColorAndCount>();
            foreach (var kvp in mergedDict)
            {
                mergedList.Add(new ColorAndCount(kvp.Key, kvp.Value));
            }

            return mergedList;
        }
        [ContextMenu("Use tool shuffle")]
        public void ShuffleCars()
        {
            Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Pause);
            Dictionary<CarType, List<CarController>> carDic = new Dictionary<CarType, List<CarController>>();

            foreach (CarSlot carSlot in listCarSlot)
            {
                if (!carSlot.gameObject.activeSelf || carSlot.isParking)
                    continue;

                if (!carDic.ContainsKey(carSlot.carController.carType))
                    carDic.Add(carSlot.carController.carType, new List<CarController>());

                carDic[carSlot.carController.carType].Add(carSlot.carController);
            }

            foreach (var kvp in carDic)
            {
                ShuffleSameTypeCar(kvp.Value);
            }
        }
        public bool CanUseShuffle()
        {
            int i = 0;
            foreach (CarSlot carSlot in listCarSlot)
            {
                if (!carSlot.gameObject.activeSelf || carSlot.isParking)
                    continue;
                i++;
            }
            if (i <= 1)
            {
                GameManager.ShowNoti("There are not enough cars on map to use Shuffle tool");
                return false;
            }

            return true;
        }
        void ShuffleSameTypeCar(List<CarController> carControllers)
        {
            //Tráo màu giữa các xe cùng loại
            List<ColorCar> colors = new List<ColorCar>();
            foreach (CarController carController in carControllers)
            {
                colors.Add(carController.ColorCar);
            }

            foreach (CarController carController in carControllers)
            {
                int rndIndex = UnityEngine.Random.Range(0, colors.Count);
                carController.ColorCar = colors[rndIndex];
                colors.RemoveAt(rndIndex);

                StartCoroutine(carController.ShuffleColorCar());
            }

            DOVirtual.DelayedCall(GlobalDefine.shuffleDuration * 10, () =>
            {
                Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Normal);
                SaveGameProgress();
            });
        }
        internal int numSlotUnlocked = 0;
        internal void UnlockVipSlot()
        {
            VipPointStop.UnlockPointStop(true, false);
            listPointStop.Insert(0, VipPointStop);
        }
        #endregion
        #region Game Progress
        internal void SaveGameProgress()
        {
            LevelData levelProgressData = new LevelData();
            levelProgressData.levelId = Profile.Level.ToString("D3");

            List<CarBelt> carBelts = carSlotPar.GetComponentsInChildren<CarBelt>().ToList();
            int j = 0;
            foreach (Transform child in carSlotPar)
            {
                CarSlot carSlot = child.GetComponent<CarSlot>();
                Basement basement = child.GetComponent<Basement>();

                if (carSlot != null)
                {
                    if (carSlot.carController.curCarBelt != null || !carSlot.gameObject.activeSelf)
                        continue;

                    CarData carData = new CarData();
                    carData.Id = j.ToString();
                    carData.posX = carSlot.transform.position.x;
                    carData.posZ = carSlot.transform.position.z;
                    carData.rotation = carSlot.transform.eulerAngles.y;

                    carData.carType = (int)carSlot.carController.carType;
                    carData.color = (int)carSlot.carController.ColorCar;
                    carData.isHiddenColor = carSlot.carController.HiddenColorCar;

                    if (carSlot.curBasement != null)
                        carData.curBasement = carSlot.curBasement.gameObject.name;
                    if (carSlot.isParking)
                        carData.countPass = carSlot.carController.CountPassenger;

                    levelProgressData.cars.Add(carData);
                }
                else if (basement != null)
                {
                    BasementData basementData = new BasementData();
                    basementData.Id = basement.gameObject.name.Substring(basement.gameObject.name.IndexOf("_") + 1); ;
                    basementData.posX = basement.transform.position.x;
                    basementData.posZ = basement.transform.position.z;
                    basementData.rotation = basement.transform.eulerAngles.y;

                    basementData.hiddenCars = new List<HiddenCar>(basement.hiddenCars);

                    levelProgressData.basements.Add(basementData);
                }
                j++;
            }

            foreach (CarBelt carBelt in carBelts)
            {
                CarBeltData carBeltData = new CarBeltData();
                carBeltData.posZ = carBelt.transform.position.z;
                carBeltData.hiddenCars = new List<HiddenCar>(carBelt.hiddenCars);

                levelProgressData.carBelts.Add(carBeltData);
            }
            levelProgressData.passengers = new List<ColorAndCount>();

            ColorCar tmpColor = ColorCar.Red;
            ColorAndCount tmpColorAndCount = new ColorAndCount(tmpColor, 0);
            int i = 0;
            foreach (Passanger passanger in _passengerQueue)
            {
                i++;
                if (passanger.ColorCar != tmpColor || i == 1)
                {
                    levelProgressData.passengers.Add(tmpColorAndCount);
                    tmpColorAndCount = new ColorAndCount(passanger.ColorCar, 1);
                    tmpColor = passanger.ColorCar;
                }
                else
                {
                    tmpColorAndCount.count++;
                    if (i == _passengerQueue.Count)
                        levelProgressData.passengers.Add(tmpColorAndCount);
                }
            }
            levelProgressData.passengers.AddRange(listColorAndCount);
            levelProgressData.passengers.RemoveAll(colorAndCount => colorAndCount.count == 0);
            levelProgressData.playedTime = Gameplay.Instance.LevelTimePlayed;
            levelProgressData.unlockVipSlot = VipPointStop.IsUnlock;
            levelProgressData.numSlotUnlock = numSlotUnlocked;

            ExportLevelData(levelProgressData, Application.persistentDataPath + GlobalDefine.LevelProgressDataPath);
        }
        internal bool CanLoadProgress()
        {
            if (Config.replayLevel)//replay level
            {
                Analytics.LogLevelStartEvent();
                Config.replayLevel = false;
                return false;
            }

            string filePath = Application.persistentDataPath + GlobalDefine.LevelProgressDataPath;
            if (!File.Exists(filePath))//not have save file yet (new level 1)
            {
                Analytics.LogLevelStartEvent();
                return false;
            }

            LevelData levelData = GetLevelDataFromJson("001", filePath);
            if (levelData == null || levelData.levelId != Profile.Level.ToString("D3"))//new level
            {
                Analytics.LogLevelStartEvent();
                return false;
            }

            return true;
        }
        #endregion
    }
}

[Serializable]
public class ColorAndCount
{
    public ColorCar colorCar;
    public int count;

    public ColorAndCount(ColorCar c, int co)
    {
        colorCar = c;
        count = co;
    }
}
[Serializable]
public class CarData
{
    public string Id;

    public float posX;
    public float posZ;

    public float rotation;

    public int carType;
    public int color;

    public bool isHiddenColor;

    public int countPass = -1;
    public string curBasement;
}
[Serializable]
public class LevelData
{
    public string levelId;
    public float playedTime = 0;
    public bool unlockVipSlot = false;

    public List<CarData> cars = new List<CarData>();
    public List<ColorAndCount> passengers = new List<ColorAndCount>();
    public List<BasementData> basements = new List<BasementData>();
    public List<CarBeltData> carBelts = new List<CarBeltData>();
    public int numSlotUnlock = 0;
}
