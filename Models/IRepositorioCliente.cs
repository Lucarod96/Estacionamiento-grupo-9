using System.Collections.Generic;

namespace Estacionamiento_grupo_9.Models
{
    public interface IRepositorioCliente
    {
        int Alta(Cliente cliente);
        int Baja(string patente);
        int Modificacion(Cliente cliente);
        Cliente? ObtenerPorPatente(string patente);
        IList<Cliente> ObtenerLista(int paginaNro, int tamPagina);
        int ObtenerCantidad();
        IList<Cliente> Buscar(string busqueda);
    }
}