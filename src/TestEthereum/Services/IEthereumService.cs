using Nethereum.Web3; 
using System.Threading.Tasks;
using TestEthereum.Model;

namespace TestEthereum.Services
{
    public interface IEthereumService
    {
        string AccountAddress { get; set; } 
        Task<bool> SaveContractToTableStorage(EthereumContractInfo contract);
        Task<EthereumContractInfo> GetContractFromTableStorage(string name);
        Task<decimal> GetBallance(string address);
        Task<bool> ReleaseContract(string name, string abi, string byteCode);
        Task<string> TryGetContractAddress(string name);

        Task<Contract> GetContract(string name);

    }
}
