using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interface;

/// <summary>
/// Interface for file system operations.
/// Provides methods for file and directory management.
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// Create a new file with specified content.
    /// </summary>
    /// <param name="path">The file path to create</param>
    /// <param name="content">The content to write to the file</param>
    /// <returns>A tuple containing the response message and status code</returns>
    Task<(string Response, int Status)> CreateFileAsync(string path, string content);

    /// <summary>
    /// Read the content of a file.
    /// </summary>
    /// <param name="path">The file path to read</param>
    /// <returns>A tuple containing the file content and status code</returns>
    Task<(string Content, int Status)> ReadFileAsync(string path);

    /// <summary>
    /// Write content to an existing file or create a new one.
    /// </summary>
    /// <param name="path">The file path to write to</param>
    /// <param name="content">The content to write</param>
    /// <param name="append">Whether to append to existing content or overwrite</param>
    /// <returns>A tuple containing the response message and status code</returns>
    Task<(string Response, int Status)> WriteFileAsync(string path, string content, bool append = false);

    /// <summary>
    /// Delete a file.
    /// </summary>
    /// <param name="path">The file path to delete</param>
    /// <returns>A tuple containing the response message and status code</returns>
    Task<(string Response, int Status)> DeleteFileAsync(string path);

    /// <summary>
    /// Copy a file from source to destination.
    /// </summary>
    /// <param name="source">The source file path</param>
    /// <param name="destination">The destination file path</param>
    /// <param name="overwrite">Whether to overwrite if destination exists</param>
    /// <returns>A tuple containing the response message and status code</returns>
    Task<(string Response, int Status)> CopyFileAsync(string source, string destination, bool overwrite = false);

    /// <summary>
    /// Move a file from source to destination.
    /// </summary>
    /// <param name="source">The source file path</param>
    /// <param name="destination">The destination file path</param>
    /// <param name="overwrite">Whether to overwrite if destination exists</param>
    /// <returns>A tuple containing the response message and status code</returns>
    Task<(string Response, int Status)> MoveFileAsync(string source, string destination, bool overwrite = false);

    /// <summary>
    /// List the contents of a directory.
    /// </summary>
    /// <param name="path">The directory path to list</param>
    /// <param name="includeFiles">Whether to include files in the listing</param>
    /// <param name="includeDirectories">Whether to include directories in the listing</param>
    /// <param name="recursive">Whether to list contents recursively</param>
    /// <returns>A tuple containing the directory listing and status code</returns>
    Task<(string Listing, int Status)> ListDirectoryAsync(string path, bool includeFiles = true, bool includeDirectories = true, bool recursive = false);

    /// <summary>
    /// Create a new directory.
    /// </summary>
    /// <param name="path">The directory path to create</param>
    /// <param name="createParents">Whether to create parent directories if they don't exist</param>
    /// <returns>A tuple containing the response message and status code</returns>
    Task<(string Response, int Status)> CreateDirectoryAsync(string path, bool createParents = true);

    /// <summary>
    /// Delete a directory.
    /// </summary>
    /// <param name="path">The directory path to delete</param>
    /// <param name="recursive">Whether to delete directory contents recursively</param>
    /// <returns>A tuple containing the response message and status code</returns>
    Task<(string Response, int Status)> DeleteDirectoryAsync(string path, bool recursive = false);

    /// <summary>
    /// Get information about a file or directory.
    /// </summary>
    /// <param name="path">The file or directory path</param>
    /// <returns>A tuple containing the file/directory information and status code</returns>
    Task<(string Info, int Status)> GetFileInfoAsync(string path);

    /// <summary>
    /// Check if a file exists.
    /// </summary>
    /// <param name="path">The file path to check</param>
    /// <returns>True if the file exists, false otherwise</returns>
    bool FileExists(string path);

    /// <summary>
    /// Check if a directory exists.
    /// </summary>
    /// <param name="path">The directory path to check</param>
    /// <returns>True if the directory exists, false otherwise</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// Get the size of a file in bytes.
    /// </summary>
    /// <param name="path">The file path</param>
    /// <returns>A tuple containing the file size and status code</returns>
    Task<(long Size, int Status)> GetFileSizeAsync(string path);

    /// <summary>
    /// Search for files by name pattern.
    /// </summary>
    /// <param name="directory">The directory to search in</param>
    /// <param name="pattern">The search pattern (supports wildcards)</param>
    /// <param name="recursive">Whether to search recursively</param>
    /// <returns>A tuple containing the search results and status code</returns>
    Task<(string Results, int Status)> SearchFilesByNameAsync(string directory, string pattern, bool recursive = false);

    /// <summary>
    /// Search for files by extension.
    /// </summary>
    /// <param name="directory">The directory to search in</param>
    /// <param name="extension">The file extension to search for (with or without dot)</param>
    /// <param name="recursive">Whether to search recursively</param>
    /// <returns>A tuple containing the search results and status code</returns>
    Task<(string Results, int Status)> SearchFilesByExtensionAsync(string directory, string extension, bool recursive = false);
}