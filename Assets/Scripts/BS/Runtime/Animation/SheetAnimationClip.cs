using System;
using UnityEngine;

namespace BS.Animators
{
    [Serializable]
    public class SheetAnimationClip
    {
        public Sprite[] Sprites => _sprites;
        public float Duration => _duration;

        [SerializeField] Sprite[] _sprites;
        [SerializeField] float _duration;

        public SheetAnimationClip()
        {
            _sprites = Array.Empty<Sprite>();
        }

        public SheetAnimationClip(Sprite[] sprites, float frameDuration)
        {
            _sprites = sprites ?? Array.Empty<Sprite>();
            _duration = frameDuration;
        }

        public float Length
        {
            get
            {
                if (_sprites == null || _sprites.Length == 0 || _duration <= 0f)
                    return 0f;

                return _duration * _sprites.Length;
            }
        }

        public Sprite GetSprite(float time, bool loop = true)
        {
            if (_sprites == null || _sprites.Length == 0)
                return null;

            if (_duration <= 0f)
                return _sprites[0];

            int index = Mathf.FloorToInt(time / _duration);
            if (loop)
                index = Mod(index, _sprites.Length);
            else
                index = Mathf.Clamp(index, 0, _sprites.Length - 1);

            return _sprites[index];
        }

        public Sprite GetSpriteNormalized(float normalizedTime)
        {
            if (_sprites == null || _sprites.Length == 0)
                return null;

            int index = Mathf.FloorToInt(Mathf.Clamp01(normalizedTime) * _sprites.Length);
            return _sprites[Mathf.Clamp(index, 0, _sprites.Length - 1)];
        }

        static int Mod(int value, int length)
        {
            int remainder = value % length;
            return remainder < 0 ? remainder + length : remainder;
        }
    }
}