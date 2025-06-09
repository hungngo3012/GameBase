using System;
using UnityEngine;
using UnityEngine.UI;

namespace NinthArt
{
	[RequireComponent(typeof(Button))]
	internal class ToggleButton : MonoBehaviour
	{
		[SerializeField] private Button button;
		[SerializeField] private Image image;

		[SerializeField] private Sprite onSprite;
		[SerializeField] private Sprite offSprite;

		private bool _active;
		private Action<bool> _toggle;
		internal void Init(bool active, Action<bool> toggle)
		{
			_toggle = toggle;
			UpdateState(active);
			button.onClick.AddListener(() =>
			{
				SoundManager.PlaySfx("BtnClick");
				UpdateState(!_active);
			});
		}

		private void UpdateState(bool active)
		{
			_active = active;
			_toggle?.Invoke(active);
			image.sprite = active ? onSprite : offSprite;
		}
	}
}


