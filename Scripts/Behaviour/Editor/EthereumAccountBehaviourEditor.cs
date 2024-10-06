using UnityEngine;
using UnityEditor;

using Nethereum.Unity.MultiToken;

namespace Nethereum.Unity.Behaviours
{
    [CustomEditor(typeof(EthereumAccountBehaviour))]
    [CanEditMultipleObjects]
    public class EthereumAccountBehaviourEditor : Editor
    {
        EthereumAccountBehaviour _accountBehaviour;

        void OnEnable()
        {
            _accountBehaviour = target as EthereumAccountBehaviour;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Refund All Tokens"))
            {
                _accountBehaviour.RefundAllOwnedTokens();
            }
        }
    }
}