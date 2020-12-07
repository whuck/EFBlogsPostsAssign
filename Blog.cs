using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace BlogsConsole
{
        public class Blog
    {
        public int BlogId { get; set; }
        public string Name { get; set; }
        [Required(ErrorMessage = "Blog Name cannot be null")]
        public List<Post> Posts { get; set; }
    }
}
