using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XUnit.Core.Model;

namespace XUnit.Core.Controllers
{
    /// <summary>
    /// 电影管理
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize(Policy = "customizePermisson")]
    public class MovieController: ControllerBase
    {
        public static List<MovieModel> movielist = new List<MovieModel>
        {
            new MovieModel{Id=1,Name="灌篮高手",Type="动漫"},
            new MovieModel{Id=2,Name="火影忍者",Type="动漫"},
            new MovieModel{Id=3,Name="寄生虫",Type="电影"},
            new MovieModel{Id=4,Name="阿凡达",Type="电影"}
        };
        public MovieController()
        {

        }
        /// <summary>
        /// 获取所有的电影
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<MovieModel> GetAllMovies()
        {
            return movielist;
        }
        /// <summary>
        /// 根据影视ID获取对应的电影
        /// </summary>
        /// <param name="id">影视ID</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IEnumerable<MovieModel> GetMovieModel(int id)
        {
            var movie = movielist.Where(a=>a.Id==id);

            return movie;
        }
        /// <summary>
        /// 新增电影
        /// </summary>
        /// <param name="id">影视ID</param>
        /// <param name="Name">电影名称</param>
        /// <param name="Type">电影类型</param>
        /// <returns></returns>
        [HttpGet]
        public object AddMovie(int id, string Name, string Type)
        {

            movielist.Add(new MovieModel { Id =
                id, Name = Name, Type = Type });

            return Ok(new { data = 0, msg = "新增成功" });
        }


    }
}
