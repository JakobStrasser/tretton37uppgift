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
        public void DownloadResource_Success()
        {
            var expectedresult = File.ReadAllBytes("TestResource.jpg");
            byte[] result = new byte[1];
            int expectedTries = 0;
            int tries = 0;
            bool failed = false;


            try
            {
                result =  DownloadResource(new HttpClient(), @"http://books.toscrape.com/media/cache/00/08/0008e65aa431ed3625ad3a4352f8e90d.jpg", 5);
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
        public void DownloadResource_RetriesThenFails()
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
                result = DownloadResource(new HttpClient(), "http://localhost", 2);
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
