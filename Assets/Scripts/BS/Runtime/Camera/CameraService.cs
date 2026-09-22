using System;
using System.Threading;
using BS.Helpers;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BS.Camera
{
    public class CameraService : IDisposable
    {
        public UnityEngine.Camera MainCamera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = UnityEngine.Camera.main;
                }

                return _camera;
            }
        }

        UnityEngine.Camera _camera;
        CancellationTokenSource _cancellationTokenSource;

        /// t : 지속시간
        /// amp : 진폭
        /// freq : 주기
        public void ShakeCamera(float t, float amp, float freq = 0.01f)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            TaskHelper.RunSafely(Shake(MainCamera, t, amp, freq, _cancellationTokenSource.Token));
        }

        async UniTask Shake(
            UnityEngine.Camera camera,
            float t, float amp, float freq,
            CancellationToken ct)
        {
            float duration = t;
            Vector3 origin = camera.transform.localPosition;

            while (duration > 0)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                duration -= freq;
                camera.transform.localPosition = new Vector3(
                    UnityEngine.Random.Range(-amp, amp),
                    UnityEngine.Random.Range(-amp, amp),
                    _camera.transform.position.z);
                await UniTask.Delay(TimeSpan.FromSeconds(freq));
            }

            camera.transform.localPosition = origin;
        }

        void IDisposable.Dispose()
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}