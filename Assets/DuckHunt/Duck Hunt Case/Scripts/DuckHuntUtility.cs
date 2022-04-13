using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LQ
{
    public enum RunningPlatform
    {
        UnityEditor,
        Ios,
        Android
    }

    public class DuckHuntUtility : MonoBehaviour
    {

        private RunningPlatform _runningPlatform;
        private void Awake()
        {
#if UNITY_IOS
            _runningPlatform = RunningPlatform.Ios;
#elif UNITY_ANDROID
            _runningPlatform = RunningPlatform.Android;
#elif UNITY_EDITOR||UNITY_EDITOR_OSX
            _runningPlatform = RunningPlatform.UnityEditor;
#endif
        }

        public RunningPlatform GetRunningPlatform()
        {
            return _runningPlatform;
        }

        public RaycastHit Raycast(Ray _ray, float rayDistance)
        {
            RaycastHit _hit;
            RaycastHit hitResult;
            if (Physics.Raycast(_ray, out _hit, rayDistance))
            {
                hitResult = _hit;
            }
            else
            {
                hitResult = new RaycastHit();
                hitResult.point = Vector3.zero;
            }
            return hitResult;
        }

        public Vector2 GetScreenWidthAndHeight()
        {
            Vector2 tempPos = new Vector2(Screen.width, Screen.height);
            return tempPos;
        }

        public float GetInputAxis(string axisName)
        {
            return Input.GetAxis(axisName);
        }

        public void Quit()
        {
            //UnityNativeCommunication.instance.Quit();
        }

    }
}