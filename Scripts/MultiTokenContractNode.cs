using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using System;

namespace Nethereum.Unity.Editors.MultiToken
{
    public class MultiTokenContractNode : MultiTokenNode
    {
        [SerializeField]
        private string _contractName;

        [SerializeField]
        private string _contractAddress;

        private bool _isDeployed = false;

        public bool isDeployed
        {
            get { return _isDeployed; }

        }

        #region UNITY EDITOR SECTION

        public string ContractName
        {
            get { return _contractName; }

            set
            {
                if (_contractName != value)
                {
#if UNITY_EDITOR
                    Undo.RecordObject(this, "Update Contract Name");
#endif

                    _contractName = value;

#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
#endif
                }
            }

        }

        public string ContractAddress
        {
            get { return _contractAddress; }

            set
            {
                if (_contractAddress != value)
                {
#if UNITY_EDITOR
                    Undo.RecordObject(this, "Update Contract Address");
#endif

                    _contractAddress = value;

#if UNITY_EDITOR
                    EditorUtility.SetDirty(this);
#endif

                    _isDeployed = true;
                }
            }

        }

#if UNITY_EDITOR
        public void AddChild(string childName)
        {
            Undo.RecordObject(this, "Add Token");
            _children.Add(childName);
            EditorUtility.SetDirty(this);
        }

        public void RemoveChild(string childName)
        {
            Undo.RecordObject(this, "Remove Token");
            _children.Remove(childName);
            EditorUtility.SetDirty(this);
        }

#endif

        #endregion

    }

}

