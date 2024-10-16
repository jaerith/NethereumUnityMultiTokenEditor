using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nethereum.Unity.Behaviours
{
    public class EthereumBalanceChangeEventLogger : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var accountBehaviour = this.GetComponent<EthereumAccountBehaviour>();
            if (accountBehaviour != null)
            {
                Debug.Log("Found EthereumAccountBehaviour as a sibling.");

                accountBehaviour.onBalanceUpdated += LogBalanceUpdate;
            }
            else
            {
                Debug.Log("ERROR!  Could not prepare EthereumBalanceChangeEventLogger since it could not " +
                          "find EthereumAccountBehaviour as a sibling.");
            }
        }

        private void LogBalanceUpdate(EthereumBalanceChangeEvent balanceChangeEvent)
        {
            Debug.Log("DEBUG: Account(" + balanceChangeEvent.AccountPublicAddress +
                      ") had its token allotment of Token Id (" + balanceChangeEvent.TokenId +
                      ") changed from [" + balanceChangeEvent.TokenPreviousBalance +
                      "] tokens to [" + balanceChangeEvent.TokenCurrentBalance + "] tokens.");
        }
    }
 }
