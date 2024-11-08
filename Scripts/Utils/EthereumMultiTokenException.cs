using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using Nethereum.Unity.MultiToken;

namespace Nethereum.Unity.Utils
{
    public class EthereumMultiTokenException : Exception
    {
        private MultiTokenContract _contract;
        private MultiTokenMintNode _mintNode;

        public EthereumMultiTokenException(MultiTokenContract contract, MultiTokenMintNode mintNode, string message = null) :
            base(AssembleMessage(contract, mintNode, message))
        { }

        private static string AssembleMessage(MultiTokenContract contract, MultiTokenMintNode mintNode, string message)
        {
            StringBuilder messageBuilder = new StringBuilder("ERROR!  Multitoken issue found.");

            try
            {
                if ((contract != null) && (mintNode != null))
                {
                    messageBuilder.Append("  In regard to contract (" + contract.name + ") and its token ID [" +
                                          mintNode.TokenId + "].\n");

                    if (!String.IsNullOrEmpty(message))
                    {
                        messageBuilder.AppendLine("Details of error are (" + message + ")");
                    }
                }
            }
            catch (Exception ex)
            {
                messageBuilder =
                    new StringBuilder("ERROR!  Exception message not assembled correctly due to -> \n(" + ex + "\n)\n");                                                 
            }

            return messageBuilder.ToString();
        }
    }
}
