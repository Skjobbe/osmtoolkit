using OsmToolkit.Serialization.Xml;

namespace OsmToolkit.Tests.Serialization.Xml
{
    [TestClass]
    public class OsmXmlSerializerTests
    {
        private OsmData? _data;
        private string _tempFilePath = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            _tempFilePath = Path.GetTempFileName();

            var time = new DateTime(2024, 03, 25, 12, 00, 00, DateTimeKind.Utc);
            var user1 = new User(1, "test-user");
            var member1 = new Member(ReferenceType.node, 678, "");
            var member2 = new Member(ReferenceType.node, 789, "");

            var header = new OsmHeader(0.6, "test", "OSM", "OSM Attribution", "ODbL");
            var bounds = new OsmCoordinateBounds(10.0, 20.0, 11.0, 21.0);
            var node1 = new Node(12345, true, 4, 567, time, user1, 10.1234567, 20.7654321);
            var node2 = new Node(2345, true, 2, 567, time, user1, 10.1234567, 20.7654321);
            node1.AddTag("node", "value");

            var nodes = new List<Node>() { node1, node2 };
            var nodeReferenceIds = new List<long>() { node1.Id, node2.Id };
            var members = new List<Member>() { member1, member2 };

            var way1 = new Way(1, true, 1, 1, time, user1, nodeReferenceIds);
            way1.AddTag("way-tag", "value");
            var ways = new List<Way>() { way1 };

            var relation1 = new Relation(1, true, 1, 1, time, user1, members);
            relation1.AddTag("relation-key", "value");
            var relations = new List<Relation>() { relation1 };

            var data = new OsmData(header, bounds, nodes, ways, relations);
            _data = data;
        }
        // SerializeAsync
        [TestMethod]
        public async Task SerializeAsync_WithOsmDataContent_ShouldReturnXmlString()
        {
            // Arrange
            var serialize = new OsmXmlSerializer();

            // Act
            var content = await serialize.SerializeAsync(_data!);

            // Arrange
            Assert.IsTrue(content.Contains("<osm"));
            Assert.IsTrue(content.Contains("""<osm version="0.6" generator="test" copyright="OSM" attribution="OSM Attribution" license="ODbL">"""));
        }

        // SerializeToFileAsync
        [TestMethod]
        public async Task SerializeToFileAsync_WithOsmDataContentToFile_ShouldCreateFile()
        {
            // Arrange
            var serialize = new OsmXmlSerializer();
            var file = _tempFilePath;

            // Act
            await serialize.SerializeToFileAsync(_data!, file);
            var content = await File.ReadAllTextAsync(_tempFilePath);

            // Assert
            Assert.IsTrue(File.Exists(file));
            Assert.IsTrue(content.Contains("<osm"));
            Assert.IsTrue(content.Contains("<node"));
            Assert.IsTrue(content.Contains("<tag"));
            Assert.IsTrue(content.Contains("<way"));
            Assert.IsTrue(content.Contains("<relation"));
            Assert.IsTrue(content.Contains("""<osm version="0.6" generator="test" copyright="OSM" attribution="OSM Attribution" license="ODbL">"""));
        }

        // SerializeToStreamAsync
        [TestMethod]
        public async Task SerializeToStreamAsync_WithOsmDataContentToStream_ShouldCreateStream()
        {
            // Arrange
            var serialize = new OsmXmlSerializer();
            using var memoryStream = new MemoryStream();

            // Act
            await serialize.SerializeToStreamAsync(_data!, memoryStream);

            // Assert
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream);
            var xml = await reader.ReadToEndAsync();

            Assert.IsTrue(xml.Contains("<node"));
            Assert.IsTrue(xml.Contains("""</node>"""));
            Assert.IsTrue(xml.Contains("""<osm version="0.6" generator="test" copyright="OSM" attribution="OSM Attribution" license="ODbL">"""));
            Assert.IsTrue(xml.Contains("""
  <node id="12345" visible="true" version="4" changeset="567" timestamp="2024-03-25T12:00:00Z" user="test-user" uid="1" lat="10.1234567" lon="20.7654321">
    <tag k="node" v="value" />
  </node>
"""
            ));
        }
    }
}