using OsmToolkit;
using OsmToolkit.Serialization.Json;
using System.Text;

namespace OsmToolkitTests._2._0._0.OsmJsonDeserializerTests
{
    [TestClass]
    public class OsmJsonDeserializerTests
    {
        private string _tempFilePathJson = string.Empty;
        private string _tempFilePathOverpass = string.Empty;
        private string _jsonString = string.Empty;
        private string _overpassString = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            _tempFilePathJson = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
            _tempFilePathOverpass = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

            var osmContent = """
            {
              "header": {
                "version": 0.6,
                "generator": "TestGen",
                "copyright": "OSM",
                "attribution": "OSM",
                "license": "ODbL"
              },
              "bounds": {
                "minlat": 10,
                "minlon": 20,
                "maxlat": 20,
                "maxlon": 30
              },
              "nodes": [
                {
                  "id": 1,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:00:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "lat": 11,
                  "lon": 21,
                  "tags": {
                    "name": "Node A",
                    "amenity": "bench"
                  }
                }
              ]
            }
            """;
            var overpassJson = """
            {
              "version": 0.6,
              "generator": "TestGenerator",
              "elements": [
                {
                  "type": "node",
                  "id": 1,
                  "lat": 59.91,
                  "lon": 10.75,
                  "tags": {
                    "name": "Node A",
                    "highway": "bus_stop"
                  }
                },
                {
                  "type": "node",
                  "id": 2,
                  "lat": 59.92,
                  "lon": 10.76,
                  "tags": {
                    "name": "Node B",
                    "amenity": "cafe"
                  }
                },
                {
                  "type": "way",
                  "id": 100,
                  "nodes": [1, 2],
                  "tags": {
                    "highway": "residential",
                    "name": "Way 1"
                  }
                },
                {
                  "type": "way",
                  "id": 101,
                  "nodes": [2, 1],
                  "tags": {
                    "highway": "secondary",
                    "name": "Way 2"
                  }
                },
                {
                  "type": "relation",
                  "id": 1000,
                  "members": [
                    { "type": "way", "ref": 100, "role": "outer" },
                    { "type": "way", "ref": 101, "role": "inner" }
                  ],
                  "tags": {
                    "type": "multipolygon",
                    "name": "Relasjon A"
                  }
                },
                {
                  "type": "relation",
                  "id": 1001,
                  "members": [
                    { "type": "node", "ref": 1, "role": "label" },
                    { "type": "node", "ref": 2, "role": "admin_centre" }
                  ],
                  "tags": {
                    "type": "boundary",
                    "admin_level": "8",
                    "name": "Relasjon B"
                  }
                }
              ]
            }
            """;


            _jsonString = osmContent;
            _overpassString = overpassJson;
            

            File.WriteAllText(_tempFilePathJson, osmContent, Encoding.UTF8);
            File.WriteAllText(_tempFilePathOverpass, overpassJson, Encoding.UTF8);

        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempFilePathJson))
            {
                File.Delete(_tempFilePathJson);
            }
            if (File.Exists(_tempFilePathOverpass))
            {
                File.Delete(_tempFilePathOverpass);
            }
        }

        // DeserializeAsync
        [TestMethod]
        public async Task DeserializeAsync_WhenValidOsmJsonProvided_ShouldCreateOsmDataCorrectly()
        {
            // Arrange
            const string json = """
            {
              "header": {
                "version": 0.6,
                "generator": "TestGen",
                "copyright": "OSM",
                "attribution": "OSM",
                "license": "ODbL"
              },
              "bounds": {
                "minlat": 10,
                "minlon": 20,
                "maxlat": 20,
                "maxlon": 30
              },
              "nodes": [
                {
                  "id": 1,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:00:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "lat": 11,
                  "lon": 21,
                  "tags": {
                    "name": "Node A",
                    "amenity": "bench"
                  }
                },
                {
                  "id": 2,
                  "visible": true,
                  "version": 1,
                  "changeset": 101,
                  "timestamp": "2023-01-01T12:01:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "lat": 12,
                  "lon": 22,
                  "tags": {
                    "name": "Node B",
                    "highway": "bus_stop"
                  }
                }
              ],
              "ways": [
                {
                  "id": 3,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:05:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "nodeRefs": [1, 2],
                  "tags": {
                    "highway": "footway"
                  }
                }
              ],
              "relations": [
                {
                  "id": 4,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:10:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "members": [
                    {
                      "type": "way",
                      "ref": 2,
                      "role": "outer"
                    }
                  ],
                  "tags": {
                    "type": "multipolygon"
                  }
                }
              ]
            }
            """;

            OsmJsonDeserializer deserializer = new();

            // Act
            var result = await deserializer.DeserializeAsync(json);

            // Assert
            var node1 = result.Nodes[0];
            var node2 = result.Nodes[1];
            var relation = result.Relations.First();
            var members = relation.Members;

            Assert.IsNotNull(result);

            Assert.AreEqual(0.6, result.Header.Version);
            Assert.AreEqual(10, result.Bounds!.MinimumLatitude);
            Assert.AreEqual(1, node1.Id);
            Assert.AreEqual("name", node1.Tags.First().Key);
            Assert.AreEqual("Node A", node1.Tags.First().Value);
            Assert.AreEqual(2, node2.Id);
            Assert.AreEqual(3, result.Ways[0].Id);
            Assert.AreEqual(4, result.Relations[0].Id);
            Assert.AreEqual(ReferenceType.way, members.First().Type);
            Assert.AreEqual(2, members.First().ReferenceId);
            Assert.AreEqual("outer", members.First().Role);
        }

        [TestMethod]
        public async Task DeserializeAsync_WithOnlyHeaderAndBounds_ShouldCreateOsmDataCorrectly()
        {
            // Arrange
            const string json = """
            {
              "header": {
                "version": 0.6,
                "generator": "TestGen",
                "copyright": "OSM",
                "attribution": "OSM",
                "license": "ODbL"
              },
              "bounds": {
                "minlat": 10,
                "minlon": 20,
                "maxlat": 20,
                "maxlon": 30
              }
            }
            """;

            OsmJsonDeserializer deserializer = new();

            // Act
            var result = await deserializer.DeserializeAsync(json);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Nodes.Count);
            Assert.AreEqual(0, result.Ways.Count);
            Assert.AreEqual(0, result.Relations.Count);
        }

        [TestMethod]
        public async Task DeserializeAsync_WithNoMembers_ShouldThrowArgumentNullException()
        {
            // Arrange
            const string json = """
            {
              "header": {
                "version": 0.6,
                "generator": "TestGen",
                "copyright": "OSM",
                "attribution": "OSM",
                "license": "ODbL"
              },
              "bounds": {
                "minlat": 10,
                "minlon": 20,
                "maxlat": 20,
                "maxlon": 30
              },
              "nodes": [
                {
                  "id": 1,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:00:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "lat": 11,
                  "lon": 21,
                  "tags": {
                    "name": "Node A",
                    "amenity": "bench"
                  }
                },
                {
                  "id": 2,
                  "visible": true,
                  "version": 1,
                  "changeset": 101,
                  "timestamp": "2023-01-01T12:01:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "lat": 12,
                  "lon": 22,
                  "tags": {
                    "name": "Node B",
                    "highway": "bus_stop"
                  }
                }
              ],
              "ways": [
                {
                  "id": 3,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:05:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "nodeRefs": [1, 2],
                  "tags": {
                    "highway": "footway"
                  }
                }
              ],
              "relations": [
                {
                  "id": 4,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:10:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "tags": {
                    "type": "multipolygon"
                  }
                }
              ]
            }
            """;

            OsmJsonDeserializer deserializer = new();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await deserializer.DeserializeAsync(json));
        }

        [TestMethod]
        public async Task DeserializeAsync_WhenMemberHaveNoElements_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            const string json = """
            {
              "header": {
                "version": 0.6,
                "generator": "TestGen",
                "copyright": "OSM",
                "attribution": "OSM",
                "license": "ODbL"
              },
              "bounds": {
                "minlat": 10,
                "minlon": 20,
                "maxlat": 20,
                "maxlon": 30
              },
              "nodes": [
                {
                  "id": 1,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:00:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "lat": 11,
                  "lon": 21,
                  "tags": {
                    "name": "Node A",
                    "amenity": "bench"
                  }
                },
                {
                  "id": 2,
                  "visible": true,
                  "version": 1,
                  "changeset": 101,
                  "timestamp": "2023-01-01T12:01:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "lat": 12,
                  "lon": 22,
                  "tags": {
                    "name": "Node B",
                    "highway": "bus_stop"
                  }
                }
              ],
              "ways": [
                {
                  "id": 3,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:05:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "nodeRefs": [1, 2],
                  "tags": {
                    "highway": "footway"
                  }
                }
              ],
              "relations": [
                {
                  "id": 4,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:10:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                "members": [
                    
                  ],
                  "tags": {
                    "type": "multipolygon"
                  }
                }
              ]
            }
            """;

            OsmJsonDeserializer deserializer = new();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(async () => await deserializer.DeserializeAsync(json));
        }

        [TestMethod]
        public async Task DeserializeAsync_WhenUserIsNullInNode_ShouldThrowArgumentNullException()
        {
            // Arrange
            const string json = """
            {
              "header": {
                "version": 0.6,
                "generator": "TestGen",
                "copyright": "OSM",
                "attribution": "OSM",
                "license": "ODbL"
              },
              "bounds": {
                "minlat": 10,
                "minlon": 20,
                "maxlat": 20,
                "maxlon": 30
              },
              "nodes": [
                {
                  "id": 1,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:00:00Z",
                  "lat": 11,
                  "lon": 21,
                  "tags": {
                    "name": "Node A",
                    "amenity": "bench"
                  }
                }
              ],
              "ways": [
                {
                  "id": 3,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:05:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "nodeRefs": [1, 2],
                  "tags": {
                    "highway": "footway"
                  }
                }
              ],
              "relations": [
                {
                  "id": 4,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:10:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "members": [
                    {
                      "type": "way",
                      "ref": 2,
                      "role": "outer"
                    }
                  ],
                  "tags": {
                    "type": "multipolygon"
                  }
                }
              ]
            }
            """;

            OsmJsonDeserializer deserializer = new();

            // Act && Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await deserializer.DeserializeAsync(json));
        }

        [TestMethod]
        public async Task DeserializeAsync_WhenUserIsNullInWay_ShouldThrowArgumentNullException()
        {
            // Arrange
            const string json = """
            {
              "header": {
                "version": 0.6,
                "generator": "TestGen",
                "copyright": "OSM",
                "attribution": "OSM",
                "license": "ODbL"
              },
              "bounds": {
                "minlat": 10,
                "minlon": 20,
                "maxlat": 20,
                "maxlon": 30
              },
              "nodes": [
                {
                  "id": 1,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:00:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "lat": 11,
                  "lon": 21,
                  "tags": {
                    "name": "Node A",
                    "amenity": "bench"
                  }
                }
              ],
              "ways": [
                {
                  "id": 3,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:05:00Z",
                  "nodeRefs": [1, 2],
                  "tags": {
                    "highway": "footway"
                  }
                }
              ],
              "relations": [
                {
                  "id": 4,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:10:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "members": [
                    {
                      "type": "way",
                      "ref": 2,
                      "role": "outer"
                    }
                  ],
                  "tags": {
                    "type": "multipolygon"
                  }
                }
              ]
            }
            """;

            OsmJsonDeserializer deserializer = new();

            // Act && Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await deserializer.DeserializeAsync(json));
        }

        [TestMethod]
        public async Task DeserializeAsync_WhenUserIsNullInRelation_ShouldThrowArgumentNullException()
        {
            // Arrange
            const string json = """
            {
              "header": {
                "version": 0.6,
                "generator": "TestGen",
                "copyright": "OSM",
                "attribution": "OSM",
                "license": "ODbL"
              },
              "bounds": {
                "minlat": 10,
                "minlon": 20,
                "maxlat": 20,
                "maxlon": 30
              },
              "nodes": [
                {
                  "id": 1,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:00:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "lat": 11,
                  "lon": 21,
                  "tags": {
                    "name": "Node A",
                    "amenity": "bench"
                  }
                }
              ],
              "ways": [
                {
                  "id": 3,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:05:00Z",
                  "user": {
                    "id": 42,
                    "name": "osm_user"
                  },
                  "nodeRefs": [1, 2],
                  "tags": {
                    "highway": "footway"
                  }
                }
              ],
              "relations": [
                {
                  "id": 4,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:10:00Z",
                  "members": [
                    {
                      "type": "way",
                      "ref": 2,
                      "role": "outer"
                    }
                  ],
                  "tags": {
                    "type": "multipolygon"
                  }
                }
              ]
            }
            """;

            OsmJsonDeserializer deserializer = new();

            // Act && Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await deserializer.DeserializeAsync(json));
        }

        [TestMethod]
        public async Task DeserializeAsync_WhenUserHaveInvalidId_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            const string json = """
            {
              "header": {
                "version": 0.6,
                "generator": "TestGen",
                "copyright": "OSM",
                "attribution": "OSM",
                "license": "ODbL"
              },
              "bounds": {
                "minlat": 10,
                "minlon": 20,
                "maxlat": 20,
                "maxlon": 30
              },
              "nodes": [
                {
                  "id": 1,
                  "visible": true,
                  "version": 1,
                  "changeset": 100,
                  "timestamp": "2023-01-01T12:00:00Z",
                  "user": {
                    "id": 0,
                    "name": "osm_user"
                  },
                  "lat": 11,
                  "lon": 21,
                  "tags": {
                    "name": "Node A",
                    "amenity": "bench"
                  }
                }
              ]
            }
            """;

            OsmJsonDeserializer deserializer = new();

            // Act && Assert
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(async () => await deserializer.DeserializeAsync(json));
        }

        [TestMethod]
        public async Task DeserializeAsync_WhenValidOverpassJsonPrivided_ShouldCreateOsmDataCorrectly()
        {
            // Arrange
            var overpassJson = """
            {
              "version": 0.6,
              "generator": "TestGenerator",
              "elements": [
                {
                  "type": "node",
                  "id": 1,
                  "lat": 59.91,
                  "lon": 10.75,
                  "tags": {
                    "name": "Node A",
                    "highway": "bus_stop"
                  }
                },
                {
                  "type": "node",
                  "id": 2,
                  "lat": 59.92,
                  "lon": 10.76,
                  "tags": {
                    "name": "Node B",
                    "amenity": "cafe"
                  }
                },
                {
                  "type": "way",
                  "id": 100,
                  "nodes": [1, 2],
                  "tags": {
                    "highway": "residential",
                    "name": "Way 1"
                  }
                },
                {
                  "type": "way",
                  "id": 101,
                  "nodes": [2, 1],
                  "tags": {
                    "highway": "secondary",
                    "name": "Way 2"
                  }
                },
                {
                  "type": "relation",
                  "id": 1000,
                  "members": [
                    { "type": "way", "ref": 100, "role": "outer" },
                    { "type": "way", "ref": 101, "role": "inner" }
                  ],
                  "tags": {
                    "type": "multipolygon",
                    "name": "Relasjon A"
                  }
                },
                {
                  "type": "relation",
                  "id": 1001,
                  "members": [
                    { "type": "node", "ref": 1, "role": "label" },
                    { "type": "node", "ref": 2, "role": "admin_centre" }
                  ],
                  "tags": {
                    "type": "boundary",
                    "admin_level": "8",
                    "name": "Relasjon B"
                  }
                }
              ]
            }
            """;

            OsmJsonDeserializer deserializer = new();

            // Act

            var result = await deserializer.DeserializeAsync(overpassJson);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0.6, result.Header.Version);
            Assert.AreEqual(1, result.Nodes[0].Id);
            Assert.AreEqual(2, result.Nodes[1].Id);
            Assert.AreEqual("highway", result.Ways[0].Tags.First().Key);
            Assert.AreEqual("residential", result.Ways[0].Tags.First().Value);
            Assert.AreEqual(1000, result.Relations[0].Id);
            Assert.AreEqual(1001, result.Relations[1].Id);

        }


        // DeserializeFromFileAsync
        [TestMethod]
        public async Task DeserializeFromFileAsync_WhenFileExist_ShouldCreateCorrectly()
        {
            // Arrange
            var file = _tempFilePathJson;

            OsmJsonDeserializer deserializer = new OsmJsonDeserializer();

            // Act
            var result = await deserializer.DeserializeFromFileAsync(file);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(0.6, result.Header.Version);
            Assert.AreEqual("TestGen", result.Header.Generator);
            Assert.AreEqual("OSM", result.Header.Copyright);
            Assert.AreEqual("OSM", result.Header.AttributionUrl);
            Assert.AreEqual("ODbL", result.Header.LicenseUrl);
        }

        [TestMethod]
        public async Task DeserializeFromFileAsync_WhenFileExistWithOverpassJson_ShouldCreateCorrectly()
        {
            // Arrange
            var file = _tempFilePathOverpass;

            OsmJsonDeserializer deserializer = new OsmJsonDeserializer();

            // Act
            var result = await deserializer.DeserializeFromFileAsync(file);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(0.6, result.Header.Version);
            Assert.AreEqual("TestGenerator", result.Header.Generator);
            Assert.AreEqual(1, result.Nodes[0].Id);
            Assert.AreEqual(2, result.Nodes[1].Id);
            Assert.AreEqual("highway", result.Ways[0].Tags.First().Key);
            Assert.AreEqual("residential", result.Ways[0].Tags.First().Value);
            Assert.AreEqual(1000, result.Relations[0].Id);
            Assert.AreEqual(1001, result.Relations[1].Id);
        }

        [TestMethod]
        public async Task DeserializeFromFileAsync_WhenFileDoesNotExist_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

            OsmJsonDeserializer deserializer = new OsmJsonDeserializer();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () => await deserializer.DeserializeFromFileAsync(nonExistingPath));            
        }

        [TestMethod]
        public async Task DeserializFromFileAsync_WhenFileFormatIsInvalid_ShouldThrowArgumentException()
        {
            // Arrange
            var nonExistingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");

            OsmJsonDeserializer deserializer = new OsmJsonDeserializer();

            // Act && Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await deserializer.DeserializeFromFileAsync(nonExistingPath));
        }

        // DeserializeFromStreamAsync
        [TestMethod]
        public async Task DeserializeFromStreamAsync_WithValidJsonStream_ShouldReturnOsmData()
        {
            // Arrange
            var bytes = Encoding.UTF8.GetBytes(_jsonString);
            using var stream = new MemoryStream(bytes);

            var deserializer = new OsmJsonDeserializer();

            // Act
            var result = await deserializer.DeserializeFromStreamAsync(stream);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(0.6, result.Header.Version);
            Assert.AreEqual("TestGen", result.Header.Generator);
            Assert.AreEqual("OSM", result.Header.Copyright);
            Assert.AreEqual("OSM", result.Header.AttributionUrl);
            Assert.AreEqual("ODbL", result.Header.LicenseUrl);
        }

        [TestMethod]
        public async Task DeserializeFromStreamAsync_WithValidOverpassJsonStream_ShouldReturnOsmData()
        {
            // Arrange
            var bytes = Encoding.UTF8.GetBytes(_overpassString);
            using var stream = new MemoryStream(bytes);

            var deserializer = new OsmJsonDeserializer();

            // Act
            var result = await deserializer.DeserializeFromStreamAsync(stream);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(0.6, result.Header.Version);
            Assert.AreEqual("TestGenerator", result.Header.Generator);
            Assert.AreEqual(1, result.Nodes[0].Id);
            Assert.AreEqual(2, result.Nodes[1].Id);
            Assert.AreEqual("highway", result.Ways[0].Tags.First().Key);
            Assert.AreEqual("residential", result.Ways[0].Tags.First().Value);
            Assert.AreEqual(1000, result.Relations[0].Id);
            Assert.AreEqual(1001, result.Relations[1].Id);
        }

    }
}


