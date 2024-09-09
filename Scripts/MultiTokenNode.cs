using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using System;

namespace Nethereum.Unity.Editors.MultiToken
{
    public class MultiTokenNode : ScriptableObject
    {
        [SerializeField]
        protected List<string> _children = new List<string>();

        [SerializeField]
        protected Rect _rect = new Rect(0, 0, 200, 200);

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

    }

}
