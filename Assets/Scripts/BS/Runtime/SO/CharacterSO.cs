using BS.Animators;
using SaintsField;
using UnityEngine;

namespace BS.SO
{
    [CreateAssetMenu(fileName = "SO_Character", menuName = "BDP/Character/Character Setting")]
    public class CharacterSO : ScriptableObject
    {
        public const string BodyIdle = "BodyIdle";
        public const string BodyNoise = "BodyNoise";
        public const string BodyError = "BodyError";
        public const string BodyCharge = "BodyCharge";
        public const string BodyOff = "BodyOff";
        public const string FaceIdle = "FaceIdle";
        public const string FaceFalling = "FaceFalling";
        public const string FaceCharge = "FaceCharge";
        public const string FaceSuccess = "FaceSuccess";
        public const string FaceOnHit = "FaceOnHit";

        public float moveSpeed = 1f;
        public Material material;

        public SaintsDictionary<string, SheetAnimationClip> animations;

        public SheetAnimationClip GetClip(string id)
        {
            if (animations != null && animations.TryGetValue(id, out SheetAnimationClip clip))
                return clip;

            return null;
        }
    }
}