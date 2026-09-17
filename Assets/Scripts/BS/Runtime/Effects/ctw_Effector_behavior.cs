using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ctw_Effector_behavior : MonoBehaviour
{
	public GameObject EffectPrefab;
	
	public GameObject[] Effect = new GameObject[100];
	public int EffectPool = 0;
	
	Transform SelfTransform;
	
    void Start(){
        
		SelfTransform = GetComponent<Transform>();
    }
	
	// Gen and Get
	
	GameObject Effect_Gen(){
		
		EffectPool++;
		
		return Instantiate(EffectPrefab, SelfTransform);
	}
	
	GameObject Effect_CheckandReturn(){
		
		int Key = 0;
		int i = 0;
		
		if (EffectPool != 0){
			for(;i<EffectPool; i++){
				if (Effect[i].GetComponent<ctw_BlockEffect_behavior>().OnWork == false){
					Key = 1;
					break;
				}
			}
		}
		
		if (Key == 0){
			Effect[i] = Effect_Gen();
		}
		
		return Effect[i];
	}
	
	public void Effect_Run(float time, Vector3 pos, Vector2 vel, int num){
		
		for (int i = 0; i < num; i++){
			
			GameObject EffectObj = Effect_CheckandReturn();
			
			EffectObj.GetComponent<Transform>().position = pos;
			EffectObj.GetComponent<Rigidbody2D>().rotation = Random.Range(0,90);
			EffectObj.GetComponent<ctw_BlockEffect_behavior>().Time = time;
			EffectObj.GetComponent<ctw_BlockEffect_behavior>().Vel = vel + new Vector2(Random.Range(-5.0f,5.0f), Random.Range(-5.0f,5.0f));
			EffectObj.GetComponent<ctw_BlockEffect_behavior>().OnWork = true;
		}
	}
	
	// Run
	
    void Update(){
        
		
    }
}
