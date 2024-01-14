using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace tretton37uppgift
{

    public partial class WebSiteDownloader
    {
        [Fact]
        public async void DownloadHtml_Success()
        {
            //TODO: This doesn't actually work
            var expectedresult = File.ReadAllText("TestHtml.html", Encoding.UTF8);
            var result = "";
            int expectedTries = 0;
            int tries = 0;
            bool failed = false;


            try
            {
                result = await DownloadHtml(new HttpClient(), "http://quotes.toscrape.com", 0);
            }
            catch (DownloadFailedException ex)
            {
                failed = true;
                tries = ex.Retries;
            }

            //assert
            Assert.Equal(expectedresult, result);
            Assert.Equal(expectedTries, tries);
            Assert.False(failed);

        }

        [Fact]
        public async void DownloadResource_Success()
        {
            var expectedresult = File.ReadAllBytes("TestResource.jpg");
            byte[] result = new byte[1];
            int expectedTries = 0;
            int tries = 0;
            bool failed = false;


            try
            {
                result = await DownloadResource(new HttpClient(), @"http://books.toscrape.com/media/cache/00/08/0008e65aa431ed3625ad3a4352f8e90d.jpg", 0);
            }
            catch (DownloadFailedException ex)
            {
                failed = true;
                tries = ex.Retries;
            }

            //assert
            Assert.Equal(expectedresult, result);
            Assert.Equal(expectedTries, tries);
            Assert.False(failed);

        }

        [Fact]
        public async void DownloadResource_RetriesFiveTimesThenFails()
        {
            //arrange
            var expectedresult = new Byte[1];
            var result = new Byte[1];
            int expectedTries = 5;
            int tries = 0;
            bool failed = false;

            //act
            try
            {
                result = await DownloadResource(new HttpClient(), "http://localhost", 0);
            }
            catch (DownloadFailedException ex) 
            { 
                failed = true; 
                tries = ex.Retries; 
            }

            //assert
            Assert.Equal(expectedresult, result);
            Assert.Equal(expectedTries, tries);
            Assert.True(failed);
        }


        [Fact]
        public async void DownloadHTML_RetriesFiveTimesThenFails()
        {
            var expectedresult = "";
            var result = "";
            int expectedTries = 5;
            int tries = 0;
            bool failed = false;

            //act
            try
            {
                result = await DownloadHtml(new HttpClient(), "http://localhost", 0);
            }
            catch (DownloadFailedException ex)
            {
                failed = true;
                tries = ex.Retries;
            }

            //assert
            Assert.Equal(expectedresult, result);
            Assert.Equal(expectedTries, tries);
            Assert.True(failed);
        }

    }

}
