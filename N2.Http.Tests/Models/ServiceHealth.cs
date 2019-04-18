using System.Collections.Generic;
using N2.Http;

namespace XUnitHttpClientTests.Models
{
    public class ServiceHealth
    {
        public string ServiceName { get; set; }
        public string AssemblyName { get; set; }
        public string AssemblyVersion { get; set; }
        public ICollection<string> IpAddresses { get; set; }
        public ResponseCode ResponseCode { get; set; }
        public string HealthMessage { get; set; }
        public ICollection<ServiceHealth> ChildServices { get; set; }
    }
}
