using System.Collections;
using UnityEngine;

namespace BS.Manager.Cameras
{
	public class CameraManager : BaseManager<CameraManager>
	{
		protected Camera _camera;

		public Camera MainCamera
		{
			get
			{
				return _camera;
			}
		}

		private void Awake()
		{
			_camera = FindObjectOfType<Camera>();
		}

		public void ShakeCamera()
		{
			ShakeCamera(3, 1, 0.01f);
		}


		/// t : 지속시간
		/// amp : 진폭
		/// freq : 주기
		public void ShakeCamera(float t, float amp, float freq = 0.01f)
		{
			StartCoroutine(Shake(t, amp, freq));
		}

		protected IEnumerator Shake(float t, float amp, float freq)
		{
			float duration = t;
			Vector3 origin = _camera.transform.localPosition;

			while (duration > 0)
			{
				duration -= freq;
				_camera.transform.localPosition = new Vector3(Random.Range(-amp, amp), Random.Range(-amp, amp), _camera.transform.position.z);
				yield return new WaitForSeconds(freq);
			}

			_camera.transform.localPosition = origin;
			yield return null;
		}
	}
}
