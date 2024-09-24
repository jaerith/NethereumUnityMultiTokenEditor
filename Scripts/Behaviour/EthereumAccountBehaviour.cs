using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Nethereum.Unity.MultiToken;

namespace Nethereum.Unity.Behaviours
{
    public class EthereumAccountBehaviour : MonoBehaviour
    {
        [SerializeField]
        private string _name;

        [SerializeField]
        private string _publicAddress = "0x96092AE9c60b39Acee9f73fFF2902a03143073E7";

        public string PublicAddress { get { return _publicAddress; } }

#if UNITY_EDITOR

        [SerializeField]
        private MultiTokenContract _contract = null;

        [SerializeField]
        private List<string> _tokenMembershipSymbols = new List<string>();

        [SerializeField]
        private List<long> _tokenMembershipAmounts = new List<long>();

        private Dictionary<string, long> tokenAmounts = new Dictionary<string, long>();

        private void Awake()
        {
            Debug.Log("Debug: EAB (" + _name + ") has awakened!");

            if (_contract != null)
            {
                foreach (var node in _contract.GetAllNodes()) 
                { 
                    if (node.IsDeployed && (node is MultiTokenMintNode))
                    { 
                        var mintNode = (MultiTokenMintNode) node;
                        if (mintNode.HasTokenOwner(_publicAddress))
                        {
                            _tokenMembershipSymbols.Add(mintNode.TokenSymbol);
                            _tokenMembershipAmounts.Add(1);
                            tokenAmounts[mintNode.TokenSymbol] = 1;

                            // NOTE : Use Async method to update token amount

                            Debug.Log("DEBUG: EAB (" + _name + ") is known to own (" + mintNode.TokenSymbol + ") tokens!");
                        }
                    }
                }
            }
        }

#endif


    }
}
