using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Estacionamiento_grupo_9.Models
{
    public class RepositorioPago : IRepositorioPago
    {
        private readonly string connectionString;

        public RepositorioPago(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("MySql") 
                ?? configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'MySql' en appsettings.json.");
        }

        public int Alta(Pago pago)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Pago (ID_Estadia, FechaEmision, PrecioXHoraAplicado, MontoTotal, ComprobanteUrl) 
                               VALUES (@idEstadia, @fechaEmision, @precioXHora, @montoTotal, @comprobanteUrl);
                               SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idEstadia", pago.ID_Estadia);
                    command.Parameters.AddWithValue("@fechaEmision", pago.FechaEmision);
                    command.Parameters.AddWithValue("@precioXHora", pago.PrecioXHoraAplicado);
                    command.Parameters.AddWithValue("@montoTotal", pago.MontoTotal);
                    command.Parameters.AddWithValue("@comprobanteUrl", (object?)pago.ComprobanteUrl ?? DBNull.Value);

                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return res;
        }

        public Pago? ObtenerPorId(int id)
        {
            Pago? pago = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT ID_Pago, ID_Estadia, FechaEmision, PrecioXHoraAplicado, MontoTotal, ComprobanteUrl 
                               FROM Pago 
                               WHERE ID_Pago = @id;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pago = new Pago
                            {
                                ID_Pago = reader.GetInt32(reader.GetOrdinal("ID_Pago")),
                                ID_Estadia = reader.GetInt32(reader.GetOrdinal("ID_Estadia")),
                                FechaEmision = reader.GetDateTime(reader.GetOrdinal("FechaEmision")),
                                PrecioXHoraAplicado = reader.GetDecimal(reader.GetOrdinal("PrecioXHoraAplicado")),
                                MontoTotal = reader.GetDecimal(reader.GetOrdinal("MontoTotal")),
                                ComprobanteUrl = reader.IsDBNull(reader.GetOrdinal("ComprobanteUrl")) ? null : reader.GetString(reader.GetOrdinal("ComprobanteUrl"))
                            };
                        }
                    }
                }
            }
            return pago;
        }

        public Pago? ObtenerPorEstadiaId(int estadiaId)
        {
            Pago? pago = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT ID_Pago, ID_Estadia, FechaEmision, PrecioXHoraAplicado, MontoTotal, ComprobanteUrl 
                               FROM Pago 
                               WHERE ID_Estadia = @estadiaId;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@estadiaId", estadiaId);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pago = new Pago
                            {
                                ID_Pago = reader.GetInt32(reader.GetOrdinal("ID_Pago")),
                                ID_Estadia = reader.GetInt32(reader.GetOrdinal("ID_Estadia")),
                                FechaEmision = reader.GetDateTime(reader.GetOrdinal("FechaEmision")),
                                PrecioXHoraAplicado = reader.GetDecimal(reader.GetOrdinal("PrecioXHoraAplicado")),
                                MontoTotal = reader.GetDecimal(reader.GetOrdinal("MontoTotal")),
                                ComprobanteUrl = reader.IsDBNull(reader.GetOrdinal("ComprobanteUrl")) ? null : reader.GetString(reader.GetOrdinal("ComprobanteUrl"))
                            };
                        }
                    }
                }
            }
            return pago;
        }

      public IList<Pago> ObtenerLista(
    int paginaNro,
    int tamPagina,
    string busqueda = "")
{
    var lista = new List<Pago>();
    int offset = (paginaNro - 1) * tamPagina;

    using var connection = new MySqlConnection(connectionString);

    string sql = @"
        SELECT p.ID_Pago,
               p.ID_Estadia,
               p.FechaEmision,
               p.PrecioXHoraAplicado,
               p.MontoTotal,
               p.ComprobanteUrl
        FROM Pago p
        INNER JOIN Estadia e ON p.ID_Estadia = e.ID_Estadia
        WHERE e.Patente LIKE @busqueda
           OR CAST(p.ID_Pago AS CHAR) LIKE @busqueda
        ORDER BY p.ID_Pago DESC
        LIMIT @limit OFFSET @offset;";

    using var command = new MySqlCommand(sql, connection);

    command.Parameters.AddWithValue(
        "@busqueda", "%" + busqueda.Trim() + "%");

    command.Parameters.AddWithValue("@limit", tamPagina);
    command.Parameters.AddWithValue("@offset", offset);

    connection.Open();

    using var reader = command.ExecuteReader();

    while (reader.Read())
    {
        lista.Add(new Pago
        {
            ID_Pago = reader.GetInt32(
                reader.GetOrdinal("ID_Pago")),

            ID_Estadia = reader.GetInt32(
                reader.GetOrdinal("ID_Estadia")),

            FechaEmision = reader.GetDateTime(
                reader.GetOrdinal("FechaEmision")),

            PrecioXHoraAplicado = reader.GetDecimal(
                reader.GetOrdinal("PrecioXHoraAplicado")),

            MontoTotal = reader.GetDecimal(
                reader.GetOrdinal("MontoTotal")),

            ComprobanteUrl = reader.IsDBNull(
                reader.GetOrdinal("ComprobanteUrl"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("ComprobanteUrl"))
        });
    }

    return lista;
}

       public int ObtenerCantidad(string busqueda = "")
{
    using var connection = new MySqlConnection(connectionString);

    string sql = @"
        SELECT COUNT(*)
        FROM Pago p
        INNER JOIN Estadia e ON p.ID_Estadia = e.ID_Estadia
        WHERE e.Patente LIKE @busqueda
           OR CAST(p.ID_Pago AS CHAR) LIKE @busqueda;";

    using var command = new MySqlCommand(sql, connection);

    command.Parameters.AddWithValue(
        "@busqueda", "%" + busqueda.Trim() + "%");

    connection.Open();

    return Convert.ToInt32(command.ExecuteScalar());
}

        public IList<Pago> Buscar(string busqueda)
        {
            var lista = new List<Pago>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT p.ID_Pago, p.ID_Estadia, p.FechaEmision, p.PrecioXHoraAplicado, p.MontoTotal, p.ComprobanteUrl 
                               FROM Pago p
                               INNER JOIN Estadia e ON p.ID_Estadia = e.ID_Estadia
                               WHERE e.Patente LIKE @busqueda OR CAST(p.ID_Pago AS CHAR) LIKE @busqueda
                               ORDER BY p.ID_Pago DESC;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@busqueda", $"%{busqueda.Trim()}%");
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Pago
                            {
                                ID_Pago = reader.GetInt32(reader.GetOrdinal("ID_Pago")),
                                ID_Estadia = reader.GetInt32(reader.GetOrdinal("ID_Estadia")),
                                FechaEmision = reader.GetDateTime(reader.GetOrdinal("FechaEmision")),
                                PrecioXHoraAplicado = reader.GetDecimal(reader.GetOrdinal("PrecioXHoraAplicado")),
                                MontoTotal = reader.GetDecimal(reader.GetOrdinal("MontoTotal")),
                                ComprobanteUrl = reader.IsDBNull(reader.GetOrdinal("ComprobanteUrl")) ? null : reader.GetString(reader.GetOrdinal("ComprobanteUrl"))
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}