using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nethereum.Unity.Behaviours
{
    public class EthereumTokenOwnership : ScriptableObject
    {
        [SerializeField]
        private long _tokenId;

        [SerializeField]
        private long _tokenBalance;

        [SerializeField]
        private long _totalTokenSupply;

        private EthereumAccountBehaviour _accountBehaviour;

        public EthereumTokenOwnership() { }

        public EthereumTokenOwnership Init(EthereumAccountBehaviour accountBehaviour, long tokenId, long tokenBalance, long tokenSupply)
        {
            _accountBehaviour = accountBehaviour;

            _tokenId          = tokenId;
            _tokenBalance     = tokenBalance;
            _totalTokenSupply = tokenSupply;

            return this;
        }

        public void RefundToken()
        {
            _accountBehaviour.RefundOwnedTokens(_tokenId, _tokenBalance);
        }
    }
}
