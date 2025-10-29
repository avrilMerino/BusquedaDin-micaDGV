using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace BusquedaDinámicaDGV
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ProbarConexion();
        }

        private void ProbarConexion()
        {
            string sql = "SELECT * FROM Cliente";
            SqlConnection cnx = new SqlConnection();
            try
            {
                cnx.Open();
                SqlCommand command = new SqlCommand(sql, cnx);
                SqlDataReader dataReader = command.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dataReader);
                dgv.DataSource = dt;
                command.Dispose();
                cnx.Close();
            }
            catch (SqlException e)
            {
                MessageBox.Show("No se puede realizar la conexion");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // evento vacío (opcional)
        }
    }
}
