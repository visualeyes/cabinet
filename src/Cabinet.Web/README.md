# Usage

# Security Considerations
Before using this project please consider the security implications of allowing file uploads.
This is not an exclusive list of all the aspects you may need to consider.

## Unrestricted File Upload
Files must be restricted to types you can trust. It is difficult to acheive this.
The following is based of the [OSWAP Guidelines](https://www.owasp.org/index.php/Unrestricted_File_Upload)

### File Names

#### Whitelist Filter
*Never accept a filename and its extension directly without having a white-list filter.*

`IFileUploadValidation` provides a method to check if a file extension is allowed `bool IsFileTypeWhitelisted(string extension, string contentType);`

#### Filename Algorithm
*It is recommended to use an algorithm to determine the filenames. For instance, a filename can be a MD5 hash of the name of file plus the date of the day.*

`IKeyProvider` provides a method to get a key based off the filename and content type `string GetKey(string fileName, string contentType);`

##### Control Characters
*All the control characters and Unicode ones should be removed from the filenames and their extensions without any exception. 
 Also, the special characters such as “;”, “:”, “>”, “<”, “/” ,”\”, additional “.”, “*”, “%”, “$”, and so on should be discarded as well. 
 If it is applicable and there is no need to have Unicode characters, it is highly recommended to only accept Alpha-Numeric characters and only 1 dot as an input for the file name and the extension; 
 in which the file name and also the extension should not be empty at all (regular expression: [a-zA-Z0-9]{1,200}\.[a-zA-Z0-9]{1,10}).*

##### Filename Length
Limit the filename length. For instance, the maximum length of the name of a file plus its extension should be less than 255 characters (without any directory) in an NTFS partition.

### File Sizes
*Limit the file size to a maximum value in order to prevent denial of service attacks (on file space or other web application’s functions such as the image resizer).*

`bool IsFileTooLarge(long fileSize)`

*Restrict small size files as they can lead to denial of service attacks. So, the minimum size of files should be considered.*

`bool IsFileTooSmall(long fileSize)`

### Prevent overwriting files
*Prevent from overwriting a file in case of having the same hash for both.*

`MultipartFileStreamProvider` puts file into unique folder in temp

Set to throw by default
`public async Task<ISaveResult[]> SaveInCabinet(HandleExistingMethod handleExisting = HandleExistingMethod.Throw, IFileScanner fileScanner = null)`

### Antivirus scanner
*Use a virus scanner on the server (if it is applicable).*
`IFileScanner` provides a method to scan and remove the file if it's dangerous `Task ScanFileAsync(string filePath);`


### CSRF
*Use Cross Site Request Forgery protection methods.*

TODO


### Logging
*Log users’ activities.*

TODO