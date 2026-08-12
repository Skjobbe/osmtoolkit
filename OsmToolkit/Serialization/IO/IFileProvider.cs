namespace OsmToolkit.Serialization.IO
{
    /// <summary>
    /// Defines a file abstraction for providing asynchronous read access to streams.
    /// </summary>
    internal interface IFileProvider
    {
        /// <summary>
        /// Asynchronously opens a file stream for reading at the specified path.
        /// </summary>
        /// <param name="path">The full path to the file to open.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with a result of the opened <see cref="Stream"/>.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Thrown when the file does not exist.</exception>
        /// <exception cref="System.UnauthorizedAccessException">Thrown when the caller does not have the required permission.</exception>
        /// <exception cref="System.IO.IOException">Thrown when the file is in use or another I/O error occurs.</exception>
        Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default);
    }
}