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

     function Add (string name, string data) {
        var hash = sha256(data);
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

     function IsValid (string name, string data) returns (bool) {
        var hash = sha256(data);
        var result = (hash == db[name].hash);
        return result;
     }
}
