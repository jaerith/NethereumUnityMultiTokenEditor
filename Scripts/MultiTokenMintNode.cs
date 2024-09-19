using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;

using Nethereum.Unity.Editors;

namespace Nethereum.Unity.Editors.MultiToken
{
    public class MultiTokenMintNode : MultiTokenNode
    {
        [SerializeField]
        private long _tokenId;

        [SerializeField]
        private string _tokenName;

        [SerializeField]
        private string _tokenSymbol;

        [SerializeField]
        private string _tokenOwnerAddress;

        [SerializeField]
        private string _tokenMetadataUrl;

        [SerializeField]
        private long _totalBalance;

        [HideInInspector]
        [SerializeField]
        private long _initialTotalBalance = 0;

        public string TokenSymbol { get { return _tokenSymbol; } }

        public string TokenOwnerAddress { get { return _tokenOwnerAddress; } }

        public string TokenMetadataUrl { get { return _tokenMetadataUrl; } }

        public long InitialTokenBalance { get { return _initialTotalBalance; } }

        public long TokenBalance { get { return _totalBalance; } }

        public bool IsNFT() { return (_initialTotalBalance == 1); }

        public void SetTokenBalance(long newBalance)
        {
            if (_initialTotalBalance == 0)
            {
                _initialTotalBalance = newBalance;
            }

            _totalBalance = newBalance;
        }


        #region UNITY EDITOR SECTION

        public void SetTokenId(long newTokenId)
        {
            _tokenId = newTokenId;
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


        #endregion
    }
}
