using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmToolkit.Serialization.Json.Dto
{
    internal class NodeDto
    {
        public string? Type { get; set; }
        public long Id { get; set; }
        public bool Visible { get; set; } = true;
        public int Version { get; set; }
        public long ChangeSet { get; set; }
        public DateTime Timestamp { get; set; }
        public UserDto? User { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public Dictionary<string, string>? Tags { get; set; }

        public Node ToDomain()
        {
            if (User is null)
            {
                throw new ArgumentNullException(nameof(User), "User cannot be null, must be defined.");
            }

            return new Node(Id, Visible, Version, ChangeSet, Timestamp, User.ToDomain(), Lat, Lon, Tags);
        }

    }
}
