using UnityEngine;
using UnityEditor;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Nethereum.Contracts.UnityERC1155.ContractDefinition
{
    [Event("TransferSingle")]
    public class UnityErc1155TransferSingleEvent : IEventDTO
    {
        [Parameter("address", "_operator", 1, true)]
        public string Operator { get; set; }

        [Parameter("address", "_from", 2, true)]
        public string From { get; set; }

        [Parameter("address", "_to", 3, true)]
        public string To { get; set; }

        [Parameter("uint256", "_id", 4, false)]
        public System.Numerics.BigInteger Id { get; set; }

        [Parameter("uint256", "_value", 5, false)]
        public System.Numerics.BigInteger Value { get; set; }
    }
}