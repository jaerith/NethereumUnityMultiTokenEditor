using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Nethereum.Unity.Behaviours;

namespace Nethereum.Unity.Behaviours.UI
{
    public class NumteHudSimpleUI : MonoBehaviour
    {
        [SerializeField]
        EthereumAccountBehaviour _accountBehaviour;

        [SerializeField]
        long _targetTokenId = 0;

        [SerializeField]
        Text _nameText;

        [SerializeField]
        Text _contractText;

        [SerializeField]
        Text _tokenText;

        [SerializeField]
        Text _tokensOwnedText;

        [SerializeField]
        Text _latestTransactionsText;

        private float _timePassedInSeconds = 0.0f;
        private int   _lastTimeThreshold   = 0;

        // Start is called before the first frame update
        void Start()
        {
            UpdateDisplay();
        }

        // Update is called once per frame
        void Update()
        {
            AdjustTimer();
        }

        private void AdjustTimer()
        {
            _timePassedInSeconds += Time.deltaTime;

            int timePassed = (int)_timePassedInSeconds;
            if ((timePassed > 1) && (timePassed > _lastTimeThreshold))
            {
                _lastTimeThreshold = timePassed;

                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            if ((_accountBehaviour != null) && (_targetTokenId > 0))
            {
                System.Numerics.BigInteger targetTokenIdBI = _targetTokenId;

                _nameText.text        = _accountBehaviour.Name;
                _contractText.text    = _accountBehaviour.Contract.GetRootNode().ContractName;
                _tokenText.text       = _accountBehaviour.GetTokenName(targetTokenIdBI);

                Debug.Log("NumteHudSimpleUI::UpdateDisplay() -> Name(" + _nameText.text +
                          "), Contract(" + _contractText.text + "), Token(" + _tokenText.text + ")");

                var tokensOwned = _accountBehaviour.GetTokensOwned(targetTokenIdBI);
                if (tokensOwned > 0)
                {
                    _tokensOwnedText.text = Convert.ToString(tokensOwned);
                }

                _latestTransactionsText.text = _accountBehaviour.GetUserFriendlyLatestTransactionsReport();
            }
        }
    }
}
