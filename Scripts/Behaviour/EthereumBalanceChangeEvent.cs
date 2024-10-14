using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nethereum.Unity.Behaviours
{
    public class EthereumBalanceChangeEvent : ScriptableObject
    {
        [SerializeField]
        public string AccountPublicAddress { get; set; }

        [SerializeField]
        public long TokenId { get; set; }

        [SerializeField]
        public long TokenPreviousBalance { get; set; }

        [SerializeField]
        public long TokenCurrentBalance { get; set; }

        [SerializeField]
        public long TotalTokenSupply { get; set; }
    }
}
