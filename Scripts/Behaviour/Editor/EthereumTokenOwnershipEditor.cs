using UnityEngine;
using UnityEditor;

namespace Nethereum.Unity.Behaviours
{
    [CustomEditor(typeof(EthereumTokenOwnership))]
    [CanEditMultipleObjects]
    public class EthereumTokenOwnershipEditor : Editor
    {
        EthereumTokenOwnership _tokenOwnership = null;

        void OnEnable()
        {
            _tokenOwnership = target as EthereumTokenOwnership;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Refund Token"))
            {
                _tokenOwnership.RefundToken();
            }
        }
    }
}