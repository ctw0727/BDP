using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ctw_Heart_behavior : MonoBehaviour
{
	public Color SelfColor;
	
	SpriteRenderer SelfRenderer;
	
    void Start(){
        
		SelfRenderer = GetComponent<SpriteRenderer>();
    }
	
	void DrawSelf(){
		
		SelfRenderer.color = SelfColor;
	}
	
    void Update(){
        
		DrawSelf();
    }
}
