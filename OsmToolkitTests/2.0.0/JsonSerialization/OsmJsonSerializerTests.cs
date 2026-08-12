using OsmToolkit;
using OsmToolkit.Serialization.Json;

namespace OsmToolkitTests._2._0._0.OsmJsonSerializerTests
{
    [TestClass]
    public class OsmJsonSerializerTests
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

            var header = new OsmHeader(0.6, "test", "OSM", "OSM", "ODbL");
            var bounds = new OsmCoordinateBounds(10.0, 20.0, 11.0, 21.0);
            var node1 = new Node(12345, true, 4, 567, time, user1, 10.1234567, 20.7654321);
            var node2 = new Node(2345, true, 2, 567, time, user1, 10.1234567, 20.7654321);
            node1.AddTag("node", "value");
            node2.AddTag("node", "value2");

            var nodes = new List<Node>() { node1, node2 };
            var nodeReferenceIds = new List<long>() { node1.Id, node2.Id };
            var members = new List<Member>() { member1, member2 };

            var way1 = new Way(1, true, 1, 1, time, user1, nodeReferenceIds);
            var way2 = new Way(2, true, 1, 1, time, user1, nodeReferenceIds);
            way1.AddTag("way-tag", "value");
            way2.AddTag("way-tag", "value2");
            var ways = new List<Way>() { way1, way2 };

            var relation1 = new Relation(1, true, 1, 1, time, user1, members);
            var relation2 = new Relation(2, true, 1, 1, time, user1, members);
            relation1.AddTag("relation-key", "value");
            relation2.AddTag("relation-key", "value2");
            var relations = new List<Relation>() { relation1, relation2 };

            var data = new OsmData(header, bounds, nodes, ways, relations);
            _data = data;
        }

        // SerializeAsync
        [TestMethod]
        public async Task SerializeAsync_WithOsmDataContent_ShouldReturnJsonString()
        {
            // Arrange
            var serialize = new OsmJsonSerializer();

            // Act
            var content = await serialize.SerializeAsync(_data!);

            // Arrange
            Assert.IsTrue(content.Contains("header"));
            Assert.IsTrue(content.Contains("bounds"));
            Assert.IsTrue(content.Contains("nodes"));
            Assert.IsTrue(content.Contains("ways"));
            Assert.IsTrue(content.Contains("relations"));
            Assert.IsTrue(content.Contains("""
                "header": {
                    "version": 0.6,
                    "generator": "test",
                    "copyright": "OSM",
                    "attribution": "OSM",
                    "license": "ODbL"
                  },
                  "bounds": {
                    "minlat": 10,
                    "minlon": 20,
                    "maxlat": 11,
                    "maxlon": 21
                  },
                """));
        }

        [TestMethod]
        public async Task SerializeAsync_WithtwoNodes_ShouldSortCorrectly()
        {
            // Arrange
            var serialize = new OsmJsonSerializer();

            // Act
            var content = await serialize.SerializeAsync(_data!);

            // Arrange
            Assert.IsTrue(content.Contains("""
                "nodes": [
                    {
                      "id": 2345,
                      "visible": true,
                      "version": 2,
                      "changeset": 567,
                      "timestamp": "2024-03-25T12:00:00Z",
                      "user": {
                        "id": 1,
                        "name": "test-user"
                      },
                      "lat": 10.1234567,
                      "lon": 20.7654321,
                      "tags": {
                        "node": "value2"
                      }
                    },
                    {
                      "id": 12345,
                      "visible": true,
                      "version": 4,
                      "changeset": 567,
                      "timestamp": "2024-03-25T12:00:00Z",
                      "user": {
                        "id": 1,
                        "name": "test-user"
                      },
                      "lat": 10.1234567,
                      "lon": 20.7654321,
                      "tags": {
                        "node": "value"
                      }
                    }
                  ]
                """));
            Console.WriteLine(content);
        }

        [TestMethod]
        public async Task SerializeAsync_WithtwoWays_ShouldSortCorrectly()
        {
            // Arrange
            var serialize = new OsmJsonSerializer();

            // Act
            var content = await serialize.SerializeAsync(_data!);

            // Arrange
            Assert.IsTrue(content.Contains("""
                "ways": [
                    {
                      "id": 1,
                      "visible": true,
                      "version": 1,
                      "changeset": 1,
                      "timestamp": "2024-03-25T12:00:00Z",
                      "user": {
                        "id": 1,
                        "name": "test-user"
                      },
                      "nodeRefs": [
                        12345,
                        2345
                      ],
                      "tags": {
                        "way-tag": "value"
                      }
                    },
                    {
                      "id": 2,
                      "visible": true,
                      "version": 1,
                      "changeset": 1,
                      "timestamp": "2024-03-25T12:00:00Z",
                      "user": {
                        "id": 1,
                        "name": "test-user"
                      },
                      "nodeRefs": [
                        12345,
                        2345
                      ],
                      "tags": {
                        "way-tag": "value2"
                      }
                    }
                  ]
                """));
        }

        [TestMethod]
        public async Task SerializeAsync_WithtwoRelations_ShouldSortCorrectly()
        {
            // Arrange
            var serialize = new OsmJsonSerializer();

            // Act
            var content = await serialize.SerializeAsync(_data!);

            // Arrange
            Assert.IsTrue(content.Contains("""
                "relations": [
                    {
                      "id": 1,
                      "visible": true,
                      "version": 1,
                      "changeset": 1,
                      "timestamp": "2024-03-25T12:00:00Z",
                      "user": {
                        "id": 1,
                        "name": "test-user"
                      },
                      "members": [
                        {
                          "type": "node",
                          "ref": 678,
                          "role": ""
                        },
                        {
                          "type": "node",
                          "ref": 789,
                          "role": ""
                        }
                      ],
                      "tags": {
                        "relation-key": "value"
                      }
                    },
                    {
                      "id": 2,
                      "visible": true,
                      "version": 1,
                      "changeset": 1,
                      "timestamp": "2024-03-25T12:00:00Z",
                      "user": {
                        "id": 1,
                        "name": "test-user"
                      },
                      "members": [
                        {
                          "type": "node",
                          "ref": 678,
                          "role": ""
                        },
                        {
                          "type": "node",
                          "ref": 789,
                          "role": ""
                        }
                      ],
                      "tags": {
                        "relation-key": "value2"
                      }
                    }
                  ]
                """));
        }

        // SerializeToFileAsync
        [TestMethod]
        public async Task SerializeToFileAsync_WithOsmDataContentToFile_ShouldCreateFile()
        {
            // Arrange
            var serialize = new OsmJsonSerializer();
            var file = _tempFilePath;

            // Act
            await serialize.SerializeToFileAsync(_data!, file);
            var content = await File.ReadAllTextAsync(_tempFilePath);

            // Assert
            Assert.IsTrue(File.Exists(file));
            Assert.IsTrue(content.Contains("header"));
            Assert.IsTrue(content.Contains("bounds"));
            Assert.IsTrue(content.Contains("nodes"));
            Assert.IsTrue(content.Contains("ways"));
            Assert.IsTrue(content.Contains("relations"));
            Assert.IsTrue(content.Contains("""
                "header": {
                    "version": 0.6,
                    "generator": "test",
                    "copyright": "OSM",
                    "attribution": "OSM",
                    "license": "ODbL"
                  },
                  "bounds": {
                    "minlat": 10,
                    "minlon": 20,
                    "maxlat": 11,
                    "maxlon": 21
                  },
                """));
        }

        // SerializeToStreamAsync
        [TestMethod]
        public async Task SerializeToStreamAsync_WithOsmDataContentToStream_ShouldCreateStream()
        {
            // Arrange
            var serialize = new OsmJsonSerializer();
            using var memoryStream = new MemoryStream();

            // Act
            await serialize.SerializeToStreamAsync(_data!, memoryStream);

            // Assert
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream);
            var content = await reader.ReadToEndAsync();

            Assert.IsTrue(content.Contains("header"));
            Assert.IsTrue(content.Contains("bounds"));
            Assert.IsTrue(content.Contains("nodes"));
            Assert.IsTrue(content.Contains("ways"));
            Assert.IsTrue(content.Contains("relations"));
            Assert.IsTrue(content.Contains("""
                "header": {
                    "version": 0.6,
                    "generator": "test",
                    "copyright": "OSM",
                    "attribution": "OSM",
                    "license": "ODbL"
                  },
                  "bounds": {
                    "minlat": 10,
                    "minlon": 20,
                    "maxlat": 11,
                    "maxlon": 21
                  },
                """));
        }
    }
}


