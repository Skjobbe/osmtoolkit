using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OsmToolkit.Serialization.Json.Dto
{
    internal class UserDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public User ToDomain()
        {
            return new User(Id, Name);
        }
    }
}
