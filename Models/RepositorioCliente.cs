using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Estacionamiento_grupo_9.Models
{
    public class RepositorioCliente : IRepositorioCliente
    {
        private readonly string connectionString;

        public RepositorioCliente(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("MySql") 
                ?? configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'MySql' en appsettings.json.");
        }

        public int Alta(Cliente cliente)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"INSERT INTO Cliente (Patente, Nombre, Apellido, Telefono) 
                               VALUES (@patente, @nombre, @apellido, @telefono);";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@patente", cliente.Patente.Trim().ToUpper());
                    command.Parameters.AddWithValue("@nombre", cliente.Nombre.Trim());
                    command.Parameters.AddWithValue("@apellido", cliente.Apellido.Trim());
                    command.Parameters.AddWithValue("@telefono", cliente.Telefono.Trim());

                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public int Modificacion(Cliente cliente)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"UPDATE Cliente 
                               SET Nombre = @nombre, Apellido = @apellido, Telefono = @telefono 
                               WHERE Patente = @patente;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@patente", cliente.Patente.Trim().ToUpper());
                    command.Parameters.AddWithValue("@nombre", cliente.Nombre.Trim());
                    command.Parameters.AddWithValue("@apellido", cliente.Apellido.Trim());
                    command.Parameters.AddWithValue("@telefono", cliente.Telefono.Trim());

                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public int Baja(string patente)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"DELETE FROM Cliente WHERE Patente = @patente;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@patente", patente.Trim().ToUpper());

                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public Cliente? ObtenerPorPatente(string patente)
        {
            Cliente? cliente = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT Patente, Nombre, Apellido, Telefono 
                               FROM Cliente 
                               WHERE Patente = @patente;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@patente", patente.Trim().ToUpper());
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cliente = new Cliente
                            {
                                Patente = reader.GetString(reader.GetOrdinal("Patente")),
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
                                Telefono = reader.GetString(reader.GetOrdinal("Telefono"))
                            };
                        }
                    }
                }
            }
            return cliente;
        }

        public IList<Cliente> ObtenerLista(int paginaNro, int tamPagina)
        {
            var lista = new List<Cliente>();
            int offset = (paginaNro - 1) * tamPagina;

            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT Patente, Nombre, Apellido, Telefono 
                               FROM Cliente 
                               ORDER BY Apellido, Nombre 
                               LIMIT @limit OFFSET @offset;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@limit", tamPagina);
                    command.Parameters.AddWithValue("@offset", offset);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Cliente
                            {
                                Patente = reader.GetString(reader.GetOrdinal("Patente")),
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
                                Telefono = reader.GetString(reader.GetOrdinal("Telefono"))
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public int ObtenerCantidad()
        {
            int cantidad = 0;
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT COUNT(*) FROM Cliente;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    cantidad = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return cantidad;
        }

        public IList<Cliente> Buscar(string busqueda)
        {
            var lista = new List<Cliente>();
            using (var connection = new MySqlConnection(connectionString))
            {
                string sql = @"SELECT Patente, Nombre, Apellido, Telefono 
                               FROM Cliente 
                               WHERE Patente LIKE @busqueda 
                                  OR Nombre LIKE @busqueda 
                                  OR Apellido LIKE @busqueda 
                                  OR Telefono LIKE @busqueda 
                               ORDER BY Apellido, Nombre;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@busqueda", $"%{busqueda.Trim()}%");
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Cliente
                            {
                                Patente = reader.GetString(reader.GetOrdinal("Patente")),
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
                                Telefono = reader.GetString(reader.GetOrdinal("Telefono"))
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}