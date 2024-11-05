using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using UnityEditor;
using UnityEngine;

using Nethereum.Unity.Behaviours;

namespace Nethereum.Unity.MultiToken
{
    public class MultiTokenMintNode : MultiTokenNode
    {
        public delegate void TransferAction(MultiTokenMintNode mintNode, string newOwner);
        public static event TransferAction OnTransfer;

        [SerializeField]
        private bool _isNFT;

        [SerializeField]
        private long _tokenId;

        [SerializeField]
        private string _tokenName;

        [SerializeField]
        private string _tokenOwnerAddress;

        [SerializeField]
        private string _tokenMetadataUrl;

        [SerializeField]
        private long _totalBalance;

        [HideInInspector]
        [SerializeField]
        private long _initialTotalBalance = 0;

        [SerializeField]
        // [OnChangedCall("onOwnersChanged")]
        private List<string> _ownerAddresses = new List<string>();

        private EthereumAccountBehaviour _tokenRecipient = null;

        public EthereumAccountBehaviour TokenRecipient { get { return _tokenRecipient; } }

        public string TokenOwnerAddress { get { return _tokenOwnerAddress; } }

        public string TokenMetadataUrl { get { return _tokenMetadataUrl; } }

        public long InitialTokenBalance { get { return _initialTotalBalance; } }

        public long TokenBalance { get { return _totalBalance; } }

        public bool IsNFT() { return _isNFT; }

        public void SetTokenBalance(long newBalance)
        {
            if (_initialTotalBalance == 0)
            {
                _initialTotalBalance = newBalance;

                _isNFT = (_initialTotalBalance == 1);
            }

            _totalBalance = newBalance;
        }

        public void AddTokenOwner(string tokenOwnerPublicAddress)
        {
            _ownerAddresses.Add(tokenOwnerPublicAddress);
        }

        public bool HasTokenOwner(string tokenOwnerPublicAddress) 
        { 
            return _ownerAddresses.Contains(tokenOwnerPublicAddress);
        }

        public IEnumerable GetTokenOwnerList() { return _ownerAddresses; }

        public void onOwnersChanged()
        {
            if (_totalBalance > 0)
            {
                if (_ownerAddresses.Count > 0)
                {
                    var newOwner = _ownerAddresses[_ownerAddresses.Count - 1];

                    if (!String.IsNullOrEmpty(newOwner.Trim()))
                    {
                        Debug.Log("DEBUG: OnTransfer() -> Transfer of Token ID (" + _tokenId + ") to new address (" + newOwner + ").");
                        OnTransfer(this, newOwner);
                    }
                }
            }
            else
            {
                Debug.Log("If attempting to assign new tokens, it cannot - no more tokens are available.");
            }
        }

        public void RequestOwnership(string requestingOwnerAddress)
        {
            if (IsDeployed)
            {
                OnTransfer(this, requestingOwnerAddress);
            }
        }

        public void SetTokenName(string newTokenName)
        {
            _tokenName = newTokenName;
        }

        public void SetTokenId(long newTokenId)
        {
            _tokenId = newTokenId;
        }

        public void SetTokenOwner(string tokenOwner)
        {
            _tokenOwnerAddress = tokenOwner;
        }

        public void SetTokenUrl(string tokenUrl)
        {
            _tokenMetadataUrl = tokenUrl;
        }

        public void SetDeployedStatus(bool isDeployed)
        {
            _isDeployed = isDeployed;
        }

        public BigInteger TokenId
        {
            get { return (BigInteger) _tokenId; }

        }

        public string TokenName
        {
            get { return _tokenName; }

            set
            {
                if (_tokenName != value)
                {
#if UNITY_EDITOR
                    Undo.RecordObject(this, "Update Token Name");
#endif

                    _tokenName = value;

#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
#endif
                }
            }

        }

        public void TransferToken(EthereumAccountBehaviour tokenRecipient)
        {
            OnTransfer(this, tokenRecipient.PublicAddress);
        }

    }
}
