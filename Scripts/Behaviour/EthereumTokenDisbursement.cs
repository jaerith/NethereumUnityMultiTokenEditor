using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Nethereum.Unity.MultiToken;

namespace Nethereum.Unity.Behaviours
{
    public class EthereumTokenDisbursement : MonoBehaviour
    {
        public delegate void DisburseAction(EthereumTokenDisbursement tokenDisbursement);
        public static event DisburseAction OnDisburse;

        [SerializeField]
        private string _name;

        public string Name { get { return _name; } }

#if UNITY_EDITOR

        [SerializeField]
        private MultiTokenContract _contract = null;

        public MultiTokenContract Contract { get { return _contract; } }

        [SerializeField]
        private AudioSource _audioSourceTokensDispersed = null;

        [SerializeField]
        private bool _autofillAccounts = false;

        [SerializeField]
        private HashSet<EthereumAccountBehaviour> _targetAccounts = new HashSet<EthereumAccountBehaviour>();

        [SerializeField]
        private HashSet<long> _disbursedTokens = new HashSet<long>();

        private float _timePassedInSeconds = 0.0f;
        private int   _lastTimeThreshold   = 0;

        void Start()
        {
            Debug.Log("Debug: ETD (" + _name + ") has started!");

            if (_autofillAccounts)
            {
                EthereumAccountBehaviour.OnAnnounce += AddTargetAccount;
            }
        }

        // Update is called once per frame
        void Update()
        {
            AdjustTimer();
        }

        public void AddTargetAccount(EthereumAccountBehaviour accountBehaviour)
        {
            _targetAccounts.Add(accountBehaviour);
        }

        private void AdjustTimer()
        {
            _timePassedInSeconds += Time.deltaTime;

            int timePassed = (int)_timePassedInSeconds;
            if ((timePassed > 1) && (timePassed > _lastTimeThreshold))
            {
                _lastTimeThreshold = timePassed;

                // NOTE: Should anything be done here
            }
        }

        public void Disburse()
        {
            if (_contract != null)
            {
                bool tokensDispersed = false;

                foreach (var disburseToken in _disbursedTokens)
                {
                    var tokenIdBig = (System.Numerics.BigInteger) disburseToken;

                    foreach (var node in _contract.GetAllNodes())
                    {
                        if (node.IsDeployed && (node is MultiTokenMintNode))
                        {
                            var mintNode = (MultiTokenMintNode) node;

                            Debug.Log("DEBUG: EthereumAccountBehaviour::RequestToken() -> " +
                                      "Requesting [1] token(s) of Token ID (" + tokenIdBig + 
                                      ") be sent to all target accounts in list.");

                            if (mintNode.TokenId == tokenIdBig)
                            {
                                _targetAccounts.ToList().ForEach(t => mintNode.TransferToken(t));

                                tokensDispersed = true;

                                break;
                            }
                        }
                    }
                }

                if (tokensDispersed && (_audioSourceTokensDispersed != null)) 
                {
                    _audioSourceTokensDispersed.Play();
                }

                if (OnDisburse != null)
                {
                    OnDisburse(this);
                }                
            }
        }

#endif


    }
}

