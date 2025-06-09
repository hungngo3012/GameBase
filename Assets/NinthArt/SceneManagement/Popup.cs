
using UnityEngine;

namespace NinthArt
{
	internal class Popup : SceneBase
	{
		[SerializeField] private new PopupAnimation animation;
		public bool usePopupShield = true;

		public virtual void Start()
		{
			if (usePopupShield)
				SceneManager.OpenSceneShield();
		}
		internal override void AnimateIn()
		{
			if (animation)
			{
				animation.AnimateIn();
			}
			else
			{
				SceneManager.OnSceneAnimatedIn(this);
			}
		}

		internal override void AnimateOut()
		{
			if (animation)
			{
				animation.AnimateOut();
			}
			else
			{
				SceneManager.OnSceneAnimatedOut(this);
			}
		}
	}
}