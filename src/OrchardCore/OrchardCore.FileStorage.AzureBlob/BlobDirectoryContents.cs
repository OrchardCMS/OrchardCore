using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.FileStorage.AzureBlob
{
    public class BlobDirectoryContents : IDirectoryContents
    {
        private readonly CloudBlobDirectory _blobDirectory;

        public bool Exists => this.Any();

        public BlobDirectoryContents(CloudBlobDirectory blobDirectory)
        {
            _blobDirectory = blobDirectory;
        }

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            return _blobDirectory.ListBlobs()
                .Select(blob => {
                    switch (blob)
                    {
                        case CloudBlobDirectory blobDirectory:
                            return (IFileInfo)new BlobDirectoryFileInfo(blobDirectory);
                        case CloudBlockBlob blockBlob:
                            return (IFileInfo)new BlockBlobFileInfo(blockBlob);
                        default:
                            throw new NotSupportedException($"{blob.GetType()} is not a supported blob type.");
                    }
                })
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
