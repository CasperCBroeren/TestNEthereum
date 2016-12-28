using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestEthereum.Services;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TestEthereum.Controllers
{
    [Route("api/[controller]")]
    public class EthereumTestController : Controller
    {
        private IEthereumService service;
        private const string abi = @"[{""constant"":false,""inputs"":[{""name"":""a"",""type"":""int256""}],""name"":""multiply"",""outputs"":[{""name"":""r"",""type"":""int256""}],""payable"":false,""type"":""function""},{""constant"":true,""inputs"":[],""name"":""lastResult"",""outputs"":[{""name"":"""",""type"":""int256""}],""payable"":false,""type"":""function""},{""inputs"":[{""name"":""multiplier"",""type"":""int256""}],""payable"":false,""type"":""constructor""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""a"",""type"":""int256""},{""indexed"":true,""name"":""sender"",""type"":""address""},{""indexed"":false,""name"":""result"",""type"":""int256""}],""name"":""Multiplied"",""type"":""event""}]";
        private const string byteCode = "0x60606040523461000057604051602080610176833981016040528080519060200190919050505b806001819055505b505b6101378061003f6000396000f30060606040526000357c0100000000000000000000000000000000000000000000000000000000900463ffffffff1680631df4f14414610049578063abcc11d81461007a575b610000565b3461000057610064600480803590602001909190505061009d565b6040518082815260200191505060405180910390f35b3461000057610087610105565b6040518082815260200191505060405180910390f35b6000600154820290503373ffffffffffffffffffffffffffffffffffffffff16827f841774c8b4d8511a3974d7040b5bc3c603d304c926ad25d168dacd04e25c4bed836040518082815260200191505060405180910390a3806000819055508090505b919050565b600054815600a165627a7a72305820682de15aedf9350417ece6b0c84246ebdb0d8f586a11a6bfc3177bf0504447e30029";

        public EthereumTestController(IEthereumService ethereumService)
        {
            service = ethereumService;
        }

        [HttpGet]
        [Route("getBalance/{walletAddress}")]
        public async Task<decimal> GetBalance([FromRoute]string walletAddress)
        {
            return await service.GetBallance(walletAddress);
        }
        [HttpGet]
        [Route("releaseContract/{name}")]
        public async Task<bool> ReleaseContract([FromRoute] string name)
        {
            return await service.ReleaseContract(name, abi, byteCode);
        }

        [HttpGet]
        [Route("checkContract/{name}")]
        public async Task<bool> CheckContract([FromRoute] string name)
        {
            return await service.TryGetContractAddress(name) != null;
        }
        [HttpGet]
        [Route("exeContract/{name}/{value}")]
        public async Task<string> ExecuteContract([FromRoute] string name, [FromRoute] int value)
        {
            string contractAddress = await service.TryGetContractAddress(name);
            var contract = await service.GetContract(name);
            if (contract == null) throw new System.Exception("Contact not present in ethereum");
            var multiplyFunction = contract.GetFunction("multiply");
            try
            {
                //multiplyFunction.CallAsync<int>(value);
                var result = await multiplyFunction.SendTransactionAsync(service.AccountAddress, value);
                return result;
            }
            catch(Exception ex)
            {
                return "error";
            }
   

        }

        [HttpGet]
        [Route("checkLV/{name}")]
        public async Task<int> CheckLV([FromRoute] string name)
        {
            string contractAddress = await service.TryGetContractAddress(name);
            var contract = await service.GetContract(name);
            if (contract == null) throw new System.Exception("Contact not present in ethereum");
            var multiplyFunction = contract.GetFunction("lastResult");

            var result = await multiplyFunction.CallAsync<int>();

            return result;

        }
    }
}
