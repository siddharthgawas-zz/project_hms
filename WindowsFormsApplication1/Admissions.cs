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
    public partial class Admissions : Form
    {
        private decimal uid;
        private OracleConnection connection;
        private OracleCommand command;
        private OracleDataReader reader;
        public Admissions()
        {
            InitializeComponent();
        }
        public Admissions(decimal uid)
        {
            InitializeComponent();
            this.uid = uid;
            connection = new OracleConnection(ConfigurationManager.ConnectionStrings["project_hms"].ConnectionString);
          
        }

        private void loadList()
        {
            listView1.Items.Clear();
            string query = "SELECT * FROM ADMISSION WHERE u_id = " + uid;
            command = new OracleCommand(query, connection);
            connection.Open();
            reader = command.ExecuteReader();

            if(!reader.HasRows)
            {
                MessageBox.Show("No Previous Admissions Found!");
                return;
            }
            int i = 1;
            while(reader.Read())
            {
                DateTime admitTime = reader.GetDateTime(1);
                
                if(DBNull.Value.Equals(reader.GetValue(2)))
                {
                    ListViewItem item = new ListViewItem(new string[] {i.ToString(),
                    admitTime.Day+"/"+admitTime.Month+"/"+admitTime.Year,
                    admitTime.Hour+":"+admitTime.Minute+":"+admitTime.Second,
                    "-/-/-","-:-:-"});
                    listView1.Items.Add(item);
                    i++;
                }
                else
                {
                    DateTime discTime = reader.GetDateTime(2);
                    ListViewItem item = new ListViewItem(new string[] {i.ToString(),
                    admitTime.Day+"/"+admitTime.Month+"/"+admitTime.Year,
                    admitTime.Hour+":"+admitTime.Minute+":"+admitTime.Second,
                    discTime.Day+"/"+discTime.Month+"/"+discTime.Year,
                    discTime.Hour+":"+discTime.Minute+":"+discTime.Second,});
                    listView1.Items.Add(item);
                    i++;
                }
               
            }

            reader.Close();
            connection.Close();
        }

        private void Admissions_Load(object sender, EventArgs e)
        {
            loadList();
        }
    }
}
