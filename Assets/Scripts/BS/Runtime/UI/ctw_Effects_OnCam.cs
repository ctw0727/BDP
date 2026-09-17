using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ctw_Effects_OnCam : MonoBehaviour
{
	public Vector3 SelfPos;
	public float Zpos;
	
	Transform _camera;
	Transform SelfTransform;
	
    void Start(){
		
        _camera = GameObject.Find("BS_Main Camera").GetComponent<Transform>();
		SelfTransform = GetComponent<Transform>();
    }

	void FollowCamera(){
		
		Vector3 camPos = _camera.position;
		
		Vector3 pos = camPos + SelfPos;
		pos = new Vector3(pos.x, pos.y, Zpos);
		
		SelfTransform.position = pos;
	}

    void Update(){
		
        FollowCamera();
    }
}
