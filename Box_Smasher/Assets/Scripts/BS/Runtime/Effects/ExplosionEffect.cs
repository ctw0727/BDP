using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    private float _maxSize = 10f;
    private float _duration = 1f;
    public bool _enable = false;

    private float t;

    public void Init(float initSize, float maxSize, float duration){
        this.transform.localScale = Vector3.one * initSize;
        _maxSize = maxSize;
        _duration = duration;
        t = 0f;
        _enable = true;
    }

    void Update(){
        if(t < _duration && _enable){   
            this.transform.localScale = Vector3.one * _maxSize * (t / _duration);

            float alpha = (1 - t / _duration);
            var renderer = this.gameObject.GetComponent<SpriteRenderer>();
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, alpha);
            renderer.material.SetFloat("_Offset", 1 - alpha);
            t += Time.deltaTime;
        }else{
            // this.transform.localScale = Vector3.one * _maxSize;
            var renderer = this.gameObject.GetComponent<SpriteRenderer>();
            // renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0f);
            renderer.enabled = false;
            this.gameObject.SetActive(false);
        }
    }
}
