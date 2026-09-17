using System.Numerics;
using System;
using UnityEngine;
namespace BS.Utils{
    public class RandomFunc
    {
        public static T RandomEnum<T>(){
            Array values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(new System.Random().Next(0, values.Length));
        }

        public static UnityEngine.Vector2 RandomVector2(float min, float max){
            return new UnityEngine.Vector2(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max));
        }
    }
}
