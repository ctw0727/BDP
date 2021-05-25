using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[ImageEffectAllowedInSceneView]
[ExecuteInEditMode]
public class ExplosionGenerator : MonoBehaviour
{   
    public Transform _futileStage;
    public float _initSize = 0;
    public float _maxSize = 1;
    public float _duration = 0.3f;

    public Material _mat = null;

    [ShowAssetPreview]
    public GameObject _explosionPrefab;
    void Update(){
        if(Input.GetMouseButtonDown(0)){
            Vector3 dest = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dest = new Vector3(dest.x, dest.y, 0);

            GameObject obj = Instantiate(_explosionPrefab);
            obj.name = "Explosion";
            obj.transform.position = dest;
            obj.transform.SetParent(_futileStage);
            var explosion = obj.AddComponent<ExplosionEffect>();
            explosion.Init(_initSize, _maxSize, _duration);
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest){
        if(_mat == null) return;

        Graphics.Blit(src, dest, _mat);
    }
}
