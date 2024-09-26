using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Unity.EditorCoroutines.Editor;

using Nethereum.Contracts.Standards.ERC1155;
using Nethereum.Contracts.UnityERC1155;
using Nethereum.Contracts.UnityERC1155.ContractDefinition;
using Nethereum.Signer;
using Nethereum.Unity.Rpc;
using Nethereum.Unity.Contracts;

namespace Nethereum.Unity.MultiToken
{
    public class MultiTokenEditor : EditorWindow
    {
        public const float CanvasSizeWidth  = 4000f;
        public const float CanvasSizeHeight = 4000f;

        public const float BackgroundDimSize = 50f;

        [SerializeField]
        private List<string> _erc1155ContractNames = new List<string>();

        [SerializeField]
        private List<string> _erc1155ContractAddresses = new List<string>();

        private Dictionary<string, string> _erc1155ContractMap = new Dictionary<string, string>();

        MultiTokenContract _selectedContract = null;

        [NonSerialized]
        GUIStyle _nodeContractStyle;

        [NonSerialized]
        GUIStyle _nodeDeployedContractStyle;

        [NonSerialized] 
        GUIStyle _nodeTokenStyle;

        [NonSerialized]
        GUIStyle _nodeDeployedTokenStyle;

        [NonSerialized]
        GUIStyle _nodeDeployedNFTStyle;

        [NonSerialized]
        MultiTokenNode _draggingNode = null;

        [NonSerialized]
        Vector2 _draggingOffset = Vector2.zero;

        [NonSerialized]
        MultiTokenNode _deployingNode = null;

        [NonSerialized]
        MultiTokenNode _mintingNode = null;

        [NonSerialized]
        MultiTokenNode _creatingNode = null;

        [NonSerialized]
        MultiTokenNode _refreshingNode = null;

        [NonSerialized]
        MultiTokenNode _requestingTransferNode = null;

        [NonSerialized]
        MultiTokenNode _requestingBurnNode = null;

        [NonSerialized]
        MultiTokenNode _deletingNode = null;

        [NonSerialized]
        MultiTokenNode _linkingParentNode = null;

        Vector2 _scrollPosition;

        [NonSerialized]
        bool _draggingCanvas = false;

        [NonSerialized]
        Vector2 _draggingCanvasOffset;

        [NonSerialized]
        string _pendingTransferToAddress = null;

        [NonSerialized]
        string _pendingBurnAmount = null;

        [MenuItem("Window/Ethereum/MultiToken Editor")]
        public static void ShowEditorWindow()
        {
            GetWindow(typeof(MultiTokenEditor), false, "Multitoken Editor");
        }

        [OnOpenAssetAttribute(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            MultiTokenContract contractInstance = EditorUtility.InstanceIDToObject(instanceID) as MultiTokenContract;

            if (contractInstance != null)
            {
                ShowEditorWindow();
                return true;
            }

            return false;
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;

            MultiTokenMintNode.OnTransfer += TransferToken;

            _nodeContractStyle = new GUIStyle();
            _nodeContractStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
            _nodeContractStyle.normal.textColor  = Color.white;
            _nodeContractStyle.padding = new RectOffset(30, 30, 30, 30);
            _nodeContractStyle.border  = new RectOffset(18, 18, 18, 18);
            _nodeContractStyle.fixedHeight *= 1.1f;
            _nodeContractStyle.fixedWidth  *= 1.1f;

            _nodeDeployedContractStyle = new GUIStyle();
            _nodeDeployedContractStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
            _nodeDeployedContractStyle.normal.textColor  = Color.white;
            _nodeDeployedContractStyle.padding = new RectOffset(30, 30, 30, 30);
            _nodeDeployedContractStyle.border  = new RectOffset(18, 18, 18, 18);
            _nodeDeployedContractStyle.fixedHeight *= 1.1f;
            _nodeDeployedContractStyle.fixedWidth  *= 1.1f;

            _nodeTokenStyle = new GUIStyle();
            _nodeTokenStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
            _nodeTokenStyle.normal.textColor = Color.white;
            _nodeTokenStyle.padding = new RectOffset(20, 20, 20, 20);
            _nodeTokenStyle.border  = new RectOffset(12, 12, 12, 12);

            _nodeDeployedTokenStyle = new GUIStyle();
            _nodeDeployedTokenStyle.normal.background = EditorGUIUtility.Load("node2") as Texture2D;
            _nodeDeployedTokenStyle.normal.textColor = Color.white;
            _nodeDeployedTokenStyle.padding = new RectOffset(20, 20, 20, 20);
            _nodeDeployedTokenStyle.border  = new RectOffset(12, 12, 12, 12);

            _nodeDeployedNFTStyle = new GUIStyle();
            _nodeDeployedNFTStyle.normal.background = EditorGUIUtility.Load("node6") as Texture2D;
            _nodeDeployedNFTStyle.normal.textColor = Color.white;
            _nodeDeployedNFTStyle.padding = new RectOffset(20, 20, 20, 20);
            _nodeDeployedNFTStyle.border = new RectOffset(12, 12, 12, 12);
        }

        private void OnSelectionChanged() 
        {
            MultiTokenContract contractInstance = Selection.activeObject as MultiTokenContract;

            if (contractInstance != null)
            {
                _selectedContract = contractInstance;

                Repaint();
            }

            if (_erc1155ContractMap.Count == 0) 
            { 
                for (int index = 0; index < _erc1155ContractNames.Count; ++index) 
                {
                    _erc1155ContractMap[_erc1155ContractNames[index]] = _erc1155ContractAddresses[index];
                }
            }

            if (_selectedContract != null) 
            {
                var nodesList = new List<MultiTokenNode>(_selectedContract.GetAllNodes());
                for (int index = 1; index < nodesList.Count(); index++)
                {
                    var node = nodesList[index];
                    if (node is MultiTokenMintNode)
                    {
                        RefreshMintNode((MultiTokenMintNode) node);
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (_selectedContract == null)
            {
                EditorGUILayout.LabelField("No Contract Selected.");
            }
            else
            {
                ProcessEvents();

                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                Rect canvas = GUILayoutUtility.GetRect(CanvasSizeWidth, CanvasSizeHeight);
                DrawBackground(canvas);

                foreach (var node in _selectedContract.GetAllNodes()) 
                {
                    DrawConnections(node);
                }

                foreach (var node in _selectedContract.GetAllNodes())
                {
                    DrawNode(node);
                }

                EditorGUILayout.EndScrollView();

                if (_deployingNode != null)
                {
                    EditorCoroutineUtility.StartCoroutine(DeployErc1155Contract(), this);

                    _deployingNode = null;
                }

                if ((_mintingNode != null) && (_mintingNode is MultiTokenMintNode))
                {
                    MintToken((MultiTokenMintNode) _mintingNode);
                    _mintingNode = null;
                }

                if ((_creatingNode != null) && (_creatingNode is MultiTokenContractNode))
                {
                    _selectedContract.CreateNode((MultiTokenContractNode) _creatingNode);
                    _creatingNode = null;
                }

                if ((_refreshingNode != null) && (_refreshingNode is MultiTokenMintNode))
                {
                    RefreshMintNode((MultiTokenMintNode) _refreshingNode);
                    _refreshingNode = null;
                }

                if ((_requestingTransferNode != null) && (_requestingTransferNode is MultiTokenMintNode))
                {
                    string transferAddress = new string(_pendingTransferToAddress);
                    TransferToken((MultiTokenMintNode) _requestingTransferNode, transferAddress);

                    _requestingTransferNode   = null;
                    _pendingTransferToAddress = string.Empty;
                }

                if ((_requestingBurnNode != null) && (_requestingBurnNode is MultiTokenMintNode))
                {
                    BurnTokenAmount((MultiTokenMintNode) _requestingBurnNode, _pendingBurnAmount);

                    _requestingBurnNode = null;
                    _pendingBurnAmount  = string.Empty;
                }

                if (_deletingNode != null)
                {
                    _selectedContract.DeleteNode(_deletingNode);
                    _deletingNode = null;
                }
            }
        }

        private void DrawBackground(Rect canvas)
        {            
            var backgroundTexture = Resources.Load("background") as Texture2D;

            Rect textureCoords =
                new Rect(0, 0, canvas.width / backgroundTexture.width, canvas.height / backgroundTexture.height);

            GUI.DrawTextureWithTexCoords(canvas, backgroundTexture, textureCoords);
        }

        private void DrawConnections(MultiTokenNode node)
        {
            Vector3 startPosition = new Vector2(node.GetRect().xMax, node.GetRect().center.y);

            foreach (MultiTokenNode childNode in _selectedContract.GetAllChildren(node))
            {
                if (childNode != null)
                {
                    Vector3 endPosition = new Vector3(childNode.GetRect().xMin, childNode.GetRect().center.y, 0);

                    Vector3 controlPointOffset = endPosition - startPosition;
                    controlPointOffset.y = 0;
                    controlPointOffset.x *= 0.8f;

                    Handles.DrawBezier(startPosition,
                                       endPosition,
                                       startPosition + controlPointOffset,
                                       endPosition - controlPointOffset,
                                       Color.white,
                                       null,
                                       4f);
                }
            }
        }

        private void ProcessEvents()
        {
            if ((Event.current.type == EventType.MouseDown) && (_draggingNode == null))
            {
                Vector2 relativeMousePosition = Event.current.mousePosition + _scrollPosition;

                _draggingNode = GetNodeAtPoint(relativeMousePosition);
                if (_draggingNode != null)
                {
                    _draggingOffset = _draggingNode.GetRect().position - Event.current.mousePosition;

                    Selection.activeObject = _draggingNode;
                }
                else
                {
                    _draggingCanvas       = true;
                    _draggingCanvasOffset = relativeMousePosition;

                    Selection.activeObject = _selectedContract;
                }
            }
            else if ((Event.current.type == EventType.MouseDrag) && (_draggingNode != null))
            {
                _draggingNode.SetRectPosition(Event.current.mousePosition + _draggingOffset);

                GUI.changed = true;
            }
            else if ((Event.current.type == EventType.MouseDrag) && _draggingCanvas)
            {
                _scrollPosition = _draggingCanvasOffset - Event.current.mousePosition;

                GUI.changed = true;
            }
            else if ((Event.current.type == EventType.MouseUp) && (_draggingNode != null))
            {
                _draggingNode   = null;
                _draggingOffset = Vector2.zero;
            }
            else if ((Event.current.type == EventType.MouseUp) && _draggingCanvas)
            {
                _draggingCanvas       = false;
                _draggingCanvasOffset = Vector2.zero;
            }
        }

        private void DrawLinkButton(MultiTokenNode targetNode)
        {
            if ((_linkingParentNode != null) && (_linkingParentNode is MultiTokenContractNode))
            {
                var contractNode = (MultiTokenContractNode) _linkingParentNode;

                if (contractNode.GetChildren().Contains(targetNode.name))
                {
                    if (GUILayout.Button("unlink"))
                    {
                        contractNode.RemoveChild(targetNode.name);
                        _linkingParentNode = null;
                    }
                }
                else if (targetNode.name != contractNode.name)
                {
                    if (GUILayout.Button("child"))
                    {
                        contractNode.AddChild(targetNode.name);
                        _linkingParentNode = null;
                    }
                }
                else
                {
                    if (GUILayout.Button("cancel"))
                    {
                        _linkingParentNode = null;
                    }
                }
            }
        }

        private void DrawNode(MultiTokenNode node)
        {
            bool isContractNode = (node is MultiTokenContractNode);

            GUIStyle currentStyle =
                !isContractNode ? 
                    (node.IsDeployed ? GetMintNodeDeployedDrawStyle((MultiTokenMintNode) node) : _nodeTokenStyle) : 
                    (node.IsDeployed ? _nodeDeployedContractStyle : _nodeContractStyle);

            GUILayout.BeginArea(node.GetRect(), currentStyle);

            if (isContractNode) 
            {
                var contractNode = (MultiTokenContractNode) node;

                var oldContractName = contractNode.ContractName;
                var newContractName = EditorGUILayout.TextField(oldContractName);
                contractNode.ContractName = newContractName;

                EditorUtility.SetDirty(_selectedContract);

                GUILayout.BeginHorizontal();

                if (contractNode.IsDeployed)
                {
                    if (GUILayout.Button("+"))
                    {
                        _creatingNode = node;
                    }
                }
                else
                {
                    if (GUILayout.Button("Deploy"))
                    {
                        _deployingNode = node;
                    }
                }

                GUILayout.EndHorizontal();
            }
            else
            {
                var mintNode = (MultiTokenMintNode) node;

                var oldTokenName = mintNode.TokenName;
                var newTokenName = EditorGUILayout.TextField(oldTokenName);
                mintNode.TokenName = newTokenName;

                EditorUtility.SetDirty(_selectedContract);

                GUILayout.BeginHorizontal();

                if (mintNode.IsDeployed)
                {
                    if (GUILayout.Button("x"))
                    {
                        _deletingNode = node;
                    }
                    if (GUILayout.Button("?"))
                    {
                        _refreshingNode = node;
                    }
                }
                else
                {
                    if (GUILayout.Button("Deploy"))
                    {
                        _mintingNode = node;
                    }
                }

                DrawLinkButton(node);

                GUILayout.EndHorizontal();

                if (mintNode.IsDeployed)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    var oldPendingTransferAddress = _pendingTransferToAddress;
                    var newPendingTransferToAddress = EditorGUILayout.TextField(oldPendingTransferAddress);
                    _pendingTransferToAddress = newPendingTransferToAddress;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Transfer"))
                    {
                        _requestingTransferNode = node;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    var oldPendingBurnAmount = _pendingBurnAmount;
                    var newPendingBurnAmount = EditorGUILayout.TextField(oldPendingBurnAmount);
                    _pendingBurnAmount = newPendingBurnAmount;                    
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Burn"))
                    {
                        _requestingBurnNode = node;
                    }
                    GUILayout.EndHorizontal();
                }

                EditorUtility.SetDirty(node);
            }

            GUILayout.EndArea();            
        }

        private MultiTokenNode GetNodeAtPoint(Vector2 mousePosition)
        {
            var absoluteMousePosition = mousePosition;

            return _selectedContract.GetAllNodes().LastOrDefault(x => x.GetRect().Contains(absoluteMousePosition));
        }

        private GUIStyle GetMintNodeDeployedDrawStyle(MultiTokenMintNode mintNode)
        {
            return mintNode.IsNFT() ? _nodeDeployedNFTStyle : _nodeDeployedTokenStyle;
        }

        private IContractTransactionUnityRequest GetTransactionUnityRequest(MultiTokenContract contract)
        {
            return new TransactionSignedUnityRequest(contract.ChainUrl, contract.PrivateKey, contract.ChainId);
        }

        private async void BurnTokenAmount(MultiTokenMintNode mintNode, string requestedBurnAmount)
        {
            if (_selectedContract != null)
            {
                long burnAmount = 0;

                Debug.Log("DEBUG: At ERC1155 contract at (" + _erc1155ContractMap[_selectedContract.GetRootNode().ContractName] +
                          "), there was a 'burn' request issued for Game Token Id (" + mintNode.TokenId + ")");

                if (!String.IsNullOrEmpty(requestedBurnAmount) && long.TryParse(requestedBurnAmount, out burnAmount))
                {
                    var burnReceipt =
                        await _selectedContract.MultiTokenService.BurnRequestAndWaitForReceiptAsync(mintNode.TokenOwnerAddress, mintNode.TokenId, burnAmount);

                    Debug.Log("DEBUG: The 'burn' request issued for Game Token Id (" + mintNode.TokenId + ") is complete.");

                    var balance =
                        await _selectedContract.MultiTokenService.BalanceOfQueryAsync(mintNode.TokenOwnerAddress, mintNode.TokenId);

                    Debug.Log("DEBUG: The current balance of ERC1155 contract at (" + _erc1155ContractMap[_selectedContract.GetRootNode().ContractName] +
                              ") for Game Token Id (" + mintNode.TokenId + ") is [" + balance + "] after the burn.");

                    long balanceNum = UnityERC1155ServiceFactory.ConvertBigIntegerToLong(balance);
                    mintNode.SetTokenBalance(balanceNum);
                }
                else
                {
                    Debug.Log("ERROR!  Pending burn amount is blank or not a number.");
                }
            }
            else
            {
                Debug.Log("DEBUG: BurnTokenAmount() cannot be invoked properly since the selected contract is NULL.");
            }
        }

        private IEnumerator DeployErc1155Contract()
        {
            if (_selectedContract == null)
            {
                Debug.Log("ERROR!  No contract has been selected for deployment.");
                yield break;
            }

            if (String.IsNullOrEmpty(_selectedContract.GetRootNode().ContractName))
            {
                Debug.Log("ERROR!  Contract must have nickname provided before being deployed.");
                yield break;
            }

            var transactionRequest = GetTransactionUnityRequest(_selectedContract);
            transactionRequest.UseLegacyAsDefault = true;

            var erc1155Contract = new UnityERC1155Deployment();

            Debug.Log("DEBUG: Signing and sending ERC1155 contract deployment");

            //deploy the contract and True indicates we want to estimate the gas
            yield return transactionRequest.SignAndSendDeploymentContractTransaction(erc1155Contract);

            if (transactionRequest.Exception != null)
            {
                Debug.Log(transactionRequest.Exception.Message);
                yield break;
            }

            var transactionHash = transactionRequest.Result;

            Debug.Log("DEBUG: Deployment transaction hash:" + transactionHash);

            //create a poll to get the receipt when mined
            var transactionReceiptPolling =
                new TransactionReceiptPollingRequest(new UnityWebRequestRpcClientFactory(_selectedContract.ChainUrl));

            Debug.Log("DEBUG: Polling to retrieve address of ERC1155 contract deployment");

            //checking every 2 seconds for the receipt
            yield return transactionReceiptPolling.PollForReceipt(transactionHash, 2);
            var deploymentReceipt = transactionReceiptPolling.Result;

            Debug.Log("DEBUG: Obtained receipt of ERC1155 contract deployment -> Address(" + deploymentReceipt.ContractAddress + ")");

            _selectedContract.InstantiateService(deploymentReceipt.ContractAddress);

            _erc1155ContractNames.Add(_selectedContract.GetRootNode().ContractName);
            _erc1155ContractAddresses.Add(deploymentReceipt.ContractAddress);

            _erc1155ContractMap[_selectedContract.GetRootNode().ContractName] = deploymentReceipt.ContractAddress;
        }

        private async void MintToken(MultiTokenMintNode mintNode)
        {
            if (_selectedContract != null)
            {
                Debug.Log("DEBUG: At ERC1155 contract at (" + _erc1155ContractMap[_selectedContract.GetRootNode().ContractName] +
                          "), there was a 'set URI' request issued for Game Token Id (" + mintNode.TokenId + ")");

                //Adding the product information
                var tokenUriReceipt =
                    await _selectedContract.MultiTokenService.SetTokenUriRequestAndWaitForReceiptAsync(mintNode.TokenId, mintNode.TokenMetadataUrl);

                Debug.Log("DEBUG: At ERC1155 contract at (" + _erc1155ContractMap[_selectedContract.GetRootNode().ContractName] +
                          "), there was a mint request issued for Game Token Id (" + mintNode.TokenId + ") with a starting balance of [" + mintNode.TokenBalance + "]");

                var mintReceipt =
                    await _selectedContract.MultiTokenService.MintRequestAndWaitForReceiptAsync(mintNode.TokenOwnerAddress, mintNode.TokenId, mintNode.TokenBalance, new byte[] { });

                var balance =
                    await _selectedContract.MultiTokenService.BalanceOfQueryAsync(mintNode.TokenOwnerAddress, mintNode.TokenId);

                Debug.Log("DEBUG: The current balance of ERC1155 contract at (" + _erc1155ContractMap[_selectedContract.GetRootNode().ContractName] +
                          ") for Game Token Id (" + mintNode.TokenId + ") is [" + balance + "]");

                mintNode.SetDeployedStatus(true);

                long balanceNum = UnityERC1155ServiceFactory.ConvertBigIntegerToLong(balance);
                mintNode.SetTokenBalance(balanceNum);
            }
            else
            {
                Debug.Log("DEBUG: MintToken() cannot be invoked properly since the selected contract is NULL.");
            }
        }

        private async void RefreshMintNode(MultiTokenMintNode mintNode)
        {
            if ((_selectedContract != null) && mintNode.IsDeployed)
            {
                Debug.Log("DEBUG: At ERC1155 contract at (" + _erc1155ContractMap[_selectedContract.GetRootNode().ContractName] +
                          "), there was a minted token refresh issued for Game Token Id (" + mintNode.TokenId + ") with a starting balance of [" + mintNode.TokenBalance + "]");

                var balance =
                    await _selectedContract.MultiTokenService.BalanceOfQueryAsync(mintNode.TokenOwnerAddress, mintNode.TokenId);

                long balanceNum = UnityERC1155ServiceFactory.ConvertBigIntegerToLong(balance);

                Debug.Log("DEBUG: The current balance of ERC1155 contract at (" + _erc1155ContractMap[_selectedContract.GetRootNode().ContractName] +
                          ") for Game Token Id (" + mintNode.TokenId + ") is [" + balanceNum + "]");

                mintNode.SetTokenBalance(balanceNum);
            }
            else
            {
                Debug.Log("DEBUG: MintToken() cannot be invoked properly since the selected contract is NULL.");
            }
        }

        private async void TransferToken(MultiTokenMintNode mintNode, string newOwner)
        {
            if (_selectedContract != null)
            {
                Debug.Log("DEBUG: TransferToken() -> Request for transfer of token (" + mintNode.TokenName + ") to address [" + newOwner + "].");

                var transfer = 
                    await _selectedContract
                          .MultiTokenService
                          .SafeTransferFromRequestAndWaitForReceiptAsync(mintNode.TokenOwnerAddress, newOwner, mintNode.TokenId, 1, new byte[] { });

                if ((bool) transfer.HasErrors())
                {
                    Debug.Log("DEBUG: Transfer of tokens to target address failed.");
                }

                mintNode.AddTokenOwner(newOwner);

                RefreshMintNode(mintNode);
            }
            else
            {
                Debug.Log("DEBUG: TransferToken() cannot be invoked properly since the selected contract is NULL.");
            }
        }
    }
}
