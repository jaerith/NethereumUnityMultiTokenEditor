using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Nethereum.Contracts.UnityERC1155;

namespace Nethereum.Unity.Behaviours.UI
{
    public class NumteHudUI : MonoBehaviour
    {
        [SerializeField]
        EthereumAccountBehaviour _accountBehaviour;

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

        int  _targetTokenIndex = 0;
        long _targetTokenId    = 0;

        List<System.Numerics.BigInteger> _tokensAvailable = new List<System.Numerics.BigInteger>();

        private float _timePassedInSeconds = 0.0f;
        private int   _lastTimeThreshold   = 0;

        // Start is called before the first frame update
        void Start()
        {
            UpdateTokenInfo();

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

                UpdateTokenInfo();

                UpdateDisplay();
            }
        }

        public void DecrementTokenIndex()
        {
            _targetTokenIndex = (_targetTokenIndex-1) < 0 ?  0 : (_targetTokenIndex - 1);

            Debug.Log("DEBUG: NumteHudUI::DecrementTokenIndex() -> Incremented index to [" + _targetTokenIndex + "]");
        }

        public void IncrementTokenIndex()
        {
            Debug.Log("DEBUG: NumteHudUI::IncrementTokenIndex() -> The tokens with available balances are: (" +
                      String.Join(",", _tokensAvailable) + ")");

            _targetTokenIndex = 
                (_targetTokenIndex+1) < _tokensAvailable.Count ? (_targetTokenIndex+1) : _targetTokenIndex;

            Debug.Log("DEBUG: NumteHudUI::IncrementTokenIndex() -> Incremented index to [" + _targetTokenIndex + "]");
        }

        private void UpdateDisplay()
        {
            if ((_accountBehaviour != null) && (_targetTokenId > 0))
            {
                System.Numerics.BigInteger targetTokenIdBI = _targetTokenId;

                _nameText.text     = _accountBehaviour.Name;
                _contractText.text = _accountBehaviour.Contract.GetRootNode().ContractName;
                _tokenText.text    = _accountBehaviour.GetTokenName(targetTokenIdBI);

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

        private void UpdateTokenInfo()
        {
            if (_accountBehaviour != null)
            {
                _tokensAvailable = _accountBehaviour.GetTokenIdsWithBalances();

                Debug.Log("DEBUG: NumteHudUI::UpdateTokenInfo() -> The tokens with available balances are: (" +
                          String.Join(",", _tokensAvailable) + ")");

                _targetTokenId =
                    (_tokensAvailable.Count > 0) ?
                        UnityERC1155ServiceFactory.ConvertBigIntegerToLong(_tokensAvailable[_targetTokenIndex]) : 0;
            }
        }
    }
}
