using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LQ
{
    public delegate Collider LuaColliderDelegate(Collider _col);
    /// <summary>
    /// 3D Trigger Event
    /// </summary>
    public class DuckHuntTrigger : MonoBehaviour
    {
        public event LuaColliderDelegate EventEnter;
        public event LuaColliderDelegate EventStay;
        public event LuaColliderDelegate EventExit;
        private void OnTriggerEnter(Collider other)
        {
            EventEnter(other);
        }

        private void OnTriggerStay(Collider other)
        {
            EventStay(other);
        }

        private void OnTriggerExit(Collider other)
        {
            EventExit(other);
        }
    }
}
