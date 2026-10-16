namespace Estacionamiento_grupo_9.Models;

public interface IRepositorioPlaza
{
    IList<Plaza> ObtenerLista(
        int pagina,
        int cantidad,
        string busqueda
    );

    int ObtenerCantidad(string busqueda);

    Plaza? ObtenerPorId(int id);

    int Alta(Plaza modelo);

    int Modificacion(Plaza modelo);

    int Baja(int id);
}