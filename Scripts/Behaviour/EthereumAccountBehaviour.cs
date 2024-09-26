using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Nethereum.Contracts.UnityERC1155;
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

        public MultiTokenContract Contract { get { return _contract; } }

        void Start()
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
                            RefreshTokenAmount(this, mintNode);
                        }
                    }
                }
            }
        }

        public async void RefreshTokenAmount(EthereumAccountBehaviour accountBehaviour, MultiTokenMintNode mintNode)
        {
            if (accountBehaviour != null)
            {
                var contractNode = accountBehaviour.Contract.GetRootNode();

                var erc1155Service = UnityERC1155ServiceFactory.CreateService(_contract);

                Debug.Log("DEBUG: At ERC1155 contract at (" + contractNode.ContractName +
                          "), there was a minted token refresh issued for Game Token Id (" + mintNode.TokenId + 
                          ") with a starting balance of [" + mintNode.InitialTokenBalance + "]");

                var balance =
                    await erc1155Service.BalanceOfQueryAsync(_publicAddress, mintNode.TokenId);

                long balanceNum = UnityERC1155ServiceFactory.ConvertBigIntegerToLong(balance);

                Debug.Log("DEBUG: The current balance of ERC1155 contract at (" + contractNode.ContractName +
                          ") of Game Token Id (" + mintNode.TokenId + ") for EAB (" + _publicAddress + ") is [" + balanceNum + "]");

                mintNode.SetTokenBalance(balanceNum);

                _tokenMembershipSymbols.Add(mintNode.TokenSymbol);
                _tokenMembershipAmounts.Add(balanceNum);

                tokenAmounts[mintNode.TokenSymbol] = balanceNum;
            }
            else
            {
                Debug.Log("ERROR! UnityERC1155ServiceSingleton::RefreshTokenAmount() -> Provided behaviour is null.");
            }
        }

        public void SetTokenBalance(string tokenSymbol, long newTokenBalance)
        {
            int tokenIndex = _tokenMembershipSymbols.IndexOf(tokenSymbol);
            if (tokenIndex >= 0)
            {
                _tokenMembershipAmounts[tokenIndex] = newTokenBalance;

                tokenAmounts[tokenSymbol] = newTokenBalance;
            }            
        }

#endif


    }
}
