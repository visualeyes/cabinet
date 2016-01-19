# Cabinet 
[![Build Status]()]()
[![Coverage Status]()]()
[![Cabinet Nuget Version]()]()
[![Cabinet Nuget Downloads]()]()

Cabinet provides abstractions over various file storage providers.
This allows you to develop IO code without having to worry about where the file is actually stored.

Plugging in a new provider is easy as:
``` csharp
cabinetFactory.RegisterFileSystemProvider()
``` 
and 
``` csharp
IFileCabinet fileCabinet = cabinetFactory.GetCabinet(new FileSystemCabinetConfig() {
    Directory = "C:\data\"
})
```

## Getting Started with Cabinet
// TODO: Description

``` csharp
IFileCabinetFactory cabinetFactory = new FileCabinetFactory(); // or inject IFileCabinetFactory with IOC

cabinetFactory
    .RegisterFileSystemProvider() // Register a file system provider to store files on disk
    .RegisterS3Provider(); // Register an Amazon S3 provider

IFileCabinet fileCabinet = cabinetFactory.GetCabinet(new FileSystemCabinetConfig() {
    Directory = "C:\data\"
});

IFileCabinet s3Cabinet = cabinetFactory.GetCabinet(new S3CabinetConfig() {
    AmazonS3Config = new AmazonS3Config(),
    BucketName = "test-bucket"
});

string fileKey = "foo/bar.txt";

ICabinetFileInfo file = s3Cabinet.GetFileAsync(fileKey);

using (var stream = file.GetFileReadStream()) {
    ISaveResult saveResult = await fileCabinet.SaveFileAsync(file.Key, stream, HandleExistingMethod.Overwrite);
}

```


## Full Example
// TODO: 