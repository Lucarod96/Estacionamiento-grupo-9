(function () {
    // Espera 250 ms después de la última tecla, así no se pide al servidor en cada letra
    function debounce(funcion, ms) {
        let temporizador;
        return function () {
            clearTimeout(temporizador);
            temporizador = setTimeout(funcion, ms);
        };
    }

    function autocompletar(opciones) {
        const input = opciones.input;
        const lista = opciones.lista;
        let numeroPedido = 0;

        function cerrar() {
            lista.innerHTML = "";
            lista.classList.add("d-none");
        }

        function mostrar(datos) {
            lista.innerHTML = "";

            if (datos.length === 0) {
                const vacio = document.createElement("div");
                vacio.className = "list-group-item text-muted small";
                vacio.textContent = opciones.textoVacio;
                lista.appendChild(vacio);
            } else {
                datos.forEach(function (dato) {
                    const boton = document.createElement("button");
                    boton.type = "button";
                    boton.className = "list-group-item list-group-item-action";
                    // textContent (y no innerHTML) evita que un dato con HTML se ejecute
                    boton.textContent = opciones.texto(dato);
                    boton.addEventListener("click", function () {
                        opciones.alElegir(dato);
                        cerrar();
                    });
                    lista.appendChild(boton);
                });
            }

            lista.classList.remove("d-none");
        }

        async function buscar() {
            const termino = input.value.trim();
            const pedido = ++numeroPedido;

            if (termino.length < opciones.minimo) {
                cerrar();
                return;
            }

            try {
                const respuesta = await fetch(
                    opciones.url + "?termino=" + encodeURIComponent(termino),
                    { headers: { "Accept": "application/json" } });

                if (!respuesta.ok) {
                    cerrar();
                    return;
                }

                const datos = await respuesta.json();

                // Si mientras tanto se hizo una búsqueda más nueva, esta respuesta ya no sirve
                if (pedido !== numeroPedido) return;

                mostrar(datos);
            } catch (error) {
                cerrar();
            }
        }

        input.addEventListener("input", debounce(buscar, 250));
        input.addEventListener("focus", buscar);

        document.addEventListener("click", function (evento) {
            if (evento.target !== input && !lista.contains(evento.target)) {
                cerrar();
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        const formulario = document.getElementById("estadia-create");
        if (!formulario) return;

        const inputPatente = document.getElementById("Patente");
        const buscadorPlaza = document.getElementById("buscador-plaza");
        const idPlaza = document.getElementById("ID_Plaza");

        autocompletar({
            input: inputPatente,
            lista: document.getElementById("sugerencias-patente"),
            url: formulario.dataset.urlCliente,
            minimo: 2,
            textoVacio: "No hay clientes con esa patente. Registralo primero en Clientes.",
            texto: function (c) { return c.patente + " — " + c.apellido + ", " + c.nombre; },
            alElegir: function (c) { inputPatente.value = c.patente; }
        });

        function etiquetaPlaza(p) {
            return "Plaza " + p.id + " — Piso " + p.piso + ", Sector " + p.sector;
        }

        autocompletar({
            input: buscadorPlaza,
            lista: document.getElementById("sugerencias-plaza"),
            url: formulario.dataset.urlPlaza,
            minimo: 0,
            textoVacio: "No hay plazas libres que coincidan.",
            texto: function (p) {
                return etiquetaPlaza(p) + " (" + p.tipo + ", $" + p.precioXHora + "/h)";
            },
            alElegir: function (p) {
                idPlaza.value = p.id;
                buscadorPlaza.value = etiquetaPlaza(p);
            }
        });

        // Si el usuario escribe después de elegir, la selección anterior deja de valer
        buscadorPlaza.addEventListener("input", function () {
            idPlaza.value = "0";
        });

        // Si el formulario volvió con errores, mostramos de nuevo la plaza elegida
        if (idPlaza.value && idPlaza.value !== "0") {
            buscadorPlaza.value = "Plaza " + idPlaza.value;
        }
    });
})();