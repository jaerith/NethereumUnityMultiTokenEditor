using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Nethereum.Unity.MultiToken;

namespace Nethereum.Unity
{
    public class EthereumAccountBehaviour : MonoBehaviour
    {
        [SerializeField]
        private string _name;

        [SerializeField]
        private string _publicAddress = "0x96092AE9c60b39Acee9f73fFF2902a03143073E7";

        public string PublicAddress { get { return _publicAddress; } }

    }
}
