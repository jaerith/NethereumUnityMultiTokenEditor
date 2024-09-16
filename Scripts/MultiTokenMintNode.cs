using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;

namespace Nethereum.Unity.Editors.MultiToken
{
    public class MultiTokenMintNode : MultiTokenNode
    {
        [SerializeField]
        private BigInteger _tokenId;

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

        public BigInteger TokenId { get { return _tokenId; } }

        public string TokenName { get { return _tokenName; } }

        public string TokenSymbol { get { return _tokenSymbol; } }

        public string TokenOwnerAddress { get { return _tokenOwnerAddress; } }

        public string TokenMetadataUrl { get { return _tokenMetadataUrl; } }

        public long TokenBalance { get { return _totalBalance; } }

        #region UNITY EDITOR SECTION

        public string tokenName
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

        public void SetDeployedStatus(bool isDeployed)
        {
            _isDeployed = isDeployed;
        }

        #endregion
    }
}
