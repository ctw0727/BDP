using System;
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

        InputSystem_Actions _inputSystemActions;
        InputSystem_Actions.PlayerActions _playerInputActions;
        Subject<Vector2> _onPlayerMoveSubject;
        Subject<Unit> _onPlayerJumpSubject;

        [Inject]
        void Initialize()
        {
            _onPlayerMoveSubject = new Subject<Vector2>();
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
            _onPlayerJumpSubject?.Dispose();
            _inputSystemActions?.Dispose();
        }
    }
}