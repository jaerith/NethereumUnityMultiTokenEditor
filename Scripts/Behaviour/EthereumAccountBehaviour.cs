using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

using Nethereum.BlockchainProcessing.Processor;
using Nethereum.Contracts.UnityERC1155;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3.Accounts;

using UnityEngine;

using Nethereum.Unity.MultiToken;
using Nethereum.Contracts;
using Nethereum.Contracts.UnityERC1155.ContractDefinition;
using UnityEditor;

namespace Nethereum.Unity.Behaviours
{
    public class EthereumAccountBehaviour : MonoBehaviour
    {
        public delegate void AnnounceAction(EthereumAccountBehaviour accountBehaviour);
        public static event AnnounceAction OnAnnounce;

        [SerializeField]
        private string _name;

        public string Name { get { return _name; } }

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

        [SerializeField]
        private int _displayLatestTransfersHistoryCount = 3;

        public int DisplayLatestTransfersHistoryCount { get { return _displayLatestTransfersHistoryCount; } }

        private Dictionary<System.Numerics.BigInteger, long> _tokenIdAmounts = 
            new Dictionary<System.Numerics.BigInteger, long>();

        private Dictionary<System.Numerics.BigInteger, string> _tokenNames = 
            new Dictionary<System.Numerics.BigInteger, string>();

        public MultiTokenContract Contract { get { return _contract; } }

        public decimal CurrentEtherBalance { get; protected set; }

        private bool _executingTransferHistoryRetrieval = false;

        private bool _alreadyAnnounced = false;

        private float _timePassedInSeconds = 0.0f;
        private int   _lastTimeThreshold   = 0;

        public string TokenTransferLogsLastTimeUpdated { get; protected set; }

        private System.Numerics.BigInteger lastBlockTransferMilestone = 0;

        private List<TransactionReceiptVO> _contractTransactions = new List<TransactionReceiptVO>();

        private List<EventLog<UnityErc1155TransferSingleEvent>> _erc1155TransferEventLogs = 
            new List<EventLog<UnityErc1155TransferSingleEvent>>();

        public List<EventLog<UnityErc1155TransferSingleEvent>> LatestTokenTransfers 
        { 
            get { return new List<EventLog<UnityErc1155TransferSingleEvent>>(_erc1155TransferEventLogs); }
        }

        public event Action<EthereumBalanceChangeEvent> onBalanceUpdated;

        void Start()
        {
            Debug.Log("Debug: EAB (" + _name + ") has started!");

            var account =
                new Account(_privateKey, (_contract != null) ? _contract.ChainId : null);

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

                RefreshEtherBalance();

                RefreshAllTokenAmounts();
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
            if ((timePassed > 1) && (timePassed > _lastTimeThreshold))
            {
                _lastTimeThreshold = timePassed;

                if (!_alreadyAnnounced && (OnAnnounce != null))
                {
                    Debug.Log("Invocation List stands at count[" + OnAnnounce.GetInvocationList().Length + "]");

                    OnAnnounce(this);
                    _alreadyAnnounced = true;
                }

                if ((_lastTimeThreshold % _refreshTokenIntervalInSeconds) == 0)
                {
                    RefreshEtherBalance();

                    RefreshAllTokenAmounts();

                    if (!_executingTransferHistoryRetrieval)
                    {
                        RetrieveRecentContractTransferHistory();
                    }
                }
            }
        }

        public List<System.Numerics.BigInteger> GetTokenIdsWithBalances()
        {
            List<System.Numerics.BigInteger> tokenIds = new List<System.Numerics.BigInteger>();

            foreach (var tokenId in _tokenIdAmounts.Keys)
            {
                var tokenIdBalance = _tokenIdAmounts[tokenId];
                if (tokenIdBalance > 0)
                {
                    tokenIds.Add(tokenId);
                }
            }

            return tokenIds;
        }

        public string GetTokenName(System.Numerics.BigInteger tokenId)
        {
            return _tokenNames.ContainsKey(tokenId) ? _tokenNames[tokenId] : "Unknown Token";
        }

        public long GetTokensOwned(System.Numerics.BigInteger tokenId) 
        {
            return _tokenIdAmounts.ContainsKey(tokenId) ? _tokenIdAmounts[tokenId] : 0;
        }

        public string GetUserFriendlyLatestTransactionsReport(int displayCount = 3)
        {
            StringBuilder transactionsReport = new StringBuilder();

            if (_erc1155TransferEventLogs.Count > 0)
            {
                var latestTransfers = LatestTokenTransfers;

                var latestTransferCount = latestTransfers.Count;

                var mostRecentTransfers =
                    latestTransfers.Count > displayCount ?
                    latestTransfers.GetRange((latestTransferCount - displayCount), displayCount) :
                    latestTransfers;

                mostRecentTransfers.Reverse();

                int trxIndex = latestTransferCount;
                foreach (var eventLog in mostRecentTransfers)
                {
                    var transfer = eventLog.Event;

                    string eventMessage =
                        "Trx # [" + trxIndex + "] ->" +
                        ((transfer.To == PublicAddress) ? "Received [" : "Sent [") +
                        transfer.Value + "] tokens of Token (" +
                        GetTokenName(transfer.Id) + ") [" + transfer.Id + "]";

                    transactionsReport.AppendLine(eventMessage);

                    --trxIndex;
                }
            }

            return transactionsReport.ToString();
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
                        var mintNode = (MultiTokenMintNode) node;

                        RefreshTokenAmount(mintNode);
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

                if ((_audioSourceEtherUpdated != null) && (etherBalance != CurrentEtherBalance))
                {
                    _audioSourceEtherUpdated.Play();
                }

                CurrentEtherBalance = etherBalance;
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

                _tokenNames[mintNode.TokenId] = mintNode.TokenName;
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

        public async void RefundOneTokenOfEachType()
        {
            if (_contract != null)
            {
                Debug.Log("DEBUG: EthereumAccountBehaviour::RefundAllOwnedTokens() -> Refunding 1 token (of each type) owned by address (" +
                          PublicAddress + ").");

                var erc1155Service = UnityERC1155ServiceFactory.CreateService(_contract, _privateKey);

                foreach (var node in _contract.GetAllNodes())
                {
                    if (node.IsDeployed && (node is MultiTokenMintNode))
                    {
                        var mintNode = (MultiTokenMintNode)node;

                        var currTokensHeld = _tokenIdAmounts[mintNode.TokenId];

                        if (mintNode.HasTokenOwner(_publicAddress) || (currTokensHeld > 0))
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

        public async void RefundAllTokens()
        {
            if (_contract != null)
            {
                Debug.Log("DEBUG: EthereumAccountBehaviour::RefundAllTokens() -> Refunding all tokens owned by address (" +
                          PublicAddress + ").");

                var erc1155Service = UnityERC1155ServiceFactory.CreateService(_contract, _privateKey);

                foreach (var node in _contract.GetAllNodes())
                {
                    if (node.IsDeployed && (node is MultiTokenMintNode))
                    {
                        var mintNode = (MultiTokenMintNode)node;

                        var currTokensHeld = _tokenIdAmounts[mintNode.TokenId];

                        if (mintNode.HasTokenOwner(_publicAddress) || (currTokensHeld > 0))
                        {
                            var transfer =
                                await erc1155Service
                                      .SafeTransferFromRequestAndWaitForReceiptAsync(_publicAddress, mintNode.TokenOwnerAddress, mintNode.TokenId, currTokensHeld, new byte[] { });
                        }
                    }
                }

                RefreshAllTokenAmounts();
            }
        }

        public async void RequestToken(long tokenId)
        {
            if (_contract != null)
            {
                System.Numerics.BigInteger requestedAmount = 1;

                var tokenIdBig = (System.Numerics.BigInteger) tokenId;

                Debug.Log("DEBUG: EthereumAccountBehaviour::RequestToken() -> Requesting [" + requestedAmount + "] token(s) " + 
                          "of Token ID (" + tokenIdBig + ") be sent to (" + PublicAddress + ").");

                foreach (var node in _contract.GetAllNodes())
                {
                    if (node.IsDeployed && (node is MultiTokenMintNode))
                    {
                        var mintNode = (MultiTokenMintNode)node;

                        if (mintNode.TokenId == tokenIdBig)
                        {
                            mintNode.TransferToken(this);

                            break;
                        }
                    }
                }

                RefreshAllTokenAmounts();
            }
        }

        public async void RetrieveRecentContractTransactionHistory()
        {
            if (_contract != null) 
            {
                List<TransactionReceiptVO> foundTransactions = new List<TransactionReceiptVO>();

                var latestBlockNumber =
                    await _contract.GetWeb3().Eth.Blocks.GetBlockNumber.SendRequestAsync();

                Debug.Log("DEBUG: Latest block number is: [" + latestBlockNumber.Value + "]");
                Debug.Log("DEBUG: Checking out transactions sent to contract at address[" + _contract.GetRootNode().ContractAddress + "]");

                //create our processor
                var processor =
                    _contract.GetWeb3().Processing.Blocks.CreateBlockProcessor(steps =>
                    {
                        //for performance we add criteria before we have the receipt to prevent unecessary data retrieval
                        //we only want to retrieve receipts if the tx was sent to the contract
                        steps
                    .TransactionStep
                    .SetMatchCriteria(t => (t.Transaction.IsFrom(_contract.PublicAddress) && t.Transaction.IsTo(PublicAddress)) ||
                                           (t.Transaction.IsFrom(PublicAddress) && t.Transaction.IsTo(_contract.PublicAddress)));

                        steps.TransactionReceiptStep.AddSynchronousProcessorHandler(tx => foundTransactions.Add(tx));
                    });

                //if we need to stop the processor mid execution - call cancel on the token
                var cancellationToken = new CancellationToken();

                //crawl the blocks
                await processor.ExecuteAsync(
                    toBlockNumber: (latestBlockNumber.Value + 25),
                    cancellationToken: cancellationToken,
                    startAtBlockNumberIfNotProcessed: (latestBlockNumber.Value - 25));

                lock (_contractTransactions)
                {
                    _contractTransactions.AddRange(foundTransactions);
                }

                Debug.Log($"Transactions. Count: {_contractTransactions.Count}");
            }
        }

        public async void RetrieveRecentContractTransferHistory()
        {
            if (_contract != null)
            {
                try
                {
                    _executingTransferHistoryRetrieval = true;

                    var foundContractTransferLogs = new List<EventLog<UnityErc1155TransferSingleEvent>>();

                    var erc1155TransferHandler =
                        new EventLogProcessorHandler<UnityErc1155TransferSingleEvent>(eventLog => foundContractTransferLogs.Add(eventLog));

                    var processingHandlers = new ProcessorHandler<FilterLog>[] { erc1155TransferHandler };

                    var contractFilter = new NewFilterInput
                    {
                        Address = new[] { _contract.GetRootNode().ContractAddress }
                    };

                    var logsProcessor =
                        _contract.GetWeb3().Processing.Logs.CreateProcessor(
                              logProcessors: processingHandlers, filter: contractFilter);

                    //if we need to stop the processor mid execution - call cancel on the token
                    var cancellationToken = new CancellationToken();

                    var latestBlockNumber =
                        await _contract.GetWeb3().Eth.Blocks.GetBlockNumber.SendRequestAsync();

                    if (lastBlockTransferMilestone == 0)
                    {
                        lastBlockTransferMilestone = latestBlockNumber.Value - 25;
                    }

                    if (lastBlockTransferMilestone > 0) 
                    {
                        Debug.Log("DEBUG: Latest block number is: [" + latestBlockNumber.Value + "]");
                        Debug.Log("DEBUG: Checking out transfers sent to contract at address[" + _contract.GetRootNode().ContractAddress + "]");

                        //crawl the required block range
                        await logsProcessor.ExecuteAsync(
                            toBlockNumber: latestBlockNumber.Value,
                            cancellationToken: cancellationToken,
                            startAtBlockNumberIfNotProcessed: lastBlockTransferMilestone);

                        lock (_erc1155TransferEventLogs)
                        {
                            var relevantTransferLogs =
                                foundContractTransferLogs.Where(tl => (tl.Event.From == PublicAddress) || ((tl.Event.To == PublicAddress)));

                            _erc1155TransferEventLogs.AddRange(relevantTransferLogs);
                        }

                        Debug.Log($"ERC1155 Logs found: [{_erc1155TransferEventLogs.Count}].");

                        lastBlockTransferMilestone = latestBlockNumber;

                        TokenTransferLogsLastTimeUpdated = DateTime.Now.ToString("MM/dd/yy H:mm:ss");
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    _executingTransferHistoryRetrieval = false;
                }                
            }
        }

        public async Task<long> TransferTokens(EthereumAccountBehaviour tokenRecipient, long tokenId, long tokenBalance)
        {
            long tokensTransferred = 0;

            if (_contract != null)
            {
                var tokenIdBig      = (System.Numerics.BigInteger) tokenId;
                var tokenBalanceBig = (System.Numerics.BigInteger) tokenBalance;

                Debug.Log("DEBUG: EthereumAccountBehaviour::TransferTokens() -> Starting to transfer [" + tokenBalanceBig +
                          "] tokens of Token ID (" + tokenIdBig + ") that are owned by address (" + PublicAddress +
                          ") to the recipient address (" + tokenRecipient.PublicAddress + ")");

                var erc1155Service = UnityERC1155ServiceFactory.CreateService(_contract, _privateKey);

                var transfer =
                    await erc1155Service
                            .SafeTransferFromRequestAndWaitForReceiptAsync(PublicAddress, tokenRecipient.PublicAddress, tokenIdBig, tokenBalanceBig, new byte[] { });

                if (transfer.Succeeded())
                {
                    tokensTransferred = tokenBalance;
                }

                RefreshAllTokenAmounts();

                tokenRecipient.RefreshAllTokenAmounts();
            }

            return tokensTransferred;
        }

        private void ResetOwnershipProperties()
        {
            _tokenOwnershipDescriptions.Clear();
            _tokenOwnerships.Clear();
        }

#endif


    }
}
