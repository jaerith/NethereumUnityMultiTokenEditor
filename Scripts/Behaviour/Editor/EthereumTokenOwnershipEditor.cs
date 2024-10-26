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

            if (_tokenOwnership.TokenTransferRecipient != null)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Name: (" + _tokenOwnership.TokenTransferRecipient.Name + ")");
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Transfer Tokens"))
                {
                    _tokenOwnership.TransferTokens(_tokenOwnership.TokenTransferRecipient);
                }

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("");
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Request Token"))
            {
                _tokenOwnership.RequestToken();
            }

            if (_tokenOwnership.TokenBalance > 0)
            {
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
}