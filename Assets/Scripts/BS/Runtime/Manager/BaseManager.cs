using UnityEngine;

namespace BS.Manager{
    /// <summary>
    /// 매니저 스크립트를 위한 싱글톤 클래스
    /// </summary>
    /// <typeparam name="T">Class Type</typeparam>
    public class BaseManager<T> : MonoBehaviour where T : MonoBehaviour{
        private static bool _shuttingDown = false;
        private static object _lock = new object();
        private static T _instance;

        public static T Instance {
            get{
                if(_shuttingDown){
                    Debug.LogWarningFormat("[Singleton] Instance ({0}) already destroyed. Returning null.", typeof(T));

                    return null;
                }

                if(_instance == null){
                    _instance = GameObject.FindObjectOfType(typeof(T)) as T;
                    if(_instance == null){
                        GameObject container = new GameObject();
                        container.name = string.Format("{0}", typeof(T));
                        _instance = container.AddComponent(typeof(T)) as T;

                        DontDestroyOnLoad(container);
                    }
                }

                return _instance;
            }
        }

        private void OnApplicationQuit(){
            _shuttingDown = true;
        }

        private void OnDestroy(){
            _shuttingDown = true;
        }
    }
}

