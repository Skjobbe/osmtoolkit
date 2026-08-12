using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmToolkit.Serialization.Json.Dto
{
    internal class WayDto
    {
        public long Id { get; set; }
        public bool Visible { get; set; }
        public int Version { get; set; }
        public long ChangeSet { get; set; }
        public DateTime Timestamp { get; set; }
        public UserDto? User { get; set; }
        public List<long> NodeRefs { get; set; } = new();
        public Dictionary<string, string> Tags { get; set; } = new();

        public Way ToDomain()
        {
            if (User is null)
            {
                throw new ArgumentNullException(nameof(User), "User cannot be null, must be defined.");
            }

            return new Way(Id, Visible, Version, ChangeSet, Timestamp, User.ToDomain(), NodeRefs, Tags);
        }
    }
}
