using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Dochain.Web.Interfaces
{
    [FunctionOutput]
    public class Document
    {
        [Parameter("uint256", "timestamp", 1)]
        public ulong Timestamp { get; set; }

        [Parameter("address", "sender", 3)]
        public string Sender { get; set; }
    }
}