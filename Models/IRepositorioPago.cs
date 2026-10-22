using System.Collections.Generic;

namespace Estacionamiento_grupo_9.Models
{
    public interface IRepositorioPago
    {
        int Alta(Pago pago);
        Pago? ObtenerPorId(int id);
        Pago? ObtenerPorEstadiaId(int estadiaId);
        IList<Pago> ObtenerLista(int paginaNro, int tamPagina);
        int ObtenerCantidad();
        IList<Pago> Buscar(string busqueda);
    }
}