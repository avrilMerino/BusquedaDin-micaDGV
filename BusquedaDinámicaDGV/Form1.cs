using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace BusquedaDinámicaDGV
{
    public partial class Form1 : Form
    {
        // Usa SIEMPRE la cadena del app.config (name="InstitutoDB")
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["InstitutoDB"].ConnectionString;

        // DataTable de la clase (para RowFilter)
        private readonly DataTable dt = new DataTable();

        public Form1()
        {
            InitializeComponent();

            // 🔴 IMPORTANTE: asegura que se ejecuta Form1_Load
            this.Load += Form1_Load;

            // Si quieres filtro dinámico, engancha el TextChanged (el diseñador no lo tenía)
            this.busqueda.TextChanged += tbBusqueda_TextChanged;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Para evitar columnas “de diseñador”
            dgv.AutoGenerateColumns = true;
            dgv.Columns.Clear();

            // 1) Solo probar conexión (sin tocar el grid)
            ProbarConexion();

            // 2) Cargar datos en dt
            traerDatos();

            // 3) Enlazar grid
            dgv.DataSource = dt;

            // 4) Rellenar combo
            rellenarComboBox();
        }

        private void ProbarConexion()
        {
            const string sql = "SELECT TOP (1) 1 FROM dbo.Alumno"; // test simple
            try
            {
                using (var cnx = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, cnx))
                {
                    cnx.Open();
                    cmd.ExecuteScalar(); // si no lanza, OK
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se puede realizar la conexión: " + ex.Message);
            }
        }

        public void consultarConRowFilter()
        {
            var col = cbCampos.Text;                           // columna elegida
            var texto = busqueda.Text?.Replace("'", "''") ?? string.Empty; // escapa comillas

            if (string.IsNullOrWhiteSpace(texto))
            {
                dt.DefaultView.RowFilter = string.Empty;
                dgv.DataSource = dt.DefaultView;
                return;
            }

            // Si la columna es string: LIKE directo. Si no, convierto a string.
            bool esString = dt.Columns[col].DataType == typeof(string);
            string filtro = esString
                ? $"[{col}] LIKE '%{texto}%'"                                 // texto
                : $"Convert([{col}], 'System.String') LIKE '%{texto}%'";      // números/fechas

            try
            {
                dt.DefaultView.RowFilter = filtro;
                dgv.DataSource = dt.DefaultView;
            }
            catch
            {
                // fallback por si el nombre de columna tuviera caracteres raros
                dt.DefaultView.RowFilter = string.Empty;
                dgv.DataSource = dt.DefaultView;
            }
        }


        private void tbBusqueda_TextChanged(object sender, EventArgs e)
        {
            consultarConRowFilter();
        }

        private void rellenarComboBox()
        {
            cbCampos.Items.Clear();
            foreach (DataColumn col in dt.Columns)
                cbCampos.Items.Add(col.ColumnName);

            if (cbCampos.Items.Count > 0)
                cbCampos.SelectedIndex = 0;
        }

        private void traerDatos()
        {
            const string sql = "SELECT AlumnoId, NIF, Nombre, Apellido1, Apellido2, FechaNac, GrupoId FROM dbo.Alumno";

            try
            {
                using (var cnx = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, cnx))
                {
                    cnx.Open();
                    dt.Clear();
                    using (var dr = cmd.ExecuteReader())
                        dt.Load(dr);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al traer datos: " + ex.Message);
            }
        }

        // Tus handlers vacíos pueden quedarse
        private void busqueda_Click(object sender, EventArgs e) { }
        private void dgv_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void busqueda_MaskInputRejected(object sender, MaskInputRejectedEventArgs e) { }
    }
}
