using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OsmToolkit.Serialization.Json.Dto
{
    internal class RelationDto
    {
        public long Id { get; set; }
        public bool Visible { get; set; }
        public int Version { get; set; }
        public long ChangeSet { get; set; }
        public DateTime Timestamp { get; set; }
        public UserDto? User { get; set; }
        public List<MemberDto>? Members { get; set; }
        public Dictionary<string, string>? Tags { get; set; }

        public Relation ToDomain()
        {
            if (Members is null)
            {
                throw new ArgumentNullException(nameof(Members), $"Members cannot be null.");
            }
            if (Members.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Members), $"Members must have at least one element.");
            }
            if (User is null)
            {
                throw new ArgumentNullException(nameof(User), "User cannot be null, must be defined.");
            }

            return new Relation(Id, Visible, Version, ChangeSet, Timestamp, User.ToDomain(), Members.Select(m => m.ToDomain()).ToList(), Tags);
        }
    }
}
