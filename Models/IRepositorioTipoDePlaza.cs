namespace Estacionamiento_grupo_9.Models;

public interface IRepositorioTipoDePlaza
{
    IList<TipoDePlaza> ObtenerLista(
        int pagina,
        int cantidad,
        string busqueda
    );

    int ObtenerCantidad(string busqueda);

    TipoDePlaza? ObtenerPorId(int id);

    int Alta(TipoDePlaza modelo);

    int Modificacion(TipoDePlaza modelo);

    int Baja(int id);
}