using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using System;

namespace Nethereum.Unity.Editors.MultiToken
{
    public class MultiTokenMintNode : MultiTokenNode
    {
        [SerializeField]
        private string _tokenName;

        [SerializeField]
        private string _tokenSymbol;

        [SerializeField]
        private string _tokenMetadataUrl;

        [SerializeField]
        private long _totalBalance;

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

        #endregion
    }
}
