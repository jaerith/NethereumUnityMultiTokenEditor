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

#if UNITY_EDITOR

        public void SetRectPosition(Vector2 newPosition)
        {
            Undo.RecordObject(this, "Move Token Node");
            _rect.position = newPosition;
            EditorUtility.SetDirty(this);
        }

#endif

        #endregion
    }
}
