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

            string query = "SELECT * FROM BED WHERE W_ID = :ward_id ORDER BY num";
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
                
                string[] i = new string[2];
                i[0] = reader.GetDecimal(0).ToString();
                if (reader.GetString(1).Equals("A"))
                {
                    comboBox5.Items.Add(reader.GetDecimal(0));
                    i[1] = "Available";
                }
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
            if(string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Enter Patient UID");
                return;
            }

            string query = "SELECT u_id, f_name, s_name FROM patient WHERE u_id = " + textBox1.Text;
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            if(!reader.HasRows)
            {
                MessageBox.Show("Patient is Not Registered.");
                connection.Close();
                return;
            }

            if(comboBox5.SelectedIndex==-1)
            {
                MessageBox.Show("Select an empty Bed");
                connection.Close();
                return;
            }

            reader.Read();
            string name = reader.GetString(1) + " " + reader.GetString(2);
            DialogResult result =
                MessageBox.Show("Allocate " + name + " Bed " + ((decimal)comboBox5.SelectedItem).ToString() + "?", 
                "Confirm this action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(result == DialogResult.No)
            {
                connection.Close();
                return;
            }
            decimal admission_id = 1;
            query = "SELECT id FROM admission ORDER BY id DESC";
            command = new OracleCommand(query, connection);
            reader = command.ExecuteReader();
            if(reader.HasRows)
            {
                reader.Read();
                admission_id = reader.GetDecimal(0);
                admission_id++;
            }

            query = "INSERT INTO admission (ID, ADMIT_TIME, U_ID) VALUES("+admission_id.ToString()+",TO_TIMESTAMP('"+
               dateTimePicker1.Value.ToShortDateString()+" "+dateTimePicker2.Value.ToLongTimeString()+
               "', 'MM/dd/yyyy HH:MI:SS AM'), " + textBox1.Text+")";
            command = new OracleCommand(query, connection);
            
            decimal i = command.ExecuteNonQuery();

            query = "UPDATE bed SET status = 'U', a_id = " + admission_id.ToString() +
                " WHERE num = " + ((decimal)comboBox5.SelectedItem).ToString() + " AND w_id = " + selected_ward_id;
            command = new OracleCommand(query, connection);

            decimal j = command.ExecuteNonQuery();
            connection.Close();
            if (i > 0&&j>0)
            {
                comboBox5.Text = "";
                textBox1.Text = "";
                load_bed(selected_ward_id);
            }
               
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
            reader = command.ExecuteReader();
            decimal bed_no = 1;
            if(reader.HasRows)
            {
                reader.Read();
                bed_no = reader.GetDecimal(0);
                bed_no++;
            }

            query = "INSERT INTO bed (NUM, STATUS, W_ID) VALUES(" + bed_no.ToString() + ", 'A', " + selected_ward_id.ToString() + ")";
            command = new OracleCommand(query, connection);
            decimal t = command.ExecuteNonQuery();
            connection.Close();
            if(t > 0)
                load_bed(selected_ward_id);
        }

        private void button6_Click(object sender, EventArgs e) //remove bed
        {
            var j = listView2.SelectedItems.GetEnumerator();
            if (listView2.SelectedIndices.Count == 0)
                return;
            j.MoveNext();
            string bed_no = ((ListViewItem)j.Current).Text;
            string query = "DELETE bed WHERE num = " + bed_no + " AND w_id = " + selected_ward_id + " AND status = 'A'";
            command = new OracleCommand(query, connection);
            connection.Open();
            decimal t = command.ExecuteNonQuery();
            connection.Close();
            if (t > 0)
                load_bed(selected_ward_id);
            else
                MessageBox.Show("Bed Could Not be Removed. Check if it is Available");

        }

        private void button4_Click(object sender, EventArgs e)//deallocate bed
        {
            var j = listView2.SelectedItems.GetEnumerator();
            if (listView2.SelectedIndices.Count == 0)
                return;
            j.MoveNext();
            string bed_no = ((ListViewItem)j.Current).Text;
            string query = "SELECT a_id FROM bed WHERE num = " + bed_no + " AND w_id = " + selected_ward_id + " AND status = 'U'";
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            if(!reader.HasRows)
            {
                connection.Close();
                return;
            }
            reader.Read();
            decimal admission_id = reader.GetDecimal(0);
            query = "UPDATE admission SET discharge_time  = TO_TIMESTAMP('" + DateTime.Now.ToShortDateString() + " " +
                DateTime.Now.ToLongTimeString() + "', 'MM/dd/yyyy HH:MI:SS PM') " +
                "WHERE id = " + admission_id;
            command = new OracleCommand(query, connection);
            decimal i = command.ExecuteNonQuery();

            query = "UPDATE bed SET a_id  = null, status='A' WHERE num= " + bed_no + " AND w_id = " + selected_ward_id;
            command = new OracleCommand(query, connection);
            decimal j1 = command.ExecuteNonQuery();
            connection.Close();
            if (i>0 && j1>0)
            {
                load_bed(selected_ward_id);
            }
         
        }
    }
}
