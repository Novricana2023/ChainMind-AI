export interface PromptTemplate {
  id: string;
  label: string;
  category: string;
  prompt: string;
}

export const PROMPT_TEMPLATES: PromptTemplate[] = [
  {
    id: "erc20",
    label: "ERC-20 Token",
    category: "Token",
    prompt: `Create a Solidity ERC-20 token called CommunityFundToken (symbol CFT) with a fixed supply of 1,000,000 tokens minted to the deployer. Include owner-only mint (capped at max supply), burn function, and custom errors. Use Solidity ^0.8.20 and OpenZeppelin Contracts v5 (utils/ReentrancyGuard if needed). Add NatSpec on all public functions.`,
  },
  {
    id: "crop-insurance",
    label: "Crop Insurance",
    category: "DeFi",
    prompt: `Create a crop insurance Solidity ^0.8.20 contract using OpenZeppelin v5 Ownable and ReentrancyGuard (utils/ path).

Requirements:
- Farmers register with cropType and region (string)
- Owner reports weather events PER REGION: reportEvent(region, eventType) e.g. ("Northern Malawi", "drought")
- Use mapping(string => mapping(string => bool)) for region => eventType, NOT global event flags
- Farmers claim only if their registered region has a verified event matching eventType
- Use mapping(address => mapping(string => bool)) for claims — one claim per eventType per farmer
- Fixed payout amount set by owner; check address(this).balance before payout
- Owner funds pool via deposit() payable function
- Custom errors, events, and NatSpec on all public/external functions`,
  },
  {
    id: "voting",
    label: "Voting / DAO",
    category: "Governance",
    prompt: `Write a Solidity ^0.8.20 voting smart contract using OpenZeppelin v5 Ownable.

Requirements:
- Owner creates proposals with description and voting deadline (block.timestamp)
- Users vote yes or no once per proposal (mapping to track votes)
- After deadline, anyone can finalize to determine if proposal passed (more yes than no)
- Emit events for proposal creation, votes, and finalization
- Custom errors for: already voted, voting closed, proposal not found
- NatSpec on all public functions`,
  },
  {
    id: "healing-token",
    label: "HealingToken",
    category: "Token",
    prompt: `Generate an ERC-20 token called HealingToken (symbol HEAL) for a non-profit health initiative using Solidity ^0.8.20 and OpenZeppelin v5.

Requirements:
- Max supply 10,000,000 tokens
- Owner can mint to verified wallet addresses only (maintain mapping of verified addresses)
- Include pause and unpause for emergencies (Pausable)
- Include burn function callable by token holders
- Custom errors, events, NatSpec
- Constructor mints initial supply to deployer`,
  },
  {
    id: "nft",
    label: "NFT Collection",
    category: "NFT",
    prompt: `Create an ERC-721 NFT collection called ArtChain using Solidity ^0.8.20 and OpenZeppelin v5.

Requirements:
- Max supply 10,000 NFTs
- Owner-only mint function with safeMint
- uint256 tokenId counter (no Counters.sol)
- baseURI set by owner
- Custom errors and NatSpec
- Events for minting`,
  },
  {
    id: "escrow",
    label: "Payment Escrow",
    category: "Utility",
    prompt: `Create a payment escrow Solidity ^0.8.20 contract using OpenZeppelin v5 Ownable and ReentrancyGuard.

Requirements:
- Buyer deposits ETH for a seller with a deadline
- Seller can release after buyer confirms OR owner mediates dispute
- Buyer can refund if seller fails to deliver before deadline
- Track escrow state: Active, Released, Refunded
- Balance checks before transfers, nonReentrant on payouts
- Custom errors, events, NatSpec`,
  },
];
