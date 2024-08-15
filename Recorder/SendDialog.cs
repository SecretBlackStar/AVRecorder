using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;

namespace Recorder
{
    public partial class SendDialog : Form
    {
        private RecordStatus status;
        private int time;
        private string responseID;
        private bool closeEvent = false;

        public SendDialog()
        {
            InitializeComponent();
        }

        public void setStatus(RecordStatus st)
        {
            status = st;
        }

        public void setTime(int t)
        {
            time = t;
        }

        private async void Btn_Send_Click(object sender, EventArgs e)
        {
            if(txt_Company.Text == "" || txt_Country.Text == "" || txt_Name.Text == "")
            {
                MessageBox.Show($"Please input all texts", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dataUrl = "http://192.168.8.110:8081/record/create";
            var uploadUrl = "http://192.168.8.110:8081/record/upload";

            var data = new
            {
                clientName = txt_Name.Text,
                gender = combo_Gender.SelectedItem.ToString(),
                nationality = txt_Country.Text,
                company = txt_Company.Text,
                duration = time
            };
            var jsonData = JsonConvert.SerializeObject(data);

            var waitProcess = new Form2("Sending Data!");
            waitProcess.Show();

            using (var client = new HttpClient())
            using (var form = new MultipartFormDataContent())
            {
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                
                try
                {
                    var response = await client.PostAsync(dataUrl, content);

                    response.EnsureSuccessStatusCode();

                    responseID = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while sending data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var filePath = status == RecordStatus.Voice ? "combined_audio.wav" : "result.avi";
                var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(filePath));

                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                form.Add(fileContent, "file", System.IO.Path.GetFileName(filePath));

                var userID = new StringContent(responseID);
                form.Add(userID, "id");
                try
                {
                    var response = await client.PostAsync(uploadUrl, form);
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while sending video: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            waitProcess.Hide();

            closeEvent = true;
            this.Close();
        }

        private void SendDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (closeEvent == false)
                e.Cancel = true;
        }

        private void SendDialog_Load(object sender, EventArgs e)
        {
            combo_Gender.Items.Add("male");
            combo_Gender.Items.Add("female");
            combo_Gender.SelectedIndex = 0;
            combo_Gender.DropDownStyle = ComboBoxStyle.DropDownList;
        }
    }
}
