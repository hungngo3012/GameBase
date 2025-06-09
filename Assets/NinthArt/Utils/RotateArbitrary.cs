using UnityEngine;

namespace NinthArt
{
	internal class RotateArbitrary : MonoBehaviour
	{
		[SerializeField] private Vector3 speed;

		private void Update()
		{
			transform.localEulerAngles += speed * Time.smoothDeltaTime;
		}
	}
}