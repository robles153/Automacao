using Automacao.Models;
using Microsoft.Data.SqlClient;

namespace Automacao.Repositories;

public class SqlExecucaoRepository : IExecucaoRepository
{
    private readonly string _connectionString;

    public SqlExecucaoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Salvar(ExecucaoAutomacao execucao)
    {
        using var connection = new SqlConnection(_connectionString);

        connection.Open();

        const string query = @"
            INSERT INTO ExecucaoAutomacao
            (
                AcaoExecutada,
                ValorCapturado,
                Sucesso,
                DataExecucao
            )
            VALUES
            (
                @AcaoExecutada,
                @ValorCapturado,
                @Sucesso,
                @DataExecucao
            )";

        using var command = new SqlCommand( query, connection);

        command.Parameters.AddWithValue("@AcaoExecutada", execucao.AcaoExecutada);

        command.Parameters.AddWithValue("@ValorCapturado", execucao.ValorCapturado);

        command.Parameters.AddWithValue("@Sucesso", execucao.Sucesso);

        command.Parameters.AddWithValue("@DataExecucao", execucao.DataExecucao);

        command.ExecuteNonQuery();
    }
}

