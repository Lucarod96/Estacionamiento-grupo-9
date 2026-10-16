using MySql.Data.MySqlClient;

namespace Estacionamiento_grupo_9.Models;

public abstract class RepositorioEstacionamientoBase
{
    private readonly string connectionString;

    protected RepositorioEstacionamientoBase(
        IConfiguration configuration)
    {
        connectionString =
            configuration.GetConnectionString("MySql")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión a MySQL.");
    }

    protected MySqlConnection Conexion()
    {
        return new MySqlConnection(connectionString);
    }

    protected static MySqlCommand Comando(
        MySqlConnection conexion,
        string sql,
        params (string Nombre, object Valor)[] parametros)
    {
        var comando = new MySqlCommand(sql, conexion);

        foreach (var parametro in parametros)
        {
            comando.Parameters.AddWithValue(
                parametro.Nombre,
                parametro.Valor);
        }

        return comando;
    }

    protected int Ejecutar(
        string sql,
        params (string Nombre, object Valor)[] parametros)
    {
        using var conexion = Conexion();

        using var comando = Comando(
            conexion,
            sql,
            parametros);

        conexion.Open();

        return comando.ExecuteNonQuery();
    }
}