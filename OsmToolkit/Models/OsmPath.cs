namespace OsmToolkit
{
    /// <summary>
    /// Represents a path of multiple <see cref="OsmEntity"/> instances between two <see cref="Node"/> instances.
    /// </summary>
    public class OsmPath
    {
        /// <summary>
        /// Data containing all of the <see cref="OsmEntity"/> instances making up <see cref="OsmPath"/>.
        /// </summary>
        public OsmData Data { get; }

        /// <summary>
        /// <see cref="Node"/> instance representing the <c>startpoint</c> of <see cref="OsmPath"/>, is <c>null</c> if path is empty.
        /// </summary>
        public Node? StartNode { get; }

        /// <summary>
        /// <see cref="Node"/> instance representing the <c>endpoint</c> of <see cref="OsmPath"/>, is <c>null</c> if path is empty.
        /// </summary>
        public Node? EndNode { get; }

        /// <summary>
        /// Total distance of the path in meters, is 0 if path is empty.
        /// </summary>
        public double TotalDistance { get; }

        /// <summary>
        /// Optional description containing additional information or messages when necessary.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Initializes a new and empty <see cref="OsmPath"/> object with no description.
        /// </summary>
        /// <param name="data">Data containing <see cref="OsmEntity"/> instances making up the path.</param>
        /// <param name="totalDistance">Total distance of the path in meters.</param>
        public OsmPath(OsmData data, double totalDistance)
        {
            if (data  == null) 
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (totalDistance < 0)
                throw new ArgumentOutOfRangeException(nameof(totalDistance), "Total distance cannot be lower than zero, must be greater than or equal to zero.");

            Data = data;
            TotalDistance = totalDistance;
        }

        /// <summary>
        /// Initializes a new and empty <see cref="OsmPath"/> object with a description.
        /// </summary>
        /// <param name="data">Data containing <see cref="OsmEntity"/> instances making up <see cref="OsmPath"/>.</param>
        /// <param name="totalDistance">Total distance of <see cref="OsmPath"/> in meters.</param>
        /// <param name="description">Optional description for additional information or messages.</param>
        public OsmPath(OsmData data, double totalDistance, string? description = null) : this(data, totalDistance)
        {
            Description = description;
        }

        /// <summary>
        /// Initializes a new <see cref="OsmPath"/> object with a startnode and endnode, but no description.
        /// </summary>
        /// <param name="data">Data containing <see cref="OsmEntity"/> instances making up <see cref="OsmPath"/>.</param>
        /// <param name="totalDistance">Total distance of <see cref="OsmPath"/> in meters.</param>
        /// <param name="startNode">Startpoint of <see cref="OsmPath"/>, must already be in <paramref name="data"/> and a <see cref="Way"/>, otherwise it is set to <c>null</c>.</param>
        /// <param name="endNode">Endpoint of <see cref="OsmPath"/>, must already be in <paramref name="data"/> and a <see cref="Way"/>, otherwise it is set to <c>null</c></param>
        public OsmPath(OsmData data, double totalDistance, Node? startNode = null, Node? endNode = null) : this(data, totalDistance)
        {
            if (data.Ways.Count > 0)
            {
                if (startNode != null && data.Nodes.Contains(startNode))
                    StartNode = startNode;

                if (endNode != null && data.Nodes.Contains(endNode))
                    EndNode = endNode;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="OsmPath"/> object with a description, startnode and endnode.
        /// </summary>
        /// <param name="data">Data containing <see cref="OsmEntity"/> instances making up <see cref="OsmPath"/>.</param>
        /// <param name="totalDistance">Total distance of <see cref="OsmPath"/> in meters.</param>
        /// <param name="startNode">Startpoint of <see cref="OsmPath"/>, must already be in <paramref name="data"/> and a <see cref="Way"/>, otherwise it is set to <c>null</c>.</param>
        /// <param name="endNode">Endpoint of <see cref="OsmPath"/>, must already be in <paramref name="data"/> and a <see cref="Way"/>, otherwise it is set to <c>null</c>.</param>
        /// <param name="description">Optional description for additional information or messages.</param>
        public OsmPath(OsmData data, double totalDistance, Node? startNode = null, Node? endNode = null, string? description = null) : this(data, totalDistance, startNode, endNode)
        {
            Description = description;
        }
    }
}
