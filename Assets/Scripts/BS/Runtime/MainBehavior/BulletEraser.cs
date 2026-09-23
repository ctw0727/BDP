using System.Collections;
using BS.Projectile;
using BS.Render;
using UnityEngine;

public class BulletEraser : MonoBehaviour
{
    /// <summary>
    /// Renderer2D의 Camera Sorting Layer Texture 캡처 범위(Default)보다 뒤에 있어야 배경 왜곡이 보입니다.
    /// </summary>
    const string DistortionSortingLayer = "Distortion";

    public GameObject _followingObj;

    readonly RenderController _render = new RenderController();
    Sprite _sprite;
    Material _material;
    Color _color = Color.white;
    float _startRadius = 0.5f;

    public void Init()
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            _startRadius = Mathf.Max(0.05f, collider.radius);
            collider.enabled = false;
        }

        SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>(true);
        if (renderer != null)
        {
            _sprite = renderer.sprite;
            _color = renderer.color;
            _material = renderer.sharedMaterial;
            renderer.enabled = false;
        }

        if (_material != null)
        {
            _render.SetRender(new RenderParams(_material)
            {
                sortingLayerID = SortingLayer.NameToID(DistortionSortingLayer),
                sortingOrder = 3,
                renderingLayerMask = 1u
            });
        }
    }

    public void EraserWave(float duration = 0.3f, float speed = 2.5f)
    {
        StopAllCoroutines();
        StartCoroutine(Wave(duration, speed));
    }

    public static BulletEraser Create(GameObject eraserPrefab, GameObject following)
    {
        if (eraserPrefab == null)
            return null;

        GameObject obj = Instantiate(eraserPrefab);
        obj.name = string.Format("{0}_Bullet_Eraser", following.name);
        BulletEraser eraser = obj.GetComponent<BulletEraser>();
        if (eraser == null)
            return null;

        eraser.Init();
        eraser._followingObj = following;
        return eraser;
    }

    IEnumerator Wave(float duration, float speed)
    {
        float radius = _startRadius;
        float remaining = duration;
        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            radius += _startRadius * speed;
            Vector2 position = FollowPosition();
            Bullet.Erase(position, radius);
            Draw(position, radius);
            yield return null;
        }

    }

    Vector2 FollowPosition()
    {
        return _followingObj != null ? (Vector2)_followingObj.transform.position : (Vector2)transform.position;
    }

    void Draw(Vector2 position, float radius)
    {
        if (_sprite == null || _material == null)
            return;

        float diameter = Mathf.Max(0.01f, radius * 2f);
        Color color = _color;
        color.a = 0.35f;
        _render.SetSprite(_sprite)
            .SetColor(color)
            .SetMatrix(Matrix4x4.TRS(position, Quaternion.identity, new Vector3(diameter, diameter, 1f)))
            .Render();
    }
}
