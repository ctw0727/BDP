using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ctw_BlockEffect_behavior : MonoBehaviour
{
	public float Time;
	public Vector2 Vel;
	public bool OnWork;
	
	Transform SelfTransform;
	Rigidbody2D SelfRigid2D;
	SpriteRenderer SelfSpriteRend;
	
    void Start(){
        
		SelfTransform = GetComponent<Transform>();
		SelfRigid2D = GetComponent<Rigidbody2D>();
		SelfSpriteRend = GetComponent<SpriteRenderer>();
    }
	
	float Math_2D_Force(Vector2 v){
		float x = v.x;
		float y = v.y;
		return Mathf.Sqrt(Mathf.Pow(x,2)+Mathf.Pow(y,2));
	}
	
	void Control(){
		
		float R = SelfSpriteRend.color.r;
		float G = SelfSpriteRend.color.g;
		float B = SelfSpriteRend.color.b;
		
		if (Time <= 0){
			
			OnWork = false;
		}
		
		switch (OnWork){
			
			case true:
				Time -= 0.01f;
				SelfSpriteRend.color = new Color(R,G,B,Time);
			break;
			
			case false:
				Time = 0f;
				SelfSpriteRend.color = new Color(R,G,B,0f);
			break;
		}
	}
	
	void Slower(){
		
		if (Math_2D_Force(Vel) >= 1f){
			
			Vel = Vel * 0.99f;
		}
		
		else{
			OnWork = false;
			Vel = new Vector2(0f,0f);
		}
	}
	
	void Render(){
		
		SelfRigid2D.linearVelocity = Vel;
	}
	
    void Update(){
        
		Control();
		Slower();
		Render();
    }
}
