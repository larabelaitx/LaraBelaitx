using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Krypton.Toolkit;
using BLL;
using Services;
using DAL;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace UI
{
    public partial class ConfigStringConn : KryptonForm
    {
        private const string TEMPLATE_SQL = "Data Source={0};Initial Catalog={1};User Id={2};Password={3};Encrypt=False;";
        private const string TEMPLATE_WIN = "Data Source={0};Initial Catalog={1};Integrated Security=True;Encrypt=False;";
        private readonly string _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConfigFile.txt");

        public ConfigStringConn()
        {
            InitializeComponent();

            this.Load += ConfigStringConn_Load;
            this.chkWindows.CheckedChanged += chkWindows_CheckedChanged;
            this.btnProbar.Click += btnProbar_Click;
            this.btnGuardar.Click += btnGuardar_Click;
        }

        private void ConfigStringConn_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtServidor.Text)) txtServidor.Text = "LARAB";
            if (string.IsNullOrWhiteSpace(txtBase.Text)) txtBase.Text = "ITX";

            TryLoadSavedConnection();
            UpdateAuthModeUI();
        }

        private void chkWindows_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAuthModeUI();
            if (chkWindows.Checked) { txtUsuarioDb.Clear(); txtPassDb.Clear(); }
        }

        private void btnProbar_Click(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;

            string cnn = BuildConnectionString();
            bool ok = TestConnection(cnn);

            KryptonMessageBox.Show(
                ok ? "Conexión establecida correctamente." : "No se pudo establecer la conexión.",
                "Prueba de conexión"
            );
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidateFields()) return;

            string cnn = BuildConnectionString();

            try
            {
                string enc = Convert.ToBase64String(Encoding.UTF8.GetBytes(cnn));
                File.WriteAllText(_configFilePath, enc);

                KryptonMessageBox.Show("Conexión guardada correctamente.", "Config DB");
                this.Close();
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show("Error al guardar la conexión.\n" + ex.Message, "Config DB");
            }
        }
        private string BuildConnectionString()
        {
            string server = txtServidor.Text.Trim();
            string db = txtBase.Text.Trim();

            if (chkWindows.Checked)
                return string.Format(TEMPLATE_WIN, server, db);

            string user = txtUsuarioDb.Text.Trim();
            string pass = txtPassDb.Text; 
            return string.Format(TEMPLATE_SQL, server, db, user, pass);
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(txtServidor.Text))
            {
                KryptonMessageBox.Show("Debés completar el Server.", "Config DB"); txtServidor.Focus(); return false;
            }
            if (string.IsNullOrWhiteSpace(txtBase.Text))
            {
                KryptonMessageBox.Show("Debés completar la Base de Datos.", "Config DB"); txtBase.Focus(); return false;
            }
            if (!chkWindows.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtUsuarioDb.Text))
                { KryptonMessageBox.Show("Debés completar el Usuario DB.", "Config DB"); txtUsuarioDb.Focus(); return false; }
                if (string.IsNullOrEmpty(txtPassDb.Text))
                { KryptonMessageBox.Show("Debés completar la Contraseña.", "Config DB"); txtPassDb.Focus(); return false; }
            }
            return true;
        }

        private void UpdateAuthModeUI()
        {
            bool integrated = chkWindows.Checked;
            txtUsuarioDb.Enabled = !integrated;
            txtPassDb.Enabled = !integrated;
        }

        private void TryLoadSavedConnection()
        {
            try
            {
                if (!File.Exists(_configFilePath)) return;
                string enc = File.ReadAllText(_configFilePath);
                if (string.IsNullOrWhiteSpace(enc)) return;

                string plain = Encoding.UTF8.GetString(Convert.FromBase64String(enc));
                ApplyConnectionStringToUI(plain);
            }
            catch
            {
                
            }
        }

        private void ApplyConnectionStringToUI(string cnn)
        {
            if (string.IsNullOrWhiteSpace(cnn)) return;

            bool isWin = Regex.IsMatch(cnn, @"Integrated\s*Security\s*=\s*True", RegexOptions.IgnoreCase);
            string server = ExtractValue(cnn, "Data Source");
            string db = ExtractValue(cnn, "Initial Catalog");

            if (!string.IsNullOrWhiteSpace(server)) txtServidor.Text = server;
            if (!string.IsNullOrWhiteSpace(db)) txtBase.Text = db;

            chkWindows.Checked = isWin;

            if (!isWin)
            {
                string user = ExtractValue(cnn, "User Id");
                if (!string.IsNullOrWhiteSpace(user)) txtUsuarioDb.Text = user;
                txtPassDb.Clear(); 
            }
        }

        private static string ExtractValue(string cnn, string key)
        {
            var m = Regex.Match(cnn, $@"{Regex.Escape(key)}\s*=\s*([^;]+)", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value.Trim() : null;
        }
        private bool TestConnection(string cnn)
        {
            try
            {
                using (var cn = new SqlConnection(cnn))
                {
                    cn.Open();
                    using (var cmd = new SqlCommand("SELECT 1", cn))
                        cmd.ExecuteScalar();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void label3_Click(object sender, EventArgs e) { }
    }
}
