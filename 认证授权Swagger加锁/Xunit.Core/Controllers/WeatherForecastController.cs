using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XUnit.Core.Model;

namespace XUnit.Core.Controllers
{
    /// <summary>
    /// 这是一个天气预报
    /// </summary>
    [ApiController]
    [Route("[controller]")]

    //[Authorize]
    // [Authorize(Roles ="admin")]
    //[Authorize(Roles ="admin,user")]
    //[Authorize(Roles = "admin")]
    //[Authorize(Roles ="user")]
    //[Authorize(Policy = "BaseRole")]

    // [Authorize(Policy = "BaseClaims")]

    [ApiExplorerSettings(IgnoreApi =true)]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// 看这 这是一个API接口注释的地方
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        /// <summary>
        /// values带id参数的get
        /// </summary>
        /// <param name="id"></param>
        /// <response code="201">返回value字符串</response>
        /// <response code="400">如果id为空</response>  
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public string Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// post方式提交电影名称
        /// </summary>
        /// <param name="movie"></param>
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public  string Post(MovieModel movie)
        {

            return movie.Name;
        }
    }
}
