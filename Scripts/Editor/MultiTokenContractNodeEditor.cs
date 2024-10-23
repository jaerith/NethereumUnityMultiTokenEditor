using UnityEngine;
using UnityEditor;

namespace Nethereum.Unity.MultiToken
{
    [CustomEditor(typeof(MultiTokenContractNode))]
    [CanEditMultipleObjects]
    public class MultiTokenContractNodeEditor : Editor
    {
        MultiTokenContractNode _contractNode = null;

        void OnEnable()
        {
            _contractNode = target as MultiTokenContractNode;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");
            GUILayout.EndHorizontal();

            if (!_contractNode.IsPaused) 
            {
                if (GUILayout.Button("Pause Contract"))
                {
                    _contractNode.PauseContract();
                }
            }
            else
            {
                if (GUILayout.Button("Unpause Contract"))
                {
                    _contractNode.UnpauseContract();
                }
            }
        }
    }
}