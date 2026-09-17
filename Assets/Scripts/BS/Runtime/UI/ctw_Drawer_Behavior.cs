using System.Collections;
using System.Collections.Generic;
using BS.Player;
using UnityEngine;

public class ctw_Drawer_Behavior : MonoBehaviour
{
	public GameObject HeartPrefab;
	//public GameObject HealthBarPrefab;
	
	Transform SelfTransform;
	
	GameObject[] Heart = new GameObject[3];
	ctw_Heart_behavior[] HeartSet = new ctw_Heart_behavior[3];
	ctw_Effects_OnCam[] EffectorSet = new ctw_Effects_OnCam[3];
	
    void Start(){
		
		SelfTransform = GetComponent<Transform>();
		
		Set_Hearts();
		Set_HealthBar();
    }
	
	void Set_Hearts(){
		
		for(int i = 0; i <= 2; i++){
			
			Heart[i] = Instantiate(HeartPrefab,new Vector3(0,0,-5), new Quaternion(0,0,0,0));
			HeartSet[i] = Heart[i].GetComponent<ctw_Heart_behavior>();
			EffectorSet[i] = Heart[i].GetComponent<ctw_Effects_OnCam>();
			
			EffectorSet[i].SelfPos = new Vector3(-31f+(float)i*2.5f,-13.5f,-1f);
			EffectorSet[i].Zpos = -5f;
		}
	}
	
	void Set_HealthBar(){
		
	}
	
	void Draw_Hearts(){
		
		float PlayerHP = GameObject.Find("BS_Player").GetComponent<PlayerController>()._health;
		
		for(int i = 0; i <= 2; i++){
			
			if (i < PlayerHP) HeartSet[i].SelfColor = new Color(1f,0.3f,0.3f,1f);
			else HeartSet[i].SelfColor = new Color(0.8f,0.8f,0.8f,0.8f);
		}
	}
	
	void Draw_HearthBar(){
		
		
	}
	
    void Update(){
		
        Draw_Hearts();
		Draw_HearthBar();
    }
}
