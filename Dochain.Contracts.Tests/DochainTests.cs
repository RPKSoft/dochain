using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.ABI.Util;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Xunit;

namespace Dochain.Contracts.Tests
{
    [FunctionOutput]
    public class Document
    {
        [Parameter("uint256", "timestamp", 1)]
        public ulong Timestamp { get; set; }

        [Parameter("address", "sender", 3)]
        public string Sender { get; set; }
    }

    public class DochainTests
    {
        private string _abi;
        private string _contractByteCode;
        private Web3 _web3;
        private readonly string _ownedAbi = Path.Combine("bin", "owned.abi");
        private readonly string _ownedBin = Path.Combine("bin", "owned.bin");
        private readonly string _dochainAbi = Path.Combine("bin", "Dochain.abi");
        private readonly string _dochainBin = Path.Combine("bin", "Dochain.bin");

        private const string SenderAddress = "0x12890d2cce102216644c59dae5baed380d84830c";
        private const string Password = "password";
        
        [Fact]
        public void CompiledContractsShouldExist()
        {
            Assert.True(File.Exists(_ownedAbi));
            Assert.True(File.Exists(_ownedBin));
            Assert.True(File.Exists(_dochainAbi));
            Assert.True(File.Exists(_dochainBin));
        }

        [Fact]
        public async void ShouldBeAbleToDeployAContract()
        {
            var contract = await DeployContract();
            var function = contract.GetFunction("getOwner");
            var result = await function.CallAsync<string>();
            Assert.Equal(SenderAddress, result);
        }

        [Fact]
        public async void ShouldValidateAddedText()
        {
            var contract = await DeployContract();
            var function = contract.GetFunction("Add");
            var name = GenerateName();
            var hash = new HexBigInteger(new Sha3Keccack().CalculateHash("test 1")).ToHexByteArray();
            var transactionHash = await function.SendTransactionAsync(SenderAddress, new HexBigInteger(500000), null, name,
                hash);
            var receipt = await GetTransactionReceipt(transactionHash);
            Assert.NotNull(receipt);

            function = contract.GetFunction("IsValid");
            var result = await function.CallAsync<bool>(name, hash);
            Assert.True(result);
            result = await function.CallAsync<bool>(name, new HexBigInteger(_web3.Sha3("test")).ToHexByteArray());
            Assert.False(result);
        }

        [Fact]
        public async void ShouldValidateAddedData()
        {
            var contract = await DeployContract();
            var function = contract.GetFunction("Add");
            var name = GenerateName();
            var hash = new Sha3Keccack().CalculateHash(new byte[] { 1, 2, 3 });
            var transactionHash = await function.SendTransactionAsync(SenderAddress, new HexBigInteger(500000), null, name,
                hash);
            var receipt = await GetTransactionReceipt(transactionHash);
            Assert.NotNull(receipt);

            function = contract.GetFunction("IsValid");
            var result = await function.CallAsync<bool>(name, hash);
            Assert.True(result);
            result = await function.CallAsync<bool>(name, new Sha3Keccack().CalculateHash(new byte[] { 1, 2, 3, 4 }));
            Assert.False(result);
        }

        [Fact]
        public async void ShouldReturnDocInfo()
        {
            var contract = await DeployContract();
            var function = contract.GetFunction("Add");
            var name = GenerateName();
            var hash = new Sha3Keccack().CalculateHash(new byte[] { 1, 2, 3 });
            var transactionHash = await function.SendTransactionAsync(SenderAddress, new HexBigInteger(500000), null, name,
                hash);
            var receipt = await GetTransactionReceipt(transactionHash);
            Assert.NotNull(receipt);

            function = contract.GetFunction("getDocInfo");
            var result = await function.CallDeserializingToObjectAsync<Document>(name);
            Assert.Equal(SenderAddress, result.Sender);
            Assert.NotNull(result.Timestamp);
        }

        private static string GenerateName()
        {
            var r = new Random();
            return $"{r.Next():x}";
        }

        [Fact]
        public async void ShouldReportAvailability()
        {
            var contract = await DeployContract();
            var function = contract.GetFunction("IsAvailable");
            var name = GenerateName();
            var result = await function.CallAsync<bool>(name);
            Assert.True(result);

            function = contract.GetFunction("Add");
            var transactionHash = await function.SendTransactionAsync(SenderAddress, new HexBigInteger(500000), null, name,
                "test1");
            var receipt = await GetTransactionReceipt(transactionHash);
            Assert.NotNull(receipt);

            function = contract.GetFunction("IsAvailable");
            result = await function.CallAsync<bool>(name);
            Assert.False(result);
        }

        private async Task<Contract> DeployContract()
        {
            _abi = File.ReadAllText(_dochainAbi);
            _contractByteCode = "0x" + File.ReadAllText(_dochainBin);
            _web3 = new Web3();
            var unlockAccountResult =
                await _web3.Personal.UnlockAccount.SendRequestAsync(SenderAddress, Password, new HexBigInteger(600));
            Assert.True(unlockAccountResult);

            var transactionHash =
                await
                    _web3.Eth.DeployContract.SendRequestAsync(_abi, _contractByteCode, SenderAddress, new HexBigInteger(1000000));
            var receipt = await GetTransactionReceipt(transactionHash);
            var contractAddress = receipt.ContractAddress;
            var contract = _web3.Eth.GetContract(_abi, contractAddress);
            return contract;
        }

        private async Task<TransactionReceipt> GetTransactionReceipt(string transactionHash)
        {
            var mineResult = await _web3.Miner.Start.SendRequestAsync(6);
            Assert.True(mineResult);

            var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            while (receipt == null)
            {
                Thread.Sleep(1000);
                receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            }
            mineResult = await _web3.Miner.Stop.SendRequestAsync();
            Assert.True(mineResult);
            return receipt;
        }
    }
}
