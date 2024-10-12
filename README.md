# NethereumUnityMultiTokenEditor
This project provides the code for an editor window within Unity that enables the deployment of [ERC1155](https://eips.ethereum.org/EIPS/eip-1155) contracts, the minting of tokens with these contracts, and the integrations between game objects and these contracts.

## Overview 
More and more games are becoming interested in the integration of Ethereum within their games.  In particular, Ethereum L2 and L3 platforms are gaining an increasing amount of attention, given that these platforms are becoming more viable in terms of cost.  However, despite more advanced tools being released each year to assist with Ethereum development, there is still a general lack of user-friendly tools for rapid development and prototyping with Ethereum contracts, especially for game developers.  The aim of this repo is to provide a user-friendly package for Unity users who want to connect their projects with Ethereum.  Even though this project might provide other types of functionality in the future, its initial (and currently) purpose will be to provide Unity users with the specific ability to create, transfer, and track the tokens of ERC-1155 contracts.  By using this project, the user will gain the ability to:

1.) Deploy ERC-1155 contract and then mint tokens, all of which can then also be burned or transferred to an account via the editor window

2.) Attach the EthereumAccountBehaviour class (with an assigned Ethereum account) to a game object, which will show an updated display of all tokens owned by that object (i.e., account) while the game is running in the Unity IDE

The general assumption of this project is that a proper game would have a framework in the middle tier (or back tier) that serves to do the actual awarding (i.e., transferring) of tokens via game actions.  So, this project does not help in the award/transfer of tokens during gameplay.  However, this project aims to help game developers by:

1.) Allowing game developers to create contracts and tokens as a way of prototying the dispersements in the game, pointing their middle tier (perhaps running on their local machines) at these contracts/tokens

2.) Providing transparecy to token ownerships and dispersements within the Unity IDE, so that they don't have to switch to another window (like a browser with Metamask) in order to observe tokens transfers and balances

## Prerequisites

* Visual Studio 2019 (minimum)
* Unity Editor 2022.2.18 (minimum)
* Installation of [Unity Package "Nethereum"](https://github.com/Nethereum/Nethereum.Unity) (Thanks, [Juan Blanco!](https://github.com/Nethereum))
* Installation of [Unity Package "Editor Coroutines"](https://docs.unity3d.com/Manual/com.unity.editorcoroutines.html)

In addition to these tools, the user should have a basic understanding of Ethereum and certain standards, especially when it comes to tokens and the ERC-1155 standard.  They should also create a set of accounts with public addresses (and respective private keys), if they don't already have any. 

By default, the contracts are pointed to a test chain (restarted every few minutes) that is run by the Nethereum project.  However, for novices, you are encouraged to run your own test chain.  And if you're an Ethereum pro ready with a L2 or your own L3, even better!

## Using the Multi Token Editor

After placing the repo classes in a subfolder of Assets in the Unity project, the user will then be able to start by opening the Editor window via the menu path "Window -> Ethereum ->MultiToken Editor". 
</br>

![Opening the Editor](https://github.com/jaerith/NethereumUnityMultiTokenEditor/blob/main/Screenshots/OpenMultiTokenEditor.png)

</br>
Once the window has been opened and moved to a convenient location, one can start by creating a Contract object (in a designated  subfolder) with the right-click menu path "Create -> Ethereum -> Create Contract".  Once the Contract object is selected, the Editor window will show the initial Contract node (colored grey due to its Pending state) that represents the potential ERC-1155 contract to be used.
</br>
</br>

![Creating Contract](https://github.com/jaerith/NethereumUnityMultiTokenEditor/blob/main/Screenshots/CreateMultiTokenContract.png)

</br>

### Deploying a ERC-1155 Contract

In the Editor window, the user can either deploy an ERC-1155 contract or connect to an existing one via the Contract node.  Once it has been connected to the contract, it will change its color to blue, indicating the connection is live.  

![Deployed Contract](https://github.com/jaerith/NethereumUnityMultiTokenEditor/blob/main/Screenshots/MultiTokenEditor_ContractNode_AfterDeployment.png)

</br>

### Minting a ERC-1155 Token

Once the connection has been made, the user can now create Token nodes in a Pending status.  Once the properties are set via the Inspector window, the tokens can be minted.  Upon completion, the node will change to a different color to indicate success: red for NFTs (i.e., [ERC-721](https://ethereum.org/en/developers/docs/standards/tokens/erc-721/)) and light blue for fungible tokens (i.e., [ERC-20](https://ethereum.org/en/developers/docs/standards/tokens/erc-20/)).
</br>

![Minted NFT](https://github.com/jaerith/NethereumUnityMultiTokenEditor/blob/main/Screenshots/MultiTokenEditor_NFT_Node_AfterMint.png)

</br>

### Transferring a ERC-1155 Token

Now the minted tokens can be transferred or burned.  If there any game development scenarios where certain EOA accounts are assigned to NPCs, then the tokens can be dispersed here in the Editor window.

![Trasferred a token](https://github.com/jaerith/NethereumUnityMultiTokenEditor/blob/main/Screenshots/MultiTokenEditor_TokenNode_AfterTransfer.png)

</br>

## Using Ethereum Account Behaviour

With the introduction of Ethereum tokens to any game, they will likely be attached to players, but they might also be attached to NPCs (or even in-game objects).  In all cases, an [EOA](https://ethereum.org/en/developers/docs/accounts/) is required to receive these tokens.  By attaching the [Ethereum Account Behaviour](https://github.com/jaerith/NethereumUnityMultiTokenEditor/blob/main/Scripts/Behaviour/EthereumAccountBehaviour.cs) script to a game object (player, NPC, or other type of asset) and then providing the right configuration (namely its EOA and the target ERC-1155 contract), the developer can observe the tokens that are received and then held by the account while the game is running within the Unity IDE.

![Ethereum Account Behaviour Attached](https://github.com/jaerith/NethereumUnityMultiTokenEditor/blob/main/Screenshots/EAB_Properties_Gameplay_Update.png)

The properties of an Ethereum Account Behaviour instance are displayed in the Inspector window, which are:

* Token Ownership Descriptions - lists a brief overview of the tokens owned by the account
* Token Ownerships - lists a collection of objects with more details about the owned tokens
* Refresh Token Interval in Seconds - the time between polling the contract for updates about the token balances
* Audio Source Token Updated (optional) - the provided audio clip that will play when any token balance has changed

In addition, there is a Refresh All Tokens button, which will return all tokens (owned by the contract) to the original pool of the contract owner.  This functionality might be helpful to the game developer who wishes to test/debug a scenario repeatedly where tokens are dispersed based on a game action.

<br/>
If the developer only wants to return one type of token, they can double-click the corresponding item in the Token Ownerships list.  In the resulting Inspector window for that object, there will be a Refresh Token button:

<br/>
<br/>

![Ethereum Token Ownership Attached](https://github.com/jaerith/NethereumUnityMultiTokenEditor/blob/main/Screenshots/EOT_Properties_Gameplay_Update.png)
