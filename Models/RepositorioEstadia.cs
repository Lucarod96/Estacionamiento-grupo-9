using MySql.Data.MySqlClient;

namespace Estacionamiento_grupo_9.Models;

public class RepositorioEstadia
    : RepositorioEstacionamientoBase, IRepositorioEstadia
{
    public RepositorioEstadia(IConfiguration configuration)
        : base(configuration)
    {
    }

    private const string SelectBase = @"
        SELECT
            e.ID_Estadia, e.ID_Plaza, e.Patente, e.ID_Staff,
            e.Ingreso, e.Egreso, e.Ocupado,
            p.Piso, p.Sector, t.Tipo, t.PrecioXHora
        FROM estadia e
        INNER JOIN plaza p ON e.ID_Plaza = p.ID_Plaza
        INNER JOIN tipodeplaza t ON p.ID_TipoDePlaza = t.ID_TipoDePlaza";

    private const string FiltroBusqueda = @"
        (@soloActivas = 0 OR e.Ocupado = 1)
        AND (e.Patente LIKE @busqueda
             OR p.Sector LIKE @busqueda
             OR CAST(e.ID_Plaza AS CHAR) LIKE @busqueda)";

    private static Estadia Leer(MySqlDataReader reader)
    {
        int ordEgreso = reader.GetOrdinal("Egreso");

        return new Estadia
        {
            ID_Estadia = reader.GetInt32(reader.GetOrdinal("ID_Estadia")),
            ID_Plaza = reader.GetInt32(reader.GetOrdinal("ID_Plaza")),
            Patente = reader.GetString(reader.GetOrdinal("Patente")),
            ID_Staff = reader.GetInt32(reader.GetOrdinal("ID_Staff")),
            Ingreso = reader.GetDateTime(reader.GetOrdinal("Ingreso")),
            Egreso = reader.IsDBNull(ordEgreso)
                ? null
                : reader.GetDateTime(ordEgreso),
            Ocupado = reader.GetBoolean(reader.GetOrdinal("Ocupado")),
            Piso = reader.GetInt32(reader.GetOrdinal("Piso")),
            Sector = reader.GetString(reader.GetOrdinal("Sector")),
            Tipo = reader.GetString(reader.GetOrdinal("Tipo")),
            PrecioXHora = reader.GetDecimal(reader.GetOrdinal("PrecioXHora"))
        };
    }

    public IList<Estadia> ObtenerLista(
        int pagina, int cantidad, string busqueda, bool soloActivas)
    {
        var lista = new List<Estadia>();

        string sql = SelectBase + " WHERE " + FiltroBusqueda + @"
            ORDER BY e.Ingreso DESC
            LIMIT @cantidad OFFSET @desplazamiento;";

        using var conexion = Conexion();
        using var comando = Comando(
            conexion, sql,
            ("@busqueda", "%" + busqueda + "%"),
            ("@soloActivas", soloActivas ? 1 : 0),
            ("@cantidad", cantidad),
            ("@desplazamiento", (pagina - 1) * cantidad));

        conexion.Open();
        using var reader = comando.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(Leer(reader));
        }

        return lista;
    }

    public int ObtenerCantidad(string busqueda, bool soloActivas)
    {
        string sql = @"
            SELECT COUNT(*)
            FROM estadia e
            INNER JOIN plaza p ON e.ID_Plaza = p.ID_Plaza
            WHERE " + FiltroBusqueda + ";";

        using var conexion = Conexion();
        using var comando = Comando(
            conexion, sql,
            ("@busqueda", "%" + busqueda + "%"),
            ("@soloActivas", soloActivas ? 1 : 0));

        conexion.Open();
        return Convert.ToInt32(comando.ExecuteScalar());
    }

    public Estadia? ObtenerPorId(int id)
    {
        string sql = SelectBase + " WHERE e.ID_Estadia = @id;";

        using var conexion = Conexion();
        using var comando = Comando(conexion, sql, ("@id", id));

        conexion.Open();
        using var reader = comando.ExecuteReader();

        return reader.Read() ? Leer(reader) : null;
    }

    public int RegistrarIngreso(Estadia modelo)
    {
        string sql = @"
            INSERT INTO estadia
                (ID_Plaza, Patente, ID_Staff, Ingreso, Egreso, Ocupado)
            VALUES
                (@plaza, @patente, @staff, @ingreso, NULL, 1);";

        using var conexion = Conexion();
        using var comando = Comando(
            conexion, sql,
            ("@plaza", modelo.ID_Plaza),
            ("@patente", modelo.Patente.Trim().ToUpper()),
            ("@staff", modelo.ID_Staff),
            ("@ingreso", modelo.Ingreso));

        conexion.Open();
        comando.ExecuteNonQuery();

        return (int)comando.LastInsertedId;
    }

    public int RegistrarSalida(int id, DateTime egreso)
    {
        // "AND Ocupado = 1" evita cerrar dos veces la misma estadía
        return Ejecutar(@"
            UPDATE estadia
            SET Egreso = @egreso, Ocupado = 0
            WHERE ID_Estadia = @id AND Ocupado = 1;",
            ("@id", id),
            ("@egreso", egreso));
    }

    public bool PlazaOcupada(int idPlaza)
    {
        return Existe(
            "SELECT COUNT(*) FROM estadia WHERE ID_Plaza = @valor AND Ocupado = 1;",
            idPlaza);
    }

    public bool PatenteAdentro(string patente)
    {
        return Existe(
            "SELECT COUNT(*) FROM estadia WHERE Patente = @valor AND Ocupado = 1;",
            patente.Trim().ToUpper());
    }

    private bool Existe(string sql, object valor)
    {
        using var conexion = Conexion();
        using var comando = Comando(conexion, sql, ("@valor", valor));

        conexion.Open();
        return Convert.ToInt32(comando.ExecuteScalar()) > 0;
    }

    public IList<Plaza> BuscarPlazasLibres(string busqueda, int maximo)
    {
        var lista = new List<Plaza>();

        string sql = @"
        SELECT p.ID_Plaza, p.ID_TipoDePlaza, p.Piso, p.Sector,
               t.Tipo, t.PrecioXHora
        FROM plaza p
        INNER JOIN tipodeplaza t ON p.ID_TipoDePlaza = t.ID_TipoDePlaza
        WHERE NOT EXISTS (
                SELECT 1 FROM estadia e
                WHERE e.ID_Plaza = p.ID_Plaza AND e.Ocupado = 1)
          AND (CAST(p.ID_Plaza AS CHAR) LIKE @busqueda
               OR p.Sector LIKE @busqueda
               OR t.Tipo LIKE @busqueda)
        ORDER BY p.Piso, p.Sector, p.ID_Plaza
        LIMIT @maximo;";

        using var conexion = Conexion();
        using var comando = Comando(
            conexion, sql,
            ("@busqueda", "%" + busqueda + "%"),
            ("@maximo", maximo));

        conexion.Open();
        using var reader = comando.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Plaza
            {
                ID_Plaza = reader.GetInt32(reader.GetOrdinal("ID_Plaza")),
                ID_TipoDePlaza = reader.GetInt32(reader.GetOrdinal("ID_TipoDePlaza")),
                Piso = reader.GetInt32(reader.GetOrdinal("Piso")),
                Sector = reader.GetString(reader.GetOrdinal("Sector")),
                Tipo = reader.GetString(reader.GetOrdinal("Tipo")),
                PrecioXHora = reader.GetDecimal(reader.GetOrdinal("PrecioXHora"))
            });
        }

        return lista;
    }
}