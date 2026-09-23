using System;
using System.Collections.Generic;
using BS.Render;
using UnityEngine;

namespace BS.Animators
{
    public class AnimState
    {
        public string Id => _id;
        public bool IsLooping => _isLooping;
        public AnimState NextState => _nextState;

        struct Branch
        {
            public AnimState state;
            public Func<bool> condition;
        }

        readonly Dictionary<string, List<Branch>> _branchedStates;
        string _id;
        bool _isLooping;

        AnimState _nextState;
        SheetAnimationClip _clip;
        float _time;
        float _normalizedTime;
        bool _useNormalized;

        public AnimState(string id, SheetAnimationClip clip, bool isLooping = false, AnimState nextState = null)
        {
            _id = id;
            _clip = clip;
            _isLooping = isLooping;
            _nextState = nextState;
            _branchedStates = new Dictionary<string, List<Branch>>();
        }

        public bool IsFinished => !_useNormalized && !_isLooping && _clip != null && _clip.Length > 0f && _time >= _clip.Length;

        public void AddBranch(string trigger, AnimState state, Func<bool> condition = null)
        {
            Branch item = new Branch
            {
                state = state,
                condition = condition
            };

            if (!_branchedStates.TryGetValue(trigger, out List<Branch> value))
            {
                value = new List<Branch>();
                _branchedStates[trigger] = value;
            }

            value.Add(item);
        }

        public void Enter()
        {
            _time = 0f;
            _normalizedTime = 0f;
            _useNormalized = false;
        }

        public void SetNormalizedTime(float normalizedTime)
        {
            _useNormalized = true;
            _normalizedTime = Mathf.Clamp01(normalizedTime);
        }

        public bool TryGetTrigger(string trigger, out AnimState state)
        {
            state = null;
            if (string.IsNullOrEmpty(trigger) || !_branchedStates.TryGetValue(trigger, out List<Branch> branches))
                return false;

            for (int i = 0; i < branches.Count; i++)
            {
                Branch branch = branches[i];
                if (branch.condition != null && !branch.condition())
                    continue;

                state = branch.state;
                return state != null;
            }

            return false;
        }

        public bool TryGetCondition(out AnimState state)
        {
            foreach (List<Branch> branches in _branchedStates.Values)
            {
                for (int i = 0; i < branches.Count; i++)
                {
                    Branch branch = branches[i];
                    if (branch.condition == null || !branch.condition())
                        continue;

                    state = branch.state;
                    return state != null;
                }
            }

            state = null;
            return false;
        }

        public void Update(float dt)
        {
            if (_useNormalized)
                return;

            _time += dt;
        }

        public void Apply(RenderController renderController)
        {
            if (renderController == null || _clip == null)
                return;

            Sprite sprite = _useNormalized
                ? _clip.GetSpriteNormalized(_normalizedTime)
                : _clip.GetSprite(_time, _isLooping);

            renderController.SetSprite(sprite);
        }
    }
}