using System.Net;
using Microsoft.AspNetCore.Mvc;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Server.Controllers
{
    [Route("v1/[controller]")]
    public class SysController : Controller
    {
        private MockServer _server;

        public SysController(MockServer server)
        {
            _server = server;
        }

        // TODO: just for debugging
        [HttpGet("settings")]
        public ServerSettings GetSettings()
        {
            return _server.Settings;
        }

        // GET api/values
        [HttpGet("health")]
        public HealthStatus GetHealth()
        {
            return new HealthStatus
            {
                Initialized = _server.Health.Initialized,
                Sealed = _server.Health.Sealed,
                Standby = _server.Health.Standby,

                ServerTimeUtc = _server.Health.ServerTimeUtc,
                Version = _server.Health.Version,

                ClusterId = _server.Health.ClusterId,
                ClusterName = _server.Health.ClusterName,
            };
        }

        [HttpGet("init")]
        public InitializationStatus GetInitStatus()
        {
            return _server.GetInitializationStatus();
        }

        [HttpGet("seal-status")]
        public SealStatus GetSealStatus()
        {
            return _server.GetSealStatus() ?? throw new VaultServerException(
                    HttpStatusCode.BadRequest,
                    "server is not yet initialized");
        }

        [HttpGet("key-status")]
        public KeyStatus GetKeyStatus()
        {
            return _server.GetKeyStatus() ?? throw new VaultServerException(
                    HttpStatusCode.ServiceUnavailable,
                    "Vault is sealed");
        }

        [HttpGet("leader")]
        public LeaderStatus GetLeaderStatus()
        {
            return _server.GetLeaderStatus() ?? throw new VaultServerException(
                    HttpStatusCode.ServiceUnavailable,
                    "Vault is sealed");
        }

        [HttpPut("init")]
        public InitializationResponse DoInit([FromBody]InitializationRequest requ)
        {
            return _server.Initialize(requ.SecretShares, requ.SecretThreshold)
                    ?? throw new VaultServerException(
                            HttpStatusCode.BadRequest,
                            "Vault is already initialized");
        }

        [HttpPut("unseal")]
        public SealStatus DoUnseal([FromBody]UnsealRequest requ)
        {
            return _server.Unseal(requ.Key, requ.Reset.GetValueOrDefault())
                    // TODO:  confirm this is the correct response for this state
                    ?? throw new VaultServerException(
                            HttpStatusCode.BadRequest,
                            "server is not yet initialized");
        }
    }
}