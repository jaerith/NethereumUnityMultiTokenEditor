# NethereumUnityMultiTokenEditor
This project provides the code for an editor window within Unity that enables the deployment of ERC1155 contracts, the minting of tokens with these contracts, and the integrations between game objects and these contracts.

## Overview 
More and more games are becoming interested in the integration of Ethereum within their games.  In particular, Ethereum L2 and L3 platforms are gaining an increasing amount of attention, given that these platforms are becoming more viable in terms of cost.  However, despite more advanced tools being released each year to assist with Ethereum development, there is still a general lack of user-friendly tools for rapid development and prototyping with Ethereum contracts, especially for game developers.  The aim of this repo is to provide a user-friendly package for Unity users who want to connect their projects with Ethereum.  Even though this project might provide other types of functionality in the future, its initial (and currently) purpose will be to provide Unity users with the specific ability to create, transfer, and track the tokens of ERC-1155 contracts.  By using this project, the user will gain the ability to:

1.) Deploy ERC-1155 contract and then mint tokens, all of which can then also be burned or transferred to an account via the editor window

2.) Attach the EthereumAccountBehaviour class (with an assigned Ethereum account) to a game object, which will show an updated display of all tokens owned by that object (i.e., account) while the game is running in the Unity IDE

The general assumption of this project is that a proper game would have a framework in the middle tier (or back tier) that serves to do the actual awarding (i.e., transferring) of tokens via game actions.  So, this project does not help in the award/transfer of tokens during gameplay.  However, this project aims to help game developers by:

1.) Allowing game developers to create contracts and tokens as a way of prototying the dispersements in the game, pointing their middle tier at these contracts/tokens

2.) Providing transparecy to token ownerships and dispersements within the Unity IDE, so that they don't have to switch to another window (like a browser with Metamask) in order to observe tokens transfers and balances

### Prerequisites

### Using the Multi Token Editor

![Opening the Editor](https://github.com/jaerith/NethereumUnityMultiTokenEditor/blob/main/Screenshots/OpenMultiTokenEditor.png)

#### Deploying a ERC-1155 Contract

![Creating a Contract](https://github.com/jaerith/NethereumUnityMultiTokenEditor/blob/main/Screenshots/CreateMultiTokenContract.png)

#### Minting a ERC-1155 Token

![Minted NFT](https://github.com/jaerith/NethereumUnityMultiTokenEditor/blob/main/Screenshots/MultiTokenEditor_NFT_Node_AfterMint.png)

#### Transferring a ERC-1155 Token

![Trasferred a token](https://github.com/jaerith/NethereumUnityMultiTokenEditor/blob/main/Screenshots/MultiTokenEditor_TokenNode_AfterTransfer.png)

### Using Ethereum Account Behaviour

![Ethereum Account Behaviour Attached](https://github.com/jaerith/NethereumUnityMultiTokenEditor/blob/main/Screenshots/EAB_Properties_Gameplay_Update.png)
