using System;
using NLog.Web;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BlogsConsole
{
    class Program
    {

        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();
        static void Main(string[] args)
        {
            logger.Info("Program started");

            try
            {
                string choice;
                do
                {
                    Console.WriteLine("1) Display All Blogs");
                    Console.WriteLine("2) Add Blog");
                    Console.WriteLine("3) Create Post");
                    Console.WriteLine("4) Display Posts");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");
                    if (choice == "1") //display all blogs
                    {
                        
                        var db = new BloggingContext();
                        var query = db.Blogs.OrderBy(p => p.Name);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{query.Count()} blog records returned");
                        Console.ForegroundColor = ConsoleColor.White;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.BlogId} - {item.Name}");
                        }
                        
                    }
                    else if (choice == "2") //add blog
                    {
                        //make blog obj, grab input for blog.Name
                        Blog blog = new Blog();
                        Console.WriteLine("Enter a name for a new Blog:");
                        blog.Name = Console.ReadLine();
                        var isValid = false;
                        //if name is null, die honorably
                        if (blog.Name == "") {
                            logger.Error("Blog Name cannot be null!");
                        } else {
                            isValid = true;
                        }
                        if (isValid)
                        {
                            var db = new BloggingContext();
                            // check for unique name
                            if (db.Blogs.Any(c => c.Name == blog.Name))
                            {
                                isValid = false;
                                logger.Error("Name exists");
                            }
                            else
                            {
                                db.AddBlog(blog);
                                logger.Info($"Blog added - {blog.Name}");
                            }
                        }
                    }
                    else if (choice == "3") //create post
                    {   
                        //get all blogs to show which to add post to
                        var db = new BloggingContext();
                        var query = db.Blogs.OrderBy(p => p.BlogId);

                        Console.WriteLine("Enter the id of a Blog to add a Post:");
                        foreach (var b in query)
                        {
                            Console.WriteLine($"[{b.BlogId}] {b.Name}");
                        }
                        
                        try {
                            //this'll throw a FormatException if an int wasn't entered
                            int id = int.Parse(Console.ReadLine());
                            //check for valid id
                            if (!db.Blogs.Any(c => c.BlogId == id)) {
                                logger.Error("There are no Blogs saved with that Id");
                            } else {
                                logger.Info($"Blog {id} selected");
                                
                                //enter post title
                                Console.WriteLine("Enter the Post title:");
                                var t = Console.ReadLine();
                                //null title check
                                if ( t == "" ) {
                                    logger.Error("Post title cannot be null");
                                } else {
                                    Console.WriteLine("Enter the Post Content:");
                                    var c = Console.ReadLine();
                                    var post = new Post();
                                    post.Title = t;
                                    post.Content = c;
                                    post.BlogId = id;
                                    db.AddPost(post);
                                    logger.Info($"Post added - {post.Title}");
                                }
                            }
                        } catch (System.FormatException) {
                            logger.Error($"Invalid Blog Id");
                        } catch (Exception e ) {
                            logger.Error(e.GetType());
                        }                                           
                    }                                        
                    else if (choice == "4") //display posts
                    {   
                        Console.WriteLine("Select the blog's posts to display:");
                        Console.WriteLine("0) Posts from all blogs");
                        var db = new BloggingContext();
                        var getBlogsquery = db.Blogs.OrderBy(p=>p.BlogId);
                        //get blog names
                        foreach(var b in getBlogsquery) {
                            Console.WriteLine($"{b.BlogId}) Posts from {b.Name}");
                        }
                        try { // throws format exception if input isnt an int
                            int id = int.Parse(Console.ReadLine());
                            IEnumerable<Blog> query;
                            if (id != 0) {
                                query = db.Blogs.Where(b => b.BlogId == id).Include("Posts").OrderBy(p => p.BlogId);
                            } else {
                                query = db.Blogs.Include("Posts").OrderBy(p => p.BlogId);
                            }
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"{query.Count()} blog records returned");
                            Console.ForegroundColor = ConsoleColor.White;
                            foreach (var item in query)
                            {
                                Console.WriteLine($"Blog: {item.Name} - Posts: {item.Posts.Count}");
                                foreach (Post p in item.Posts)
                                {
                                    Console.WriteLine($"\tTitle: {p.Title}");
                                    Console.WriteLine($"\tContent: {p.Content}");
                                }
                            }
                            } catch (System.FormatException) {
                                logger.Error("Please enter 0 or a valid blog id");
                        }
                       
                    }
                    Console.WriteLine();
                    
                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }
    }
}
