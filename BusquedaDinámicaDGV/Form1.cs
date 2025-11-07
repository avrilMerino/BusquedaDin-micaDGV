using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace BusquedaDinámicaDGV
{
    public partial class Form1 : Form
    {



        private readonly DataTable dt = new DataTable();
        public Form1()
        {
            InitializeComponent();


        }

        private void grupoBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.grupoBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.institutoDBDataSet);

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: esta línea de código carga datos en la tabla 'institutoDBDataSet.Alumno' Puede moverla o quitarla según sea necesario
            this.alumnoTableAdapter.Fill(this.institutoDBDataSet.Alumno);
            // TODO: esta línea de código carga datos en la tabla 'institutoDBDataSet.Alumno' Puede moverla o quitarla según sea necesario
            this.alumnoTableAdapter.Fill(this.institutoDBDataSet.Alumno);
            // TODO: esta línea de código carga datos en la tabla 'institutoDBDataSet.Grupo' Puede moverla o quitarla según sea necesario.
            this.grupoTableAdapter.Fill(this.institutoDBDataSet.Grupo);

        }
    }
}
