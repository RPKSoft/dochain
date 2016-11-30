pragma solidity ^0.4.6;

contract owned {
    address private owner;

    function owned() {
        owner = msg.sender;
    }

    modifier onlyOwner {
        if (msg.sender != owner) throw;
        _;
    }

    function transferOwnership(address newOwner) onlyOwner {
        owner = newOwner;
    }

    function getOwner() constant returns (address) {
        return owner;
    }
}