using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using System;

namespace Nethereum.Unity.MultiToken
{
    public class MultiTokenContractNode : MultiTokenNode
    {
        public delegate void PauseAction(MultiTokenContractNode contractNode);
        public static event PauseAction OnPause;

        public delegate void UnpauseAction(MultiTokenContractNode contractNode);
        public static event UnpauseAction OnUnpause;

        [SerializeField]
        private string _contractName;

        [SerializeField]
        private string _contractAddress;

        [SerializeField]
        private bool _paused = false;

        public bool IsPaused 
        { 
            get { return _paused; } 
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

        public void PauseContract()
        {
            OnPause(this);
            _paused = true;
        }

        public void UnpauseContract()
        {
            OnUnpause(this);
            _paused = false;
        }

        #endregion

    }

}

