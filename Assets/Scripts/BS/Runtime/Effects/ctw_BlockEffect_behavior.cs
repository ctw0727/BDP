using BS.Render;
using UnityEngine;

public class ctw_BlockEffect_behavior : MonoBehaviour
{
    public float Time;
    public Vector2 Vel;
    public bool OnWork;

    readonly RenderController _render = new RenderController();
    Vector2 _position;
    float _duration = 1f;
    float _angle;
    Sprite _sprite;
    Material _material;
    Color _color = Color.white;
    Vector3 _scale = Vector3.one;

    void Awake()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            _sprite = renderer.sprite;
            _material = renderer.sharedMaterial;
            _color = renderer.color;
            renderer.enabled = false;
        }

        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        if (rigidbody != null)
            rigidbody.simulated = false;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        _scale = transform.localScale;
        if (_material != null)
        {
            _render.SetRender(new RenderParams(_material)
            {
                sortingOrder = 4,
                renderingLayerMask = 1u
            });
        }
    }

    public void Play(Vector2 position, Vector2 velocity, float duration)
    {
        _position = position;
        Vel = velocity;
        _duration = Mathf.Max(0.01f, duration);
        Time = _duration;
        OnWork = true;
        _angle = UnityEngine.Random.Range(0f, 90f);
    }

    void Update()
    {
        if (!OnWork)
            return;

        Time -= UnityEngine.Time.deltaTime;
        Vel *= Mathf.Pow(0.99f, UnityEngine.Time.deltaTime * 60f);
        _position += Vel * UnityEngine.Time.deltaTime;

        if (Time <= 0f || Vel.magnitude < 1f)
        {
            OnWork = false;
            return;
        }

        if (_sprite == null || _material == null)
            return;

        Color color = _color;
        color.a = Mathf.Clamp01(Time / _duration);
        _render.SetSprite(_sprite)
            .SetColor(color)
            .SetMatrix(Matrix4x4.TRS(_position, Quaternion.Euler(0f, 0f, _angle), _scale))
            .Render();
    }
}
