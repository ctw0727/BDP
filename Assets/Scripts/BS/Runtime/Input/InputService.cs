using System;
using System.Threading;
using BS.Physics;
using Cysharp.Threading.Tasks;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BS.Runtime.Input
{
    public class InputService : InputSystem_Actions.IPlayerActions, IDisposable
    {
        public Observable<Vector2> OnPlayerMove => _onPlayerMove.AsObservable();
        public Observable<Unit> OnPlayerJump => _onPlayerJump.AsObservable();
        public Observable<Vector2> OnPlayerCharge => _onPlayerCharge.AsObservable();
        public Observable<Vector2> OnPlayerChargeRelease => _onPlayerChargeRelease.AsObservable();

        InputSystem_Actions _inputSystemActions;
        InputSystem_Actions.PlayerActions _playerInputActions;

        Subject<Vector2> _onPlayerMove;
        Subject<Vector2> _onPlayerCharge;
        Subject<Vector2> _onPlayerChargeRelease;
        Subject<Unit> _onPlayerJump;

        CancellationTokenSource _chargeCts;
        UnityEngine.Camera _mainCamera;

        [Inject]
        void Initialize()
        {
            _mainCamera = UnityEngine.Camera.main;

            _onPlayerMove = new Subject<Vector2>();
            _onPlayerCharge = new Subject<Vector2>();
            _onPlayerChargeRelease = new Subject<Vector2>();
            _onPlayerJump = new Subject<Unit>();

            _inputSystemActions = new InputSystem_Actions();
            _playerInputActions = _inputSystemActions.Player;
            _playerInputActions.AddCallbacks(this);
        }

        #region Interface implementation of InputSystem_Actions.IPlayerActions

        void InputSystem_Actions.IPlayerActions.OnMove(InputAction.CallbackContext context)
        {
            _onPlayerMove.OnNext(context.ReadValue<Vector2>());
        }

        void InputSystem_Actions.IPlayerActions.OnJump(InputAction.CallbackContext context)
        {
            _onPlayerJump.OnNext(Unit.Default);
        }

        void InputSystem_Actions.IPlayerActions.OnCharge(InputAction.CallbackContext context)
        {
            if (_chargeCts != null)
            {
                _chargeCts.Cancel();
                _chargeCts = null;
            }

            _chargeCts = new CancellationTokenSource();
            HoldCharge(context, _chargeCts.Token).Forget();
        }

        async UniTask HoldCharge(InputAction.CallbackContext context, CancellationToken ct)
        {
            Vector2 mouseWorldPosition = default;
            Vector3 mouseScreenPosition = default;

            while (context.ReadValueAsButton())
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                await UniTask.DelayFrame(1, cancellationToken: ct);

                mouseScreenPosition = Mouse.current.position.ReadValue();
                mouseScreenPosition.z = -_mainCamera.transform.position.z;
                mouseWorldPosition = _mainCamera.ScreenToWorldPoint(mouseScreenPosition);

                _onPlayerCharge.OnNext(mouseWorldPosition);

#if UNITY_EDITOR
                PhysicsDrawer.DrawCircle(mouseWorldPosition, 1f, Color.red);
#endif
            }

            // on charge end
            mouseScreenPosition = Mouse.current.position.ReadValue();
            mouseScreenPosition.z = -_mainCamera.transform.position.z;
            mouseWorldPosition = _mainCamera.ScreenToWorldPoint(mouseScreenPosition);
            _onPlayerChargeRelease.OnNext(mouseWorldPosition);
        }

        #endregion

        public void SetPlayerInputEnable(bool enable)
        {
            if (enable)
            {
                _playerInputActions.Enable();
            }
            else
            {
                _playerInputActions.Disable();
            }
        }

        void IDisposable.Dispose()
        {
            _onPlayerMove?.Dispose();
            _onPlayerMove = null;

            _onPlayerJump?.Dispose();
            _onPlayerJump = null;

            _onPlayerCharge?.Dispose();
            _onPlayerCharge = null;

            _onPlayerChargeRelease?.Dispose();
            _onPlayerChargeRelease = null;

            _inputSystemActions?.Dispose();
            _chargeCts?.Dispose();
        }
    }
}