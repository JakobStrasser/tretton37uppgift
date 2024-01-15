Console application to crawl and download http://books.toscrape.com

Download and build application (.net 8)

Usage: tretton37uppgift download [--u <String>] [--o <String>] [--help]

Options:
--u <String>    URL to download
--o <String>    Output folder
-h, --help      Show help message


Design 

The objective was to write a website scraper that would replicate the file structure of the downloaded website enabling offline viewing.

Thee application should do the following:

*  Recursively traverses all pages on https://books.toscrape.com/
*  Downloads and saves all files (pages, images...) to disk while keeping the file structure
*  Shows some kind of progress information in the console

I first wrote the recursive algorithm that downloads the website and follows all internal links, as well as saves static resources like images. Then I ensured that saving to disk works.
Next was adding async/await as well as anonymous functions for parallelism.

A while ago I saw a youtube video about a CLI package, Cocona, that makes console applications run like web applications in .net with a host so added that in to try it out.

There was no easy unit tests to do since the application was one big recursive function so broke out the download function and wrote a unit test for that as well as handling retries on timeouts or other request-issues.

Testing had to be put in a partial class because of the CLI package. So I did not use the recommended pattern of dependency injection of a HttpClientFactory.
