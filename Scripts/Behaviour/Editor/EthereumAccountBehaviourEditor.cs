using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Nethereum.Unity.Behaviours
{
    [CustomEditor(typeof(EthereumAccountBehaviour))]
    [CanEditMultipleObjects]
    public class EthereumAccountBehaviourEditor : Editor
    {
        int _lastRecordedTransferCount = 0;

        List<string> _recentTransferDescriptions = new List<string>();

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

            if (GUILayout.Button("Refund a Token of Each Type"))
            {
                if (EditorUtility.DisplayDialog("Refund 1 token from each collection?",
                                                "Are you sure you want to refund 1 token from each collected pool?", 
                                                "Refund Them", "Keep Them"))
                {
                    _accountBehaviour.RefundOneTokenOfEachType();
                }
            }

            if (GUILayout.Button("Refund All Tokens"))
            {
                if (EditorUtility.DisplayDialog("Refund all tokens?",
                                                "Are you sure you want to refund all tokens?",
                                                "Refund Them", "Keep Them"))
                {
                    _accountBehaviour.RefundAllTokens();
                }
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

                if (_lastRecordedTransferCount != latestTransferCount)
                {
                    _recentTransferDescriptions.Clear();

                    int trxIndex = latestTransferCount;
                    foreach (var eventLog in mostRecentTransfers)
                    {
                        var transfer = eventLog.Event;

                        string eventMessage =
                            "Trx # [" + trxIndex + "] ->" +
                            ((transfer.To == _accountBehaviour.PublicAddress) ? "Received [" : "Sent [") +
                            transfer.Value + "] tokens of Token (" +
                            _accountBehaviour.GetTokenName(transfer.Id) + ") [" + transfer.Id + "]";

                        _recentTransferDescriptions.Add(eventMessage);

                        --trxIndex;
                    }

                    _lastRecordedTransferCount = latestTransferCount;
                }

                foreach (var transferDescription in _recentTransferDescriptions)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.TextArea(transferDescription);
                    GUILayout.EndHorizontal();
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