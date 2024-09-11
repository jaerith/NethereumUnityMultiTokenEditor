using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

using Nethereum.Unity.Rpc;

using Nethereum.Web3.Accounts;
using Nethereum.Web3;
using Nethereum.Signer;
using Nethereum.Unity.Contracts;
using Nethereum.Unity.Rpc;

using Nethereum.Contracts.UnityERC1155;
using Nethereum.Contracts.UnityERC1155.ContractDefinition;


namespace Nethereum.Unity.Editors.MultiToken
{
    [CreateAssetMenu(fileName = "New MultiToken Contract", menuName = "Multitoken Contract", order = 0)]
    public class MultiTokenContract : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string _chainUrl = "http://testchain.nethereum.com:8545";

        [SerializeField]
        private string _privateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";

        [SerializeField]
        private long _chainId = 444444444500; //Nethereum test chain, chainId

        private Account   _account = null;
        private Web3.Web3 _web3    = null;

        private UnityERC1155Service _erc1155Service = null;

        [SerializeField]
        private readonly Vector2 NewNodeOffset = new Vector2(250, 0);

        [SerializeField]
        private MultiTokenContractNode rootNode = null;

        [SerializeField]
        List<MultiTokenNode> nodes = new List<MultiTokenNode>();

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
        }

        public IEnumerable<MultiTokenNode> GetAllNodes()
        {
            return nodes;
        }

        public MultiTokenContractNode GetRootNode() { return rootNode; }

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

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (nodes.Count <= 0)
            {
                var newChild = MakeNode(null);
                AddNode(newChild);

                rootNode = (MultiTokenContractNode) newChild;
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

                newChild.name = Guid.NewGuid().ToString();

                newChild.SetRectPosition(parent.GetRect().position + NewNodeOffset);

                parent.AddChild(newChild.name);
            }
            else
            {
                newChild = CreateInstance<MultiTokenContractNode>();

                newChild.name = Guid.NewGuid().ToString();

                newChild.SetRectPosition(parent.GetRect().position + NewNodeOffset);
            }

            return newChild;
        }

        private void RemoveDanglingChildren(MultiTokenMintNode targetNode)
        {
            rootNode.RemoveChild(targetNode.name);
        }

#endif
        #endregion
    }
}