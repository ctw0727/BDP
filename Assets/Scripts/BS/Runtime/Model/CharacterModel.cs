using BS.Animators;
using BS.SO;
using UnityEngine;
using UnityEngine.Rendering;

namespace BS.Model
{
    public struct CharacterFrame
    {
        public Vector2 position;
        public Vector2 velocity;
        public float rollRadius;
        public bool isCharging;
        public bool isFalling;
        public bool attackSuccess;
        public float chargeProgress;
        public float aimAngle;
        public int facingX;
    }

    public class CharacterModel
    {
        const string Idle = "Idle";
        const string Charging = "Charging";
        const string OnHitId = "OnHit";
        const string BlackNoise = "BlackNoise";
        const string OffId = "Off";
        const string AttackSuccessId = "AttackSuccess";
        const string Falling = "Falling";

        readonly BS.Animators.Animator _body;
        readonly BS.Animators.Animator _face;

        Vector2 _position;
        Vector2 _scale = Vector2.one;
        float _rollAngle;
        Color _bodyColor = Color.white;
        Color _faceColor = Color.white;
        bool _isCharging;
        bool _isFaceFalling;
        bool _isFaceVisible = true;
        float _blinkDuration;
        float _blinkRemaining;
        float _blinkFrequency = 0.03f;

        public bool IsBlinking => _blinkRemaining > 0f;
        public float MoveSpeed { get; }

        public CharacterModel(CharacterSO character)
        {
            if (character == null)
                throw new System.Exception("CharacterSO가 없습니다.");
            if (character.material == null)
                throw new System.Exception("CharacterSO에 Material이 없습니다.");

            MoveSpeed = character.moveSpeed;
            _body = BuildBody(character);
            _face = BuildFace(character);
        }

        public void AttackSuccess()
        {
            _face.SetTrigger(AttackSuccessId);
        }

        public void OnHit(bool charging)
        {
            if (IsBlinking)
                return;

            BeginBlink(0.9f, 0.03f);
            _body.SetTrigger(OnHitId);
            if (!charging)
                _face.SetTrigger(OnHitId);
        }

        public void Dead()
        {
            SetAlpha(0.35f);
        }

        public void Off()
        {
            _body.SetTrigger(BlackNoise);
            _isFaceVisible = false;
        }

        public void On()
        {
            _body.SetTrigger(Idle);
            _isFaceVisible = true;
        }

        public void Tick(float dt, in CharacterFrame frame)
        {
            _position = frame.position;
            _isCharging = frame.isCharging;
            _isFaceFalling = frame.isFalling && !frame.isCharging && !frame.attackSuccess;

            if (_isCharging)
                _rollAngle = frame.aimAngle;
            else
                Roll(frame.velocity, frame.rollRadius, dt);

            float leftRight = frame.facingX < 0 ? -1f : 1f;
            float upDown = IsUpsideDown(_rollAngle, _isCharging) ? -1f : 1f;
            _scale = new Vector2(leftRight * upDown, upDown);
            TickBlink(dt);

            _body.Update(dt);
            _face.Update(dt);

            if (!_isCharging)
                return;

            if (_body.Current?.Id == Charging)
                _body.SetNormalizedTime(frame.chargeProgress);
            if (_face.Current?.Id == Charging)
                _face.SetNormalizedTime(frame.chargeProgress);
        }

        public void Render()
        {
            Matrix4x4 matrix = Matrix4x4.TRS(
                _position,
                Quaternion.Euler(0f, 0f, _rollAngle),
                new Vector3(_scale.x, _scale.y, 1f));

            _body.Render(matrix, _bodyColor);
            if (_isFaceVisible)
                _face.Render(matrix, _faceColor);
        }

        /// <summary>
        /// 차지 중에는 90~270도, 평소에는 170~190도 구간에서 스프라이트를 상하로 뒤집습니다.
        /// </summary>
        static bool IsUpsideDown(float angle, bool charging)
        {
            float z = Mathf.Repeat(angle, 360f);
            return charging ? (90f < z && z < 270f) : (170f < z && z < 190f);
        }

        public static float ChargeProgress(float currentPower, float maxPower)
        {
            if (maxPower <= 0f)
                return 0f;

            float t = Mathf.Abs(currentPower) / maxPower;
            float progress = EaseQuadOut(Mathf.Clamp01(t));

            return progress;
        }

        static float EaseQuadOut(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        BS.Animators.Animator BuildBody(CharacterSO character)
        {
            AnimState idle = new AnimState(Idle, Clip(character, CharacterSO.BodyIdle), true);
            AnimState noise = new AnimState(BlackNoise, Clip(character, CharacterSO.BodyNoise), true);
            AnimState error = new AnimState(OnHitId, Clip(character, CharacterSO.BodyError), false, idle);
            AnimState charge = new AnimState(Charging, Clip(character, CharacterSO.BodyCharge), true);
            AnimState off = new AnimState(OffId, Clip(character, CharacterSO.BodyOff), true);
            charge.AddBranch(Idle, idle, () => !_isCharging);

            var animator = new BS.Animators.Animator(idle);
            animator.RenderController.SetRender(CreateRenderParams(character.material, 0)).SetColor(_bodyColor);
            animator.AddAnyState(OnHitId, error);
            animator.AddAnyState(BlackNoise, noise);
            animator.AddAnyState(OffId, off);
            animator.AddAnyState(Idle, idle);
            animator.AddAnyState(Charging, charge, () => _isCharging);
            return animator;
        }

        BS.Animators.Animator BuildFace(CharacterSO character)
        {
            AnimState idle = new AnimState(Idle, Clip(character, CharacterSO.FaceIdle), true);
            AnimState falling = new AnimState(Falling, Clip(character, CharacterSO.FaceFalling), true);
            AnimState charge = new AnimState(Charging, Clip(character, CharacterSO.FaceCharge), true);
            AnimState success = new AnimState(AttackSuccessId, Clip(character, CharacterSO.FaceSuccess), false, idle);
            AnimState onHit = new AnimState(OnHitId, Clip(character, CharacterSO.FaceOnHit), false, idle);
            falling.AddBranch(Idle, idle, () => !_isFaceFalling);
            charge.AddBranch(Idle, idle, () => !_isCharging);

            var animator = new BS.Animators.Animator(idle);
            animator.RenderController.SetRender(CreateRenderParams(character.material, 1)).SetColor(_faceColor);
            animator.AddAnyState(AttackSuccessId, success);
            animator.AddAnyState(OnHitId, onHit);
            animator.AddAnyState(Falling, falling, () => _isFaceFalling);
            animator.AddAnyState(Charging, charge, () => _isCharging);
            return animator;
        }

        static SheetAnimationClip Clip(CharacterSO character, string id)
        {
            SheetAnimationClip clip = character.GetClip(id);
            if (clip == null)
                throw new System.Exception($"CharacterSO에 {id} 클립이 없습니다.");

            return clip;
        }

        static RenderParams CreateRenderParams(Material material, int sortingOrder)
        {
            return new RenderParams(material)
            {
                sortingOrder = sortingOrder,
                renderingLayerMask = 1u,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false
            };
        }

        void BeginBlink(float duration, float frequency)
        {
            _blinkDuration = duration;
            _blinkRemaining = duration;
            _blinkFrequency = Mathf.Max(0.0001f, frequency);
        }

        void TickBlink(float dt)
        {
            if (_blinkRemaining <= 0f)
                return;

            _blinkRemaining -= dt;
            if (_blinkRemaining <= 0f)
            {
                SetAlpha(1f);
                return;
            }

            float elapsed = _blinkDuration - _blinkRemaining;
            SetAlpha(Mathf.Abs(Mathf.Cos((Mathf.PI / _blinkFrequency) * elapsed)));
        }

        void SetAlpha(float alpha)
        {
            _bodyColor.a = alpha;
            _faceColor.a = alpha;
        }

        void Roll(Vector2 velocity, float radius, float dt)
        {
            if (Mathf.Abs(velocity.x) < 0.01f)
                return;

            float rollRadius = Mathf.Max(0.01f, radius);
            _rollAngle -= velocity.x / rollRadius * dt * Mathf.Rad2Deg;
        }
    }
}
