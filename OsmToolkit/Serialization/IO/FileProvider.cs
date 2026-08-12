namespace OsmToolkit.Serialization.IO
{
    /// <summary>
    /// Default implementation of <see cref="IFileProvider"/> that opens local files for asynchronous reading.
    /// </summary>
    internal class FileProvider : IFileProvider
    {
        /// <inheritdoc />
        public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);

            return Task.FromResult<Stream>(stream);
        }
    }
}