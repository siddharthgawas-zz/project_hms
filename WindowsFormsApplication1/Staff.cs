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
    public partial class Staff : Form
    {
        private OracleConnection connection;
        private OracleCommand command;
        private OracleDataReader reader;

        private List<decimal> type_id = new List<decimal>();
        private List<decimal> status_id = new List<decimal>();

        public Staff()
        {
            InitializeComponent();
            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker1.Enabled = false;
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
            loadComboBox();
            loadWholeList();
        }

        private void button4_Click(object sender, EventArgs e) //modify button
        {
            if(listView1.SelectedItems.Count > 0)
            {
                var j = listView1.SelectedItems.GetEnumerator();
                j.MoveNext();
                decimal id = decimal.Parse(((ListViewItem)j.Current).Text);
                StaffAddModify s = new StaffAddModify(id);
                s.Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)//add employee
        {
            StaffAddModify s = new StaffAddModify();
            s.Show();
        }

        private void loadComboBox()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox2.Text = "";
            comboBox1.Text = "";
            comboBox1.Items.Add("");
            comboBox2.Items.Add("");
            type_id.Clear();
            status_id.Clear();
            type_id.Add(-1);
            status_id.Add(-1);

            string query = "SELECT * FROM type_table ORDER BY id";
            command = new OracleCommand(query,connection);
            connection.Open();
            reader = command.ExecuteReader();
            while(reader.Read())
            {
                type_id.Add(reader.GetDecimal(0));
                comboBox1.Items.Add(reader.GetString(1));
            }

            query = "SELECT * FROM status_table ORDER BY id";
            command = new OracleCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                status_id.Add(reader.GetDecimal(0));
                comboBox2.Items.Add(reader.GetString(1));
            }
            connection.Close();
        }

        private void loadWholeList()
        {

            string query = "SELECT eid,f_name,m_name,s_name,gender,dob,email_id,address,contact_no,type_table.type,status_table.status "+
                "FROM employee,status_table,type_table WHERE employee.status = status_table.id AND employee.employee_type = type_table.id ORDER BY eid";
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            listView1.Items.Clear();
            while (reader.Read())
            {
                string[] record = new string[] {
                    reader.GetDecimal(0).ToString(),
                    reader.GetString(1) + " "+ reader.GetString(2) +" " +reader.GetString(3),
                    reader.GetString(4),
                    reader.GetDateTime(5).ToShortDateString(),
                    "-","-","-",
                    reader.GetString(9),
                    reader.GetString(10)
                };

                if (!DBNull.Value.Equals(reader.GetValue(6)))
                    record[4] = reader.GetString(6);
                if (!DBNull.Value.Equals(reader.GetValue(7)))
                    record[5] = reader.GetString(7);
                if (!DBNull.Value.Equals(reader.GetValue(8)))
                    record[6] = reader.GetDecimal(8).ToString();
                ListViewItem item = new ListViewItem(record);
                listView1.Items.Add(item);
            }

            connection.Close();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                dateTimePicker1.Enabled = true;
            else
                dateTimePicker1.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            checkBox1.Checked = false;
            comboBox3.Text = "";
            comboBox3.SelectedIndex = -1;
            loadComboBox();
            loadWholeList();
        }

        private void button1_Click(object sender, EventArgs e)//search
        {
            
            bool filter_selected= false;
            string query = "SELECT eid,f_name,m_name,s_name,gender,dob,email_id,address,contact_no,type_table.type,status_table.status  " +
                "FROM employee,status_table,type_table WHERE employee.status = status_table.id AND employee.employee_type = type_table.id " ;
            if(!string.IsNullOrEmpty(textBox1.Text))
            {
                query += " AND f_name = '" + textBox1.Text+"' ";
                filter_selected = true;
            }
            if (!string.IsNullOrEmpty(textBox2.Text))
            {
                query += " AND m_name = '" + textBox2.Text+"' ";
                filter_selected = true;
            }
            if (!string.IsNullOrEmpty(textBox3.Text))
            {
                query += " AND s_name = '" + textBox3.Text+"' ";
                filter_selected = true;
            }
            if (!string.IsNullOrEmpty(textBox4.Text))
            {
                query += " AND eid = " + textBox4.Text;
                filter_selected = true;
            }
            if (!string.IsNullOrEmpty(textBox5.Text))
            {
                query += " AND contact_no = " + textBox5.Text;
                filter_selected = true;
            }
            if (!string.IsNullOrEmpty(textBox6.Text))
            {
                query += " AND address = '" + textBox6.Text + "' ";
                filter_selected = true;
            }
            if (!string.IsNullOrEmpty(textBox7.Text))
            {
                query += " AND email_id = '" + textBox7.Text + "' ";
                filter_selected = true;
            }

            if(comboBox1.SelectedIndex >0 )
            {
                var j = type_id.GetEnumerator();
                for (int i = 0; i <= comboBox1.SelectedIndex; i++)
                    j.MoveNext();
                query += " AND employee.employee_type = " + j.Current;
                filter_selected = true;
            }
            if (comboBox2.SelectedIndex > 0)
            {
                var j = status_id.GetEnumerator();
                for (int i = 0; i <= comboBox2.SelectedIndex; i++)
                    j.MoveNext();
                query += " AND employee.status = " + j.Current;
                filter_selected = true;
            }
            if(comboBox3.SelectedIndex>0)
            {
                query += " AND gender = '" + comboBox3.GetItemText(comboBox3.SelectedItem).Substring(0,1) +"' ";
                filter_selected = true;
            }
            if(checkBox1.Checked)
            {
                query += " AND dob = TO_DATE('" + dateTimePicker1.Value.ToShortDateString() + "', 'MM/dd/yyyy')";
                filter_selected = true;
            }

            if(!filter_selected)
            {
                MessageBox.Show("Please Select Atleast One Filter");
                return;
            }
            query += " ORDER BY eid ASC";
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            listView1.Items.Clear();
            if (!reader.HasRows)
                MessageBox.Show("No Match Found");
            while (reader.Read())
            {
                string[] record = new string[] {
                    reader.GetDecimal(0).ToString(),
                    reader.GetString(1) + " "+ reader.GetString(2) +" " +reader.GetString(3),
                    reader.GetString(4),
                    reader.GetDateTime(5).ToShortDateString(),
                    "-","-","-",
                    reader.GetString(9),
                    reader.GetString(10)
                };

                if (!DBNull.Value.Equals(reader.GetValue(6)))
                    record[4] = reader.GetString(6);
                if (!DBNull.Value.Equals(reader.GetValue(7)))
                    record[5] = reader.GetString(7);
                if (!DBNull.Value.Equals(reader.GetValue(8)))
                    record[6] = reader.GetDecimal(8).ToString();
                ListViewItem item = new ListViewItem(record);
                listView1.Items.Add(item);
            }
            listView1.SelectedIndices.Clear();
            connection.Close();

        }
    }
}
