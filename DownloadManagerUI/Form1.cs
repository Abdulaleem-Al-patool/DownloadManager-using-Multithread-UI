using MultithreadDownloadManager.DownloadManager;
using Shared;

namespace DownloadManagerUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            string link = txtLink.Text.Trim();

            if (string.IsNullOrWhiteSpace(link))
            {
                MessageBox.Show("Please enter a valid URL.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fileName = UrlHandler.GetFileNameFromUrl(link);
            string destination = System.IO.Path.Combine(Application.StartupPath, fileName);


            btnDownload.Enabled = false;
            progressBar1.Value = 0;


            var progressValue = new Progress<double>(p => progressBar1.Value = Math.Min(100, (int)p));


            var progressStatus = new Progress<string>(msg => this.Text = msg);
            try
            {

                var service = new DownloadManagerService();
                await service.StartDownloadAsync(link, destination, progressValue, progressStatus);

                MessageBox.Show("File downloaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDownload.Enabled = true;
                this.Text = "Download Manager";
            }
        }
    }
}