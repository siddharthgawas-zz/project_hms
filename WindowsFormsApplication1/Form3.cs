using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.Configuration;
using System.Data.OracleClient;
using System.IO;
namespace WindowsFormsApplication1
{
    public partial class Form3 : Form
    {
        private OracleConnection connection;
        private OracleCommand command;
        private OracleDataReader reader;
        private bool add = true;
        private string imagePath = null;
        public Form3()
        {
            InitializeComponent();
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
            dateTimePicker1.Value = DateTime.Today;
        }

        public Form3(decimal uid)
        {
            InitializeComponent();
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
            textBox4.Text = uid.ToString();
            dateTimePicker1.Value = DateTime.Today;
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
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

        private void button3_Click(object sender, EventArgs e)
        {
            if (add)
            {
                try
                {
                    addData();
                    add = false;
                    imagePath = null;
                    textBox4.Enabled = false;
                }
                catch (OracleException except)
                {
                    MessageBox.Show("Error Occured. Please Enter Valid Data");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                }
            }

            else
            {
                try
                {
                    updateData();
                    add = false;
                    textBox4.Enabled = false;
                    imagePath = null;
                }
                catch (OracleException except)
                {
                    MessageBox.Show("Error Occured. Please Enter Valid Data");
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                }
            }
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

        private void updateData()
        {
            long uid = long.Parse(textBox4.Text);

            string query = "UPDATE patient SET f_name = :f_name, m_name = :m_name, s_name = :s_name, " +
                "dob = :dob, address = :address, gender = :gender, contact_no = :contact_no, profile_pic = :profile_pic "
                + " WHERE u_id = :u_id";
            string query2 = "UPDATE patient SET f_name = :f_name, m_name = :m_name, s_name = :s_name, " +
                "dob = :dob, address = :address, gender = :gender, contact_no = :contact_no "
                + " WHERE u_id = :u_id";
            if (imagePath == null)
            {
                command = new OracleCommand(query2, connection);
                connection.Open();
                command.CommandType = CommandType.Text;
            }

            else
            {
                command = new OracleCommand(query, connection);
                connection.Open();
                command.CommandType = CommandType.Text;

                byte[] imageBytes = getProfilePicture();
                OracleParameter param1 = command.Parameters.Add(new OracleParameter("profile_pic", OracleType.Blob));
                param1.Direction = ParameterDirection.Input;
                param1.Value = imageBytes;
            }
            OracleParameter param =
                command.Parameters.Add(new OracleParameter("f_name", OracleType.VarChar));
            param.Direction = ParameterDirection.Input;
            param.Value = textBox1.Text;

            param =
               command.Parameters.Add(new OracleParameter("u_id", OracleType.Number));
            param.Direction = ParameterDirection.Input;
            param.Value = Decimal.Parse(textBox4.Text);

            param = command.Parameters.Add(new OracleParameter("m_name", OracleType.VarChar));
            param.Direction = ParameterDirection.Input;
            param.Value = textBox2.Text;

            param = command.Parameters.Add(new OracleParameter("s_name", OracleType.VarChar));
            param.Direction = ParameterDirection.Input;
            param.Value = textBox3.Text;

            param = command.Parameters.Add(new OracleParameter("dob", OracleType.DateTime));
            param.Direction = ParameterDirection.Input;
            param.Value = dateTimePicker1.Value.ToShortDateString();

            param = command.Parameters.Add(new OracleParameter("gender", OracleType.VarChar));
            param.Direction = ParameterDirection.Input;
            if (radioButton1.Checked)
                param.Value = "M";
            else if (radioButton2.Checked)
                param.Value = "F";
            else if (radioButton3.Checked)
                param.Value = "O";

            param = command.Parameters.Add(new OracleParameter("address", OracleType.VarChar));
            param.Direction = ParameterDirection.Input;
            param.Value = richTextBox1.Text;

            param = command.Parameters.Add(new OracleParameter("contact_no", OracleType.Number));
            param.Direction = ParameterDirection.Input;
            param.Value = Decimal.Parse(textBox5.Text);

            int i = command.ExecuteNonQuery();
            if (i > 0)
            {
                MessageBox.Show("Patient Information Updated");
            }

            else
            {
                MessageBox.Show("Update Failed");
            }

        }
        private void addData()
        {
            string query = "insert into PATIENT values (" + textBox4.Text + ", '" + textBox1.Text + "','" + textBox2.Text + "','"
              + textBox3.Text + "', TO_DATE('" + dateTimePicker1.Value.ToShortDateString() + "', 'MM/dd/yyyy'),'" + richTextBox1.Text + "',";
            if (radioButton1.Checked)

                query += "'M',";
            else if (radioButton2.Checked)
                query += "'F',";
            else if (radioButton3.Checked)
                query += "'O',";
            query += textBox5.Text + ", :imageBytes)";
            connection.Open();
            command = new OracleCommand(query, connection);
            command.CommandType = CommandType.Text;

            byte[] imageBytes = getProfilePicture();
            if (imageBytes == null)
            {
                OracleParameter param = new OracleParameter();
                param.ParameterName = "imageBytes";
                param.Value = DBNull.Value;
                param.Direction = ParameterDirection.Input;
                param.OracleType = OracleType.Blob;
                command.Parameters.Add(param);
            }
            else
            {
                OracleParameter param = new OracleParameter();
                param.ParameterName = "imageBytes";
                param.Value = imageBytes;
                param.Direction = ParameterDirection.Input;
                param.OracleType = OracleType.Blob;
                command.Parameters.Add(param);

            }
            int i = command.ExecuteNonQuery();
            if (i > 0)
            {
                MessageBox.Show("Transaction Successfull");
                add = false;

            }
            else
                MessageBox.Show("Transaction Failed");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form3 f = new Form3();
            f.Show();
            this.Dispose(false);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            bool condition1 = string.IsNullOrEmpty(textBox1.Text) &&
                string.IsNullOrEmpty(textBox2.Text) &&
                string.IsNullOrEmpty(textBox3.Text) &&
                string.IsNullOrEmpty(textBox4.Text);
            bool condition2 = radioButton1.Checked || radioButton2.Checked || radioButton3.Checked;
            if (condition1 && !condition2)
            {
                MessageBox.Show("Choose Atleast One Filter");
                return;
            }

            bool first_arg = true;
            string query = "SELECT u_id, f_name, m_name, s_name FROM patient WHERE ";
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                if (!first_arg)
                {
                    query += " AND ";

                }
                query += " f_name= '" + textBox1.Text + "' ";
                first_arg = false;
            }
            if (!string.IsNullOrEmpty(textBox2.Text))
            {
                if (!first_arg)
                {
                    query += " AND ";

                }
                query += " m_name= '" + textBox2.Text + "' ";
                first_arg = false;
            }
            if (!string.IsNullOrEmpty(textBox3.Text))
            {
                if (!first_arg)
                {
                    query += " AND ";

                }
                query += " s_name= '" + textBox3.Text + "' ";
                first_arg = false;
            }
            if (!string.IsNullOrEmpty(textBox4.Text))
            {
                if (!first_arg)
                {
                    query += " AND ";

                }
                query += " u_id = " + decimal.Parse(textBox4.Text);
                first_arg = false;
            }
            if (radioButton1.Checked)
            {
                if (!first_arg)
                {
                    query += " AND ";

                }
                query += " gender= 'M' ";
                first_arg = false;
            }
            if (radioButton2.Checked)
            {
                if (!first_arg)
                {
                    query += " AND ";

                }
                query += " gender= 'F' ";
                first_arg = false;
            }
            if (radioButton3.Checked)
            {
                if (!first_arg)
                {
                    query += " AND ";

                }
                query += " gender= 'O' ";
                first_arg = false;
            }

            updateList(query);
        }

        private void updateList(string query)
        {
            listView1.Items.Clear();
            connection.Open();
            command = new OracleCommand(query, connection);
            reader = command.ExecuteReader();
            if (!reader.HasRows)
                MessageBox.Show("PATIENT NOT FOUND!");
            else
            {
                while(reader.Read())
                {
                    string name = reader.GetString(1) + " " + reader.GetString(2) + " " + reader.GetString(3); // GetString(columnIndex)
                    string uid = reader.GetOracleNumber(0).ToString(); //order of columns returned by select 
                    ListViewItem item = new ListViewItem(new string[] { uid,name });
                    listView1.Items.Add(item);
                }
                button5.Enabled = true;
                button6.Enabled = true;
            }
            connection.Close();
            reader.Close();
        }


        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
                return;
           decimal id = decimal.Parse(e.Item.Text);

            try
            {
                string query = "SELECT * FROM PATIENT WHERE u_id = " + id;
                command = new OracleCommand(query, connection);
                command.CommandType = CommandType.Text;
                connection.Open();
                reader = command.ExecuteReader();
                while(reader.Read())
                {
                    textBox4.Text = reader.GetOracleNumber(0).ToString();
                    textBox1.Text = reader.GetString(1);
                    textBox2.Text = reader.GetString(2);
                    textBox3.Text = reader.GetString(3);
                    dateTimePicker1.Value = new DateTime(reader.GetOracleDateTime(4).Year,
                        reader.GetOracleDateTime(4).Month, reader.GetOracleDateTime(4).Day);
                    richTextBox1.Text = reader.GetOracleString(5).ToString();

                    string gender = reader.GetOracleString(6).ToString();
                    if (gender.Equals("M"))
                        radioButton1.Checked = true;
                    else if (gender.Equals("F"))
                        radioButton2.Checked = true;
                    else if (gender.Equals("O"))
                        radioButton3.Checked = true;

                    textBox5.Text = reader.GetOracleNumber(7).ToString();
                    try
                    {
                        byte[] imageBytes = new byte[10485760];//System.InvalidOperationException
                        reader.GetBytes(8, 0, imageBytes, 0, 10485760);
                        Bitmap image = (Bitmap)(new ImageConverter()).ConvertFrom(imageBytes);
                        pictureBox1.Image = image;
                    }
                    catch(System.InvalidOperationException)
                    {
                        pictureBox1.Image = pictureBox1.InitialImage;

                    }
                }
                add = false;
                textBox4.Enabled =  false;
            }
            catch(OracleException)
            {

            }
            finally
            {
                connection.Close();
                reader.Close();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Admissions f = new Admissions(decimal.Parse(textBox4.Text));
            f.Text = f.Text + " " + textBox1.Text + " " + textBox2.Text + " " + textBox3.Text;
            f.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose(false);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            decimal uid = decimal.Parse(textBox4.Text);
            connection.Close();
            Appointments f = new Appointments(uid);
            f.Show();
        }
    }
}