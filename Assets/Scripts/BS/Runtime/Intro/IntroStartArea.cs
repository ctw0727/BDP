using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace BS.Intro{
    public class IntroStartArea : MonoBehaviour
    {
        [HideInInspector]
        public UnityEvent OnEnter;

        public void Init(){
            OnEnter = new UnityEvent();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.tag.Equals("Player") && OnEnter != null){
                OnEnter.Invoke();
                Destroy(this.gameObject);
            }
        }

        private void OnDestroy(){
            OnEnter.RemoveAllListeners();
        }
    }
}
