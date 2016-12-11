pragma solidity ^0.4.6;

import "./Common.sol";

contract Dochain is owned {
    function Dochain() owned() {
    }
    
    struct DocInfo {
        uint256 timestamp; 
        bytes32 hash; 
        address sender;
    }

     mapping (string => DocInfo) private db;

     function Add (string name, bytes32 hash) {
        var docInfo = DocInfo ({
            timestamp: block.timestamp,
            hash: hash,
            sender: msg.sender
        });

        if (db[name].hash != 0) {
            throw;
        }
        db[name] = docInfo;
     }

     function IsAvailable (string name) returns (bool) {
        return (db[name].hash == 0);
     }

     function IsValid (string name, bytes32 hash) returns (bool) {
        var result = (hash == db[name].hash);
        return result;
     }

     function getDocInfo(string name) returns (uint256 timestamp, address sender) {
         return (db[name].timestamp, db[name].sender);
     }
}
