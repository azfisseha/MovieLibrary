using CsvHelper;
using CsvHelper.Configuration.Attributes;
using MovieLibrary.Models;
using System;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
//using Microsoft.Extensions.Logging;

namespace MovieLibrary.Services;

/// <summary>
///     This concrete service and method only exists an example.
///     It can either be copied and modified, or deleted.
/// </summary>
public class FileService : IFileService
{
    //I haven't explored logging with C# in approximately a decade..
    //I'll pretend logging doesn't exist for now, but I'll leave the previously existing sample code for future reference.

    //private readonly ILogger<IFileService> _logger;
    
    private readonly string _fileName;

    public List<Movie> Movies { get; set; } = new();

    //public FileService(ILogger<IFileService> logger)
    public FileService()
    {
        //_logger = logger;
        _fileName = "Files/movies.csv";
    }
    
    public void Display()
    {
        //Modified solution based on the MovieRepository solution covered in class

        if(Movies.Count == 0)
        {
            //Log error here
            // (or would it be better to simply call Read() and continue with displaying?)
            Console.WriteLine("Read movies from the file first before displaying");
            return;
        }

        Console.WriteLine($"{"Id",-5} | {"Title",-80} | {"Genres",-10}");
        var entry = 0;

        foreach (var movie in Movies) 
        {
            var movies = Movies.Skip(entry).Take(10);

            foreach (var m in movies)
            {
                Console.WriteLine($"{m.Id,-5} | {m.Title,-80} | {string.Join(",", m.Genres),-10}");
                entry++;
            }
            movies = Movies.Skip(entry).Take(10);

            if (!ContinueDisplaying())
            {
                break;
            }
        }
    }

    private bool ContinueDisplaying()
    {
        Console.WriteLine("Hit Enter to continue or ESC to cancel");
        var input = Console.ReadKey();
        while (input.Key != ConsoleKey.Enter && input.Key != ConsoleKey.Escape)
        {
            input = Console.ReadKey();
            Console.WriteLine("Hit Enter to continue or ESC to cancel");
        }

        if (input.Key == ConsoleKey.Escape)
        {
            return false;
        }

        return true;
    }

    public void Read()
    {
        //Solution based on the MovieRepository solution covered in class

        //_logger.Log(LogLevel.Information, "Reading");
        if(!File.Exists(_fileName))
        {
            //log error
            Console.WriteLine($"File {_fileName} does not exist");
        }
        else{
            using (var sr = new StreamReader(_fileName))
            {
                using (var csv = new CsvReader(sr, CultureInfo.InvariantCulture)) 
                {
                    csv.Context.RegisterClassMap<MovieMap>();

                    var movie = new Movie();
                    var records = csv.GetRecords<Movie>();
                    Movies = records.ToList();
                }
            }
        }
    }

    public void Write()
    {
        //_logger.Log(LogLevel.Information, "Writing");

        if (!File.Exists(_fileName))
        {
            //log error here
            Console.WriteLine($"{_fileName} does not exist");
            return;
        }
        if (Movies.Count == 0)
        {
            //Log error here
            // (or would it be better to simply call Read() and continue with writing?)
            Console.WriteLine("Read in file first before updating");
            return;
        }

        var movie = new Movie();

        Console.Write("Enter Title: ");
        movie.Title = Console.ReadLine();
        var identical = false;
        foreach (var record in Movies)
        {
            if (string.Equals(movie.Title, record.Title, StringComparison.InvariantCulture))
            {
                Console.WriteLine("Could not add movie entry - identical to existing entry");
                Console.WriteLine($"ID : {record.Id} | Title : {record.Title} | {string.Join(",", record.Genres)}");
                identical = true;
                break;
            }
        }

        if (!identical)
        {
            movie.Genres = new List<string>();
            var addtlGenres = true;
            do
            {
                Console.Write("Enter Genre (X when done): ");

                string input = Console.ReadLine();
                if (input.ToUpper() == "X")
                {
                    if (movie.Genres.Count == 0)
                    {
                        Console.WriteLine("Enter at least one Genre to continue");
                    }
                    else
                    {
                        addtlGenres = false;
                    }
                }
                else
                {
                    movie.Genres.Add(input);
                }
            } while (addtlGenres);


            movie.Id = Movies[Movies.Count - 1].Id + 1;
            Movies.Add(movie);
            using (var stream = new StreamWriter(_fileName, true))
            {
                //Need to spend some more understanding the CSVHelper package.
                //The commented code isn't actually updating the file as expected.

                //using (var csv = new CsvWriter(stream, CultureInfo.InvariantCulture))
                //{
                //    csv.Context.RegisterClassMap<MovieMap>();
                //    csv.WriteRecords(Movies);
                //    csv.Flush();
                //}

                stream.WriteLine($"{movie.Id},{movie.Title},{string.Join("|", movie.Genres)}");
                stream.Close();
            }
        }
    }
}
