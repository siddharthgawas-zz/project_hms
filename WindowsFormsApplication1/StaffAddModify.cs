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
using System.IO;
namespace WindowsFormsApplication1
{
    public partial class StaffAddModify : Form
    {
        private OracleConnection connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
        private decimal id = 1000;
        private OracleCommand command;
        private OracleDataReader reader;
        private List<decimal> type_id = new List<decimal>();
        private List<decimal> status_id = new List<decimal>();
        private List<decimal> ward_id = new List<decimal>();
        private List<decimal> nurse_type = new List<decimal>();
        private string imagePath = null;
        private bool updateFlag = false;
        public StaffAddModify()
        {
            InitializeComponent();
            loadComboBoxes();
            loadEmployeeId();
            updateFlag = false;
        }
        public StaffAddModify(decimal i)
        {
            id = i;
            InitializeComponent();
            loadComboBoxes();
            loadEmployee();
            updateFlag = true;
        }


        private void StaffAddModify_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex == 0)
            {
                groupBox2.Enabled = false;
                groupBox3.Enabled = false;
                groupBox4.Enabled = false;
            }
            else if(comboBox1.SelectedIndex== 1)
            {
                groupBox2.Enabled = true;
                groupBox3.Enabled = false;
                groupBox4.Enabled = false;
            }
            else if (comboBox1.SelectedIndex == 2)
            {
                groupBox2.Enabled = false;
                groupBox3.Enabled = true;
                groupBox4.Enabled = false;
            }
            else if (comboBox1.SelectedIndex == 3)
            {
                groupBox2.Enabled = false;
                groupBox3.Enabled = false;
                groupBox4.Enabled = true;
            }

        }

        private void loadComboBoxes()
        {
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox4.Items.Clear();
            comboBox6.Items.Clear();
            comboBox5.Items.Clear();

            comboBox2.Text = "";
            comboBox1.Text = "";
            comboBox4.Text = "";
            comboBox6.Text = "";
            comboBox5.Text = "";

            type_id.Clear();
            status_id.Clear();
            ward_id.Clear();

            string query = "SELECT * FROM type_table ORDER BY id";
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            while (reader.Read())
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

            query = "SELECT * FROM ward ORDER BY id";
            command = new OracleCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                ward_id.Add(reader.GetDecimal(0));
                comboBox4.Items.Add(reader.GetString(1));
                comboBox6.Items.Add(reader.GetString(1));
            }

            query = "SELECT * FROM nurse_type ORDER BY id";
            command = new OracleCommand(query, connection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                nurse_type.Add(reader.GetDecimal(0));
                comboBox5.Items.Add(reader.GetString(1));
            }
            connection.Close();

            comboBox1.SelectedIndex = 1;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 1;
            comboBox6.SelectedIndex = 0;
            
        }
        private void loadEmployeeId()
        {
            string query = "SELECT eid FROM employee ORDER BY eid DESC";
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            if(reader.HasRows)
            {
                reader.Read();
                id = reader.GetDecimal(0);
                id++;
            }
            textBox4.Text = id.ToString();
            connection.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Dispose(false);
        }

        private byte[] getProfilePicture()
        {
            if (imagePath == null)
                return null;
            FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            byte[] imageBytes = new byte[fs.Length];
            fs.Read(imageBytes, 0, System.Convert.ToInt32(fs.Length));
            return imageBytes;
        }

        private void button1_Click(object sender, EventArgs e) //save button
        {
            if (updateFlag)
            {
                updateEmployee();
            }
            else
            {
                addEmployee();
               
            }
        }
        private void addEmployee()
        {
            bool condition = string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text) ||
              string.IsNullOrEmpty(textBox3.Text) || string.IsNullOrEmpty(richTextBox1.Text) ||
              string.IsNullOrEmpty(textBox5.Text) ||
              (groupBox2.Enabled && (string.IsNullOrEmpty(textBox6.Text) || string.IsNullOrEmpty(textBox8.Text)));
            if (condition)
            {
                MessageBox.Show("Please Enter all the mandatory details");
                return;
            }

            string query = "INSERT INTO employee (EID, F_NAME,	M_NAME,	S_NAME,	GENDER,	DOB,	EMAIL_ID	,ADDRESS,	CONTACT_NO,	STATUS,	EMPLOYEE_TYPE,DISPLAY_PIC) " +
                "VALUES(:eid, :f_name, :m_name, :s_name, :gender, :dob, :email_id, :address, :contact, :status,:employee_type,:display_pic)";
            command = new OracleCommand(query, connection);
            connection.Open();
            OracleParameter p = new OracleParameter("eid", OracleType.Number);
            p.Direction = ParameterDirection.Input;
            p.Value = decimal.Parse(textBox4.Text);
            command.Parameters.Add(p);

            p = new OracleParameter("f_name", OracleType.VarChar);
            p.Direction = ParameterDirection.Input;
            p.Value = textBox1.Text;
            command.Parameters.Add(p);
            p = new OracleParameter("m_name", OracleType.VarChar);
            p.Direction = ParameterDirection.Input;
            p.Value = textBox2.Text;
            command.Parameters.Add(p);
            p = new OracleParameter("s_name", OracleType.VarChar);
            p.Direction = ParameterDirection.Input;
            p.Value = textBox3.Text;
            command.Parameters.Add(p);
            p = new OracleParameter("gender", OracleType.VarChar);
            p.Direction = ParameterDirection.Input;
            p.Value = comboBox3.GetItemText(comboBox3.SelectedItem).Substring(0, 1);
            command.Parameters.Add(p);
            p = new OracleParameter("dob", OracleType.DateTime);
            p.Direction = ParameterDirection.Input;
            p.Value = dateTimePicker1.Value;
            command.Parameters.Add(p);
            p = new OracleParameter("email_id", OracleType.VarChar);
            p.Direction = ParameterDirection.Input;
            if (string.IsNullOrEmpty(textBox7.Text))
                p.Value = DBNull.Value;
            else
                p.Value = textBox7.Text;

            command.Parameters.Add(p);
            p = new OracleParameter("address", OracleType.VarChar);
            p.Direction = ParameterDirection.Input;
            p.Value = richTextBox1.Text;
            command.Parameters.Add(p);
            p = new OracleParameter("contact", OracleType.Number);
            p.Direction = ParameterDirection.Input;
            p.Value = decimal.Parse(textBox5.Text);
            command.Parameters.Add(p);
            p = new OracleParameter("status", OracleType.Number);
            p.Direction = ParameterDirection.Input;
            var j = status_id.GetEnumerator();
            for (int i = 0; i <= comboBox2.SelectedIndex; i++)
                j.MoveNext();
            p.Value = j.Current;
            command.Parameters.Add(p);
            p = new OracleParameter("employee_type", OracleType.Number);
            p.Direction = ParameterDirection.Input;
            j = type_id.GetEnumerator();
            for (int i = 0; i <= comboBox1.SelectedIndex; i++)
                j.MoveNext();
            p.Value = j.Current;
            command.Parameters.Add(p);

            p = new OracleParameter("display_pic", OracleType.Blob);
            p.Direction = ParameterDirection.Input;
            byte[] imageBytes = getProfilePicture();
            if (imageBytes == null)
                p.Value = DBNull.Value;
            else
                p.Value = imageBytes;
            command.Parameters.Add(p);
            int k = command.ExecuteNonQuery();
            if (k == 0)
            {
                MessageBox.Show("Could not insert employee");
                connection.Close();
                return;
            }
            if (groupBox2.Enabled)//doctor
            {
                decimal d_id = 1;
                query = "SELECT d_id FROM doctor ORDER BY d_id DESC";
                command = new OracleCommand(query, connection);
                reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    d_id = reader.GetDecimal(0);
                    d_id++;
                }

                query = "INSERT INTO doctor VALUES(" + d_id.ToString() + ",'" + textBox6.Text + "'," + textBox8.Text + ", " + id.ToString() + ")";
                command = new OracleCommand(query, connection);
                k = command.ExecuteNonQuery();

            }
            else if (groupBox3.Enabled) //nurse
            {
                j = nurse_type.GetEnumerator();
                for (int i = 0; i <= comboBox5.SelectedIndex; i++)
                    j.MoveNext();
                var l = ward_id.GetEnumerator();
                for (int i = 0; i <= comboBox4.SelectedIndex; i++)
                    l.MoveNext();

                query = "INSERT INTO nurse VALUES(" + id.ToString() + "," + j.Current + "," + l.Current + ")";
                command = new OracleCommand(query, connection);
                k = command.ExecuteNonQuery();
            }

            else if (groupBox4.Enabled) //janitor
            {
                var l = ward_id.GetEnumerator();
                for (int i = 0; i <= comboBox6.SelectedIndex; i++)
                    l.MoveNext();

                query = "INSERT INTO janitor VALUES(" + id.ToString() + "," + l.Current + ")";
                command = new OracleCommand(query, connection);
                k = command.ExecuteNonQuery();
            }
            if (k > 0)
            {
                MessageBox.Show("Employee Added");
                updateFlag = true;
            }
            connection.Close();
        }

        private void loadEmployee()
        {
            textBox4.Text = id.ToString();
            comboBox1.Enabled = false;

            string query = "SELECT * FROM employee WHERE eid = " + textBox4.Text;
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();
            if(reader.HasRows)
            {
                reader.Read();
                textBox1.Text = reader.GetString(1);
                textBox2.Text = reader.GetString(2);
                textBox3.Text = reader.GetString(3);
                if (reader.GetString(4).Equals("M"))
                    comboBox3.SelectedIndex = 0;
                else if (reader.GetString(4).Equals("F"))
                    comboBox3.SelectedIndex = 1;
                else if (reader.GetString(4).Equals("O"))
                    comboBox3.SelectedIndex = 2;
                dateTimePicker1.Value = reader.GetDateTime(5);
                if (!DBNull.Value.Equals(reader.GetValue(6)))
                    textBox7.Text = reader.GetString(6);
                if (!DBNull.Value.Equals(reader.GetValue(7)))
                    richTextBox1.Text = reader.GetString(7);
                if (!DBNull.Value.Equals(reader.GetValue(8)))
                    textBox5.Text = reader.GetDecimal(8).ToString();

                if (reader.GetDecimal(9) == 1) //Wroking
                    comboBox2.SelectedIndex = 0;
                else if (reader.GetDecimal(9) == 2)//Retired
                    comboBox2.SelectedIndex = 1;
                else if (reader.GetDecimal(9) == 3)//Resigned
                    comboBox2.SelectedIndex = 2;


                if(!DBNull.Value.Equals(reader.GetValue(11)))
                {
                    byte[] imageBytes = new byte[10485760];//System.InvalidOperationException
                    reader.GetBytes(11, 0, imageBytes, 0, 10485760);
                    Bitmap image = (Bitmap)(new ImageConverter()).ConvertFrom(imageBytes);
                    pictureBox1.Image = image;
                }

                if (reader.GetDecimal(10) == 1)//ADmin
                {
                    comboBox1.SelectedIndex = 0;
                }
                else if (reader.GetDecimal(10) == 2)//Doctor
                {
                    comboBox1.SelectedIndex = 1;
                    query = "SELECT * FROM doctor WHERE eid = " + textBox4.Text;
                    command = new OracleCommand(query, connection);
                    reader = command.ExecuteReader();

                    reader.Read();
                    textBox6.Text = reader.GetString(1);
                    textBox8.Text = reader.GetDecimal(2).ToString();
                }
                else if (reader.GetDecimal(10) == 3)//Nurse
                {
                    comboBox1.SelectedIndex = 2;
                    query = "SELECT * FROM nurse WHERE eid = " + textBox4.Text;
                    command = new OracleCommand(query, connection);
                    reader = command.ExecuteReader();

                    reader.Read();
                    decimal nurse = reader.GetDecimal(1);
                    decimal ward = reader.GetDecimal(2);

                    var j = nurse_type.GetEnumerator();
                    int i = 0;
                    while(j.MoveNext())
                    {
                        if(j.Current == nurse)
                        {
                            comboBox5.SelectedIndex = i;
                            break;
                        }
                        i++; 
                    }

                    j = ward_id.GetEnumerator();
                    i = 0;
                    while (j.MoveNext())
                    {
                        if (j.Current == ward)
                        {
                            comboBox4.SelectedIndex = i;
                            break;
                        }
                        i++;
                    }

                    connection.Close();
                }
                else if (reader.GetDecimal(10) == 4)//Janitor
                {
                    comboBox1.SelectedIndex = 3;
                    query = "SELECT * FROM janitor WHERE eid = " + textBox4.Text;
                    command = new OracleCommand(query, connection);
                    reader = command.ExecuteReader();

                    reader.Read();
                    decimal ward = reader.GetDecimal(1);

                    var j = ward_id.GetEnumerator();
                    int i = 0;
                    while (j.MoveNext())
                    {
                        if (j.Current == ward)
                        {
                            comboBox6.SelectedIndex = i;
                            break;
                        }
                        i++;
                    }
                    connection.Close();
                }
            }
            else
            {
                MessageBox.Show("Error While loading Data");
            }
            connection.Close();
        }

        private void updateEmployee()
        {
            bool condition = string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text) ||
            string.IsNullOrEmpty(textBox3.Text) || string.IsNullOrEmpty(richTextBox1.Text) ||
            string.IsNullOrEmpty(textBox5.Text) ||
            (groupBox2.Enabled && (string.IsNullOrEmpty(textBox6.Text) || string.IsNullOrEmpty(textBox8.Text)));
            if (condition)
            {
                MessageBox.Show("Please Enter all the mandatory details");
                return;
            }

            string query1 = "UPDATE employee SET f_name = :f_name, m_name= :m_name, s_name = :s_name, dob= :dob, email_id = :email_id, address = :address, contact_no = :contact_no," +
                " status = :status, gender= :gender WHERE eid = " + id;
            string query2 = "UPDATE employee SET display_pic= :display_pic WHERE eid = " + id;
            string query;

            command = new OracleCommand(query1, connection);
            connection.Open();
            OracleParameter p = new OracleParameter("f_name", OracleType.VarChar);
            p.Direction = ParameterDirection.Input;
            p.Value = textBox1.Text;
            command.Parameters.Add(p);
            p = new OracleParameter("m_name", OracleType.VarChar);
            p.Direction = ParameterDirection.Input;
            p.Value = textBox2.Text;
            command.Parameters.Add(p);
            p = new OracleParameter("s_name", OracleType.VarChar);
            p.Direction = ParameterDirection.Input;
            p.Value = textBox3.Text;
            command.Parameters.Add(p);
            p = new OracleParameter("gender", OracleType.VarChar);
            p.Direction = ParameterDirection.Input;
            p.Value = comboBox3.GetItemText(comboBox3.SelectedItem).Substring(0, 1);
            command.Parameters.Add(p);
            p = new OracleParameter("dob", OracleType.DateTime);
            p.Direction = ParameterDirection.Input;
            p.Value = dateTimePicker1.Value;
            command.Parameters.Add(p);
            p = new OracleParameter("email_id", OracleType.VarChar);
            p.Direction = ParameterDirection.Input;
            if (string.IsNullOrEmpty(textBox7.Text))
                p.Value = DBNull.Value;
            else
                p.Value = textBox7.Text;

            command.Parameters.Add(p);
            p = new OracleParameter("address", OracleType.VarChar);
            p.Direction = ParameterDirection.Input;
            p.Value = richTextBox1.Text;
            command.Parameters.Add(p);
            p = new OracleParameter("contact_no", OracleType.Number);
            p.Direction = ParameterDirection.Input;
            p.Value = decimal.Parse(textBox5.Text);
            command.Parameters.Add(p);
            p = new OracleParameter("status", OracleType.Number);
            p.Direction = ParameterDirection.Input;
            var j = status_id.GetEnumerator();
            for (int i = 0; i <= comboBox2.SelectedIndex; i++)
                j.MoveNext();
            p.Value = j.Current;
            command.Parameters.Add(p);

            int k = command.ExecuteNonQuery();
            if (k == 0)
            {
                MessageBox.Show("Could not Update employee");
                connection.Close();
                return;
            }

            if (groupBox2.Enabled)//doctor
            {
                query = "UPDATE doctor SET SPECIALIZATION = '" + textBox6.Text + "',EMERGENCY_CN=" + textBox8.Text + " WHERE eid=" + id;
                command = new OracleCommand(query, connection);
                k = command.ExecuteNonQuery();
            }
            else if (groupBox3.Enabled) //nurse
            {
                j = nurse_type.GetEnumerator();
                for (int i = 0; i <= comboBox5.SelectedIndex; i++)
                    j.MoveNext();
                var l = ward_id.GetEnumerator();
                for (int i = 0; i <= comboBox4.SelectedIndex; i++)
                    l.MoveNext();

                query = "UPDATE nurse SET type = " + j.Current + ", ward_id= " + l.Current + " WHERE eid = " + id;
                command = new OracleCommand(query, connection);
                k = command.ExecuteNonQuery();
            }

            else if (groupBox4.Enabled) //janitor
            {
                var l = ward_id.GetEnumerator();
                for (int i = 0; i <= comboBox6.SelectedIndex; i++)
                    l.MoveNext();
                query = "UPDATE janitor SET ward_id= " + l.Current + " WHERE eid = " + id;
                command = new OracleCommand(query, connection);
                k = command.ExecuteNonQuery();
            }

            if(imagePath!=null)
            {
                byte[] imageBytes = getProfilePicture();
                command = new OracleCommand(query2, connection);
                p = new OracleParameter("display_pic",OracleType.Blob);
                p.Direction = ParameterDirection.Input;
                p.Value = imageBytes;
                command.Parameters.Add(p);

                k = command.ExecuteNonQuery();
            }
            if (k > 0)
                MessageBox.Show("Details Updated");
            else
                MessageBox.Show("Update Failed");
            connection.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {
           
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            OpenFileDialog pic = new OpenFileDialog();
            pic.Filter = "image files(jpg,jpeg,png)|*.jpeg; *.jpg; *.png";
            if (pic.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(pic.FileName);
                imagePath = pic.FileName;
            }
        }
    }
}
