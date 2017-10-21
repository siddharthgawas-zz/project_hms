using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.Configuration;
namespace WindowsFormsApplication1
{
    public partial class Form2 : Form
    {
        private decimal eid;
        private OracleConnection connection;
        private OracleCommand command;
        private OracleDataReader reader;
        public Form2()
        {
            InitializeComponent();
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
            loadHospitalDetails();
        }

        public Form2(decimal eid)
        {
            InitializeComponent();
            this.eid = eid;
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
            loadHospitalDetails();
        }
        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void loadHospitalDetails()
        {
            string query = "Select name,contact_number FROM hospital WHERE id = 1";
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                label1.Text = reader.GetString(0);
                label2.Text = "Contact No: "+reader.GetDecimal(1).ToString();
            }
            else
            {
                label1.Text = "<Hospital Name>";
                label2.Text = "Contact No: ";
            }
            connection.Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Ward w = new Ward();
            w.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Appointments a = new Appointments();
            a.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form3 f = new Form3();
            f.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Dispose(false);
            Form1 f = new Form1();
            f.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string query = "SELECT eid FROM employee, type_table WHERE eid = " + eid +
                " AND employee.employee_type = type_table.id AND type='Admin'";
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            if(reader.HasRows)
            {
                connection.Close();
                Settings s = new Settings();
                s.Show();
            }
            else
            {
                connection.Close();
                MessageBox.Show("Please login as administrator to access this module");
            }
            connection.Close();
        }
    }
}
