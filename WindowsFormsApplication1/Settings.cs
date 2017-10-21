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
    public partial class Settings : Form
    {
        private OracleConnection connection;
        private OracleCommand command;
        private OracleDataReader reader;
        private List<decimal> avail_users = new List<decimal>();
        public Settings()
        {
            InitializeComponent();
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
            loadCurrentUsers();
            loadComboBox();
            load_hospital_details(true);
        }

        private void loadCurrentUsers()
        {
            string query = "SELECT user_table.eid, username FROM user_table, employee, status_table WHERE employee.status = status_table.id"+
                " AND status_table.status='Working' AND employee.eid=user_table.eid ORDER BY user_table.eid ASC";
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            listView1.Items.Clear();
            while (reader.Read())
            {
                ListViewItem item = new ListViewItem(new string[] { reader.GetDecimal(0).ToString(), reader.GetString(1) });
                listView1.Items.Add(item);
            }
            connection.Close();
        }

        private void loadComboBox()
        {
            string query = "SELECT employee.eid, f_name, s_name FROM employee, status_table WHERE employee.status = status_table.id AND status_table.status =" +
                " 'Working' AND employee.eid NOT IN (SELECT eid FROM user_table) ";
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            comboBox1.Items.Clear();
            avail_users.Clear();
            while (reader.Read())
            {
                comboBox1.Items.Add(reader.GetString(1 )+ " " + reader.GetString(2));
                avail_users.Add(reader.GetDecimal(0));
            }
            connection.Close();
        }

        private bool load_hospital_details(bool updateUi)
        {
            string query = "Select * FROM hospital WHERE id = 1";
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            
            if (reader.HasRows)
            {
                if (!updateUi)
                {
                    connection.Close();
                    return true;
                }
                reader.Read();
                textBox1.Text = reader.GetString(0);
                textBox3.Text = reader.GetDecimal(2).ToString();
                richTextBox1.Text = reader.GetString(1).ToString();
                connection.Close();
                return true;
            }
            connection.Close();
            return false;
        }
    
        private void button1_Click(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex <0 || string.IsNullOrEmpty(textBox2.Text)
                || string.IsNullOrEmpty(textBox4.Text) || string.IsNullOrEmpty(textBox5.Text))
            {
                MessageBox.Show("Please enter all user details");
                return;
            }
            string query = "SELECT eid FROM user_table WHERE username = '" + textBox2.Text+"'";
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            if(reader.HasRows)
            {
                connection.Close();
                MessageBox.Show("Username is already taken");
                textBox2.Text = "";
                textBox4.Text = "";
                textBox5.Text = "";
                return;
            }

            if(!textBox4.Text.Equals(textBox5.Text))
            {
                connection.Close();
                MessageBox.Show("Password Does not match");
                textBox4.Text = "";
                textBox5.Text = "";
                return;
            }
            var j = avail_users.GetEnumerator();
            for (int i = 0; i <= comboBox1.SelectedIndex; i++)
                j.MoveNext();
            query = "INSERT INTO user_table VALUES('" + textBox2.Text + "','" + textBox4.Text + "'," + j.Current+")";
            command = new OracleCommand(query, connection);
            int k = command.ExecuteNonQuery();
            connection.Close();
            if(k>0)
            {
                MessageBox.Show("User Added!");
                comboBox1.Text = "";
                comboBox1.SelectedIndex = -1;
                textBox2.Text = "";
                textBox4.Text = "";
                textBox5.Text = "";
                loadComboBox();
                loadCurrentUsers();
            }
            else
                MessageBox.Show("Some error has occured");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedIndices.Count==0)
            {
                MessageBox.Show("Please select an user to remove");
                return;
            }
            var user = listView1.SelectedItems.GetEnumerator();
            user.MoveNext();
            string eid =((ListViewItem)user.Current).Text;

            string query = "DELETE user_table WHERE eid = " + eid;
            command = new OracleCommand(query, connection);
            connection.Open();
            int i = command.ExecuteNonQuery();
            connection.Close();
            if(i>0)
            {
                loadComboBox();
                loadCurrentUsers();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool has_record = load_hospital_details(false);
            //update
            if(has_record)
            {
                if(string.IsNullOrEmpty(textBox1.Text)||string.IsNullOrEmpty(textBox3.Text)||string.IsNullOrEmpty(richTextBox1.Text))
                {
                    MessageBox.Show("Do not leave hospital details empty");
                    return;
                }
                string query = "UPDATE hospital SET name = '" + textBox1.Text + "', " +
                    "address = '" + richTextBox1.Text + "',contact_number=" + textBox3.Text
                    + " WHERE id = 1";
                command = new OracleCommand(query, connection);
            }
            //insert
            else
            {
                if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox3.Text) || string.IsNullOrEmpty(richTextBox1.Text))
                {
                    MessageBox.Show("Do not leave hospital details empty");
                    return;
                }
                string query = "INSERT INTO hospital VALUES('" + textBox1.Text + "', " +
                    "'" + richTextBox1.Text + "'," + textBox3.Text + ",1)";
                command = new OracleCommand(query, connection);
            }
          
            connection.Open();
            int i = command.ExecuteNonQuery();
            if (i > 0)
            {
                connection.Close();
                load_hospital_details(true);
                MessageBox.Show("Hospital Details Updated");
            }
            connection.Close();
        }
    }
}
