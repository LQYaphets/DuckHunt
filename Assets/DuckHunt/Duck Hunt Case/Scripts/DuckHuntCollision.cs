using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LQ
{  
    public delegate void LuaCollisionDelegate(Collision _col);
    /// <summary>
    /// 3D Collision Event
    /// </summary>
    public class DuckHuntCollision : MonoBehaviour
    {
        public event LuaCollisionDelegate EventEnter;
        public event LuaCollisionDelegate EventStay;
        public event LuaCollisionDelegate EventExit;
        private void OnCollisionEnter(Collision collision)
        {
            EventEnter(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            EventStay(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            EventExit(collision);
        }
    }
}