using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace BusquedaDinámicaDGV
{
    public partial class Form1 : Form
    {
// PASO 0 (Infraestructura): Cadena de conexión desde app.config
        //  Debe existir <connectionStrings><add name="InstitutoDB" .../>

        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["InstitutoDB"].ConnectionString;

// PASO 1 (Modelo en memoria): DataTable que contendrá TODA la tabla "Alumno"
        // - Mantenerlo como CAMPO de clase es CLAVE para poder filtrar con RowFilter

        private readonly DataTable dt = new DataTable();
        public Form1()
        {
            InitializeComponent();

// PASO 2 (Eventos): Aseguramos que se ejecuta el pipeline de carga y filtro
            // - Form1_Load: hará la carga de datos y el primer enlace
            // - TextChanged del cuadro de búsqueda: disparará el filtrado en caliente
            this.Load += Form1_Load;
            this.busqueda.TextChanged += tbBusqueda_TextChanged;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
// PASO 3 (Preparar el DataGridView):
            // - Evitamos que las columnas puestas en el diseñador bloqueen el autogenerado.
            // - "AutoGenerateColumns = true" hará que el DGV cree columnas a partir de dt.
            dgv.AutoGenerateColumns = true;
            dgv.Columns.Clear();

// PASO 4 (Comprobar conexión rápidamente:
            ProbarConexion();

// PASO 5 (Cargar datos UNA VEZ):
            traerDatos();

// PASO 6 (DataBinding inicial):
           
            dgv.DataSource = dt;

// PASO 7 (Rellenar ComboBox de columnas):
            // - Rellenamos con NOMBRES reales de columnas, salidos del DataTable,
            //   para que el usuario elija sobre qué campo quiere filtra
            rellenarComboBox();
        }

        private void ProbarConexion()
        {
            // Consulta mínima para comprobar conectividad (no arrastra datos)
            const string sql = "SELECT TOP (1) 1 FROM dbo.Alumno";
            try
            {
                using (var cnx = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, cnx))
                {
                    cnx.Open();
                    cmd.ExecuteScalar(); // si no lanza excepción, la conexión es OK
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se puede realizar la conexión: " + ex.Message);
            }
        }

        // PASO 8 (Filtrado dinámico):
        // - Se ejecuta cada vez que cambia el texto de búsqueda.
        // - Usamos DataView.RowFilter sobre dt.DefaultView (NO volvemos a consultar BD).
        // - Si la columna no es string (números/fechas), la Convertimos a string para usar LIKE.
        // - Buscamos en cualquier parte del valor con '%texto%'. Cambia a '{texto}%' si quieres "empieza por".
        public void consultarConRowFilter()
        {
            string col = cbCampos.Text;                               // columna elegida
            string texto = busqueda.Text?.Replace("'", "''") ?? "";   // escapar comillas simples

            // Si el cuadro está vacío => quitamos filtro
            if (string.IsNullOrWhiteSpace(texto))
            {
                dt.DefaultView.RowFilter = string.Empty;
                dgv.DataSource = dt.DefaultView;   // (opcional, ya estaba enlazado)
                return;
            }

            bool esString = dt.Columns[col].DataType == typeof(string);

            // Filtro expresado en sintaxis de DataColumn (NO SQL). Ojo con corchetes en el nombre.
            string filtro = esString
                ? $"[{col}] LIKE '%{texto}%'"                                 // texto
                : $"Convert([{col}], 'System.String') LIKE '%{texto}%'";      // números/fechas

            try
            {
                dt.DefaultView.RowFilter = filtro; // aplica filtro sobre la vista
                dgv.DataSource = dt.DefaultView;   // el DGV muestra la vista filtrada
            }
            catch
            {
                // Si hubiera un nombre raro/espacios sin corchetes, volvemos a sin filtro
                dt.DefaultView.RowFilter = string.Empty;
                dgv.DataSource = dt.DefaultView;
            }
        }

        // Evento del TextBox: cada pulsación actualiza el filtro
        private void tbBusqueda_TextChanged(object sender, EventArgs e)
        {
            consultarConRowFilter();
        }

// PASO 9 (Combo de columnas):
        // - Se llena a partir de dt.Columns para usar nombres REALES de la tabla.
        // - Selecciona la primera columna por defecto.
        private void rellenarComboBox()
        {
            cbCampos.Items.Clear();
            foreach (DataColumn col in dt.Columns)
                cbCampos.Items.Add(col.ColumnName);

            if (cbCampos.Items.Count > 0)
                cbCampos.SelectedIndex = 0;
        }

// PASO 5 (Implementación): Cargar los datos en memoria
        // - Ejecuta SELECT y hace dt.Load(dr).
        // - dt.Clear() por si recargas (evita duplicados).
        private void traerDatos()
        {
            const string sql =
                "SELECT AlumnoId, NIF, Nombre, Apellido1, Apellido2, FechaNac, GrupoId FROM dbo.Alumno";

            try
            {
                using (var cnx = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, cnx))
                {
                    cnx.Open();

                    dt.Clear();                   // limpia contenido previo
                    using (var dr = cmd.ExecuteReader())
                        dt.Load(dr);             // vuelca todo el DataReader al DataTable
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al traer datos: " + ex.Message);
            }
        }
        private void busqueda_Click(object sender, EventArgs e) { }
        private void dgv_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void busqueda_MaskInputRejected(object sender, MaskInputRejectedEventArgs e) { }
    }
}
