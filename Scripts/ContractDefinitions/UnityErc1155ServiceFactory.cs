using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nethereum.Contracts.Standards.ERC1155;
using Nethereum.Unity.Behaviours;
using Nethereum.Unity.MultiToken;
using Nethereum.Web3.Accounts;

namespace Nethereum.Contracts.UnityERC1155
{
    public class UnityERC1155ServiceFactory : MonoBehaviour
    {
        static public UnityERC1155Service CreateService(MultiTokenContract contract)
        {
            var account = new Account(contract.PrivateKey, contract.ChainId);

            var web3 = new Web3.Web3(account, contract.ChainUrl);
            web3.TransactionManager.UseLegacyAsDefault = true; //Using legacy option instead of 1559

            return new UnityERC1155Service(web3, contract.ContractAddress);
        }

        static public long ConvertBigIntegerToLong(System.Numerics.BigInteger bigIntegerAmount)
        {
            return unchecked((long)(ulong)(bigIntegerAmount & ulong.MaxValue));
        }

    }
}