using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Blogs.Models;
using Oqtane.Blogs.Repository;

namespace Oqtane.Blogs.Controllers
{
    [Route("{site}/api/[controller]")]
    public class BlogController : Controller
    {
        private readonly IStreamHubRepository _Blogs;
        private readonly ILogManager _logger;

        public BlogController(IStreamHubRepository Blogs, ILogManager logger)
        {
            _Blogs = Blogs;
            _logger = logger;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Roles = Constants.RegisteredRole)]
        public IEnumerable<StreamHub> Get(string moduleid)
        {
            return _Blogs.GetBlogs(int.Parse(moduleid));
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public StreamHub Get(int id)
        {
            return _Blogs.GetBlog(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public StreamHub Post([FromBody] StreamHub Blog)
        {
            if (ModelState.IsValid)
            {
                Blog = _Blogs.AddBlog(Blog);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Blog Added {Blog}", Blog);
            }
            return Blog;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public StreamHub Put(int id, [FromBody] StreamHub Blog)
        {
            if (ModelState.IsValid)
            {
                Blog = _Blogs.UpdateBlog(Blog);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Blog Updated {Blog}", Blog);
            }
            return Blog;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            _Blogs.DeleteBlog(id);
            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Blog Deleted {BlogId}", id);
        }
    }
}
