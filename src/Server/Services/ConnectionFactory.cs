using MySql.Data.MySqlClient;

public class ConnectionFactory
{
    private readonly string _masterConnectionString;

    public ConnectionFactory(string masterConnectionString)
    {
        _masterConnectionString = masterConnectionString;
    }

    public MySqlConnection GetConnectionForUser(string databaseName)
    {
        var connectionString = $"{_masterConnectionString};Database={databaseName}";
        return new MySqlConnection(connectionString);
    }
}
