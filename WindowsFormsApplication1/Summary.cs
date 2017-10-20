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
        private decimal prescription_id = 0;
        private OracleDataReader reader;
        private List<decimal> meds = new List<decimal>();
        private decimal selected_med_index = -1;
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
            load_prescription_data();
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

        private void load_prescription_data()
        {
            meds.Clear();
            listView1.Items.Clear();
            string query = "SELECT prescription_id FROM appointment_case WHERE id = " + appointment_id.ToString();
            command = new OracleCommand(query, connection);
            command.CommandType = CommandType.Text;
            connection.Open();
            reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                prescription_id = reader.GetDecimal(0);
            }

            query = "SELECT id, med_name FROM medicines WHERE prescription_id = " + prescription_id.ToString();
            command = new OracleCommand(query, connection);
            command.CommandType = CommandType.Text;
            reader = command.ExecuteReader();
            if(!reader.HasRows)
            {
                reader.Close();
                connection.Close();
                return;
            }
            while(reader.Read())
            {
                meds.Add(reader.GetDecimal(0));
                listView1.Items.Add(reader.GetString(1));
            }

            reader.Close();
            connection.Close();
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

        private void button1_Click(object sender, EventArgs e)//add medicine
        {
            if (string.IsNullOrEmpty(textBox1.Text))
                return;
            decimal med_id = 1;
            string query = "SELECT id FROM medicines ORDER BY id desc";
            command = new OracleCommand(query, connection);
            command.CommandType = CommandType.Text;
            connection.Open();
            reader = command.ExecuteReader();
            if(reader.HasRows)
            {
                reader.Read();
                med_id = reader.GetDecimal(0);
                med_id++;
            }

            query = "INSERT INTO medicines VALUES(:med_name, :ps_id, :id)";
            command = new OracleCommand(query, connection);
            command.CommandType = CommandType.Text;

            OracleParameter param = command.Parameters.Add(new OracleParameter("med_name", OracleType.VarChar));
            param.Direction = ParameterDirection.Input;
            param.Value = textBox1.Text;

            param = command.Parameters.Add(new OracleParameter("ps_id", OracleType.Number));
            param.Direction = ParameterDirection.Input;
            param.Value = prescription_id;

            param = command.Parameters.Add(new OracleParameter("id", OracleType.Number));
            param.Direction = ParameterDirection.Input;
            param.Value = med_id;

            decimal t = command.ExecuteNonQuery();
            if(t>0)
            {
                connection.Close();
                textBox1.Text = "";
                //refresh list
                load_prescription_data();
            }
            
            connection.Close();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
                return;
            selected_med_index = e.ItemIndex;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selected_med_index < 0)
                return;
            var j = meds.GetEnumerator();
            for (int i = 0; i <= selected_med_index; i++)
                j.MoveNext();
            string query = "DELETE medicines WHERE id = " + j.Current;
            command = new OracleCommand(query, connection);
            command.CommandType = CommandType.Text;
            connection.Open();
            decimal t = command.ExecuteNonQuery();
            connection.Close();
            if (t > 0)
                load_prescription_data();
        }
    }
}
