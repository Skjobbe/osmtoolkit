using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmToolkit.Serialization.Json.Dto
{
    internal class MemberDto
    {
        public ReferenceType Type { get; set; }
        public long Ref { get; set; }
        public string? Role { get; set; }

        public Member ToDomain()
        {
            return new Member(Type, Ref, Role);
        }
    }
}
