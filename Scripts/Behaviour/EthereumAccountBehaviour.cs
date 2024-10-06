using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Nethereum.Contracts.UnityERC1155;
using Nethereum.Unity.MultiToken;
using UnityEditor;

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

        [SerializeField]
        private int _refreshTokenIntervalInSeconds = 15;

        [SerializeField]
        private AudioSource _audioSourceTokenUpdated = null;

        private Dictionary<System.Numerics.BigInteger, long> _tokenIdAmounts = new Dictionary<System.Numerics.BigInteger, long>();

        private Dictionary<string, long> _tokenSymbolAmounts = new Dictionary<string, long>();

        public MultiTokenContract Contract { get { return _contract; } }

        private float timePassedInSeconds = 0.0f;
        private int   lastTimeThreshold   = 0;

        void Start()
        {
            Debug.Log("Debug: EAB (" + _name + ") has awakened!");

            ResetOwnershipProperties();

            if (_contract != null)
            {
                foreach (var node in _contract.GetAllNodes()) 
                { 
                    if (node.IsDeployed && (node is MultiTokenMintNode))
                    { 
                        var mintNode = (MultiTokenMintNode) node;
                        if (mintNode.HasTokenOwner(_publicAddress))
                        {
                            RefreshTokenAmount(mintNode);
                        }
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            AdjustTimer();
        }

        private void AdjustTimer()
        {
            timePassedInSeconds += Time.deltaTime;

            int timePassed = (int) timePassedInSeconds;
            if ((timePassed > 1) && (timePassed > lastTimeThreshold) && ((timePassed % _refreshTokenIntervalInSeconds) == 0))
            {
                lastTimeThreshold = timePassed;

                RefreshAllTokenAmounts();
            }
        }

        public void RefreshAllTokenAmounts()
        {
            if (_contract != null)
            {
                ResetOwnershipProperties();

                Debug.Log("DEBUG: EthereumAccountBehaviour::RefreshAllTokenAmounts() -> Refreshing all minted token balances owned by account (" +
                          PublicAddress + ").");

                foreach (var node in _contract.GetAllNodes())
                {
                    if (node.IsDeployed && (node is MultiTokenMintNode))
                    {
                        var mintNode = (MultiTokenMintNode)node;
                        if (mintNode.HasTokenOwner(_publicAddress))
                        {
                            RefreshTokenAmount(mintNode);
                        }
                    }
                }
            }
        }

        public async void RefreshTokenAmount(MultiTokenMintNode mintNode)
        {
            if (Contract != null)
            {
                var contractNode = Contract.GetRootNode();

                var erc1155Service = UnityERC1155ServiceFactory.CreateService(_contract);

                Debug.Log("DEBUG: At ERC1155 contract at (" + contractNode.ContractName +
                          "), there was a minted token refresh issued for Game Token Id (" + mintNode.TokenId + 
                          ") with a starting balance of [" + mintNode.InitialTokenBalance + "]");

                var balance =
                    await erc1155Service.BalanceOfQueryAsync(_publicAddress, mintNode.TokenId);

                var totalBalance = await erc1155Service.TotalSupplyQueryAsync(mintNode.TokenId);

                long currBalanceNum  = _tokenIdAmounts.ContainsKey(mintNode.TokenId) ? _tokenIdAmounts[mintNode.TokenId] : 0;
                long balanceNum      = UnityERC1155ServiceFactory.ConvertBigIntegerToLong(balance);
                long tokenIdNum      = UnityERC1155ServiceFactory.ConvertBigIntegerToLong(mintNode.TokenId);
                long totalBalanceNum = UnityERC1155ServiceFactory.ConvertBigIntegerToLong(totalBalance);

                Debug.Log("DEBUG: The current balance of ERC1155 contract at (" + contractNode.ContractName +
                          ") of Game Token Id (" + mintNode.TokenId + ") for EAB (" + _publicAddress + ") is [" + balanceNum + "]");

                if ((balanceNum != currBalanceNum) && (_audioSourceTokenUpdated != null))
                {
                    _audioSourceTokenUpdated.Play();
                }

                _tokenOwnershipDescriptions.Add("Token (" + mintNode.TokenId + ") -> Balance: [" + balanceNum + "]");

                _tokenOwnerships.Add(ScriptableObject.CreateInstance<EthereumTokenOwnership>().Init(tokenIdNum, balanceNum, totalBalanceNum));

                _tokenIdAmounts[mintNode.TokenId] = balanceNum;

                _tokenSymbolAmounts[mintNode.TokenSymbol] = balanceNum;
            }
            else
            {
                Debug.Log("ERROR! EthereumAccountBehaviour::RefreshTokenAmount() -> Contract is null.");
            }
        }

        private void ResetOwnershipProperties()
        {
            _tokenOwnershipDescriptions.Clear();
            _tokenOwnerships.Clear();
        }

#endif


    }
}
