using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using System;

namespace Nethereum.Unity.MultiToken
{
    public class MultiTokenNode : ScriptableObject
    {
        [SerializeField]
        protected List<string> _children = new List<string>();

        [SerializeField]
        protected Rect _rect = new Rect(0, 0, 200, 200);

        [SerializeField]
        protected bool _isDeployed = false;

        public bool IsDeployed
        {
            get { return _isDeployed; }
        }

        public List<string> GetChildren()
        {
            return new List<string>(_children);
        }

        public Rect GetRect()
        {
            return _rect;
        }

        public Vector2 GetRectPosition()
        {
            return _rect.position;
        }

#if UNITY_EDITOR

        public void SetIsDeployed(bool isDeployed)
        {
            _isDeployed = isDeployed;
        }

        public void SetRectPosition(Vector2 newPosition)
        {
            Undo.RecordObject(this, "Move Node");
            _rect.position = newPosition;
            EditorUtility.SetDirty(this);
        }

#endif

    }

}
