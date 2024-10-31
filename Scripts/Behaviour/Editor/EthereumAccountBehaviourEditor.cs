using System;
using UnityEngine;
using UnityEditor;

using Nethereum.Unity.MultiToken;
using static Cinemachine.CinemachineBlendDefinition;
using UnityEngine.UIElements;

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

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Ether Balance");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            var etherBalance = Convert.ToString(_accountBehaviour.CurrentEtherBalance);
            EditorGUILayout.TextField(etherBalance, GetGUIStyle(_accountBehaviour.CurrentEtherBalance));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("");
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Refund All Tokens"))
            {
                _accountBehaviour.RefundAllOwnedTokens();
            }

            var latestTokenTransfers  = _accountBehaviour.LatestTokenTransfers;
            var latestTransfersUpdate = _accountBehaviour.TokenTransferLogsLastTimeUpdated;
            var latestTransferCount   = latestTokenTransfers.Count;
            var displayCount          = _accountBehaviour.DisplayLatestTransfersHistoryCount;
            if (latestTokenTransfers.Count > 0) 
            {
                var mostRecentTransfers =
                    latestTransferCount > displayCount ?
                    latestTokenTransfers.GetRange((latestTransferCount - displayCount), displayCount) :
                    latestTokenTransfers;

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("");
                GUILayout.EndHorizontal();

                string transactionHistoryLabel =
                    "Latest [" + displayCount + "] Transfers (" + latestTransfersUpdate + ")";

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(transactionHistoryLabel);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("");
                GUILayout.EndHorizontal();

                mostRecentTransfers.Reverse();

                int trxIndex = latestTransferCount;
                foreach (var eventLog in mostRecentTransfers)
                {
                    var transfer = eventLog.Event;

                    string eventMessage =
                        "Trx # [" + trxIndex + "] ->" +
                        ((transfer.To == _accountBehaviour.PublicAddress) ? "Received [" : "Sent [") +
                        transfer.Value + "] tokens of Token ID (" + transfer.Id + ")";

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.TextArea(eventMessage);
                    GUILayout.EndHorizontal();

                    --trxIndex;
                }
            }            
        }

        private GUIStyle GetGUIStyle(decimal etherBalance)
        {
            int       setFontSize     = 0;
            Texture2D normalTexture2D = Texture2D.whiteTexture;

            if (etherBalance < 0.1m)
            {
                // normalTexture2D = Texture2D.redTexture;
                setFontSize     = 18;
            }
            else if ((etherBalance > 0.1m) && (etherBalance < 0.5m))
            {                
                // normalTexture2D = Texture2D.grayTexture;
                setFontSize     = 14;
            }                
            else
            {
                // normalTexture2D = Texture2D.whiteTexture;
                setFontSize     = 12;
            }                

            var guiStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = setFontSize,
                normal = new GUIStyleState() { background = normalTexture2D },
                hover = new GUIStyleState() { background = Texture2D.grayTexture },
                active = new GUIStyleState() { background = Texture2D.blackTexture }                
            };

            if (etherBalance < 0.1m)
            {
                guiStyle.normal.textColor = Color.red;
            }
            else if ((etherBalance > 0.1m) && (etherBalance < 0.5m))
            {
                guiStyle.normal.textColor = Color.magenta;
            }
            else
            {
                guiStyle.normal.textColor = Color.black;
            }
            
            return guiStyle;
        }
    }
}