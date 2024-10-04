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
        private List<string> _tokenOwnershipDescriptions = new List<string>();

        [SerializeField]
        private List<EthereumTokenOwnership> _tokenOwnerships = new List<EthereumTokenOwnership>();

        private Dictionary<string, long> tokenAmounts = new Dictionary<string, long>();

        public MultiTokenContract Contract { get { return _contract; } }

        void Start()
        {
            Debug.Log("Debug: EAB (" + _name + ") has awakened!");

            _tokenOwnershipDescriptions.Clear();
            _tokenOwnerships.Clear();

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

                var totalBalance = await erc1155Service.TotalSupplyQueryAsync(mintNode.TokenId);

                long balanceNum      = UnityERC1155ServiceFactory.ConvertBigIntegerToLong(balance);
                long tokenIdNum      = UnityERC1155ServiceFactory.ConvertBigIntegerToLong(mintNode.TokenId);
                long totalBalanceNum = UnityERC1155ServiceFactory.ConvertBigIntegerToLong(totalBalance);

                Debug.Log("DEBUG: The current balance of ERC1155 contract at (" + contractNode.ContractName +
                          ") of Game Token Id (" + mintNode.TokenId + ") for EAB (" + _publicAddress + ") is [" + balanceNum + "]");

                mintNode.SetTokenBalance(balanceNum);

                _tokenOwnershipDescriptions.Add("Token (" + mintNode.TokenId + ") -> Balance: [" + balanceNum + "]");

                _tokenOwnerships.Add(new EthereumTokenOwnership(tokenIdNum, balanceNum, totalBalanceNum));

                tokenAmounts[mintNode.TokenSymbol] = balanceNum;
            }
            else
            {
                Debug.Log("ERROR! UnityERC1155ServiceSingleton::RefreshTokenAmount() -> Provided behaviour is null.");
            }
        }

#endif


    }
}
