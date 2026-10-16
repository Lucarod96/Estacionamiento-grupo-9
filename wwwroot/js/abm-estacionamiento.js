(() => {
    const root = document.getElementById("abm-estacionamiento");

    if (!root) return;

    if (!window.Vue) {
        root.removeAttribute("v-cloak");
        root.textContent =
            "No se pudo cargar Vue. Revisá Internet y recargá.";
        return;
    }

    const entidad = root.dataset.entidad;
    const esPlaza = entidad === "Plaza";
    const base = root.dataset.base;

    let secuenciaTipos = 0;
    let secuenciaLista = 0;

    Vue.createApp({
        data() {
            return {
                admin: root.dataset.admin === "true",
                clave: esPlaza ? "id_Plaza" : "id_TipoDePlaza",

                items: [],
                busqueda: "",
                pagina: 1,
                paginas: 1,
                total: 0,

                cargando: false,
                guardando: false,
                error: "",
                mensaje: "",

                abierto: false,
                nuevo: true,
                form: {},

                tipos: [],
                tiposAbiertos: false,
                tipoSeleccionado: "",
                tipoPagina: 1,
                tiposHayMas: false,
                tipoBuscando: false,
                tipoConsultado: false
            };
        },

        mounted() {
            this.cargar(1);
        },

        methods: {
            dinero(valor) {
                return Number(valor).toLocaleString("es-AR", {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                });
            },

            async pedir(url, opciones = {}) {
                const respuesta = await fetch(base + url, {
                    credentials: "same-origin",
                    ...opciones
                });

                if (
                    respuesta.redirected ||
                    respuesta.status === 401 ||
                    respuesta.status === 403
                ) {
                    throw new Error(
                        "Iniciá sesión con un usuario que tenga permiso."
                    );
                }

                const esJson = respuesta.headers
                    .get("content-type")
                    ?.includes("application/json");

                const datos = esJson
                    ? await respuesta.json()
                    : null;

                if (!respuesta.ok || !esJson) {
                    throw new Error(
                        datos?.mensaje ||
                        "No se pudo completar la operación."
                    );
                }

                return datos;
            },

            async cargar(pagina) {
                const version = ++secuenciaLista;

                this.cargando = true;
                this.error = "";

                try {
                    const parametros = new URLSearchParams({
                        pagina: pagina,
                        busqueda: this.busqueda
                    });

                    const datos = await this.pedir(
                        entidad + "/Listar?" + parametros
                    );

                    if (version !== secuenciaLista) return;

                    this.items = datos.items;
                    this.pagina = datos.pagina;
                    this.paginas = datos.paginas;
                    this.total = datos.total;
                } catch (error) {
                    if (version === secuenciaLista) {
                        this.error = error.message;
                    }
                } finally {
                    if (version === secuenciaLista) {
                        this.cargando = false;
                    }
                }
            },

            limpiarTipos() {
                ++secuenciaTipos;

                this.tipos = [];
                this.tiposAbiertos = false;
                this.tipoSeleccionado = "";
                this.tipoPagina = 1;
                this.tiposHayMas = false;
                this.tipoBuscando = false;
                this.tipoConsultado = false;
            },

            abrirNuevo() {
                this.limpiarTipos();

                this.form = esPlaza
                    ? {
                        id_Plaza: null,
                        id_TipoDePlaza: 0,
                        piso: 0,
                        sector: ""
                    }
                    : {
                        id_TipoDePlaza: null,
                        tipo: "",
                        precioXHora: null
                    };

                this.nuevo = true;
                this.abierto = true;
                this.error = "";
                this.mensaje = "";
            },

            editar(item) {
                this.limpiarTipos();

                this.form = { ...item };

                this.tipoSeleccionado = esPlaza
                    ? item.tipo
                    : "";

                this.nuevo = false;
                this.abierto = true;
                this.error = "";
                this.mensaje = "";
            },

            alternarTipos() {
                if (this.tiposAbiertos) {
                    ++secuenciaTipos;

                    this.tiposAbiertos = false;
                    this.tipoBuscando = false;
                    return;
                }

                this.tiposAbiertos = true;
                this.buscarTipos(1);
            },

            async buscarTipos(pagina) {
                const version = ++secuenciaTipos;

                this.tipoBuscando = true;
                this.error = "";

                try {
                    const parametros = new URLSearchParams({
                        pagina: pagina,
                        busqueda: ""
                    });

                    const datos = await this.pedir(
                        "TipoDePlaza/Listar?" + parametros
                    );

                    if (version !== secuenciaTipos) return;

                    this.tipos = datos.items;
                    this.tiposHayMas =
                        datos.pagina < datos.paginas;

                    this.tipoPagina = datos.pagina;
                    this.tipoConsultado = true;
                } catch (error) {
                    if (version === secuenciaTipos) {
                        this.error = error.message;
                    }
                } finally {
                    if (version === secuenciaTipos) {
                        this.tipoBuscando = false;
                    }
                }
            },

            elegirTipo(tipo) {
                this.form.id_TipoDePlaza =
                    tipo.id_TipoDePlaza;

                this.limpiarTipos();

                this.tipoSeleccionado = tipo.tipo;
            },

            opciones(datos) {
                const token = root.querySelector(
                    'input[name="__RequestVerificationToken"]'
                ).value;

                return {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": token
                    },
                    body: JSON.stringify(datos)
                };
            },

            async guardar() {
                if (this.guardando) return;

                if (esPlaza && !this.form.id_TipoDePlaza) {
                    this.error =
                        "Seleccioná un tipo de plaza.";
                    return;
                }

                this.guardando = true;
                this.error = "";
                this.mensaje = "";

                try {
                    const datos = await this.pedir(
                        entidad + "/Guardar?nuevo=" + this.nuevo,
                        this.opciones(this.form)
                    );

                    this.abierto = false;
                    this.limpiarTipos();

                    await this.cargar(this.pagina);

                    this.mensaje = datos.mensaje;
                } catch (error) {
                    this.error = error.message;
                } finally {
                    this.guardando = false;
                }
            },

            async eliminar(item) {
                if (this.guardando) return;

                const confirmado = confirm(
                    "¿Eliminar el registro " +
                    item[this.clave] +
                    "?"
                );

                if (!confirmado) return;

                this.guardando = true;
                this.error = "";
                this.mensaje = "";

                try {
                    const datos = await this.pedir(
                        entidad + "/Eliminar?id=" + item[this.clave],
                        this.opciones({})
                    );

                    if (
                        this.form[this.clave] === item[this.clave]
                    ) {
                        this.abierto = false;
                        this.limpiarTipos();
                    }

                    await this.cargar(this.pagina);

                    this.mensaje = datos.mensaje;
                } catch (error) {
                    this.error = error.message;
                } finally {
                    this.guardando = false;
                }
            }
        }
    }).mount(root);
})();