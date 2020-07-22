using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XUnit.Core.Model
{
    /// <summary>
    /// 这是电影实体类
    /// </summary>
    public class MovieModel
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 影片名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }
    }
}
