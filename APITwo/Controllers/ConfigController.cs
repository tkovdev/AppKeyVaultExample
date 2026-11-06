using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APITwo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<string> GetConfig()
        {
            var dbCtx = _configuration.GetValue<string>("ConnectionStrings:AppDbContext");
            return dbCtx ?? throw new NullReferenceException("Config does not exist");
        }
    }
}