using OsmToolkit.Serialization.IO;
using OsmToolkit.Serialization.Xml;
using System.Text;

namespace OsmToolkit.Tests.Serialization.Xml
{

    public class FakeFileProvider : IFileProvider
    {
        private readonly Stream _stream;

        public FakeFileProvider(Stream stream)
        {
            _stream = stream;
        }

        public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
        {
            // Reset stream position so it can be read from the beginning.
            _stream.Position = 0;
            return Task.FromResult(_stream);
        }
    }
    public class FakeFileProviderThatThrows : IFileProvider
    {
        public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
        {
            throw new FileNotFoundException($"File not found: {path}");
        }
    }

    [TestClass]
    public class OsmXmlDeserializerTests
    {
        private string _tempFilePath = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            _tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".osm");

            var osmContent = """
                <osm version="0.6" generator="test" copyright="OSM" attribution="OSM Attribution" license="ODbL">
                    <bounds minlat="10.0" minlon="20.0" maxlat="11.0" maxlon="21.0" />
                    <node id="12345" visible="true" version="4" changeset="567" timestamp="2024-03-25T12:00:00Z" user="tester" uid="54321" lat="10.1234567" lon="20.7654321" />
                </osm>
                """;

            File.WriteAllText(_tempFilePath, osmContent, Encoding.UTF8);

        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }

        // Tests to DeserializeAsync
        [TestMethod]
        public async Task DeseralizeAsync_WhenValidOsmXmlProvided_ShouldCreateOsmHeaderAndBoundCorrectly() 
        {
            // Arrange
            const string xml = """
            <osm version="0.6" generator="TestGen" copyright="OSM" attribution="OSM Attribution" license="ODbL">
              <bounds minlat="10.0" minlon="20.0" maxlat="11.0" maxlon="21.0" />
            </osm>
            """;

            OsmXmlDeserializer deserializer = new();

            // Act
            var result = await deserializer.DeserializeAsync(xml);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Header);

            Assert.AreEqual(0.6, result.Header.Version);
            Assert.AreEqual("TestGen", result.Header.Generator);
            Assert.AreEqual("OSM", result.Header.Copyright);
            Assert.AreEqual("OSM Attribution", result.Header.AttributionUrl);
            Assert.AreEqual("ODbL", result.Header.LicenseUrl);

            Assert.AreEqual(10.0, result.Bounds!.MinimumLatitude);
            Assert.AreEqual(20.0, result.Bounds.MinimumLongitude);
            Assert.AreEqual(11.0, result.Bounds.MaximumLatitude);
            Assert.AreEqual(21.0, result.Bounds.MaximumLongitude);

        }

        [TestMethod]
        public async Task DeseralizeAsync_WhenOsmHeaderHasInvalidVersion_ShouldThrowInvalidDataError()
        {
            // Arrange
            const string xml = """
            <osm generator="TestGen" copyright="OSM" attribution="OSM Attribution" license="ODbL">
              <bounds minlat="10.0" minlon="20.0" maxlat="11.0" maxlon="21.0" />
            </osm>
            """;

            OsmXmlDeserializer deserializer = new();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidDataException>(() =>
                deserializer.DeserializeAsync(xml));

        }

        [TestMethod]
        public async Task DeserializeAsync_WhenValidOsmXmlWithWayProvided_ShouldParseWayCorrectly() 
        {
            // Arrange
            var xml = """
            <osm version="0.6" generator="TestGen" copyright="OSM" attribution="OSM Attribution" license="ODbL">
              <bounds minlat="10.0" minlon="20.0" maxlat="11.0" maxlon="21.0" />
              <way id="1234" visible="true" version="2" changeset="567" timestamp="2024-03-25T12:00:00Z" user="tester" uid="54321">
                <nd ref="1001"/>
                <nd ref="1002"/>
                <tag k="highway" v="residential"/>
                <tag k="name" v="Test Street"/>
            </way>
            </osm>
            """;

            OsmXmlDeserializer deserializer = new();

            // Act 
            var result = await deserializer.DeserializeAsync(xml);

            var way = result.Ways[0];

            // Assert
            Assert.AreEqual(1234, way.Id);
            Assert.AreEqual(1001, way.NodeReferenceIds.First());
            Assert.AreEqual(1002, way.NodeReferenceIds[1]);
            Assert.AreEqual("highway", way.Tags.First().Key);
            Assert.AreEqual("residential", way.Tags.First().Value);
        }
        [TestMethod]
        public async Task DeserializeAsync_WhenValidOsmXmlWithRelationProvided_ShouldParseRelationCorrectly() 
        {
            // Arrange
            var xml = """
                <osm version="0.6" generator="TestGen" copyright="OSM" attribution="OSM Attribution" license="ODbL">
                  <bounds minlat="10.0" minlon="20.0" maxlat="11.0" maxlon="21.0" />
                  <relation id="12345" visible="true" version="1" changeset="567" timestamp="2024-03-25T12:00:00Z" user="tester" uid="54321">
                    <member type="relation" ref="678" role="house"/>
                    <member type="node" ref="876" role="edge"/>
                    <tag k="some_key" v="a_value"/>
                    <tag k="some_other_key" v="other_value"/>
                  </relation>
                </osm>
                """;
            OsmXmlDeserializer deserializer = new OsmXmlDeserializer();

            // Act
            var result = await deserializer.DeserializeAsync(xml);

            var relation = result.Relations.First();
            var members = relation.Members;
            var tags = relation.Tags;
            
            // Assert
            Assert.AreEqual(12345, relation.Id);
            Assert.AreEqual(54321, relation.User!.Id);

            Assert.AreEqual(ReferenceType.relation, members.First().Type);
            Assert.AreEqual(678, members.First().ReferenceId);
            Assert.AreEqual("house", members.First().Role);
            Assert.AreEqual(ReferenceType.node, members[1].Type);
            Assert.AreEqual(876, members[1].ReferenceId);
            Assert.AreEqual("edge", members[1].Role);

            Assert.AreEqual("some_key", tags.First().Key);
            Assert.AreEqual("a_value", tags.First().Value);

            Assert.AreEqual("some_other_key", tags.Last().Key);
            Assert.AreEqual("other_value", tags.Last().Value);
        }
        [TestMethod]
        public async Task DeserializerAsync_WhenValidOsmXmlProvided_ShouldParseNodeCorrectly()
        {
            // Arrange
            var xml = """
                <osm version="0.6" generator="TestGen" copyright="OSM" attribution="OSM Attribution" license="ODbL">
                  <bounds minlat="10.0" minlon="20.0" maxlat="11.0" maxlon="21.0" />
                  <node id="12345" visible="true" version="4" changeset="567" timestamp="2024-03-25T12:00:00Z" user="tester" uid="54321" lat="10.1234567" lon="20.7654321">
                    <tag k="some_key" v="a_value"/>
                  </node>
                </osm>
                """;
            OsmXmlDeserializer deserializer = new OsmXmlDeserializer();

            // Act
            var result = await deserializer.DeserializeAsync(xml);

            var node = result.Nodes.First();
            var tags = node.Tags;
            var time = new DateTime(2024, 03, 25, 12, 00, 00);

            // Assert
            Assert.AreEqual(12345, node.Id);
            Assert.AreEqual(54321, node.User!.Id);
            Assert.AreEqual(10.1234567, node.Latitude);
            Assert.AreEqual(20.7654321, node.Longitude);
            Assert.AreEqual(time, node.Timestamp);

            Assert.AreEqual("some_key", tags.First().Key);
            Assert.AreEqual("a_value", tags.First().Value);
        }

        [TestMethod]
        public async Task DeserializeAsync_WhenTagValueIsEmpty_ShouldParseNode()
        {
            // Arrange
            var xml = """
                <osm version="0.6" generator="TestGen" copyright="OSM" attribution="OSM Attribution" license="ODbL">
                  <bounds minlat="10.0" minlon="20.0" maxlat="11.0" maxlon="21.0" />
                  <node id="12345" visible="true" version="4" changeset="567" timestamp="2024-03-25T12:00:00Z" user="tester" uid="54321" lat="10.1234567" lon="20.7654321">
                    <tag k="some_key" v=""/>
                  </node>
                </osm>
                """;

            OsmXmlDeserializer deserializer = new OsmXmlDeserializer();

            // Act
            var result = await deserializer.DeserializeAsync(xml);

            var node = result.Nodes.First();
            var tags = node.Tags;

            // Assert
            Assert.AreEqual("", node.Tags["some_key"]);
        }

        [TestMethod]
        public async Task DeserializeAsync_WhenTagValueIsNull_ShouldParseNode()
        {
            // Arrange
            var xml = """
                <osm version="0.6" generator="TestGen" copyright="OSM" attribution="OSM Attribution" license="ODbL">
                  <bounds minlat="10.0" minlon="20.0" maxlat="11.0" maxlon="21.0" />
                  <node id="12345" visible="true" version="4" changeset="567" timestamp="2024-03-25T12:00:00Z" user="tester" uid="54321" lat="10.1234567" lon="20.7654321">
                    <tag k="some_key"/>
                  </node>
                </osm>
                """;

            OsmXmlDeserializer deserializer = new OsmXmlDeserializer();

            // Act
            var result = await deserializer.DeserializeAsync(xml);

            var node = result.Nodes.First();
            var tags = node.Tags;

            // Assert
            Assert.AreEqual("", node.Tags["some_key"]);
        }

        [TestMethod]
        public async Task DeserializeAsync_WhenTagKeyIsEmpty_ShouldParseNode()
        {
            // Arrange
            var xml = """
                <osm version="0.6" generator="TestGen" copyright="OSM" attribution="OSM Attribution" license="ODbL">
                  <bounds minlat="10.0" minlon="20.0" maxlat="11.0" maxlon="21.0" />
                  <node id="12345" visible="true" version="4" changeset="567" timestamp="2024-03-25T12:00:00Z" user="tester" uid="54321" lat="10.1234567" lon="20.7654321">
                    <tag k="" v="something"/>
                  </node>
                </osm>
                """;

            OsmXmlDeserializer deserializer = new OsmXmlDeserializer();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidDataException>(async () => await deserializer.DeserializeAsync(xml));
        }

        [TestMethod]
        public async Task DeserializeAsync_WhenTagKeyIsNull_ShouldParseNode()
        {
            // Arrange
            var xml = """
                <osm version="0.6" generator="TestGen" copyright="OSM" attribution="OSM Attribution" license="ODbL">
                  <bounds minlat="10.0" minlon="20.0" maxlat="11.0" maxlon="21.0" />
                  <node id="12345" visible="true" version="4" changeset="567" timestamp="2024-03-25T12:00:00Z" user="tester" uid="54321" lat="10.1234567" lon="20.7654321">
                    <tag v="something"/>
                  </node>
                </osm>
                """;

            OsmXmlDeserializer deserializer = new OsmXmlDeserializer();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidDataException>(async () => await deserializer.DeserializeAsync(xml));
        }

        [TestMethod]
        public async Task DeserializeAsync_WhenOsmHeaderIsMissing_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var xml = """
                <root>
                  <bounds minlat="10.0" minlon="20.0" maxlat="11.0" maxlon="21.0" />
                  <node id="12345" visible="true" version="4" changeset="567" timestamp="2024-03-25T12:00:00Z" user="tester" uid="54321" lat="10.1234567" lon="20.7654321">
                    <tag k="some_key" v="a_value"/>
                  </node>
                </root>
                """;
            
            OsmXmlDeserializer deserializer = new OsmXmlDeserializer();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await deserializer.DeserializeAsync(xml)); 

         }

        [TestMethod]
        public async Task DeserializeAsync_WhenBoundIsMissing_BoundsShouldBeNull()
        {
            // Arrange
            var xml = """
                <osm version="0.6" generator="TestGen" copyright="OSM" attribution="OSM Attribution" license="ODbL">
                  <node id="12345" visible="true" version="4" changeset="567" timestamp="2024-03-25T12:00:00Z" user="tester" uid="54321" lat="10.1234567" lon="20.7654321">
                    <tag k="some_key" v="a_value"/>
                  </node>
                </osm>
                """;

            OsmXmlDeserializer deserializer = new OsmXmlDeserializer();

            // Act
            OsmData data = await deserializer.DeserializeAsync(xml);

            // Assert
            Assert.IsNull(data.Bounds);

        }

        [TestMethod]
        public async Task DeserializeAsync_WhenUserIsMissing_UserShouldBeNull()
        {
            // Arrange
            var xml = """
                <osm version="0.6" generator="TestGen" copyright="OSM" attribution="OSM Attribution" license="ODbL">
                  <node id="12345" visible="true" version="4" changeset="567" timestamp="2024-03-25T12:00:00Z" uid="54321" lat="10.1234567" lon="20.7654321">
                    <tag k="some_key" v="a_value"/>
                  </node>
                </osm>
                """;

            OsmXmlDeserializer deserializer = new OsmXmlDeserializer();

            // Act
            OsmData data = await deserializer.DeserializeAsync(xml);

            // Assert
            Assert.IsNull(data.Nodes.First().User);

        }

        [TestMethod]
        public async Task DeserializeAsync_WhenUserIdIsMissing_UserShouldBeNull()
        {
            // Arrange
            var xml = """
                <osm version="0.6" generator="TestGen" copyright="OSM" attribution="OSM Attribution" license="ODbL">
                  <node id="12345" visible="true" version="4" changeset="567" timestamp="2024-03-25T12:00:00Z" user="tester" lat="10.1234567" lon="20.7654321">
                    <tag k="some_key" v="a_value"/>
                  </node>
                </osm>
                """;

            OsmXmlDeserializer deserializer = new OsmXmlDeserializer();

            // Act
            OsmData data = await deserializer.DeserializeAsync(xml);

            // Assert
            Assert.IsNull(data.Nodes.First().User);
        }

        // Tests to DeserialzeFromFileAsync
        [TestMethod]
        public async Task DeserializFromFileAsync_WhenFileExist_ShouldCreateCorrecly()
        {
            // Arrange
            var file = _tempFilePath;

            OsmXmlDeserializer deserializer = new OsmXmlDeserializer();

            // Act
            var result = await deserializer.DeserializeFromFileAsync(file);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(0.6, result.Header.Version);
            Assert.AreEqual("test", result.Header.Generator);
            Assert.AreEqual("OSM", result.Header.Copyright);
            Assert.AreEqual("OSM Attribution", result.Header.AttributionUrl);
            Assert.AreEqual("ODbL", result.Header.LicenseUrl);
        }

        [TestMethod]
        public async Task DeserializFromFileAsync_WhenFileDoesNotExist_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".osm");

            OsmXmlDeserializer deserializer = new OsmXmlDeserializer();

            // Act && Assert
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () => await deserializer.DeserializeFromFileAsync(nonExistingPath));
        }

        [TestMethod]
        public async Task DeserializFromFileAsync_WhenFileFormatIsInvalid_ShouldArgumentException()
        {
            // Arrange
            var nonExistingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");

            OsmXmlDeserializer deserializer = new OsmXmlDeserializer();

            // Act && Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await deserializer.DeserializeFromFileAsync(nonExistingPath));
        }

        [TestMethod]
        public async Task DeserializeFromFileAsync_WithValidData_ReturnsValidOsmData()
        {
            // Arrange: create valid OSM XML content as a string.
            string validOsmXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <osm version=""0.6"" generator=""FakeGenerator"">
                <bounds minlat=""10.0"" minlon=""20.0"" maxlat=""15.0"" maxlon=""25.0""/>
                    <node id=""1"" visible=""true"" version=""1"" changeset=""1"" timestamp=""2025-01-01T12:15:30Z"" user=""testuser"" uid=""1"" lat=""12.0"" lon=""22.0"">
                        <tag k=""amenity"" v=""cafe""/>
                    </node>
            </osm>";
            // Convert the string to a MemoryStream.
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(validOsmXml));
            var fakeFileProvider = new FakeFileProvider(memoryStream);

            // Inject the fake file provider into the deserializer.
            var deserializer = new OsmXmlDeserializer(fileProvider: fakeFileProvider);

            // Act: call DeserializeFromFileAsync (the file path is irrelevant with the fake).
            OsmData result = await deserializer.DeserializeFromFileAsync("dummyPath.osm");

            // Assert: validate that the returned OsmData is correctly populated.
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Header);
            Assert.IsNotNull(result.Bounds);
            Assert.AreEqual(1, result.Nodes.Count);
            Assert.AreEqual(0, result.Ways.Count);
            Assert.AreEqual(0, result.Relations.Count);
        }

        [TestMethod]
        public async Task DeserializeFromFileAsync_InvalidFilePath_ThrowsFileNotFoundException()
        {
            // Arrange
            var fakeFileProvider = new FakeFileProviderThatThrows();
            var deserializer = new OsmXmlDeserializer(fileProvider: fakeFileProvider);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () => await deserializer.DeserializeFromFileAsync("nonExistentFile.xml"));
        }

        // DeserializeFromStreamAsync
        [TestMethod]
        public async Task DeserializeFromStreamAsync_WithValidXmlStream_ShouldReturnOsmData()
        {
            // Arrange
            var xml = """
                <osm version="0.6" generator="test" copyright="OSM" attribution="OSM Attribution" license="ODbL">
                    <bounds minlat="10.0" minlon="20.0" maxlat="11.0" maxlon="21.0" />
                    <node id="12345" visible="true" version="4" changeset="567" timestamp="2024-03-25T12:00:00Z" user="tester" uid="54321" lat="10.1234567" lon="20.7654321" />
                </osm>
                """;

            var bytes = Encoding.UTF8.GetBytes(xml);
            using var stream = new MemoryStream(bytes);

            var deserializer = new OsmXmlDeserializer();

            // Act
            var result = await deserializer.DeserializeFromStreamAsync(stream);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(0.6, result.Header.Version);
            Assert.AreEqual("test", result.Header.Generator);
            Assert.AreEqual("OSM", result.Header.Copyright);
            Assert.AreEqual("OSM Attribution", result.Header.AttributionUrl);
            Assert.AreEqual("ODbL", result.Header.LicenseUrl);
        }

        [TestMethod]
        public async Task DeserializeFromStreamAsync_WithInvalidXml_ShouldThrowXmlException()
        {
            // Arrange
            var invalidXml = "<osm><node></osm>"; 
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidXml));
            var deserializer = new OsmXmlDeserializer();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidDataException>(() =>
                deserializer.DeserializeFromStreamAsync(stream));
        }
    }
}
