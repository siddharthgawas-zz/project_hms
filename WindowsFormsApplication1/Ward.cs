using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.OracleClient;
namespace WindowsFormsApplication1
{
    public partial class Ward : Form
    {
        private OracleConnection connection = new OracleConnection();
        private OracleCommand command;
        private OracleDataReader reader;
        private List<decimal> wards = new List<decimal>();
        private List<decimal> emp_in_ward = new List<decimal>();
        private List<decimal> emp_avail = new List<decimal>();

        private decimal selected_ward_id = -1;
        public Ward()
        {
            InitializeComponent();
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
            dateTimePicker1.Value = DateTime.Today;
            dateTimePicker2.Value = DateTime.Now;
            load_wards();
        }

        private void load_wards()
        {
            wards.Clear();
            string query = "SELECT  * FROM WARD";
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            while(reader.Read())
            {
                wards.Add(reader.GetDecimal(0));
                comboBox1.Items.Add(reader.GetString(1));
            }
            connection.Close();
        }

        private void load_employees_in_ward(decimal ward_id)//load listview
        {
            string query = "SELECT employee.eid, f_name, s_name, type_table.type FROM employee, nurse, type_table WHERE nurse.ward_id = :ward_id AND nurse.eid = employee.eid AND employee.employee_type = type_table.id";
            command = new OracleCommand(query, connection);
            connection.Open();
            OracleParameter p = command.Parameters.Add(new OracleParameter("ward_id", OracleType.Number));
            p.Direction = ParameterDirection.Input;
            p.Value = ward_id;
            reader = command.ExecuteReader();
            emp_in_ward.Clear();
            listView1.Items.Clear();
            while(reader.Read())
            {
                emp_in_ward.Add(reader.GetDecimal(0));
                ListViewItem item = new ListViewItem(new string[] { reader.GetString(1) + " " + reader.GetString(2),
                reader.GetString(3)});
                listView1.Items.Add(item);
            }

            query = "SELECT employee.eid, f_name, s_name, type_table.type FROM employee, janitor, type_table WHERE janitor.ward_id = :ward_id AND janitor.eid= employee.eid  AND employee.employee_type = type_table.id";
            command = new OracleCommand(query, connection);
             p = command.Parameters.Add(new OracleParameter("ward_id", OracleType.Number));
            p.Direction = ParameterDirection.Input;
            p.Value = ward_id;
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                emp_in_ward.Add(reader.GetDecimal(0));
                ListViewItem item = new ListViewItem(new string[] { reader.GetString(1) + " " + reader.GetString(2),
                reader.GetString(3)});
                listView1.Items.Add(item);
            }
            connection.Close();
        }


        private void load_all_employees(decimal ward_id)//load combo box
        {
            string query = "SELECT employee.eid, f_name, s_name, type_table.type FROM employee, nurse, type_table WHERE nurse.ward_id != :ward_id AND nurse.eid = employee.eid AND employee.employee_type = type_table.id";

            command = new OracleCommand(query, connection);
            connection.Open();
            OracleParameter p = command.Parameters.Add(new OracleParameter("ward_id", OracleType.Number));
            p.Direction = ParameterDirection.Input;
            p.Value = ward_id;
            reader = command.ExecuteReader();
            emp_avail.Clear();
            comboBox3.Items.Clear();
            comboBox3.Text = "";
            while (reader.Read())
            {
                emp_avail.Add(reader.GetDecimal(0));
                comboBox3.Items.Add(reader.GetString(1) + " " + reader.GetString(2) + " - " + reader.GetString(3));
            }

            query = "SELECT employee.eid, f_name, s_name, type_table.type FROM employee, janitor, type_table WHERE janitor.ward_id != :ward_id AND janitor.eid= employee.eid  AND employee.employee_type = type_table.id";

            command = new OracleCommand(query, connection);
            p = command.Parameters.Add(new OracleParameter("ward_id", OracleType.Number));
            p.Direction = ParameterDirection.Input;
            p.Value = ward_id;
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                emp_avail.Add(reader.GetDecimal(0));
                comboBox3.Items.Add(reader.GetString(1) + " " + reader.GetString(2) + " - " + reader.GetString(3));
            }
            connection.Close();

        }

        private void load_bed(decimal ward_id)//comboBox
        {

            string query = "SELECT * FROM BED WHERE W_ID = :ward_id AND STATUS = 'A'";
            command = new OracleCommand(query, connection);
            connection.Open();
            OracleParameter p = command.Parameters.Add(new OracleParameter("ward_id", OracleType.Number));
            p.Direction = ParameterDirection.Input;
            p.Value = ward_id;
            reader = command.ExecuteReader();
            comboBox5.Items.Clear();
            listView2.Items.Clear();
            while (reader.Read())
            { 
                comboBox5.Items.Add(reader.GetDecimal(0));
                string[] i = new string[2];
                i[0] = reader.GetDecimal(0).ToString();
                if (reader.GetString(1).Equals("A"))
                    i[1] = "Available";
                else if (reader.GetString(1).Equals("U"))
                    i[1] = "Unavailable";
                ListViewItem item = new ListViewItem(i);
                listView2.Items.Add(item);
            }
            connection.Close();
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var j = wards.GetEnumerator();
            int k = comboBox1.SelectedIndex;
            for (int i = 0; i <= k; i++)
                j.MoveNext();
            selected_ward_id = j.Current;
            panel1.Enabled = true;
            load_employees_in_ward(selected_ward_id);
            load_all_employees(selected_ward_id);
            load_bed(selected_ward_id);
        }

        private void label4_Click(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)//allocate bed
        {
            //on succesfull load_bed()
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex < 0)
                return;

            var j = emp_avail.GetEnumerator();
            for (int i = 0; i <= comboBox3.SelectedIndex; i++)
                j.MoveNext();
            string query = "UPDATE nurse SET WARD_ID = "+selected_ward_id+" WHERE eid = " + j.Current;
            command = new OracleCommand(query, connection);
            command.CommandType = CommandType.Text;
            connection.Open();
            int t = command.ExecuteNonQuery();
            connection.Close();
           
            query = "UPDATE janitor SET WARD_ID = " + selected_ward_id + " WHERE eid = " + j.Current;
            command = new OracleCommand(query, connection);
            command.CommandType = CommandType.Text;
            connection.Open();
            int f = command.ExecuteNonQuery();
            connection.Close();
            if (t > 0 || f>0)
            {
                load_employees_in_ward(selected_ward_id);
                load_all_employees(selected_ward_id);
            }
        } //add to ward

        private void button5_Click(object sender, EventArgs e) // add beds
        {
            string query = "SELECT num FROM bed WHERE w_id = "+ selected_ward_id+" ORDER BY num DESC";
            command = new OracleCommand(query, connection);
            connection.Open();
            
        }
    }
}
