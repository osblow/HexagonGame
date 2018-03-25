using System;
using System.Collections.Generic;
using UnityEngine;

namespace Osblow
{
    public class InitGlobals : MonoBehaviour
    {
        private void Start()
        {
            Globals.Instance.Start();
        }

        private void Update()
        {
            Globals.Instance.Update(Time.deltaTime);
        }

        private void LateUpdate()
        {
            Globals.Instance.LateUpdate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            
        }

        private void OnDestroy()
        {
            Globals.Instance.Destroy();
        }
    }
}
