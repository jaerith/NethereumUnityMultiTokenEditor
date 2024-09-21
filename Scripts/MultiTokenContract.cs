using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

using Nethereum.Web3.Accounts;
using Nethereum.Web3;
using Nethereum.Signer;
using Nethereum.Unity.Contracts;
using Nethereum.Unity.Rpc;

using Nethereum.Contracts.UnityERC1155;
using Nethereum.Contracts.UnityERC1155.ContractDefinition;
using Org.BouncyCastle.Crypto.Tls;
using Nethereum.Contracts.Standards.ERC1155;

namespace Nethereum.Unity.MultiToken
{
    [CreateAssetMenu(fileName = "New MultiToken Contract", menuName = "Ethereum/Multitoken Contract", order = 0)]
    public class MultiTokenContract : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string _chainUrl = "http://testchain.nethereum.com:8545";

        [SerializeField]
        private string _privateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";

        [SerializeField]
        private long _chainId = 444444444500; //Nethereum test chain, chainId

        [SerializeField]
        private long _maxTokenId = 1; //Nethereum test chain, chainId

        private Account   _account = null;
        private Web3.Web3 _web3    = null;

        private UnityERC1155Service _erc1155Service = null;

        private MultiTokenContractNode rootNode = null;

        [SerializeField]
        private List<MultiTokenNode> nodes = new List<MultiTokenNode>();

        [SerializeField]
        private readonly Vector2 NewNodeOffset = new Vector2(250, 0);

        [SerializeField]
        Dictionary<string, MultiTokenNode> nodeLookup = new Dictionary<string, MultiTokenNode>();

#if UNITY_EDITOR
        private void Awake()
        {
            OnValidate();
        }
#endif

        private void OnValidate()
        {
            nodeLookup.Clear();

            nodes.ForEach(x => nodeLookup[x.name] = x);

            if (_account == null)
            {
                _account = new Account(_privateKey, _chainId);

                //Now let's create an instance of Web3 using our account pointing to our nethereum testchain
                _web3 = new Web3.Web3(_account, _chainUrl);
                _web3.TransactionManager.UseLegacyAsDefault = true; //Using legacy option instead of 1559
            }

            if ((nodes.Count > 0) && (_erc1155Service == null))
            {
                if (nodes[0] is MultiTokenContractNode)
                {
                    rootNode = (MultiTokenContractNode) nodes[0];

                    Debug.Log("DEBUG: Instantiating ERC1155 service on load of MultiTokenContract.");

                    _erc1155Service = new UnityERC1155Service(_web3, GetRootNode().ContractAddress);
                }
            }
        }

        public IEnumerable<MultiTokenNode> GetAllNodes()
        {
            return nodes;
        }

        public MultiTokenContractNode GetRootNode() { return rootNode; }

        public string ChainUrl { get { return _chainUrl; } }

        public string PrivateKey { get { return _privateKey; } }

        public long ChainId { get { return _chainId; } }

        public IEnumerable<MultiTokenNode> GetAllChildren(MultiTokenNode parentNode)
        {
            foreach (var childId in parentNode.GetChildren())
            {
                yield return (nodeLookup.ContainsKey(childId) ? nodeLookup[childId] : null);
            }
        }

        public IEnumerable<MultiTokenNode> GetContractChildren(MultiTokenContractNode parentNode)
        {
            foreach (var childId in parentNode.GetChildren())
            {
                if (nodeLookup.ContainsKey(childId))
                    yield return nodeLookup[childId];
            }
        }

        public UnityERC1155Service MultiTokenService
        {
            get { return _erc1155Service; }
        }

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (nodes.Count <= 0)
            {
                var newChild = MakeNode(null);
                AddNode(newChild);

                rootNode = (MultiTokenContractNode) newChild;
            }
            else
            {
                rootNode = (MultiTokenContractNode) nodes[0];
            }

            if (!String.IsNullOrEmpty(AssetDatabase.GetAssetPath(this)))
            {
                GetAllNodes().ToList()
                             .Where(x => String.IsNullOrEmpty(AssetDatabase.GetAssetPath(x))).ToList()
                             .ForEach(x => AssetDatabase.AddObjectToAsset(x, this));
            }
#endif
        }

        public void OnAfterDeserialize()
        {
            // NOTE: This method will likely never be implemented, but throwing the NotImplementedException causes an error in the Unity compiler
            // throw new NotImplementedException();
        }

        #region UNITY_EDITOR_REGION

#if UNITY_EDITOR

        private void AddNode(MultiTokenNode newChild)
        {
            nodes.Add(newChild);

            OnValidate();
        }

        public void CreateNode(MultiTokenContractNode parent)
        {
            var newChild = MakeNode(parent);

            Undo.RegisterCreatedObjectUndo(newChild, "Created Token Node");

            Undo.RecordObject(this, "Added Token Node");

            AddNode(newChild);
        }

        public void DeleteNode(MultiTokenNode targetNode)
        {
            Undo.RecordObject(this, "Deleted MultiToken Node");

            nodes.Remove(targetNode);

            OnValidate();

            if (targetNode is MultiTokenMintNode)
            {
                RemoveDanglingChildren((MultiTokenMintNode) targetNode);
            }

            Undo.DestroyObjectImmediate(targetNode);
        }

        private MultiTokenNode MakeNode(MultiTokenContractNode parent)
        {
            MultiTokenNode newChild = null;

            if (parent != null)
            {
                newChild = CreateInstance<MultiTokenMintNode>();

                var mintChild = (MultiTokenMintNode) newChild;

                mintChild.name = Guid.NewGuid().ToString();

                mintChild.SetTokenId(_maxTokenId++);

                mintChild.SetRectPosition(parent.GetRect().position + NewNodeOffset);

                parent.AddChild(mintChild.name);
            }
            else
            {
                newChild = CreateInstance<MultiTokenContractNode>();

                newChild.name = Guid.NewGuid().ToString();
            }

            return newChild;
        }

        private void RemoveDanglingChildren(MultiTokenMintNode targetNode)
        {
            rootNode.RemoveChild(targetNode.name);
        }

        public void InstantiateService(string contractAddress)
        {
            GetRootNode().ContractAddress = contractAddress;

            _erc1155Service = new UnityERC1155Service(_web3, contractAddress);
        }

#endif
        #endregion
    }
}
