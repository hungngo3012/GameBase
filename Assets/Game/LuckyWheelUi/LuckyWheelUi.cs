using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
    internal class LuckyWheelUi : Popup
    {
        public LuckyWheelRewardConfig luckyWheelRewardConfig;

        [SerializeField] LuckyWheelRewardUiComponent wheelRewardUiComponentBasePrefab;

        [SerializeField] Button spin_On;
        [SerializeField] PopupRewardUi popupRewardUi;
        [SerializeField] Button closeBtn;
        [SerializeField] TextMeshProUGUI numSpinTxt;

        [SerializeField] Transform rewardsParent;
        [SerializeField] ParticleSystem confettiVFX;
        public override void Start()
        {
            base.Start();

            UpdateWheelState();
            spin_On.onClick.AddListener(() => OnClickSpin());
            spin_On.onClick.AddListener(() => SoundManager.PlaySfx("btn"));

            closeBtn.onClick.AddListener(() =>
            {
                SoundManager.PlaySfx("btn");
                ExecuteClose();
            });

            popupRewardUi.closeBtn.onClick.AddListener(() => popupRewardUi.CloseWithAnimation());
            if (rewardsParent.childCount <= 1)
                GenerateRewardUi();
        }
        void GenerateRewardUi()
        {
            int i = 0;
            foreach (LuckyWheelReward luckyWheelReward in luckyWheelRewardConfig.rewards)
            {
                LuckyWheelRewardUiComponent rewardUi = Instantiate(wheelRewardUiComponentBasePrefab, rewardsParent);
                rewardUi.transform.localPosition = Vector3.zero;

                RectTransform rewardUiRect = rewardUi.GetComponent<RectTransform>();

                if (rewardUiRect != null)
                {
                    rewardUiRect.eulerAngles = new Vector3(rewardUiRect.rotation.x, rewardUiRect.rotation.y, i * (360.0f / (float)luckyWheelRewardConfig.rewards.Count));
                }
                if (rewardUi != null)
                {
                    rewardUi.icon.sprite = luckyWheelReward.item.icon;
                    rewardUi.numTxt.text = luckyWheelReward.num.ToString();
                    rewardUi.gameObject.name = rewardUi.icon.name + "_" + rewardUi.numTxt.text;
                }
                i++;
            }
        }

        public void OnClickSpin()
        {
            if (CanSpin())
            {
                StartCoroutine(SpinWithRatio());
            }    
            else if(string.IsNullOrEmpty(GameManager.Notify))
                GameManager.ShowNoti("You have run out of spins");
        }
        [ContextMenu("TestSpin")]
        public void TestSpin()
        {
            StartCoroutine(SpinWithRatio());
        }
        IEnumerator SpinWithRatio()
        {
            int selectedRewardIndex = RandomSelectIndex();
            float offSet = Random.Range(-20.0f, 20.0f);
            float duration = 4.5f;

            if (selectedRewardIndex == -1)
                yield break;

            spin_On.gameObject.SetActive(false);
            closeBtn.gameObject.SetActive(false);

            float zRotationTarget = -((float)selectedRewardIndex / (float)luckyWheelRewardConfig.rewards.Count) * 360.0f;
            float animRotation = zRotationTarget - 360.0f * Random.Range(8, 15) + offSet;
            Sequence spinSequence = DOTween.Sequence();
            spinSequence.Append(rewardsParent.DORotate(new Vector3(0, 0, animRotation), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuad));

            SoundManager.PlaySfx("spin");
            yield return spinSequence.Play().WaitForCompletion();

            ItemManager.AddItem(luckyWheelRewardConfig.rewards[selectedRewardIndex].item, luckyWheelRewardConfig.rewards[selectedRewardIndex].num);

            //confetti
            confettiVFX.Play();
            SoundManager.PlaySfx("confetti");

            popupRewardUi.gameObject.SetActive(true);
            popupRewardUi.icon.sprite = luckyWheelRewardConfig.rewards[selectedRewardIndex].item.icon;
            //popupRewardUi.icon.SetNativeSize();
            popupRewardUi.numTxt.text = luckyWheelRewardConfig.rewards[selectedRewardIndex].num.ToString();
            Profile.LuckyWheelProgress++;

            SoundManager.PlaySfx("reward");
            UpdateWheelState();
            spin_On.gameObject.SetActive(true);
            closeBtn.gameObject.SetActive(true);
        }
        int RandomSelectIndex()
        {
            float rnd = Random.Range(0.0f, 1.0f);
            float sumRatio = 0.0f;

            if (luckyWheelRewardConfig.rewards.Count <= 0)
                return -1;

            int i = 0;
            foreach (LuckyWheelReward luckyWheelReward in luckyWheelRewardConfig.rewards)
            {
                sumRatio += luckyWheelReward.ratio;

                if (sumRatio >= rnd)
                {
                    return i;
                }
                i++;
            }

            return (luckyWheelRewardConfig.rewards.Count - 1);
        }

        void UpdateWheelState()
        {
            numSpinTxt.text = GetProgText();
        }

        public static bool CanSpin()
        {
            return (Profile.LuckyWheelProgress < GlobalDefine.numDailySpin);
        }
        public static string GetProgText()
        {
            if (CanSpin())
                return (GlobalDefine.numDailySpin - Profile.LuckyWheelProgress).ToString() + "/" + GlobalDefine.numDailySpin.ToString();
            else
                return ("0/" + GlobalDefine.numDailySpin.ToString());
        }
        public static float GetProgVal()
        {
            if (CanSpin())
            {
                return 1.0f;
            }
            else
            {
                return (float)Profile.LuckyWheelProgress / (float)GlobalDefine.numDailySpin;
            }
        }
        void ExecuteClose()
        {
            //Gameplay.Instance.ChangePlayingState(Gameplay.PlayingState.Normal);
            SceneManager.ClosePopup();
        }
    }
}
