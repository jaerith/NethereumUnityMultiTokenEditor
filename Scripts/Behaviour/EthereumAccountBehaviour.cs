using System;
using System.Collections;
using System.Collections.Generic;

using Nethereum.Contracts.UnityERC1155;
using Nethereum.Util;
using Nethereum.Web3.Accounts;

using UnityEngine;
using UnityEditor;

using Nethereum.Unity.MultiToken;

namespace Nethereum.Unity.Behaviours
{
    public class EthereumAccountBehaviour : MonoBehaviour
    {
        [SerializeField]
        private string _name;

        [SerializeField]
        private string _privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";

        private string _publicAddress = "0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266"; // "0x96092AE9c60b39Acee9f73fFF2902a03143073E7";

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

        [SerializeField]
        private AudioSource _audioSourceEtherUpdated = null;

        private Dictionary<System.Numerics.BigInteger, long> _tokenIdAmounts = new Dictionary<System.Numerics.BigInteger, long>();

        private Dictionary<string, long> _tokenSymbolAmounts = new Dictionary<string, long>();

        public MultiTokenContract Contract { get { return _contract; } }

        private decimal _currentEtherBalance = 0.0m;

        public decimal CurrentEtherBalance { get { return _currentEtherBalance; } }

        private float _timePassedInSeconds = 0.0f;
        private int   _lastTimeThreshold   = 0;

        public event Action<EthereumBalanceChangeEvent> onBalanceUpdated;

        void Start()
        {
            Debug.Log("Debug: EAB (" + _name + ") has started!");

            var account = new Account(_privateKey, _contract.ChainId);

            _publicAddress = account.Address;

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
            _timePassedInSeconds += Time.deltaTime;

            int timePassed = (int) _timePassedInSeconds;
            if ((timePassed > 1) && (timePassed > _lastTimeThreshold) && ((timePassed % _refreshTokenIntervalInSeconds) == 0))
            {
                _lastTimeThreshold = timePassed;

                RefreshEtherBalance();

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

        public async void RefreshEtherBalance()
        {
            if (_contract != null)
            {
                var hexBalanceAmount =
                    await _contract.GetWeb3().Eth.GetBalance.SendRequestAsync(PublicAddress);

                var etherBalance = UnitConversion.Convert.FromWei(hexBalanceAmount.Value);

                Debug.Log("DEBUG: Refreshed Ether balance for account (" + PublicAddress + "): [" + etherBalance + "].");

                if ((_audioSourceEtherUpdated != null) && (etherBalance != _currentEtherBalance))
                {
                    _audioSourceEtherUpdated.Play();
                }

                _currentEtherBalance = etherBalance;
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

                    var balanceUpdatedEvent =
                        new EthereumBalanceChangeEvent()
                        {
                            AccountPublicAddress = PublicAddress
                            , TokenId = UnityERC1155ServiceFactory.ConvertBigIntegerToLong(mintNode.TokenId)
                            , TokenPreviousBalance = currBalanceNum
                            , TokenCurrentBalance = balanceNum
                            , TotalTokenSupply = totalBalanceNum
                        };

                    onBalanceUpdated(balanceUpdatedEvent);
                }

                _tokenOwnershipDescriptions.Add("Token (" + mintNode.TokenId + ") -> Balance: [" + balanceNum + "]");

                _tokenOwnerships.Add(ScriptableObject.CreateInstance<EthereumTokenOwnership>().Init(this, tokenIdNum, balanceNum, totalBalanceNum));

                _tokenIdAmounts[mintNode.TokenId] = balanceNum;

                _tokenSymbolAmounts[mintNode.TokenSymbol] = balanceNum;
            }
            else
            {
                Debug.Log("ERROR! EthereumAccountBehaviour::RefreshTokenAmount() -> Contract is null.");
            }
        }

        public async void RefundOwnedTokens(long tokenId, long tokenBalance)
        {
            if (_contract != null)
            {
                var tokenIdBig      = (System.Numerics.BigInteger) tokenId;
                var tokenBalanceBig = (System.Numerics.BigInteger) tokenBalance;

                Debug.Log("DEBUG: EthereumAccountBehaviour::RefundOwnedTokens() -> Refunding [" + tokenBalanceBig + "] tokens of Token ID (" +
                          tokenIdBig + ") that are owned by address (" + PublicAddress + ").");

                var erc1155Service = UnityERC1155ServiceFactory.CreateService(_contract, _privateKey);

                foreach (var node in _contract.GetAllNodes())
                {
                    if (node.IsDeployed && (node is MultiTokenMintNode))
                    {
                        var mintNode = (MultiTokenMintNode)node;

                        if (mintNode.HasTokenOwner(_publicAddress) && (mintNode.TokenId == tokenIdBig))
                        {
                            var transfer =
                                await erc1155Service
                                      .SafeTransferFromRequestAndWaitForReceiptAsync(_publicAddress, mintNode.TokenOwnerAddress, mintNode.TokenId, tokenBalanceBig, new byte[] { });

                            break;
                        }
                    }
                }

                RefreshAllTokenAmounts();
            }
        }

        public async void RefundAllOwnedTokens()
        {
            if (_contract != null)
            {
                Debug.Log("DEBUG: EthereumAccountBehaviour::RefundAllOwnedTokens() -> Refunding all tokens owned by address (" +
                          PublicAddress + ").");

                var erc1155Service = UnityERC1155ServiceFactory.CreateService(_contract, _privateKey);

                foreach (var node in _contract.GetAllNodes())
                {
                    if (node.IsDeployed && (node is MultiTokenMintNode))
                    {
                        var mintNode = (MultiTokenMintNode)node;
                        if (mintNode.HasTokenOwner(_publicAddress))
                        {
                            var transfer =
                                await erc1155Service
                                      .SafeTransferFromRequestAndWaitForReceiptAsync(_publicAddress, mintNode.TokenOwnerAddress, mintNode.TokenId, 1, new byte[] { });
                        }
                    }
                }

                RefreshAllTokenAmounts();
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
