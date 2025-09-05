namespace Server.IntegrationTests;

[CollectionDefinition("integration")]
public class IntegrationCollection : ICollectionFixture<MySqlFixture>, ICollectionFixture<RedisFixture> { }
