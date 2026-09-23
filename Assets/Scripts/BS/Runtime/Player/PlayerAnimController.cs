using BS.Anim;
using BS.Animators;
using BS.Camera;
using BS.SO;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace BS.Player
{
    public class PlayerAnimController : BaseAnimController
    {
        const string Idle = "Idle";
        const string Charging = "Charging";
        const string OnHitId = "OnHit";
        const string BlackNoise = "BlackNoise";
        const string OffId = "Off";
        const string AttackSuccessId = "AttackSuccess";
        const string Falling = "Falling";

        [SerializeField] CharacterSO _character;
        [SerializeField] Material _material;

        protected PlayerController _player;
        BS.Animators.Animator _bodyAnimator;
        BS.Animators.Animator _faceAnimator;

        Color _bodyColor = Color.white;
        Color _faceColor = Color.white;
        bool _isFaceFalling;
        int prevLeftRightDirection = 1;

        [Inject] CameraService _cameraService;

        public override void Init() { }

        public void Init(PlayerController player)
        {
            if (_material == null)
            {
                throw new System.Exception("Material is null");
            }

            _player = player;
            BuildAnimators();
        }

        public void SpriteAlphaBlink(float t, float freq)
        {
            StartCoroutine(AlphaBlink(t, freq, SetAlpha));
        }

        public void AttackSuccess()
        {
            _faceAnimator?.SetTrigger(AttackSuccessId);
        }

        public void OnHit()
        {
            if (_player._isInvincible && !isBlink)
            {
                SpriteAlphaBlink(0.9f, 0.03f);
                _bodyAnimator?.SetTrigger(OnHitId);
                if (!_player._isCharging)
                    _faceAnimator?.SetTrigger(OnHitId);
            }
        }

        public void Dead()
        {
            if (_player._isDead)
                SetAlpha(0.35f);
        }

        public void Off()
        {
            _bodyAnimator?.SetTrigger(BlackNoise);
        }

        public void On()
        {
            _bodyAnimator?.SetTrigger(Idle);
        }

        public override void Render()
        {
            if (_bodyAnimator == null || _faceAnimator == null)
                return;

            _isFaceFalling = _player.IsFalling() && !_player._isCharging && !_player._attackSuccess;
            float dt = Time.deltaTime;
            _bodyAnimator.Update(dt);
            _faceAnimator.Update(dt);

            if (_player._isCharging)
            {
                float progress = ChargeProgress();
                if (_bodyAnimator.Current?.Id == Charging)
                    _bodyAnimator.SetNormalizedTime(progress);
                if (_faceAnimator.Current?.Id == Charging)
                    _faceAnimator.SetNormalizedTime(progress);
            }

            FlipSprite();
        }

        void BuildAnimators()
        {
            SheetAnimationClip bodyIdleClip = Clip(CharacterSO.BodyIdle);
            SheetAnimationClip bodyNoiseClip = Clip(CharacterSO.BodyNoise);
            SheetAnimationClip bodyErrorClip = Clip(CharacterSO.BodyError);
            SheetAnimationClip bodyChargeClip = Clip(CharacterSO.BodyCharge);
            SheetAnimationClip bodyOffClip = Clip(CharacterSO.BodyOff);

            AnimState bodyIdle = new AnimState(Idle, bodyIdleClip, true);
            AnimState bodyNoise = new AnimState(BlackNoise, bodyNoiseClip, true);
            AnimState bodyError = new AnimState(OnHitId, bodyErrorClip, false, bodyIdle);
            AnimState bodyCharge = new AnimState(Charging, bodyChargeClip, true);
            AnimState bodyOff = new AnimState(OffId, bodyOffClip, true);
            bodyCharge.AddBranch(Idle, bodyIdle, () => !_player._isCharging);

            _bodyAnimator = new BS.Animators.Animator(bodyIdle);
            _bodyAnimator.RenderController.SetRender(CreateRenderParams(0)).SetColor(_bodyColor);
            _bodyAnimator.AddAnyState(OnHitId, bodyError);
            _bodyAnimator.AddAnyState(BlackNoise, bodyNoise);
            _bodyAnimator.AddAnyState(OffId, bodyOff);
            _bodyAnimator.AddAnyState(Idle, bodyIdle);
            _bodyAnimator.AddAnyState(Charging, bodyCharge, () => _player._isCharging);

            SheetAnimationClip faceIdleClip = Clip(CharacterSO.FaceIdle);
            SheetAnimationClip faceFallingClip = Clip(CharacterSO.FaceFalling);
            SheetAnimationClip faceChargeClip = Clip(CharacterSO.FaceCharge);
            SheetAnimationClip faceSuccessClip = Clip(CharacterSO.FaceSuccess);
            SheetAnimationClip faceOnHitClip = Clip(CharacterSO.FaceOnHit);

            AnimState faceIdle = new AnimState(Idle, faceIdleClip, true);
            AnimState faceFalling = new AnimState(Falling, faceFallingClip, true);
            AnimState faceCharge = new AnimState(Charging, faceChargeClip, true);
            AnimState faceSuccess = new AnimState(AttackSuccessId, faceSuccessClip, false, faceIdle);
            AnimState faceOnHit = new AnimState(OnHitId, faceOnHitClip, false, faceIdle);
            faceFalling.AddBranch(Idle, faceIdle, () => !_isFaceFalling);
            faceCharge.AddBranch(Idle, faceIdle, () => !_player._isCharging);

            _faceAnimator = new BS.Animators.Animator(faceIdle);
            _faceAnimator.RenderController.SetRender(CreateRenderParams(1)).SetColor(_faceColor);
            _faceAnimator.AddAnyState(AttackSuccessId, faceSuccess);
            _faceAnimator.AddAnyState(OnHitId, faceOnHit);
            _faceAnimator.AddAnyState(Falling, faceFalling, () => _isFaceFalling);
            _faceAnimator.AddAnyState(Charging, faceCharge, () => _player._isCharging);
        }

        SheetAnimationClip Clip(string id)
        {
            SheetAnimationClip clip = _character != null ? _character.GetClip(id) : null;
            if (clip == null)
                Debug.LogError($"CharacterSO에 {id} 클립이 없습니다.", this);

            return clip;
        }

        RenderParams CreateRenderParams(int sortingOrder)
        {
            return new RenderParams(_material)
            {
                sortingOrder = sortingOrder,
                layer = gameObject.layer,
                renderingLayerMask = 1u,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false
            };
        }

        float ChargeProgress()
        {
            if (_player._maxPower <= 0f)
                return 0f;

            float progress = (Mathf.Abs(_player._currentPower) / _player._maxPower) * 0.75f;
            return progress > 0.75f ? 1f : progress;
        }

        void SetAlpha(float alpha)
        {
            _bodyColor.a = alpha;
            _faceColor.a = alpha;
        }

        void FlipSprite()
        {
            float rotationZ = Mathf.Abs(_player.transform.rotation.eulerAngles.z);

            int directionUpDown = 1;
            if (_player._isCharging)
            {
                if (90f < rotationZ && rotationZ < 270f)
                    directionUpDown = -1;
            }
            else if (170f < rotationZ && rotationZ < 190f)
            {
                directionUpDown = -1;
            }

            int directionLeftRight = prevLeftRightDirection;
            if (_player._isCharging)
            {
                Vector2 pos = transform.position;
                Vector2 mouseOnWorld = _cameraService.MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                directionLeftRight = (mouseOnWorld - pos).x <= 0f ? -1 : 1;
                prevLeftRightDirection = directionLeftRight;
            }
            else if (Mathf.Abs(_player._moveDirection) > 0f)
            {
                directionLeftRight = (int)(_player._moveDirection / Mathf.Abs(_player._moveDirection));
                prevLeftRightDirection = directionLeftRight;
            }
        }
    }
}
