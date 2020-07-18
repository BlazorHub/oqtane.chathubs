using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Blogs.Models;

namespace Oqtane.Blogs.Services
{
    public interface IBlogService 
    {
        Task<List<Blog>> GetBlogsAsync(int ModuleId);

        Task<Blog> GetBlogAsync(int BlogId);

        Task<Blog> AddBlogAsync(Blog Blog);

        Task<Blog> UpdateBlogAsync(Blog Blog);

        Task DeleteBlogAsync(int BlogId);
    }
}
