using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tretton37uppgift
{
    internal class DownloadFailedException : Exception
    {
        public int Retries { get; init; }
        public new Exception InnerException { get; init; }

        public DownloadFailedException(int retries, Exception innerException)
        {
            Retries = retries;
            InnerException = innerException;
        }
    }
}
