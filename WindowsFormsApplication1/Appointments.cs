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
    public partial class Appointments : Form
    {
        private OracleConnection connection;
        private OracleCommand command;
        private OracleDataReader reader;
        public Appointments()
        {
            InitializeComponent();
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);

        }

        public Appointments(decimal uid)
        {
            InitializeComponent();
            textBox1.Text = uid.ToString();
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
            searchByUidOnly(uid);
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void searchByUidOnly(decimal uid)
        {
            /*
             SELECT id, appointment_case.status, appointment_type, date_of_app,time_of_app, patient.f_name, 
             patient.m_name, patient.s_name,employee.f_name, employee.m_name, employee.s_name 
             FROM appointment_case, patient, doctor, employee WHERE p_id = :p_id AND p_id = u_id AND 
             appointment_case.d_id = doctor.d_id AND doctor.eid = employee.eid;
             */
            string query = "  SELECT id, appointment_case.status, appointment_type, date_of_app,time_of_app,"
                +" patient.f_name, patient.m_name, patient.s_name,employee.f_name, employee.m_name, employee.s_name"+
                " FROM appointment_case, patient, doctor, employee WHERE p_id = :p_id AND p_id = u_id "+ 
                "AND appointment_case.d_id = doctor.d_id AND doctor.eid = employee.eid";
            connection.Open();
            command = new OracleCommand(query,connection);
            command.CommandType = CommandType.Text;

            OracleParameter param = command.Parameters.Add(new OracleParameter("p_id", OracleType.Number));
            param.Value = uid;
            param.Direction = ParameterDirection.Input;

            reader = command.ExecuteReader();

            if(!reader.HasRows)
            {
                MessageBox.Show("Appointments Not Found!");
                return;
            }
            listView1.Items.Clear();
            while(reader.Read())
            {
                string[] data = new string[7];
                data[0] = reader.GetDecimal(0).ToString();
                data[6] = reader.GetString(1);
                if (data[6].Equals("I"))
                    data[6] = "Incomplete";
                else if (data[6].Equals("C"))
                    data[6] = "Complete";
                else if (data[6].Equals("E"))
                    data[6] = "Cancelled";
                data[5] = reader.GetString(2);
                if (data[5].Equals("N"))
                    data[5] = "Normal";
                else if (data[5].Equals("E"))
                    data[5] = "Emergency";
                //more data to get from appointment_case

                data[3] = reader.GetDateTime(3).ToShortDateString();
                data[4] = reader.GetDateTime(4).ToShortTimeString();
                data[1] = reader.GetString(5) + " " + reader.GetString(6) + " " + reader.GetString(7);
                data[2] = reader.GetString(8) + " " + reader.GetString(9) + " " + reader.GetString(10);
                ListViewItem item = new ListViewItem(data);
                listView1.Items.Add(item);
            }
            connection.Close();
            reader.Close();
        }
    }
}
