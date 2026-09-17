using System.Collections;
using System.Collections.Generic;
using BS.Player;
using UnityEngine;

// All Codes worked well on 2020-07-06

public class ctw_Platform_behavior : MonoBehaviour
{
	
	public bool Trigger = false;
	
	int DownCool = 0;
	
	Transform PlatformTransform;
	BoxCollider2D PlatformCollider;
	
	Transform PlayerTransform;
	
    void Start(){
		
        PlatformTransform = GetComponent<Transform>();
		PlatformCollider = gameObject.AddComponent<BoxCollider2D>() as BoxCollider2D;
		PlatformCollider.offset = new Vector2(0,-2);
		PlatformCollider.size = new Vector2(1,4);
		
		PlayerTransform = FindObjectOfType<PlayerController>().transform;
    }
	
	void Control(){
		
		PlayerController CallScript = FindObjectOfType<PlayerController>();
		
		if ((PlayerTransform.position.y >= PlatformTransform.position.y + 0.2)&&(DownCool == 0)){
			PlatformCollider.isTrigger = false;
			
			if ((CallScript._down == true) && (PlayerTransform.position.y - PlatformTransform.position.y <= 3)){
				PlatformCollider.isTrigger = true;
				DownCool = 1;
				Invoke("Cooler",0.5f);
			}
		}
		
		if ((PlayerTransform.position.y < PlatformTransform.position.y - 1)&&(DownCool == 0)){
			PlatformCollider.isTrigger = true;
		}
	}
	
	void Cooler(){
		DownCool = 0;
	}
	
    void Update(){
		
		Control();
		
		Trigger = PlatformCollider.isTrigger;
    }
}
