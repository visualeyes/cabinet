# Cabinet 
[![Build status](https://ci.appveyor.com/api/projects/status/q7c183o0jte2roao/branch/master?svg=true)](https://ci.appveyor.com/project/visualeyes-builder/cabinet/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/visualeyes/cabinet/badge.svg?branch=master)](https://coveralls.io/github/visualeyes/cabinet?branch=master)


Cabinet provides abstractions over various file storage providers.
This allows you to develop IO code without having to worry about where the file is actually stored.

Plugging in a new provider is easy as:
```csharp
cabinetFactory.RegisterFileSystemProvider()
``` 
and 
```csharp
IFileCabinet fileCabinet = cabinetFactory.GetCabinet(new FileSystemCabinetConfig() {
    Directory = @"C:\data\"
});
```

The project has Core packages and Provider packages.
Core packages provide abstractions and helpers.

| Core Packages | Version |
| ------------- | ------- |
| Core    | [![Cabinet.Core Nuget Version](https://img.shields.io/nuget/v/Cabinet.Core.svg)](https://www.nuget.org/packages/Cabinet.Core/) |
| Config  | [![Cabinet.Config Nuget Version](https://img.shields.io/nuget/v/Cabinet.Config.svg)](https://www.nuget.org/packages/Cabinet.Config/) |
| Web     | [![Cabinet.Web Nuget Version](https://img.shields.io/nuget/v/Cabinet.Web.svg)](https://www.nuget.org/packages/Cabinet.Web/) |

We current support the following providers

| Provider              | Provider Package | Config Package |
| --------------------- | ---------------- | -------------- | 
| File System           | [![Cabinet.FileSystem Nuget Version](https://img.shields.io/nuget/v/Cabinet.FileSystem.svg)](https://www.nuget.org/packages/Cabinet.FileSystem/) | [![Cabinet.FileSystem.Config Nuget Version](https://img.shields.io/nuget/v/Cabinet.FileSystem.Config.svg)](https://www.nuget.org/packages/Cabinet.FileSystem.Config/) |
| Migrator              | [![Cabinet.Migrator Nuget Version](https://img.shields.io/nuget/v/Cabinet.Migrator.svg)](https://www.nuget.org/packages/Cabinet.Migrator/) | [![Cabinet.Migrator.Config Nuget Version](https://img.shields.io/nuget/v/Cabinet.Migrator.Config.svg)](https://www.nuget.org/packages/Cabinet.Migrator.Config/) |
| Amazon S3             | [![Cabinet.S3 Nuget Version](https://img.shields.io/nuget/v/Cabinet.S3.svg)](https://www.nuget.org/packages/Cabinet.S3/) | [![Cabinet.S3.Config Nuget Version](https://img.shields.io/nuget/v/Cabinet.S3.Config.svg)](https://www.nuget.org/packages/Cabinet.S3.Config/) |
| Azure Blog            | [TODO](https://github.com/visualeyes/cabinet/issues/1) | [TODO](https://github.com/visualeyes/cabinet/issues/1) |
| Google Cloud Storage  | [TODO](https://github.com/visualeyes/cabinet/issues/13) | [TODO](https://github.com/visualeyes/cabinet/issues/13) |

## Getting Started with Cabinet
Cabinet is designed to be very configurable and pluggable.

```csharp
IFileCabinetFactory cabinetFactory = new FileCabinetFactory(); // or inject IFileCabinetFactory with IOC

cabinetFactory
    .RegisterFileSystemProvider() // Register a file system provider to store files on disk
    .RegisterS3Provider(); // Register an Amazon S3 provider

IFileCabinet fileCabinet = cabinetFactory.GetCabinet(new FileSystemCabinetConfig() {
    Directory = @"C:\data\"
});

IFileCabinet s3Cabinet = cabinetFactory.GetCabinet(new S3CabinetConfig() {
    AWSCredentials = new BasicAWSCredentials("accessKey", "secretKey"), 
    AmazonS3Config = new AmazonS3Config(),
    BucketName = "test-bucket"
});

string fileKey = "foo/bar.txt";

using (var stream = await s3Cabinet.OpenReadStreamAsync(fileKey)) { // open stream for file in s3
    ISaveResult saveResult = await fileCabinet.SaveFileAsync(fileKey, stream, HandleExistingMethod.Overwrite); // save file to disk
}

```

# Questions or Problems?
Open an issue

# Contributions
Yes please. Please open an issue before before sending a pull requests.
All changes need to be discussed before they are accepted.

# License
MIT