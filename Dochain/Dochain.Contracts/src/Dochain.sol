pragma solidity ^0.4.6;

import "./Common.sol";

contract Dochain is owned {
    
    struct DocInfo {
        uint256 timestamp; 
        bytes32 hash; 
        address sender;
    }

     mapping (string => DocInfo) private db;

     function Add (string name, string data) {
        var hash = sha256(data);
        var docInfo = DocInfo ({
            timestamp: block.timestamp,
            hash: hash,
            sender: tx.origin
        });

        if (db[name].hash != 0) {
            throw;
        }
        db[name] = docInfo;
     }

     function IsValid (string name, string data) returns (bool) {
        var hash = sha256(data);
        var result = (hash == db[name].hash);
        return result;
     }
}
