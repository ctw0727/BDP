using System;
using System.Collections;
using BS.Enemy.Boss;
using UnityEngine;

public class BulletEraser : MonoBehaviour
{
	public GameObject _followingObj;
	public SpriteRenderer _sprite;
	CircleCollider2D _collider;

	// collider 원래 반지름
	float _originRadius;

	
    public void Init(){
		_collider = GetComponent<CircleCollider2D>() as CircleCollider2D;
		_originRadius = _collider.radius;
		_sprite.enabled = false;
    }

	public void EraserWave(float t = 0.3f, float speed = 2.5f){
		StartCoroutine(StartEraserWave(t, speed));
	}

	protected IEnumerator StartEraserWave(float t, float speed){
		_sprite.enabled = true;

		float duration = t;

		while(duration > 0){
			duration -= Time.deltaTime;

			this.transform.localScale += (speed * Vector3.one);
			_collider.radius += (_originRadius * speed);
			yield return new WaitForSeconds(Time.deltaTime);
		}

		this.transform.localScale = Vector3.one;
		_collider.radius = _originRadius;
		_sprite.enabled = false;
		yield return null;
	}

	/// <summary>
	/// Bullet Eraser를 만듭니다.
	/// </summary>
	/// <param name="eraserPrefab">Bullet Eraser가 될 prefab</param>
	/// <param name="following">Bullet Eraser가 따라가야할 gameobject</param>
	/// <returns></returns>
	public static BulletEraser Create(GameObject eraserPrefab, GameObject following){
		GameObject obj = Instantiate(eraserPrefab);
		obj.name = String.Format("{0}_Bullet_Eraser", following.name);
		BulletEraser eraser = obj.GetComponent<BulletEraser>();
		eraser?.Init();
		eraser._followingObj = following;
		return eraser;
	}

	private void Update()
	{
		if(_followingObj != null){
			this.transform.position = _followingObj.transform.position;
		}
	}
}
