using BuildingBlocks.IntegrationTest.Fixtures;
using Xunit;

namespace Screening.IntegrationTests;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>;