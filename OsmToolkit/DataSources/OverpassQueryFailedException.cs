namespace OsmToolkit.DataSources
{
    /// <summary>
    /// Thrown when Overpass responds with HTTP 200 but a top-level <c>remark</c> field, which Overpass sets when a
    /// query fails server-side (e.g. hitting the execution timeout or memory ceiling) without failing the HTTP
    /// request itself. Kept distinct from a generic <see cref="InvalidOperationException"/> so this specific,
    /// often-transient failure can be told apart from a malformed/unparseable response body, which is not
    /// retried since the same failure would just recur.
    /// </summary>
    public class OverpassQueryFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OverpassQueryFailedException"/> class.
        /// </summary>
        /// <param name="remark">The remark text reported by Overpass describing the server-side failure.</param>
        public OverpassQueryFailedException(string remark)
            : base(remark)
        {
        }
    }
}
