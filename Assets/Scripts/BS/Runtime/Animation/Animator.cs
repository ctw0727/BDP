using System;
using System.Collections.Generic;
using BS.Render;
using UnityEngine;

namespace BS.Animators
{
    public class Animator
    {
        public AnimState Current => _currentState;
        public RenderController RenderController => _renderController;

        AnimState _currentState;
        readonly AnimState _anyState;
        readonly RenderController _renderController;
        readonly List<string> _triggers = new List<string>();

        public Animator(AnimState initialState)
        {
            _renderController = new RenderController();
            _anyState = new AnimState("anyState", null);
            _currentState = initialState;
            _currentState?.Enter();
        }

        public void AddAnyState(string trigger, AnimState state, Func<bool> condition = null)
        {
            _anyState.AddBranch(trigger, state, condition);
        }

        public void SetTrigger(string trigger)
        {
            if (!string.IsNullOrEmpty(trigger))
                _triggers.Add(trigger);
        }

        public void SetNormalizedTime(float normalizedTime)
        {
            _currentState?.SetNormalizedTime(normalizedTime);
        }

        public void Update(float dt)
        {
            if (_currentState == null)
                return;

            if (TryConsumeTrigger(out AnimState triggered))
            {
                ChangeState(triggered);
            }
            else if (_anyState.TryGetCondition(out AnimState anyNext) && anyNext != _currentState)
            {
                ChangeState(anyNext);
            }
            else if (_currentState.TryGetCondition(out AnimState next) && next != _currentState)
            {
                ChangeState(next);
            }

            _currentState.Update(dt);

            if (_currentState.IsFinished && _currentState.NextState != null)
                ChangeState(_currentState.NextState);
        }

        public void Render(Matrix4x4 matrix, Color color)
        {
            if (_currentState == null)
                return;

            _currentState.Apply(_renderController);
            _renderController.SetColor(color).SetMatrix(matrix).Render();
        }

        void ChangeState(AnimState state)
        {
            if (state == null)
                return;

            _currentState = state;
            _currentState.Enter();
        }

        bool TryConsumeTrigger(out AnimState state)
        {
            for (int i = 0; i < _triggers.Count; i++)
            {
                if (_anyState.TryGetTrigger(_triggers[i], out state) || _currentState.TryGetTrigger(_triggers[i], out state))
                {
                    _triggers.Clear();
                    return true;
                }
            }

            _triggers.Clear();
            state = null;
            return false;
        }
    }
}