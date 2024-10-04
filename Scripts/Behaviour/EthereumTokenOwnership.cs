using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nethereum.Unity.Behaviours
{
    public class EthereumTokenOwnership : ScriptableObject
    {
        [SerializeField]
        private long TokenId;

        [SerializeField]
        private long TokenBalance;

        [SerializeField]
        private long TotalTokenSupply;

        public EthereumTokenOwnership(long tokenId, long tokenBalance, long tokenSupply)
        {
            TokenId          = tokenId;
            TokenBalance     = tokenBalance;
            TotalTokenSupply = tokenSupply;
        }
    }
}
