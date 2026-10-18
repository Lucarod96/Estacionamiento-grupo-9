namespace Estacionamiento_grupo_9.Models;

public interface IRepositorioEstadia
{
    IList<Estadia> ObtenerLista(
        int pagina, int cantidad, string busqueda, bool soloActivas);

    int ObtenerCantidad(string busqueda, bool soloActivas);

    Estadia? ObtenerPorId(int id);

    // Devuelve el ID de la estadía creada
    int RegistrarIngreso(Estadia modelo);

    // Devuelve 0 si la estadía no existe o ya estaba cerrada
    int RegistrarSalida(int id, DateTime egreso);

    bool PlazaOcupada(int idPlaza);

    bool PatenteAdentro(string patente);

    IList<Plaza> BuscarPlazasLibres(string busqueda, int maximo);
}