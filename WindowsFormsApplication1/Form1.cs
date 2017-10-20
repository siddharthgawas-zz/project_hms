using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Data.OracleClient;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private OracleConnection connection;
        private OracleCommand command;
        private OracleDataReader reader;
        public Form1()
        {
            InitializeComponent();
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(textBox1.Text)|| string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Please Enter Valid Credentials");
                return;
            }

            string query = "SELECT * FROM user_table WHERE username = '" + textBox1.Text+"'";
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            if(!reader.HasRows)
            {
                MessageBox.Show("User does not exist");
                connection.Close();
                return;
            }
            reader.Read();
            if(textBox2.Text.Equals(reader.GetString(1)))
            {
                Form2 form2 = new Form2();
                form2.Show();
                connection.Close();
                this.Dispose(false);
            }
            else
            {
                MessageBox.Show("Incorrect Password");
                textBox1.Text = "";
                textBox2.Text = "";  
            }
            connection.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
        }
    }
}
