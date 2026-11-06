using ClientesApp.Models;
using ClientesApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ContentAlignment = System.Drawing.ContentAlignment;

namespace ClientesApp
{
    // La ventana principal. Todo el diseño se crea aquí por código.
    //
    // CÓMO SE USA (esto mismo se escribe en la ventana, abajo):
    //   - Un clic en una fila ........ selecciona la fila entera
    //   - Doble clic en un dato ...... edita solo ese dato
    //   - Tecla Supr o botón Eliminar  borra las filas seleccionadas
    //
    // SOBRE LOS ERRORES: nunca bloquean nada. Un dato incorrecto se rodea de rojo
    // y, debajo de la rejilla, se escribe en rojo el ejemplo del formato correcto.
    // Se puede seguir editando, importando y guardando igualmente.
    public class FormPrincipal : Form
    {
        // El rojo de aviso. Se usa en el borde de la celda Y en el texto del ejemplo.
        private static readonly Color ColorAviso = Color.FromArgb(220, 80, 80);
        private static readonly Color FondoAviso = Color.FromArgb(255, 248, 225);
        private static readonly Color TextoAviso = Color.FromArgb(120, 85, 20);

        // ----- Cosas que se ven en pantalla -----
        private readonly DataGridView _rejilla = new DataGridView();

        private readonly ProgressBar _barra = new ProgressBar();
        private readonly Panel _panelSuperior = new Panel();
        private readonly Button _btnCsv = new Button();
        private readonly Button _btnJson = new Button();
        private readonly Button _btnEliminar = new Button();
        private readonly Label _etiquetaAyuda = new Label();    // instrucciones (gris)
        private readonly Label _etiquetaEjemplo = new Label();  // ejemplos correctos (rojo)

        // ----- Datos -----
        // BindingList: si la lista cambia, la rejilla se actualiza sola (y al revés).
        // La fila N de la rejilla es siempre el elemento N de esta lista
        // (la rejilla no se puede ordenar, así que no se descuadran).
        private readonly BindingList<ClienteDesktop> _clientes = new BindingList<ClienteDesktop>();

        // ----- Ayudantes -----
        private readonly ImportadorClientes _importador = new ImportadorClientes();
        private readonly AlmacenClientes _almacen = new AlmacenClientes();

        public FormPrincipal()
        {
            Text = "Clientes";
            Width = 1000;
            Height = 550;

            // ---------- Botones de arriba ----------
            _btnCsv.Text = "Importar CSV";
            _btnCsv.SetBounds(10, 8, 120, 26);
            _btnCsv.Click += async (s, e) => await Importar("csv");

            _btnJson.Text = "Importar JSON";
            _btnJson.SetBounds(140, 8, 120, 26);
            _btnJson.Click += async (s, e) => await Importar("json");

            _btnEliminar.Text = "Eliminar fila (Supr)";
            _btnEliminar.SetBounds(270, 8, 120, 26);
            _btnEliminar.Click += (s, e) => EliminarSeleccion();

            _panelSuperior.Dock = DockStyle.Top;
            _panelSuperior.Height = 40;
            _panelSuperior.Controls.Add(_btnCsv);
            _panelSuperior.Controls.Add(_btnJson);
            _panelSuperior.Controls.Add(_btnEliminar);

            // ---------- Zona de abajo: ayuda, ejemplo en rojo y barra ----------
            _etiquetaAyuda.Dock = DockStyle.Bottom;
            _etiquetaAyuda.Height = 22;
            _etiquetaAyuda.TextAlign = ContentAlignment.MiddleLeft;
            _etiquetaAyuda.ForeColor = SystemColors.GrayText;
            _etiquetaAyuda.Text = "  Clic: seleccionar fila   |   Doble clic: editar un dato   |   Supr: eliminar fila   |   * = obligatorio";

            _etiquetaEjemplo.Dock = DockStyle.Bottom;
            _etiquetaEjemplo.Height = 24;
            _etiquetaEjemplo.TextAlign = ContentAlignment.MiddleLeft;
            _etiquetaEjemplo.ForeColor = ColorAviso;

            _barra.Dock = DockStyle.Bottom;
            _barra.Height = 16;
            _barra.Minimum = 0;
            _barra.Maximum = 100;

            // ---------- Rejilla ----------
            _rejilla.Dock = DockStyle.Fill;
            _rejilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Columnas definidas a mano: así Id y FechaModificacion NO aparecen
            _rejilla.AutoGenerateColumns = false;
            AgregarColumna("Dni", "DNI");
            AgregarColumna("Nombre", "Nombre");
            AgregarColumna("Apellidos", "Apellidos");
            AgregarColumna("FechaNacimiento", "Fecha nacimiento");
            AgregarColumna("Telefono", "Teléfono");
            AgregarColumna("Email", "Email");

            // Un clic selecciona la FILA ENTERA...
            _rejilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _rejilla.MultiSelect = true; // (Ctrl o Mayús para elegir varias)

            // ...y solo se edita con doble clic (se programa más abajo)
            _rejilla.EditMode = DataGridViewEditMode.EditProgrammatically;

            _rejilla.AllowUserToAddRows = true;      // fila vacía al final para añadir
            _rejilla.AllowUserToDeleteRows = false;  // borrar lo hacemos nosotros (con confirmación)

            _rejilla.DataSource = _clientes;         // conectamos la lista con la rejilla

            // Eventos de la rejilla
            _rejilla.CellDoubleClick += Rejilla_CellDoubleClick;
            _rejilla.CellPainting += Rejilla_CellPainting;
            _rejilla.CellValueChanged += Rejilla_CellValueChanged;
            _rejilla.CurrentCellChanged += (s, e) => ActualizarEjemplo();
            _rejilla.KeyDown += Rejilla_KeyDown;

            _rejilla.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 250);
            _rejilla.DefaultCellStyle.SelectionForeColor = Color.FromArgb(35, 45, 60);
            // ---------- Montamos la ventana ----------
            // Orden importante: el ÚLTIMO control añadido se coloca primero, en el borde.
            // La rejilla (Fill) debe ir al frente (BringToFront) para ocupar solo el hueco que sobra.
            Controls.Add(_rejilla);
            Controls.Add(_panelSuperior);
            Controls.Add(_etiquetaAyuda);
            Controls.Add(_etiquetaEjemplo);
            Controls.Add(_barra);
            _rejilla.BringToFront();

            // Al abrir cargamos, al cerrar guardamos
            Load += FormPrincipal_Load;
            FormClosing += FormPrincipal_FormClosing;
        }

        // =====================================================================
        //  ARRANQUE Y CIERRE
        // =====================================================================

        // Al abrir la ventana: carga los clientes del fichero (si existe)
        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            try
            {
                foreach (ClienteDesktop c in _almacen.Cargar())
                    _clientes.Add(c);
            }
            catch (FormatException ex)
            {
                // El fichero existe pero su formato no es el esperado
                MessageBox.Show(
                    ex.Message + "\n\nSe empezará con la lista vacía y, al cerrar, se creará un fichero nuevo.",
                    "Formato incorrecto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // Cualquier otro fallo (permisos, disco...)
                MessageBox.Show("No se pudo leer el fichero guardado: " + ex.Message, "Aviso");
            }
        }

        // Al cerrar la ventana: guarda los clientes en el fichero.
        // Solo se guardan los que tienen bien los datos OBLIGATORIOS (los de *).
        // Los errores en datos opcionales no impiden guardar.
        private void FormPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {
            _rejilla.EndEdit(); // si había un dato a medio escribir, lo damos por terminado

            List<ClienteDesktop> aGuardar = _clientes
                .Where(ValidadorCliente.ObligatoriosCorrectos)
                .ToList();
            int descartados = _clientes.Count - aGuardar.Count;

            // Si hay clientes sin los obligatorios, avisamos y dejamos elegir
            if (descartados > 0)
            {
                string aviso = (descartados == 1
                                   ? "Hay 1 cliente"
                                   : "Hay " + descartados + " clientes")
                             + " sin los datos obligatorios (*) bien rellenados."
                             + "\nNo se guardará(n)."
                             + "\n\nAceptar: cerrar y descartarlos."
                             + "\nCancelar: volver para corregirlos.";

                DialogResult respuesta = MessageBox.Show(
                    aviso, "Datos obligatorios incompletos",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                if (respuesta != DialogResult.OK)
                {
                    e.Cancel = true; // no se cierra la ventana

                    // Dejamos seleccionada la primera fila con problema
                    int fila = _clientes.ToList().FindIndex(c => !ValidadorCliente.ObligatoriosCorrectos(c));
                    if (fila >= 0) _rejilla.CurrentCell = _rejilla[0, fila];
                    return;
                }
            }

            try
            {
                _almacen.Guardar(aGuardar);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar: " + ex.Message, "Error");
            }
        }

        // =====================================================================
        //  COLUMNAS
        // =====================================================================

        // Crea una columna enlazada a una propiedad de ClienteDesktop.
        // Si la propiedad tiene [Required] en el modelo, el título lleva " *".
        private void AgregarColumna(string propiedad, string titulo)
        {
            bool obligatoria = typeof(ClienteDesktop)
                .GetProperty(propiedad)
                .IsDefined(typeof(RequiredAttribute), false);

            var columna = new DataGridViewTextBoxColumn();
            columna.Name = propiedad;
            columna.DataPropertyName = propiedad;                       // propiedad enlazada
            columna.HeaderText = obligatoria ? titulo + " *" : titulo;  // lo que se ve
            columna.Tag = titulo;                                       // título sin "*" (para el aviso en rojo)
            _rejilla.Columns.Add(columna);
        }

        // =====================================================================
        //  EDITAR, ELIMINAR Y PINTAR
        // =====================================================================

        // Doble clic en un dato: se edita solo ese dato
        private void Rejilla_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return; // doble clic en una cabecera: nada

            _rejilla.CurrentCell = _rejilla[e.ColumnIndex, e.RowIndex];
            _rejilla.BeginEdit(true); // true = selecciona el texto para sobrescribirlo
        }

        // Tecla Supr con la rejilla seleccionada (y sin estar escribiendo en un dato)
        private void Rejilla_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && !_rejilla.IsCurrentCellInEditMode)
            {
                EliminarSeleccion();
                e.Handled = true;
            }
        }

        // Borra las filas seleccionadas (botón o tecla Supr). Pide confirmación una vez.
        private void EliminarSeleccion()
        {
            // Filas seleccionadas, sin contar la fila vacía de "añadir"
            List<ClienteDesktop> aBorrar = _rejilla.SelectedRows
                .Cast<DataGridViewRow>()
                .Where(fila => !fila.IsNewRow)
                .Select(fila => (ClienteDesktop)fila.DataBoundItem)
                .ToList();

            if (aBorrar.Count == 0) return; // no hay nada seleccionado

            string pregunta = aBorrar.Count == 1
                ? "¿Eliminar el cliente seleccionado?"
                : "¿Eliminar los " + aBorrar.Count + " clientes seleccionados?";

            if (MessageBox.Show(pregunta, "Eliminar", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != DialogResult.Yes) return;

            foreach (ClienteDesktop cliente in aBorrar)
                _clientes.Remove(cliente);

            ActualizarEjemplo();
        }

        // Se ejecuta al terminar de editar un dato
        private void Rejilla_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _clientes.Count) return;

            // Apuntamos cuándo se tocó este cliente (dato interno, no se ve)
            _clientes[e.RowIndex].FechaModificacion = DateTime.Now;

            _rejilla.InvalidateRow(e.RowIndex); // repinta la fila para actualizar los bordes rojos
            ActualizarEjemplo();
        }

        // Se ejecuta cada vez que hay que dibujar una celda.
        // Si su dato está mal: borde rojo (sin fondo) y el ejemplo correcto dentro,
        // en rojo tenue: al principio si la celda está vacía, o después del dato si hay texto.
        private void Rejilla_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return; // cabeceras: pintado normal

            e.Paint(e.CellBounds, DataGridViewPaintParts.All); // 1) celda normal

            string ejemplo = EjemploSiHayError(e.RowIndex, e.ColumnIndex);

            if (ejemplo != null)
            {
                // 2) borde rojo
                using (var lapiz = new Pen(ColorAviso, 2))
                {
                    e.Graphics.DrawRectangle(lapiz,
                        e.CellBounds.X + 1, e.CellBounds.Y + 1,
                        e.CellBounds.Width - 3, e.CellBounds.Height - 3);
                }

                // 3) ejemplo tenue dentro de la celda
                DibujarEjemploEnCelda(e, ejemplo);
            }

            e.Handled = true; // ya hemos pintado nosotros
        }

        private void DibujarEjemploEnCelda(DataGridViewCellPaintingEventArgs e, string ejemplo)
        {
            string valor = Convert.ToString(e.FormattedValue) ?? "";
            bool vacia = valor.Length == 0;
            Font fuente = e.CellStyle.Font ?? _rejilla.Font;

            const int margenIzq = 4;   // el texto de la celda empieza ~4 px dentro
            const int separacion = 8;  // hueco entre el dato y el ejemplo
            const int margenDer = 4;

            // Dónde empieza el ejemplo: al principio si no hay dato, o tras el dato
            int anchoDato = vacia
                ? 0
                : TextRenderer.MeasureText(e.Graphics, valor, fuente, Size.Empty, TextFormatFlags.NoPadding).Width
                  + separacion;

            int x = e.CellBounds.X + margenIzq + anchoDato;
            int ancho = e.CellBounds.Right - x - margenDer;

            // Con el dato escrito, solo dibujamos si el ejemplo cabe ENTERO
            int anchoEjemplo = TextRenderer.MeasureText(e.Graphics, ejemplo, fuente, Size.Empty, TextFormatFlags.NoPadding).Width;
            if (!vacia && anchoEjemplo > ancho) return;
            if (ancho < 30) return;

            // TextRenderer no admite transparencia: mezclamos el rojo con el fondo a mano
            Color colorTexto = _rejilla.Rows[e.RowIndex].Selected
                ? TextoAviso
                : Color.FromArgb(
                    (ColorAviso.R * 6 + 255 * 4) / 10,
                    (ColorAviso.G * 6 + 255 * 4) / 10,
                    (ColorAviso.B * 6 + 255 * 4) / 10);

            var zona = new Rectangle(x, e.CellBounds.Y + 2, ancho, e.CellBounds.Height - 4);

            TextRenderer.DrawText(e.Graphics, ejemplo, fuente, zona, colorTexto,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
        }

        // Devuelve el ejemplo correcto del dato de esta celda si está mal, o null si está bien
        private string EjemploSiHayError(int fila, int columna)
        {
            if (fila >= _clientes.Count) return null; // fila vacía de "añadir": sin avisos

            string propiedad = _rejilla.Columns[columna].DataPropertyName;
            return ValidadorCliente.ErrorDe(_clientes[fila], propiedad);
        }

        // Escribe en rojo, bajo la rejilla, el ejemplo correcto de cada dato mal
        // de la fila actual. Ejemplo:  Formato correcto →  Email: nombre@gmail.com
        private void ActualizarEjemplo()
        {
            _etiquetaEjemplo.Text = "";

            DataGridViewCell celda = _rejilla.CurrentCell;
            if (celda == null || celda.RowIndex >= _clientes.Count) return;

            Dictionary<string, string> errores = ValidadorCliente.TodosLosErrores(_clientes[celda.RowIndex]);
            var ejemplos = new List<string>();

            // Recorremos las columnas para que los ejemplos salgan en el mismo orden que la rejilla
            foreach (DataGridViewColumn columna in _rejilla.Columns)
            {
                string ejemplo;
                if (errores.TryGetValue(columna.DataPropertyName, out ejemplo))
                    ejemplos.Add(columna.Tag + ": " + ejemplo);
            }

            if (ejemplos.Count > 0)
                _etiquetaEjemplo.Text = "  Formato correcto →  " + string.Join("      ", ejemplos);
        }

        // =====================================================================
        //  IMPORTAR
        // =====================================================================

        // Importa un fichero. "formato" es "csv" o "json".
        private async Task Importar(string formato)
        {
            // 1) Preguntamos al usuario qué fichero quiere
            var dialogo = new OpenFileDialog();
            dialogo.Filter = formato == "csv" ? "Ficheros CSV|*.csv" : "Ficheros JSON|*.json";
            if (dialogo.ShowDialog() != DialogResult.OK) return; // canceló

            // 2) Preparamos la barra: cada vez que el importador avise, la movemos
            _barra.Value = 0;
            var progreso = new Progress<int>(porcentaje => _barra.Value = porcentaje);

            try
            {
                // 3) Leemos el fichero en segundo plano para no congelar la ventana
                List<ClienteDesktop> leidos = await Task.Run(() =>
                    formato == "csv"
                        ? _importador.ImportarCsv(dialogo.FileName, progreso)
                        : _importador.ImportarJson(dialogo.FileName, progreso));

                // 4) Añadimos TODOS a la rejilla, salvo los de DNI repetido
                int anadidos = 0, repetidos = 0, porRevisar = 0;
                foreach (ClienteDesktop cliente in leidos)
                {
                    if (DniYaExiste(cliente.Dni))
                    {
                        repetidos++;
                        continue;
                    }

                    _clientes.Add(cliente);
                    anadidos++;
                    if (ValidadorCliente.TieneErrores(cliente)) porRevisar++;
                }

                // 5) Resumen corto
                string mensaje = "Clientes añadidos: " + anadidos;
                if (porRevisar > 0) mensaje += "\nCon datos por revisar (en rojo): " + porRevisar;
                if (repetidos > 0) mensaje += "\nIgnorados por DNI repetido: " + repetidos;
                MessageBox.Show(mensaje, "Importación terminada");

                ActualizarEjemplo();
            }
            catch (FormatException ex)
            {
                // El fichero se pudo leer, pero su contenido no tiene el formato esperado
                MessageBox.Show(ex.Message, "Formato incorrecto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // Fichero ilegible por otro motivo: avisamos sin que la app explote
                MessageBox.Show("No se pudo importar: " + ex.Message, "Error");
            }
        }

        // ¿Ya hay un cliente con este DNI? (un DNI vacío nunca cuenta como repetido)
        private bool DniYaExiste(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni)) return false;

            return _clientes.Any(c => string.Equals(c.Dni, dni, StringComparison.OrdinalIgnoreCase));
        }
    }
}