﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Dochain.Web.Interfaces;
using Nethereum.ABI.Util;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace Dochain.Web.Services
{
    public class NethereumDochainService : IDochainService
    {
        private readonly string _abi;
        private readonly Web3 _web3;
        private readonly string _senderAddress;
        private readonly string _password;
        private readonly Contract _contract;

        public NethereumDochainService(string abi, string address, string senderAddress, string password) : 
            this(abi, senderAddress, password)
        {
            _contract = _web3.Eth.GetContract(_abi, address);
        }

        public NethereumDochainService(string abi, string senderAddress, string password)
        {
            _web3 = new Web3();
            _abi = abi;
            _senderAddress = senderAddress;
            _password = password;
        }

        public async Task Add(string name, string value)
        {
            var hash = new HexBigInteger(_web3.Sha3(value)).ToHexByteArray();
            await AddHash(name, hash);
        }

        public async Task Add(string name, byte[] value)
        {
            var hash = new Sha3Keccack().CalculateHash(value);
            await AddHash(name, hash);
        }

        private async Task AddHash(string name, byte[] hash)
        {
            var function = _contract.GetFunction("Add");
            await UnlockAccount();
            var transactionHash = await function.SendTransactionAsync(_senderAddress, new HexBigInteger(500000), null, name,
                hash);
            await GetTransactionReceipt(transactionHash);
        }

        public async Task<bool> IsAvailable(string name)
        {
            await UnlockAccount();
            var function = _contract.GetFunction("IsAvailable");
            var result = await function.CallAsync<bool>(name);
            return result;
        }

        public async Task<bool> IsValid(string name, string value)
        {
            var hash = new HexBigInteger(_web3.Sha3(value)).ToHexByteArray();
            return await IsValidHash(name, hash);
        }

        public async Task<bool> IsValid(string name, byte[] value)
        {
            var hash = new Sha3Keccack().CalculateHash(value);
            return await IsValidHash(name, hash);
        }

        public async Task<Document> GetDocumentInfo(string name)
        {
            await UnlockAccount();
            var function = _contract.GetFunction("getDocInfo");
            var result = await function.CallDeserializingToObjectAsync<Document>(name);
            return result;
        }

        private async Task<bool> IsValidHash(string name, byte[] hash)
        {
            await UnlockAccount();
            var function = _contract.GetFunction("IsValid");
            var result = await function.CallAsync<bool>(name, hash);
            return result;
        }

        public async Task<string> Deploy(string byteCode)
        {
            await UnlockAccount();

            var transactionHash =
                await
                    _web3.Eth.DeployContract.SendRequestAsync(_abi, byteCode, _senderAddress, new HexBigInteger(1000000));
            var receipt = await GetTransactionReceipt(transactionHash);
            var contractAddress = receipt.ContractAddress;
            return contractAddress;
        }

        private async Task UnlockAccount()
        {
            var unlockAccountResult =
                await _web3.Personal.UnlockAccount.SendRequestAsync(_senderAddress, _password, new HexBigInteger(100));
            if (!unlockAccountResult)
                throw new InvalidOperationException("Unable to unlock");
        }

        private async Task<TransactionReceipt> GetTransactionReceipt(string transactionHash)
        {
            var mineResult = await _web3.Miner.Start.SendRequestAsync(6);
            if (!mineResult)
                throw new InvalidOperationException("Unable to start miner.");

            var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            while (receipt == null)
            {
                Thread.Sleep(1000);
                receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            }
            mineResult = await _web3.Miner.Stop.SendRequestAsync();
            if (!mineResult)
                throw new InvalidOperationException("Unable to stop miner.");
            return receipt;
        }
    }
}