using System.Collections;
using System.Collections.Generic;
using BS.Utils;
using UnityEngine.Events;
using UnityEngine;


namespace BS.UI{    
    public class StartMenuManager : MonoBehaviour
    {
        [HideInInspector]
        public UnityEvent OnPlay;
        private CanvasGroup _canvas;

        public void Init(){
            _canvas = GetComponent<CanvasGroup>();
            OnPlay = new UnityEvent();
        }

        public IEnumerator FadeOutStartMenuLoop(float t){
            float duration = t;

            _canvas.blocksRaycasts = false;
            _canvas.interactable = false;
            while(duration > 0){
                duration -= Time.deltaTime;
                _canvas.alpha = (duration / t);
                yield return null;
            }

            this.OnOff(false);
            _canvas.alpha = 0;
            yield return null;
        }

        public void OnOff(bool signal){
            _canvas.blocksRaycasts = signal;
            _canvas.interactable = signal;
            this.gameObject.SetActive(signal);
        }

        public void Play(){
            Debug.Log("게임 시작");
            if(OnPlay != null){
                OnPlay.Invoke();
            }
        }

        public void Settings(){
            Debug.Log("설정 보기");
        }

        public void Exit(){
            AppHelper.Quit();
        }

        private void OnDestroy(){
            OnPlay.RemoveAllListeners();
        }
    }
}
