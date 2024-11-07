using UnityEngine;
using UnityEditor;

namespace Nethereum.Unity.Behaviours
{
    [CustomEditor(typeof(EthereumTokenDisbursement))]
    [CanEditMultipleObjects]
    public class EthereumTokenDisbursementEditor : Editor
    {
        EthereumTokenDisbursement _tokenDisbursement = null;

        void OnEnable()
        {
            _tokenDisbursement = target as EthereumTokenDisbursement;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_tokenDisbursement.Contract != null)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("");
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Disburse"))
                {
                    _tokenDisbursement.Disburse();
                }
            }
        }
    }
}
