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
        private List<decimal> d_id;
        private OracleConnection connection;
        private OracleCommand command;
        private OracleDataReader reader;
        private bool add_flag = true;
        private decimal selected_id = -1;

        public Appointments()
        {
            InitializeComponent();
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
            button3.Enabled = false;
            loadDoctors();
            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker2.Value = DateTime.Now;
        }

        public Appointments(decimal uid)
        {
            InitializeComponent();
            textBox1.Text = uid.ToString();
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
            searchByUidOnly(uid);
            button3.Enabled = true;
            button2.Enabled = false;
            loadDoctors();
            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker2.Value = DateTime.Now;
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)//modify
        {
            if (selected_id == -1)
                return;

            if(comboBox1.SelectedIndex<=0 && comboBox2.SelectedIndex <=0 && comboBox3.SelectedIndex<=0&&
                !checkBox1.Checked)
            {
                MessageBox.Show("Change Atleast One Detail!");
                return;
            }
            string query = "UPDATE appointment_case SET ";
            bool first = true;
            if(comboBox1.SelectedIndex >0)
            {
                var iter = d_id.GetEnumerator();
                for (int i = 0; i <= comboBox1.SelectedIndex; i++)
                    iter.MoveNext();
                query += " d_id = " + iter.Current;
                first = false;
            }

            if(checkBox1.Checked)
            {
                if (first)
                {
                    query += " date_of_app = TO_DATE('" + dateTimePicker1.Value.ToShortDateString() + "', 'MM/dd/yyyy')";
                    first = false;
                }
                else
                    query += ", date_of_app = TO_DATE('" + dateTimePicker1.Value.ToShortDateString() + "', 'MM/dd/yyyy')";
                query += ", time_of_app = TO_TIMESTAMP('" + dateTimePicker2.Value.ToString()+ "', 'MM/dd/yyyy HH:MI:SS PM')";
            }

            if(comboBox2.SelectedIndex>0)
            {
                if (!first)
                    query += ", ";
                query += " appointment_type =  ";
                first = false;
                if (comboBox2.SelectedIndex == 1)
                    query += "'N'";
                else if (comboBox2.SelectedIndex == 2)
                    query += "'E'";
            }

            if(comboBox3.SelectedIndex > 0)
            {
                if (!first)
                    query += ", ";
                query += " status =  ";
                first = false;
                if (comboBox3.SelectedIndex == 1)
                    query += "'C'";
                else if (comboBox3.SelectedIndex == 2)
                    query += "'I'";
                else if (comboBox3.SelectedIndex == 3)
                    query += "'E'";
            }
            query += " WHERE id = " + selected_id.ToString();
            command = new OracleCommand(query, connection);
            connection.Open();
            decimal t = command.ExecuteNonQuery();
            if (t > 0)
                MessageBox.Show("Update Successful");
            else
                MessageBox.Show("Update Failed");
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            comboBox3.SelectedIndex = -1;
            checkBox1.Checked = false;
            connection.Close();
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
                connection.Close();
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

        private void loadDoctors()
        {
            string query = "SELECT d_id,f_name, s_name, specialization FROM doctor, employee, status_table WHERE doctor.eid = employee.eid AND "
                + " status_table.id = employee.status AND status_table.status='Working'";
            d_id = new List<decimal>();
            d_id.Add(-1);
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            while(reader.Read())
            {
                d_id.Add(reader.GetDecimal(0));
                string s = reader.GetString(1) + " " + reader.GetString(2) + "-" + reader.GetString(3);
                comboBox1.Items.Add(s);
            }
            connection.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                dateTimePicker1.Enabled = true;
                dateTimePicker2.Enabled = true;
            }

            else
            {
                dateTimePicker1.Enabled = false;
                dateTimePicker2.Enabled = false ;
            }
        }

        private void button4_Click(object sender, EventArgs e) //clear
        {
            this.Dispose(false);
            Appointments app = new Appointments();
            app.Show();
        }

        private void button2_Click(object sender, EventArgs e)//add appointment
        {
            if(string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Enter Patient UID");
                return;
            }

            decimal uid = decimal.Parse(textBox1.Text);
            string query = "SELECT u_id FROM patient WHERE u_id = " + uid;
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            //patient record does not exist.
            if(!reader.HasRows)
            {
                //open patient form
                Form3 f = new Form3(uid);
                f.Show();
                MessageBox.Show("Patient Not Found. Please add patient details");
                connection.Close();
                return;
            }

            bool condition = (comboBox1.SelectedIndex <= 0) || (comboBox2.SelectedIndex <= 0) || (comboBox3.SelectedIndex <= 0)||!checkBox1.Checked;
            if(condition)
            {
                MessageBox.Show("Please enter all the details");
                connection.Close();
                return;
            }

            decimal id = 1;
            query = "SELECT id FROM appointment_case ORDER BY id DESC";
            command = new OracleCommand(query, connection);
            reader = command.ExecuteReader();
            if(reader.HasRows)
            {
                reader.Read();
                id = reader.GetDecimal(0);
                id++;
            }

            decimal new_prescription = 1;
            query = "SELECT * FROM prescription ORDER BY prescription_id DESC";
            command = new OracleCommand(query, connection);
            reader = command.ExecuteReader();
            if(reader.HasRows)
            {
                reader.Read();
                new_prescription = reader.GetDecimal(0);
                new_prescription++;
            }

            query = "INSERT INTO prescription VALUES(" + new_prescription + ")";
            command = new OracleCommand(query, connection);
            command.ExecuteNonQuery();

            query = "INSERT INTO appointment_case (id, status, appointment_type, date_of_app, time_of_app, "
                + "d_id, p_id, prescription_id) VALUES( :id, :status, :type, TO_DATE('"+dateTimePicker1.Value.ToShortDateString()
                +"','MM/dd/yyyy'), :app_time, :d_id, :p_id, :prescription_id)";
            command = new OracleCommand(query, connection);
            OracleParameter p = command.Parameters.Add(new OracleParameter("id", OracleType.Number));
            p.Direction = ParameterDirection.Input;
            p.Value = id;

            p = command.Parameters.Add(new OracleParameter("status", OracleType.VarChar));
            p.Direction = ParameterDirection.Input;
            if (comboBox3.SelectedIndex == 1)
                p.Value = "C"; //completed
            else if (comboBox3.SelectedIndex == 2)
                p.Value = "I"; //incomplete
            else if (comboBox3.SelectedIndex == 3)
                p.Value = "E"; //cancelled 

            p = command.Parameters.Add(new OracleParameter("type", OracleType.VarChar));
            p.Direction = ParameterDirection.Input;
            if (comboBox2.SelectedIndex == 1)
                p.Value = "N"; //normal
            else if (comboBox2.SelectedIndex == 2)
                p.Value = "E"; //emergency

           /* p = command.Parameters.Add(new OracleParameter("app_date", OracleType.DateTime));
            p.Direction = ParameterDirection.Input;
            p.Value = dateTimePicker1.Value;*/

            p = command.Parameters.Add(new OracleParameter("app_time", OracleType.DateTime));
            p.Direction = ParameterDirection.Input;
            p.Value = dateTimePicker2.Value;

            /*
            p = command.Parameters.Add(new OracleParameter("summary", OracleType.VarChar));
            p.Direction = ParameterDirection.Input;
            p.Value = DBNull.Value;


            p = command.Parameters.Add(new OracleParameter("pres", OracleType.Number));
            p.Direction = ParameterDirection.Input;
            p.Value = DBNull.Value;
            */
            p = command.Parameters.Add(new OracleParameter("prescription_id", OracleType.Number));
            p.Direction = ParameterDirection.Input;
            p.Value = new_prescription;

            p = command.Parameters.Add(new OracleParameter("d_id", OracleType.Number));
            p.Direction = ParameterDirection.Input;
            var iter = d_id.GetEnumerator();
            for (int i = 0; i <= comboBox1.SelectedIndex; i++)
                iter.MoveNext();
            p.Value = iter.Current;

            p = command.Parameters.Add(new OracleParameter("p_id", OracleType.Number));
            p.Direction = ParameterDirection.Input;
            p.Value = decimal.Parse(textBox1.Text);

            command.CommandType = CommandType.Text;
            decimal j = command.ExecuteNonQuery();
            if (j >= 0)
                MessageBox.Show("Record Added Successfully");
            else
                MessageBox.Show("Some Error Occured");
            connection.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool condition = (!checkBox1.Checked) && string.IsNullOrEmpty(textBox1.Text) &&
                (comboBox1.SelectedIndex <= 0) && (comboBox2.SelectedIndex <= 0) && (comboBox3.SelectedIndex <= 0);

            if(condition)
            {
                MessageBox.Show("Choose Atleast One Filter");
                return;
            }

            string query = "  SELECT id, appointment_case.status, appointment_type, date_of_app,time_of_app,"
                + " patient.f_name, patient.m_name, patient.s_name,employee.f_name, employee.m_name, employee.s_name" +
                " FROM appointment_case, patient, doctor, employee WHERE p_id = u_id " +
                "AND appointment_case.d_id = doctor.d_id AND doctor.eid = employee.eid ";

            if(!string.IsNullOrEmpty(textBox1.Text))
                query += " AND p_id = " + textBox1.Text;
            if(comboBox1.SelectedIndex>0)
            {
                var iter = d_id.GetEnumerator();
                for (int i = 0; i <= comboBox1.SelectedIndex; i++)
                    iter.MoveNext();
                query += " AND appointment_case.d_id = " + iter.Current;
            }
            if(comboBox2.SelectedIndex>0)
            {
                if(comboBox2.SelectedIndex == 1)
                    query += " AND appointment_type = 'N'";
                if (comboBox2.SelectedIndex == 2)
                    query += " AND appointment_type = 'E'";
            }
            if (comboBox3.SelectedIndex > 0)
            {
                if (comboBox3.SelectedIndex == 1)
                    query += " AND appointment_case.status = 'C'";
                if (comboBox3.SelectedIndex == 2)
                    query += " AND appointment_case.status = 'I'";
                if (comboBox3.SelectedIndex == 3)
                    query += " AND appointment_case.status = 'E'";

            }
            if(checkBox1.Checked)
                query += " AND date_of_app = TO_DATE('" + dateTimePicker1.Value.ToShortDateString() + "', 'MM/dd/yyyy')";

            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            bool flag = reader.HasRows;
            if(!flag)
                MessageBox.Show("No Appointments Found!");
            else
            {
                listView1.Items.Clear();
                while (reader.Read())
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
               
            }
            connection.Close();
            reader.Close();
        }//search appointment...

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            button3.Enabled = true;
            button2.Enabled = false;
            textBox1.Enabled = false;
            if (!e.IsSelected)
                return;
            selected_id = decimal.Parse(e.Item.Text);

        }

        private void button5_Click(object sender, EventArgs e)//view edit summary
        {
            if(selected_id != -1)
            {
                Summary s = new Summary(selected_id);
                s.Show();
            }
        }
    }
}
