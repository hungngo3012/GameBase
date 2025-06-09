
using UnityEngine;

namespace NinthArt
{
	internal class PopupAnimation : MonoBehaviour
	{
		[SerializeField] protected Popup popup;
		public virtual void AnimateIn()
		{
			SceneManager.OnSceneAnimatedIn(popup);
		}

		public virtual void AnimateOut()
		{
			SceneManager.OnSceneAnimatedOut(popup);
		}
	}
}