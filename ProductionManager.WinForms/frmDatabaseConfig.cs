using System.IO;
using System.ServiceProcess; // Cần NuGet: System.ServiceProcess.ServiceController
using System.Windows.Forms;
using MySql.Data.MySqlClient; // Cần NuGet: MySql.Data
using Newtonsoft.Json.Linq;   // Cần NuGet: Newtonsoft.Json

namespace ProductionManager.WinForms
{
    public partial class frmDatabaseConfig : Form
    {
        public frmDatabaseConfig()
        {
            InitializeComponent();
        }

        private void frmDatabaseConfig_Load(object sender, EventArgs e)
        {
            LoadLocalMySQLServices();
            LoadCurrentSettings();
        }

        // 1. QUÉT SERVER TRÊN MÁY (LOCAL SERVICES)
        private void LoadLocalMySQLServices()
        {
            cboServer.Items.Clear();
            cboServer.Items.Add("localhost");
            cboServer.Items.Add("127.0.0.1");

            try
            {
                ServiceController[] services = ServiceController.GetServices();
                foreach (var service in services)
                {
                    if (service.ServiceName.ToLower().Contains("mysql") ||
                        service.ServiceName.ToLower().Contains("mariadb"))
                    {
                        if (!cboServer.Items.Contains("localhost"))
                        {
                            cboServer.Items.Add("localhost");
                        }
                    }
                }
            }
            catch { /* Bỏ qua lỗi quyền Admin */ }

            cboServer.SelectedIndex = 0;
            txtPort.Text = "3306";
        }

        // 2. ĐỌC CẤU HÌNH CŨ
        private void LoadCurrentSettings()
        {
            string path = "appsettings.json";
            if (File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    var jObject = JObject.Parse(json);

                    var connStr = jObject["ConnectionStrings"]?["MySqlConnection"]?.ToString();
                    if (!string.IsNullOrEmpty(connStr))
                    {
                        var builder = new MySqlConnectionStringBuilder(connStr);
                        cboServer.Text = builder.Server;
                        txtPort.Text = builder.Port.ToString();
                        txtDatabase.Text = builder.Database;
                        txtUser.Text = builder.UserID;
                        txtPassword.Text = builder.Password;
                    }
                }
                catch { }
            }
        }

        // 3. TẠO CHUỖI KẾT NỐI
        private string GetConnectionString()
        {
            var builder = new MySqlConnectionStringBuilder();
            builder.Server = cboServer.Text.Trim();
            // Nếu port rỗng hoặc lỗi thì lấy 3306
            builder.Port = uint.TryParse(txtPort.Text.Trim(), out uint p) ? p : 3306;
            builder.Database = txtDatabase.Text.Trim();
            builder.UserID = txtUser.Text.Trim();
            builder.Password = txtPassword.Text.Trim();
            builder.AllowUserVariables = true;
            builder.CharacterSet = "utf8mb4";

            return builder.ConnectionString;
        }

        // 4. NÚT TEST
        private void btnTest_Click(object sender, EventArgs e)
        {
            string connStr = GetConnectionString();
            using (var conn = new MySqlConnection(connStr))
            {
                try
                {
                    btnTest.Text = "Đang thử...";
                    btnTest.Enabled = false;
                    conn.Open();
                    MessageBox.Show($"Kết nối thành công!\nVer: {conn.ServerVersion}", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Thất bại:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnTest.Text = "Kiểm tra kết nối";
                    btnTest.Enabled = true;
                }
            }
        }

        // 5. NÚT SAVE
        private void btnSave_Click(object sender, EventArgs e)
        {
            string path = "appsettings.json";
            try
            {
                JObject jObject;
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    jObject = JObject.Parse(json);
                }
                else
                {
                    jObject = new JObject();
                }

                jObject["DatabaseProvider"] = "MySql";
                if (jObject["ConnectionStrings"] == null) jObject["ConnectionStrings"] = new JObject();
                jObject["ConnectionStrings"]["MySqlConnection"] = GetConnectionString();

                File.WriteAllText(path, jObject.ToString());

                MessageBox.Show("Lưu thành công! Vui lòng khởi động lại ứng dụng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu file: " + ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
