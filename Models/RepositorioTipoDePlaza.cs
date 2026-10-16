using MySql.Data.MySqlClient;

namespace Estacionamiento_grupo_9.Models;

public class RepositorioTipoDePlaza
    : RepositorioEstacionamientoBase, IRepositorioTipoDePlaza
{
    public RepositorioTipoDePlaza(IConfiguration configuration)
        : base(configuration)
    {
    }

    private static TipoDePlaza Leer(MySqlDataReader reader)
    {
        return new TipoDePlaza
        {
            ID_TipoDePlaza = reader.GetInt32(
                reader.GetOrdinal("ID_TipoDePlaza")),

            Tipo = reader.GetString(
                reader.GetOrdinal("Tipo")),

            PrecioXHora = reader.GetDecimal(
                reader.GetOrdinal("PrecioXHora"))
        };
    }

    public IList<TipoDePlaza> ObtenerLista(
        int pagina,
        int cantidad,
        string busqueda)
    {
        var lista = new List<TipoDePlaza>();

        int desplazamiento = (pagina - 1) * cantidad;

        string sql = @"
            SELECT ID_TipoDePlaza, Tipo, PrecioXHora
            FROM tipodeplaza
            WHERE Tipo LIKE @busqueda
            ORDER BY Tipo, ID_TipoDePlaza
            LIMIT @cantidad OFFSET @desplazamiento;";

        using var conexion = Conexion();

        using var comando = Comando(
            conexion,
            sql,
            ("@busqueda", "%" + busqueda + "%"),
            ("@cantidad", cantidad),
            ("@desplazamiento", desplazamiento));

        conexion.Open();

        using var reader = comando.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(Leer(reader));
        }

        return lista;
    }

    public int ObtenerCantidad(string busqueda)
    {
        string sql = @"
            SELECT COUNT(*)
            FROM tipodeplaza
            WHERE Tipo LIKE @busqueda;";

        using var conexion = Conexion();

        using var comando = Comando(
            conexion,
            sql,
            ("@busqueda", "%" + busqueda + "%"));

        conexion.Open();

        return Convert.ToInt32(comando.ExecuteScalar());
    }

    public TipoDePlaza? ObtenerPorId(int id)
    {
        string sql = @"
            SELECT ID_TipoDePlaza, Tipo, PrecioXHora
            FROM tipodeplaza
            WHERE ID_TipoDePlaza = @id;";

        using var conexion = Conexion();

        using var comando = Comando(
            conexion,
            sql,
            ("@id", id));

        conexion.Open();

        using var reader = comando.ExecuteReader();

        if (reader.Read())
        {
            return Leer(reader);
        }

        return null;
    }

    public int Alta(TipoDePlaza modelo)
    {
        string sql = @"
            INSERT INTO tipodeplaza
                (ID_TipoDePlaza, Tipo, PrecioXHora)
            VALUES
                (@id, @tipo, @precio);";

        return Ejecutar(
            sql,
            ("@id", modelo.ID_TipoDePlaza),
            ("@tipo", modelo.Tipo.Trim()),
            ("@precio", modelo.PrecioXHora));
    }

    public int Modificacion(TipoDePlaza modelo)
    {
        string sql = @"
            UPDATE tipodeplaza
            SET Tipo = @tipo,
                PrecioXHora = @precio
            WHERE ID_TipoDePlaza = @id;";

        return Ejecutar(
            sql,
            ("@id", modelo.ID_TipoDePlaza),
            ("@tipo", modelo.Tipo.Trim()),
            ("@precio", modelo.PrecioXHora));
    }

    public int Baja(int id)
    {
        string sql = @"
            DELETE FROM tipodeplaza
            WHERE ID_TipoDePlaza = @id;";

        return Ejecutar(
            sql,
            ("@id", id));
    }
}