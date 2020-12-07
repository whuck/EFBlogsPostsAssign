using System;
using NLog.Web;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BlogsConsole
{
    class Program
    {

        //nlog dotnet add package NLog.Web.AspNetCore
        //dotnet add package Microsoft.EntityFrameworkCore.SqlServer
        //dotnet add package Microsoft.EntityFrameworkCore.Tools
//         // create static instance of Logger
//
//creates migrations folder an dclasses to do data migration
// - dotnet ef migrations add CreateDatabase
// - dotnet ef database update
//dotnet add package Microsoft.Extension.Configuration

//dotnet tool update --global dotnet-ef
// only have to do that once.
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
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.BlogId} - {item.Name}");
                            // foreach (var post in item.Posts) {
                            //     Console.WriteLine($"Post:{post.PostId}-{post.Title}::{post.Content}");
                            // }
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (choice == "2") //add blog
                    {
                        Blog blog = new Blog();
                        Console.WriteLine("Enter a name for a new Blog:");
                        blog.Name = Console.ReadLine();

                        ValidationContext context = new ValidationContext(blog, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(blog, context, results, true);
                        if (isValid)
                        {
                            var db = new BloggingContext();
                            // check for unique name
                            if (db.Blogs.Any(c => c.Name == blog.Name))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists", new string[] { "Name" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                db.AddBlog(blog);
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "3") //create post
                    {   
                        //get all blogs to show which to add post to
                        var db = new BloggingContext();
                        var query = db.Blogs.OrderBy(p => p.BlogId);

                        Console.WriteLine("Enter the id of a Blog to add a Post:");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (var b in query)
                        {
                            Console.WriteLine($"[{b.BlogId}] {b.Name}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        
                        int id = int.Parse(Console.ReadLine());

                        //error id is not a number
                        //error id does not exist in db
                        //Console.Clear();
                        logger.Info($"Blog {id} selected");
                //enter post title
                        Console.WriteLine("Enter the Post title:");
                        var t = Console.ReadLine();
                //error title cannot be null
                //enter post content- can be null
                        Console.WriteLine("Enter the Post Content:");
                        var c = Console.ReadLine();

                        var post = new Post();
                        post.Title = t;
                        post.Content = c;
                        post.BlogId = id;
                        db.AddPost(post);

                        //lazy loading, will not populate the category.Products list
                        //Category category = db.Categories.FirstOrDefault(c => c.CategoryId == id);
                        
                        //eager loading, will load Products list
                        //Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                        
                        // Console.WriteLine($"{blog.CategoryName} - {blog.Description}");
                        // foreach (Post p in blog.Posts)
                        // {
                        //     Console.WriteLine(p.ProductName);
                        // }                                                
                    }                                        
                    else if (choice == "4") //display posts
                    {   
                        Console.WriteLine("Select the blog's posts to display:");
                        Console.WriteLine("0) Posts from all blogs");
                        var db = new BloggingContext();
                        var getBlogsquery = db.Blogs.OrderBy(p=>p.BlogId);
                        foreach(var b in getBlogsquery) {
                            Console.WriteLine($"{b.BlogId}) Posts from {b.Name}");
                        }
                        
                        int id = int.Parse(Console.ReadLine());
                        
                        var query = db.Blogs.Where(b => b.BlogId == id).Include("Posts").OrderBy(p => p.BlogId);
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.Name}");
                            foreach (Post p in item.Posts)
                            {
                                Console.WriteLine($"\t{p.Title}::{p.Content}");
                            }
                        }
                    }
                    Console.WriteLine();
                } while (choice.ToLower() != "q");
                // // Create and save a new Blog
                // Console.Write("Enter a name for a new Blog: ");
                // var name = Console.ReadLine();

                // var blog = new Blog { Name = name };

                // var db = new BloggingContext();
                // db.AddBlog(blog);
                // logger.Info("Blog added - {name}", name);

                // // Display all Blogs from the database
                // var query = db.Blogs.OrderBy(b => b.Name);

                // Console.WriteLine("All blogs in the database:");
                // foreach (var item in query)
                // {
                //     Console.WriteLine(item.Name);
                // }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }
    }
}
