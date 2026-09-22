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
        public Observable<Vector2> OnPlayerMove => _onPlayerMoveSubject.AsObservable();
        public Observable<Unit> OnPlayerJump => _onPlayerJumpSubject.AsObservable();
        public Observable<Vector2> OnPlayerCharge => _onPlayerChargeSubject.AsObservable();

        InputSystem_Actions _inputSystemActions;
        InputSystem_Actions.PlayerActions _playerInputActions;

        Subject<Vector2> _onPlayerMoveSubject;
        Subject<Vector2> _onPlayerChargeSubject;
        Subject<Unit> _onPlayerJumpSubject;

        CancellationTokenSource _chargeCts;
        UnityEngine.Camera _mainCamera;

        [Inject]
        void Initialize()
        {
            _mainCamera = UnityEngine.Camera.main;

            _onPlayerMoveSubject = new Subject<Vector2>();
            _onPlayerChargeSubject = new Subject<Vector2>();
            _onPlayerJumpSubject = new Subject<Unit>();

            _inputSystemActions = new InputSystem_Actions();
            _playerInputActions = _inputSystemActions.Player;
            _playerInputActions.AddCallbacks(this);
        }

        #region Interface implementation of InputSystem_Actions.IPlayerActions

        void InputSystem_Actions.IPlayerActions.OnMove(InputAction.CallbackContext context)
        {
            _onPlayerMoveSubject.OnNext(context.ReadValue<Vector2>());
        }

        void InputSystem_Actions.IPlayerActions.OnJump(InputAction.CallbackContext context)
        {
            _onPlayerJumpSubject.OnNext(Unit.Default);
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
            while (context.ReadValueAsButton())
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                await UniTask.DelayFrame(1, cancellationToken: ct);

                Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
                mouseScreenPosition.z = -_mainCamera.transform.position.z;
                Vector2 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(mouseScreenPosition);

                _onPlayerChargeSubject.OnNext(mouseWorldPosition);

#if UNITY_EDITOR
                PhysicsDrawer.DrawCircle(mouseWorldPosition, 1f, Color.red);
#endif
            }

            // on charge end
            _onPlayerChargeSubject.OnNext(Vector2.zero);
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
            _onPlayerMoveSubject?.Dispose();
            _onPlayerMoveSubject = null;

            _onPlayerJumpSubject?.Dispose();
            _onPlayerJumpSubject = null;

            _onPlayerChargeSubject?.Dispose();
            _onPlayerChargeSubject = null;

            _inputSystemActions?.Dispose();
            _chargeCts?.Dispose();
        }
    }
}