using MySql.Data.MySqlClient;

namespace Estacionamiento_grupo_9.Models;

public class RepositorioPlaza
    : RepositorioEstacionamientoBase, IRepositorioPlaza
{
    public RepositorioPlaza(IConfiguration configuration)
        : base(configuration)
    {
    }

    private static Plaza Leer(MySqlDataReader reader)
    {
        return new Plaza
        {
            ID_Plaza = reader.GetInt32(
                reader.GetOrdinal("ID_Plaza")),

            ID_TipoDePlaza = reader.GetInt32(
                reader.GetOrdinal("ID_TipoDePlaza")),

            Piso = reader.GetInt32(
                reader.GetOrdinal("Piso")),

            Sector = reader.GetString(
                reader.GetOrdinal("Sector")),

            Tipo = reader.GetString(
                reader.GetOrdinal("Tipo")),

            PrecioXHora = reader.GetDecimal(
                reader.GetOrdinal("PrecioXHora"))
        };
    }

    public IList<Plaza> ObtenerLista(
        int pagina,
        int cantidad,
        string busqueda)
    {
        var lista = new List<Plaza>();

        int desplazamiento = (pagina - 1) * cantidad;

        string sql = @"
            SELECT
                p.ID_Plaza,
                p.ID_TipoDePlaza,
                p.Piso,
                p.Sector,
                t.Tipo,
                t.PrecioXHora
            FROM plaza p
            INNER JOIN tipodeplaza t
                ON p.ID_TipoDePlaza = t.ID_TipoDePlaza
            WHERE
                p.Sector LIKE @busqueda
                OR t.Tipo LIKE @busqueda
                OR CAST(p.ID_Plaza AS CHAR) LIKE @busqueda
            ORDER BY p.Piso, p.Sector, p.ID_Plaza
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
            FROM plaza p
            INNER JOIN tipodeplaza t
                ON p.ID_TipoDePlaza = t.ID_TipoDePlaza
            WHERE
                p.Sector LIKE @busqueda
                OR t.Tipo LIKE @busqueda
                OR CAST(p.ID_Plaza AS CHAR) LIKE @busqueda;";

        using var conexion = Conexion();

        using var comando = Comando(
            conexion,
            sql,
            ("@busqueda", "%" + busqueda + "%"));

        conexion.Open();

        return Convert.ToInt32(comando.ExecuteScalar());
    }

    public Plaza? ObtenerPorId(int id)
    {
        string sql = @"
            SELECT
                p.ID_Plaza,
                p.ID_TipoDePlaza,
                p.Piso,
                p.Sector,
                t.Tipo,
                t.PrecioXHora
            FROM plaza p
            INNER JOIN tipodeplaza t
                ON p.ID_TipoDePlaza = t.ID_TipoDePlaza
            WHERE p.ID_Plaza = @id;";

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

    public int Alta(Plaza modelo)
    {
        string sql = @"
            INSERT INTO plaza
                (ID_Plaza, ID_TipoDePlaza, Piso, Sector)
            VALUES
                (@id, @idTipo, @piso, @sector);";

        return Ejecutar(
            sql,
            ("@id", modelo.ID_Plaza),
            ("@idTipo", modelo.ID_TipoDePlaza),
            ("@piso", modelo.Piso),
            ("@sector", modelo.Sector.Trim()));
    }

    public int Modificacion(Plaza modelo)
    {
        string sql = @"
            UPDATE plaza
            SET
                ID_TipoDePlaza = @idTipo,
                Piso = @piso,
                Sector = @sector
            WHERE ID_Plaza = @id;";

        return Ejecutar(
            sql,
            ("@id", modelo.ID_Plaza),
            ("@idTipo", modelo.ID_TipoDePlaza),
            ("@piso", modelo.Piso),
            ("@sector", modelo.Sector.Trim()));
    }

    public int Baja(int id)
    {
        string sql = @"
            DELETE FROM plaza
            WHERE ID_Plaza = @id;";

        return Ejecutar(
            sql,
            ("@id", id));
    }
}