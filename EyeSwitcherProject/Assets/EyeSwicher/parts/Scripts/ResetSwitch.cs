//Copyright (c) 2022 Iodoform. MIT license (see license.txt)

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace EyeSwitcher
{
    public class ResetSwitch : UdonSharpBehaviour
    {
        [SerializeField]
        private ExchangeCube _exchangeCube;
    
        public override void Interact()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ResetViewPoints));
        }
    
        public void ResetViewPoints()
        {
            _exchangeCube._ResetViewPoints();
        }
    }
}

