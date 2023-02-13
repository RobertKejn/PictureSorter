
using System.Net;
using System.Text;
using System.Windows.Forms.Design;
using Newtonsoft.Json;
using TwoCaptcha.Captcha;




namespace PictureSorter
{
    public partial class Form1 : Form
    {
        //private HttpWebResponse r;
        private int Count = 0;
        private int FilesCount = 0;
        private string FolderPath;
        private string publishFolderPath;

        List<int> upscale_x_folders = new List<int>();

        long timeDelay = 300 * 24 * 60 * 60;
        string owner_id = "-218097297"; //-147923159 -218097297
        string token = "vk1.a.eK7Lv8psNLeBO3D0qhPt5df_id5OuJfLLgPQ1w5-B9QXNg0LkE3qJ8AuuoOB49IWqI8d-PBoRRdrYPinWLCbBswIvRlqWa_eRUw6nOn5yyVZyaChnUhl_WX0tcaaJeJTHwVZxfRwb8cHIM1HKI3LCoORPdqmJ9vyamdW0wtGml5BeGRM5IRqNIZapjuea2CJfrxcTiBP2glnW2bDDfroQQ";
        string API_KEY = "b62aa90fc9386229fe9f662cff3c393e";

        public Form1()
        {
            InitializeComponent();
            textBox3.Text = "4000";
            textBox4.Text = "4000";

            textBox8.Text = token;
            textBox9.Text = API_KEY;
            textBox10.Text = owner_id;
            textBox11.Text = (timeDelay / 24 / 60 / 60).ToString();
        }
        #region ����� ���������� � ���������� ������
        private void button1_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    FolderPath = fbd.SelectedPath;
                    textBox1.Text = FolderPath;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int x, y, mx = 1, my = 1;
            try
            {
                x = int.Parse(textBox3.Text);
                y = int.Parse(textBox4.Text);

                int mtp = x * y;

                if (textBox2.Text != String.Empty && textBox5.Text != String.Empty)
                {
                    mx = int.Parse(textBox2.Text);
                    my = int.Parse(textBox5.Text);
                }

                DirectoryInfo d = new DirectoryInfo(FolderPath);
                FileInfo[] Files = d.GetFiles("*.png");
                Files = Files.Concat(d.GetFiles("*.jpg")).ToArray();
                Files = Files.Concat(d.GetFiles("*.jpeg")).ToArray();

                foreach (FileInfo file in Files)
                {
                    Image im = Image.FromFile(file.FullName);
                    if (im != null)
                    {
                        int coef;
                        if (im.Width * im.Height / (double)(mx * my) > 1) coef = (int)Math.Round(Math.Sqrt(mtp / (double)(im.Width * im.Height)));
                        else coef = (int)Math.Ceiling(Math.Sqrt(mtp / (double)(im.Width * im.Height)));
                        if (coef >= 1)
                        {
                            string fileName = file.Name;
                            string filePath = file.FullName;
                            string newDirectory = FolderPath + "\\upscale_x" + coef.ToString();
                            string newFilePath = newDirectory + "\\" + fileName;
                            bool exists = Directory.Exists(newDirectory);
                            if (!exists)
                            {
                                Directory.CreateDirectory(newDirectory);
                                upscale_x_folders.Add(coef);
                            }
                            File.Copy(filePath, newFilePath);
                        }
                    }
                    im.Dispose();
                }
                MessageBox.Show("Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }
        #endregion
        private async void button3_Click(object sender, EventArgs e)
        {
            if (publishFolderPath != string.Empty)
            {
                DirectoryInfo d = new DirectoryInfo(publishFolderPath);
                FileInfo[] Files = d.GetFiles("*.png");
                Files = Files.Concat(d.GetFiles("*.jpg")).ToArray();
                Files = Files.Concat(d.GetFiles("*.jpeg")).ToArray();
                Files = Files.Concat(d.GetFiles("*.gif")).ToArray();

                FilesCount = Files.Length;
                Count = 0;
                foreach (var file in Files)
                {
                    try
                    {
                        string filePath = file.FullName;
                        string fileName = file.Name;
                        //AsyncPasreFile(filePath, fileName);
                        object args = new object[2] { filePath, fileName };
                        Thread thread = new Thread(new ParameterizedThreadStart(AsyncPasreFile));
                        thread.Start(args);
                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        listBox1.Items.Add("ERROR:   "+file.Name+ " " + ex.Message);
                        listBox1.TopIndex = listBox1.Items.Count - 1;
                    }
                }
            }
        }

        private async void AsyncPasreFile(object args)
        {
            Array argArray = new object[2];
            argArray = (Array)args;
            string filePath = (string)argArray.GetValue(0);
            string fileName = (string)argArray.GetValue(1);
            listBox1.Invoke(() => { listBox1.Items.Add("TRY TO PARSE   " + fileName); listBox1.TopIndex = listBox1.Items.Count - 1; });
            await Task.Run(() => { ParseFile(filePath, fileName); });
        }
        #region ����� ������
        private void button4_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    publishFolderPath = fbd.SelectedPath;
                    textBox6.Text = publishFolderPath;
                    DirectoryInfo d = new DirectoryInfo(publishFolderPath);
                    FileInfo[] Files = d.GetFiles("*.png");
                    Files = Files.Concat(d.GetFiles("*.jpg")).ToArray();
                    Files = Files.Concat(d.GetFiles("*.jpeg")).ToArray();
                    Files = Files.Concat(d.GetFiles("*.gif")).ToArray();
                    textBox7.Text = "0/" + Files.Length.ToString();
                }
            }
        }
        #endregion

        #region ������� ������ � ������
        private string ConvertResponseToString(HttpWebResponse response)
        {
            string s;
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                s = sr.ReadToEnd();
            }
            return s;
        }
        #endregion
        private HttpWebResponse AskForUpload(string token)
        {
            string url = "https://api.vk.com/method/docs.getUploadServer?access_token=" + token + "&v=5.89";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response;
        }

        private async Task ParseFile(string filePath, string fileName)
        {
            var response = AskForUpload(token);
            string s = ConvertResponseToString(response);
            AskFileUploadResponse askFileUploadResponse = JsonConvert.DeserializeObject<AskFileUploadResponse>(s);

            FileUploadResponse fileUploadResponse = new FileUploadResponse();
            try
            {
                var wc = new WebClient();
                s = Encoding.ASCII.GetString(wc.UploadFile(askFileUploadResponse.Response.upload_url, filePath));
                fileUploadResponse = JsonConvert.DeserializeObject<FileUploadResponse>(s);
            }
            catch (Exception ex)
            {
                listBox1.Invoke(() => { listBox1.Items.Add("ERROR:   "+ fileName+ " " + ex.Message); listBox1.TopIndex = listBox1.Items.Count - 1; });
            }

            var url = "https://api.vk.com/method/docs.save?file=" + fileUploadResponse.file + "&title=" + fileName + "&return_tags=0&access_token=" + token + "&v=5.89";
            var request = (HttpWebRequest)WebRequest.Create(url);
            response = (HttpWebResponse)request.GetResponse();
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                s = sr.ReadToEnd();
            }

            if (s.IndexOf("Captcha") != -1)
            {
                ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(s);
                response = await CaptchaResolve(errorResponse.error, fileUploadResponse, fileName);
                
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    s = sr.ReadToEnd();
                }
            }
            listBox1.Invoke(() => { listBox1.Items.Add("SUCCESFULLY UPLOADED TO DOCUMENTS   "+fileName); listBox1.TopIndex = listBox1.Items.Count - 1; });

            FileSaveResponse fileSaveResponse = JsonConvert.DeserializeObject<FileSaveResponse>(s);
            DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
            string time = (dto.ToUnixTimeSeconds() + timeDelay).ToString();
            url = "https://api.vk.com/method/wall.post?owner_id=" + owner_id + "&friends_only=0&from_group=0&message=" + "� � �\r\n\r\n~~~~\r\n� ��������: " + (fileSaveResponse.response[0].title).Substring(0, fileSaveResponse.response[0].title.IndexOf(".")) + "\r\n~~~~" + "&attachments=doc" + fileSaveResponse.response[0].owner_id.ToString() + "_" + fileSaveResponse.response[0].id.ToString() + "&signed=0&publish_date=" + time + "&mark_as_ads=0&close_comments=0&mute_notifications=0&access_token=" + token + "&v=5.89";
            request = (HttpWebRequest)WebRequest.Create(url);
            response = (HttpWebResponse)request.GetResponse();
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                s = sr.ReadToEnd();
            }
            listBox1.Invoke(() => { listBox1.Items.Add("POSTED   "+ fileName); listBox1.TopIndex = listBox1.Items.Count - 1; });
            Count++;
            textBox7.Invoke(() => { textBox7.Text = Count.ToString() + "/" + FilesCount.ToString(); });   
        }

        private async Task<string> CaptchaResolve(CaptchaResponse captchaResponse)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(captchaResponse.captcha_img, publishFolderPath + "\\" + captchaResponse.captcha_sid + ".jpg");
            }

            var solver = new TwoCaptcha.TwoCaptcha(API_KEY);
            Normal captcha = new Normal();
            captcha.SetFile(publishFolderPath + "\\" + captchaResponse.captcha_sid + ".jpg");
            string code = "";
            try
            {
                string captchaID = await solver.Send(captcha);
                await Task.Delay(20 * 1000);
                code = await solver.GetResult(captchaID);
            }
            catch (AggregateException e)
            {
                listBox1.Invoke(() => { listBox1.Items.Add("Capthca FAILURED"); listBox1.TopIndex = listBox1.Items.Count - 1; });
            }
            File.Delete(publishFolderPath + "\\" + captchaResponse.captcha_sid + ".jpg");
            return code;
        } // ������� ����� � ��������� ����

        #region ������� ����� � ��������
        private async Task<HttpWebResponse> CaptchaResolve(CaptchaResponse captchaResponse, FileUploadResponse fileUploadResponse, string fileName)
        {
            listBox1.Invoke(() => { listBox1.Items.Add("CAPTCHA REQUIRED   " + fileName); listBox1.TopIndex = listBox1.Items.Count - 1; });
            using (var client = new WebClient())
            {
                client.DownloadFile(captchaResponse.captcha_img, publishFolderPath + "\\" + captchaResponse.captcha_sid + ".jpg");
            }

            var solver = new TwoCaptcha.TwoCaptcha(API_KEY);
            Normal captcha = new Normal();
            captcha.SetFile(publishFolderPath + "\\" + captchaResponse.captcha_sid + ".jpg");
            string code = "";
            try
            {
                string captchaID = await solver.Send(captcha);
                await Task.Delay(20 * 1000);
                code = await solver.GetResult(captchaID);
                if (code != null) listBox1.Invoke(() => { listBox1.Items.Add("CAPTCHA RESOLVED:   " + code); listBox1.TopIndex = listBox1.Items.Count - 1; });
            }
            catch (AggregateException e)
            {
                listBox1.Invoke(() => { listBox1.Items.Add("CAPTCHA FAILED   "+ fileName); listBox1.TopIndex = listBox1.Items.Count - 1; });
            }
            File.Delete(publishFolderPath + "\\" + captchaResponse.captcha_sid + ".jpg");

            string url = "https://api.vk.com/method/docs.save?file=" + fileUploadResponse.file + "&title=" + fileName + "&return_tags=0&captcha_sid=" + captchaResponse.captcha_sid + "&captcha_key=" + code + "&access_token=" + token + "&v=5.89";
            var request = (HttpWebRequest)WebRequest.Create(url);
            return (HttpWebResponse)request.GetResponse();
        }
        #endregion
        #region ���������� ��������
        private void button5_Click_1(object sender, EventArgs e)
        {
            if (textBox8.Text != String.Empty) token = textBox8.Text;
            if (textBox9.Text != String.Empty) API_KEY = textBox9.Text;
            if (textBox10.Text != String.Empty) owner_id = textBox10.Text;

            if (textBox11.Text != String.Empty)
            {
                try
                {
                    int d = int.Parse(textBox11.Text);
                    if (d > 360) d = 360;
                    else if (d < 0) d = 0;
                    textBox11.Text = d.ToString();
                    timeDelay = d * 24 * 60 * 60;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }
        #endregion 
    }
}