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
        private const string abi = @"[{""constant"":false,""inputs"":[],""name"":""name"",""outputs"":[{""name"":""a"",""type"":""string""}],""payable"":false,""type"":""function""},{""constant"":false,""inputs"":[{""name"":""a"",""type"":""int256""}],""name"":""multiply"",""outputs"":[{""name"":""r"",""type"":""int256""}],""payable"":false,""type"":""function""},{""constant"":false,""inputs"":[],""name"":""hunderdOne"",""outputs"":[{""name"":""r"",""type"":""int256""}],""payable"":false,""type"":""function""},{""constant"":false,""inputs"":[{""name"":""a"",""type"":""int256""}],""name"":""double"",""outputs"":[{""name"":""r"",""type"":""int256""}],""payable"":false,""type"":""function""},{""inputs"":[{""name"":""multiplier"",""type"":""int256""}],""payable"":false,""type"":""constructor""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""a"",""type"":""int256""},{""indexed"":true,""name"":""sender"",""type"":""address""},{""indexed"":false,""name"":""result"",""type"":""int256""}],""name"":""Multiplied"",""type"":""event""}]";
        private const string byteCode = "0x606060405234610000576040516020806102af833981016040528080519060200190919050505b806000819055505b505b6102708061003f6000396000f30060606040526000357c0100000000000000000000000000000000000000000000000000000000900463ffffffff16806306fdde031461005f5780631df4f144146100f55780635783b6c5146101265780636ffa1caa14610149575b610000565b346100005761006c61017a565b60405180806020018281038252838181518152602001915080519060200190808383600083146100bb575b8051825260208311156100bb57602082019150602081019050602083039250610097565b505050905090810190601f1680156100e75780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b346100005761011060048080359060200190919050506101c8565b6040518082815260200191505060405180910390f35b3461000057610133610229565b6040518082815260200191505060405180910390f35b34610000576101646004808035906020019091905050610233565b6040518082815260200191505060405180910390f35b6020604051908101604052806000815250604060405190810160405280600481526020017f6675636b0000000000000000000000000000000000000000000000000000000081525090505b90565b6000600054820290503373ffffffffffffffffffffffffffffffffffffffff16827f841774c8b4d8511a3974d7040b5bc3c603d304c926ad25d168dacd04e25c4bed836040518082815260200191505060405180910390a38090505b919050565b6000606590505b90565b60006002820290508090505b9190505600a165627a7a72305820875e36359e856c66f01da156ab70eda06c1a2a827456fbacf7aa86905f42a7f90029";

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
        public async Task<int> ExecuteContract([FromRoute] string name, [FromRoute] int value)
        {
            string contractAddress = await service.TryGetContractAddress(name);
            var contract = await service.GetContract(name);
            if (contract == null) throw new System.Exception("Contact not present in ethereum");
            var multiplyFunction = contract.GetFunction("multiply");

            var result = await multiplyFunction.CallAsync<int>(value);

            return result;

        }
    }
}
