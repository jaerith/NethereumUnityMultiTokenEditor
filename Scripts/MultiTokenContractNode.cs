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

        #region UNITY EDITOR SECTION

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

        public void SetRectPosition(Vector2 newPosition)
        {
            Undo.RecordObject(this, "Move Contract Node");
            _rect.position = newPosition;
            EditorUtility.SetDirty(this);
        }

#endif

        #endregion

    }

}

