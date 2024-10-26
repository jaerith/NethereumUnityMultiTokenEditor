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

        public long TokenBalance { get { return _tokenBalance; } }

        [SerializeField]
        private long _totalTokenSupply;

        [SerializeField]
        private EthereumAccountBehaviour _tokenTransferRecipient = null;

        public EthereumAccountBehaviour TokenTransferRecipient { get { return _tokenTransferRecipient; } }

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

        public void TransferTokens(EthereumAccountBehaviour tokenRecipient) 
        {
            _accountBehaviour.TransferTokens(tokenRecipient, _tokenId, _tokenBalance);

            _tokenBalance   = 0;

            _tokenTransferRecipient = null;
        }

        public void RequestToken()
        {
            _accountBehaviour.RequestToken(_tokenId);

            _tokenBalance     += 1;
            _totalTokenSupply -= 1;
        }

        public void RefundToken()
        {
            _accountBehaviour.RefundOwnedTokens(_tokenId, _tokenBalance);

            _totalTokenSupply += _tokenBalance;
            _tokenBalance     = 0;            
        }
    }
}
