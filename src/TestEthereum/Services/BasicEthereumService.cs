﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Nethereum.Web3;
using System;
using System.Threading.Tasks;
using TestEthereum.Model;

namespace TestEthereum.Services
{
    public class BasicEthereumService: IEthereumService
    {
        public string AccountAddress { get; set; }
        public string Password { get; set; }
        private Nethereum.Web3.Web3 _web3;
        
        private string _storageKey;
        private string _storageAccount;

        public BasicEthereumService(IOptions<EthereumSettings> config)
        {
            _web3 = new Web3("http://localhost:8545");
            AccountAddress = config.Value.EhtereumAccount;
            Password = config.Value.EhtereumPassword;
            _storageAccount = config.Value.StorageAccount;
            _storageKey = config.Value.StorageKey;
        }


        public async Task<bool> SaveContractToTableStorage(EthereumContractInfo contract)
        {
            StorageCredentials credentials = new StorageCredentials(_storageAccount, _storageKey);
            CloudStorageAccount account = new CloudStorageAccount(credentials, true);
            var client = account.CreateCloudTableClient();

            var tableRef = client.GetTableReference("ethtransactions");
            await tableRef.CreateIfNotExistsAsync();

            TableOperation ops = TableOperation.InsertOrMerge(contract);
            await tableRef.ExecuteAsync(ops);
            return true;
        }

        public async Task<EthereumContractInfo> GetContractFromTableStorage(string name)
        {
            StorageCredentials credentials = new StorageCredentials(_storageAccount, _storageKey);
            CloudStorageAccount account = new CloudStorageAccount(credentials, true);
            var client = account.CreateCloudTableClient();

            var tableRef = client.GetTableReference("ethtransactions");
            await tableRef.CreateIfNotExistsAsync();

            TableOperation ops = TableOperation.Retrieve<EthereumContractInfo>("contract", name);
            var tableResult = await tableRef.ExecuteAsync(ops);
            if (tableResult.HttpStatusCode == 200)
                return (EthereumContractInfo)tableResult.Result;
            else
                return null;
        }

        public async Task<decimal> GetBallance(string address)
        {
            var ballance = await _web3.Eth.GetBalance.SendRequestAsync(address);
            return _web3.Convert.FromWei(ballance.Value, 18);
        }

        public async Task<bool> ReleaseContract(string name, string abi, string byteCode)
        {
            // check contractName
            var existing = await this.GetContractFromTableStorage(name);
            if (existing != null) throw new Exception($"Contract {name} is present in storage");

            var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, Password, new Nethereum.Hex.HexTypes.HexBigInteger(120));
            if (resultUnlocking)
            {
                try
                {
                    var transactionHash = await _web3.Eth.DeployContract.SendRequestAsync(abi, byteCode, AccountAddress, new Nethereum.Hex.HexTypes.HexBigInteger(4700000), 4);


                    EthereumContractInfo eci = new EthereumContractInfo(name, abi, byteCode, transactionHash);
                    return await SaveContractToTableStorage(eci);
                }
                catch(Exception exc)
                {
                    
                }

            }
            return false;

        }

        public async Task<string> TryGetContractAddress(string name)
        {
            // check contractName
            var existing = await this.GetContractFromTableStorage(name);
            if (existing == null) throw new Exception($"Contract {name} does not exist in storage");

            if (!String.IsNullOrEmpty(existing.ContractAddress))
                return existing.ContractAddress;
            else
            {
                var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, Password, new Nethereum.Hex.HexTypes.HexBigInteger(120));
                if (resultUnlocking)
                {
                    var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(existing.TransactionHash);
                    if (receipt != null)
                    {
                        existing.ContractAddress = receipt.ContractAddress;
                        await SaveContractToTableStorage(existing);
                        return existing.ContractAddress;
                    }

                }

            }
            return null;
        }

        public async Task<Contract> GetContract(string name)
        {
            var existing = await this.GetContractFromTableStorage(name);
            if (existing == null) throw new Exception($"Contract {name} does not exist in storage");
            if (existing.ContractAddress == null) throw new Exception($"Contract address for {name} is empty. Please call TryGetContractAddress until it returns the address");

            var resultUnlocking = await _web3.Personal.UnlockAccount.SendRequestAsync(AccountAddress, Password, new Nethereum.Hex.HexTypes.HexBigInteger(120));
            if (resultUnlocking)
            {
                return _web3.Eth.GetContract(existing.Abi, existing.ContractAddress);

            }
            return null;
        }
    }
}
