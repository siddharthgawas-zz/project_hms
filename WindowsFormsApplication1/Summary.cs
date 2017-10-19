using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.Configuration;
namespace WindowsFormsApplication1
{
    public partial class Summary : Form
    {
        decimal appointment_id;
        private OracleConnection connection;
        private OracleCommand command;
        private OracleDataReader reader;
        public Summary()
        {
            InitializeComponent();
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
        }

        public Summary(decimal appointment_id)
        {
            InitializeComponent();
            this.appointment_id = appointment_id;
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
            loadSummaryData();
        }

        private void loadSummaryData()
        {
            string query = "SELECT summary FROM appointment_case WHERE id = " + appointment_id.ToString();
            command = new OracleCommand(query, connection);
            command.CommandType = CommandType.Text;
            connection.Open();
            reader = command.ExecuteReader();
            if(reader.HasRows)
            {
                reader.Read();
                if(!DBNull.Value.Equals(reader.GetValue(0)))
                {
                    string summary = reader.GetString(0);
                    richTextBox1.Text = summary;
                }
            }


            reader.Close();
            connection.Close();
        }

        private void loadPrescriptionData()
        {

        }
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)//update summary
        {
            string query = "UPDATE appointment_case SET summary = '" + richTextBox1.Text + "' WHERE id = " + appointment_id;
            command = new OracleCommand(query, connection);
            command.CommandType = CommandType.Text;
            connection.Open();
            decimal t = command.ExecuteNonQuery();
            if (t > 0)
                MessageBox.Show("Summary Updated");
            else
                MessageBox.Show("Error Occured");
            connection.Close();
        }
    }
}
