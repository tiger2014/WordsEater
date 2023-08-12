using AngleSharp;
using Microsoft.EntityFrameworkCore;
using NAudio.Wave;
using System.Diagnostics;
using System.Xml.Linq;
using WordsEater.Models;

namespace WordsEater
{
    public partial class Form1 : Form
    {
        public static string dataPath = "";
        public static string databasePath = "";
        public static string samples = ""; 

        private List<Word> oriwords = new List<Word>();//存所有的单词
        private List<Word> words = new List<Word>();//存20个单词
        private List<Word> studiedWords = new List<Word>();
        private List<Word> wrongWords = new List<Word>();
        private List<Label> labelList = new List<Label>();

        //private WindowsMediaPlayer wplayer = new WindowsMediaPlayer();
        private Image labelBackground = Properties.Resources.labelLocker;

        private StudyType studyType;

        /// <summary>
        /// //0：锁定，1：正在处理，2：正确，3：倒计时
        /// </summary>
        private int[] locker = new int[20];

        private int presentWord = -1;
        private Label presentLabel = new Label();

        private class Word
        {
            public string word { get; set; }
            public string difinition { get; set; }

            public string examples { get; set; }
        }

        public Form1()
        {
            InitializeComponent();

            // 怎么发布为 msi文件： https://blog.51cto.com/u_13280061/3080173

            // 1. 读取配置项的值
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.config");
            var xml = XDocument.Load(configPath);
            var adds = xml.Root.Element("appSettings")?.Elements("add");
            if (adds != null)
            {
                foreach (var item in adds)
                {
                    if (item?.Attribute("key")?.Value == "dataPath") dataPath = AppDomain.CurrentDomain.BaseDirectory + item?.Attribute("value")?.Value ?? "";
                }
            }

            textBoxFolderPath.Text = dataPath;
            textBox1.Text = $"{DateTime.Now.ToString("HH:mm")}: start to study"+Environment.NewLine+ $"working folder: {dataPath}";
            databasePath = dataPath + "\\DataBase\\WordList.db";

            textBoxFolderPath.Enabled = false;
            //buttonSelectFolder.Enabled = false;
            //checkBox1.Enabled = false;
            //textBoxFolderPath.Visible = false;
            //buttonSelectFolder.Visible = false;
            //checkBox1.Visible = false;
            //textBoxFolderPath.Text = @"C:\D\workspace\VSCode\WordsEater\Data";
            using (var db = new WordListdbContext())
            {
                var allrecords = db.Words.ToList();
                oriwords = allrecords.Where(s => s.TotalStudiedTimes == null).Select(s => new Word { word = s.word, difinition = s.ChineseInterpretation, examples = s.Examples }).ToList();
                studiedWords = allrecords.Where(s => s.TotalStudiedTimes != null).Select(s => new Word { word = s.word, difinition = s.ChineseInterpretation, examples = s.Examples }).ToList();
                wrongWords = allrecords.Where(s => s.TotalWrongTimes != null).Select(s => new Word { word = s.word, difinition = s.ChineseInterpretation, examples = s.Examples }).ToList();
            }
        }

        //private void label_MouseEnter(object sender, EventArgs e)
        //{
        //    Label label = (Label)sender;
        //    label.BackColor = Color.Gray;
        //}

        //private void label_MouseLeave(object sender, EventArgs e)
        //{
        //    Label label = (Label)sender;
        //    label.BackColor = Color.FromArgb(36, 255, 255, 255);
        //}

        private void Form1_Load(object sender, EventArgs e)
        {
            // 绘制背景图片
            labelBackground = new Bitmap(labelBackground, label1.Width, label1.Height);
            Image backgroud = new Bitmap(Properties.Resources.winformbackgroud, this.Width, this.Height);
            //

            this.BackgroundImage = backgroud;
            var labels = new List<Label>();
            var tempLabels = this.Controls.OfType<Label>().ToList();
            for (int i = 20; i > 0; i--)
            {
                labels.Add(tempLabels[i]);
            }
            foreach (var label in labels)
            {
                Debug.WriteLine(label.Name);
                string name = label.Name;
                if (name == "label1") { label.Tag = 1; ; labelList.Add(label); }
                else if (name == "label2") { label.Tag = 2; labelList.Add(label); }
                else if (name == "label3") { label.Tag = 3; labelList.Add(label); }
                else if (name == "label4") { label.Tag = 4; labelList.Add(label); }
                else if (name == "label5") { label.Tag = 5; labelList.Add(label); }
                else if (name == "label6") { label.Tag = 6; labelList.Add(label); }
                else if (name == "label7") { label.Tag = 7; labelList.Add(label); }
                else if (name == "label8") { label.Tag = 8; labelList.Add(label); }
                else if (name == "label9") { label.Tag = 9; labelList.Add(label); }
                else if (name == "label10") { label.Tag = 10; labelList.Add(label); }
                else if (name == "label11") { label.Tag = 11; labelList.Add(label); }
                else if (name == "label12") { label.Tag = 12; labelList.Add(label); }
                else if (name == "label13") { label.Tag = 13; labelList.Add(label); }
                else if (name == "label14") { label.Tag = 14; labelList.Add(label); }
                else if (name == "label15") { label.Tag = 15; labelList.Add(label); }
                else if (name == "label16") { label.Tag = 16; labelList.Add(label); }
                else if (name == "label17") { label.Tag = 17; labelList.Add(label); }
                else if (name == "label18") { label.Tag = 18; labelList.Add(label); }
                else if (name == "label19") { label.Tag = 19; labelList.Add(label); }
                else if (name == "label20") { label.Tag = 20; labelList.Add(label); }
                //else if(name.Contains("21")) label.Tag= "21";

                //label.BackColor = Color.FromArgb(36, 255, 255, 255);
                //label.BackColor = Color.g;
                if (label.Name != "label21")
                {
                    label.Font = new Font("微软雅黑", 18);
                    label.ForeColor = Color.White;
                    label.BackColor = SystemColors.GradientActiveCaption;
                    //label.Image = Properties.Resources._lock;
                    label.TextAlign = ContentAlignment.MiddleCenter;
                    label.Visible = true;
                    label.Click += wordLabel_Click;
                    //label.MouseEnter += label_MouseEnter;
                    //label.MouseLeave += label_MouseLeave;
                    label.Text = "";
                    label.Enabled = false;
                }
            }
            buttonA.Visible = false;
            buttonB.Visible = false;
            buttonC.Visible = false;
            buttonD.Visible = false;

            buttonA.Click += buttonABCD_Click;
            buttonB.Click += buttonABCD_Click;
            buttonC.Click += buttonABCD_Click;
            buttonD.Click += buttonABCD_Click;

            textBox1.ReadOnly = true;
            progressBar1.Visible = false;
        }

        private async void buttonABCD_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int clickedTag = (int)button.Tag;
            if (correct == clickedTag)
            {
                button.Text = "正确";
                button.BackColor = Color.LightGreen;
                presentLabel.Text += Environment.NewLine + words[presentWord].difinition;
                locker[presentWord] = 2;
                using (var db = new WordListdbContext())
                {
                    var word = await db.Words.Where(s => s.word == words[presentWord].word).FirstOrDefaultAsync();
                    if (word != null)
                    {
                        word.TotalStudiedTimes = word.TotalStudiedTimes ?? 0 + 1;
                        await db.SaveChangesAsync();
                    }
                }
                if (!studiedWords.Any(s => s.word == words[presentWord].word)) studiedWords.Add(words[presentWord]);
            }
            else
            {
                button.Text = "错误";
                button.BackColor = Color.OrangeRed;
                presentLabel.Text = "Click me";
                locker[presentWord] = 0;

                using (var db = new WordListdbContext())
                {
                    var word = await db.Words.Where(s => s.word == words[presentWord].word).FirstOrDefaultAsync();
                    if (word != null)
                    {
                        word.TotalStudiedTimes = word.TotalStudiedTimes ?? 0 + 1;
                        word.TotalWrongTimes = word.TotalWrongTimes ?? 0 + 1;
                        await db.SaveChangesAsync();
                    }
                }
                if (!wrongWords.Any(s => s.word == words[presentWord].word)) wrongWords.Add(words[presentWord]);
            }

            buttonA.Enabled = false;
            buttonB.Enabled = false;
            buttonC.Enabled = false;
            buttonD.Enabled = false;

            presentWord = -1;

            presentLabel.Focus();

            if (locker.Where(s => s == 2).Count() == words.Count)
            {
                UpdateWordsList(studyType);
            }
        }

        private void wordLabel_Click(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            label.BackColor = Color.Gray;
            int index = (int)label.Tag;//所点击的label的索引

            buttonA.Enabled = true;
            buttonB.Enabled = true;
            buttonC.Enabled = true;
            buttonD.Enabled = true;

            buttonA.BackColor = Color.White;
            buttonB.BackColor = Color.White;
            buttonC.BackColor = Color.White;
            buttonD.BackColor = Color.White;

            if (index > words.Count) return;

            if (1 == locker[index - 1] || 2 == locker[index - 1])
            {
                using var audioFile = new AudioFileReader($"{textBoxFolderPath.Text}\\mp3\\{words[index - 1].word.Trim()}.mp3");
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            if (presentWord != -1 && locker[presentWord] == 1)//如果正在处理的单词没有选择答案
            {
                return;
            }
            if (0 == locker[index - 1])//如果当前label是锁着的
            {
                presentLabel = label;
                presentWord = index - 1;
                locker[index - 1] = 1;
                label.Image = null;
                label.Text = words[index - 1].word;
                GenerateABCD();
                using var audioFile = new AudioFileReader($"{textBoxFolderPath.Text}\\mp3\\{words[index - 1].word.Trim()}.mp3");
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(500);
                    }
                }
            }


            var text = "";
            var textList = words[index - 1].examples?.Split(Environment.NewLine).ToList() ?? new List<string>();
            foreach (var item in textList)
            {
                if (!string.IsNullOrWhiteSpace(item)) text += item + Environment.NewLine + Environment.NewLine;
            }
            samples = text;
            if (checkBox1.Checked == true)
            {                
                textBox1.Text = samples;
                presentLabel.Focus();
            }

            buttonA.Visible = true;
            buttonB.Visible = true;
            buttonC.Visible = true;
            buttonD.Visible = true;
        }

        private int correct = 0;

        /// <summary>
        /// 生成4个选项
        /// </summary>
        private void GenerateABCD()//随机生成选项
        {
            Random random = new Random();
            string A = oriwords[random.Next(0, oriwords.Count - 1)].difinition.Split('|')[1].Trim();
            string B = oriwords[random.Next(0, oriwords.Count - 1)].difinition.Split('|')[1].Trim();
            string C = oriwords[random.Next(0, oriwords.Count - 1)].difinition.Split('|')[1].Trim();
            string D = oriwords[random.Next(0, oriwords.Count - 1)].difinition.Split('|')[1].Trim();

            buttonA.Text = "A. " + A;
            buttonB.Text = "B. " + B;
            buttonC.Text = "C. " + C;
            buttonD.Text = "D. " + D;

            correct = random.Next(1, 4);
            string correctword = words[presentWord].difinition.Split('|')[1].Trim();
            switch (correct)
            {
                case 1:
                    buttonA.Text = "A. " + correctword;
                    break;

                case 2:
                    buttonB.Text = "B. " + correctword;
                    break;

                case 3:
                    buttonC.Text = "C. " + correctword;
                    break;

                case 4:
                    buttonD.Text = "D. " + correctword;
                    break;

                default:
                    break;
            }

            buttonA.Tag = 1;
            buttonB.Tag = 2;
            buttonC.Tag = 3;
            buttonD.Tag = 4;

            buttonA.Visible = true;
            buttonB.Visible = true;
            buttonC.Visible = true;
            buttonD.Visible = true;
        }

        private enum StudyType
        {
            学习新单词,
            复习答对的单词,
            学习答错的单词
        }

        private async void UpdateWordsList(StudyType? studyType = null)
        {
            //if (isUpdatingWordsList) return;
            //isUpdatingWordsList = true;

            var newOriwords = new List<Word>();
            if (studyType == null || studyType == StudyType.学习新单词)
            {
                var temp = new List<Word>();
                foreach (var item in oriwords)
                {
                    if (!studiedWords.Any(s => s.word == item.word))
                    {
                        temp.Add(item);
                    }
                }

                if (temp.Count > 20)
                {
                    foreach (var item in temp)
                    {
                        if (!words.Any(s => s.word == item.word))
                        {
                            newOriwords.Add(item);
                        }
                    }
                }
                else
                {
                    newOriwords = temp;
                }
            }
            else if (studyType == StudyType.复习答对的单词)
            {
                var temp = new List<Word>();
                if (studiedWords.Count > 20)
                {
                    foreach (var item in studiedWords)
                    {
                        if (!words.Any(s => s.word == item.word))
                        {
                            temp.Add(item);
                        }
                    }
                }
                else
                {
                    temp = studiedWords;
                }
                newOriwords = temp;
            }
            else if (studyType == StudyType.学习答错的单词)
            {
                var temp = new List<Word>();
                if (wrongWords.Count > 20)
                {
                    foreach (var item in wrongWords)
                    {
                        if (!words.Any(s => s.word == item.word))
                        {
                            temp.Add(item);
                        }
                    }
                }
                else
                {
                    temp = wrongWords;
                }
                newOriwords = temp;
            }
            int wordsCount = newOriwords.Count >= 20 ? 20 : newOriwords.Count;
            Random random = new Random((int)DateTime.Now.Ticks);
            words.Clear();
            for (int i = 0; i < wordsCount; i++)
            {
                int index = random.Next(0, newOriwords.Count);
                if (!words.Any(s => s.word == newOriwords[index].word))
                {
                    words.Add(newOriwords[index]);
                }
                else
                {
                    i--;
                }
            }

            //string http = "http://dict.youdao.com/speech?audio=";
            string http = @"http://dict.youdao.com/dictvoice?type=0&audio=";    // type = 0 美音，= 1 英音
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            progressBar1.Visible = true;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = wordsCount;
            for (int i = 0; i < wordsCount; i++)
            {
                progressBar1.Value = i;

                // get mp3
                if (File.Exists($"{textBoxFolderPath.Text}\\mp3\\{words[i].word.Trim()}.mp3")) continue;
                try
                {
                    var response = await client.GetAsync(http + words[i].word);
                    response.EnsureSuccessStatusCode();
                    // 获取响应内容流
                    using Stream responseStream = await response.Content.ReadAsStreamAsync();

                    // 使用 FileStream 将响应内容保存到文件中
                    using Stream fileStream = new FileStream($"{textBoxFolderPath.Text}\\mp3\\{words[i].word.Trim()}.mp3", FileMode.Create);
                    await responseStream.CopyToAsync(fileStream);
                }
                catch (Exception e)
                {
                    UpdateWordsList(StudyType.学习答错的单词);
                }

                // get samples
                if (string.IsNullOrWhiteSpace(words[i].examples))
                {
                    var address = @"https://www.dictionary.com/browse/" + words[i].word.Trim();

                    var document = await context.OpenAsync(address);
                    var examples = document.QuerySelector("#examples");
                    var sample = examples?.QuerySelectorAll("p");
                    //var sample = document.QuerySelectorAll("div.sentence");
                    string sentences = "";
                    if (sample != null)
                    {
                        foreach (var item1 in sample)
                        {
                            sentences += item1.InnerHtml.Replace("<em>", "").Replace("</em>", "") + Environment.NewLine;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(sentences))
                    {
                        var sql = $"update Words set Examples = {sentences} where word = {words[i].word}";
                        using var db = new WordListdbContext();
                        await db.Database.ExecuteSqlRawAsync(sql);
                    }
                }
            }
            progressBar1.Visible = false;

            //reset20个label
            for (int i = 0; i < labelList.Count; i++)
            {
                labelList[i].Text = "";
                labelList[i].TextAlign = ContentAlignment.MiddleCenter;
                labelList[i].BackColor = SystemColors.GradientActiveCaption;
                labelList[i].Enabled = false;
                locker[i] = 0;
            }

            for (int i = 0; i < words.Count; i++)
            {
                labelList[i].Text = "Click me";
                labelList[i].Enabled = true;
            }

            // reset ABCD
            buttonA.Text = "A";
            buttonB.Text = "B";
            buttonC.Text = "C";
            buttonD.Text = "D";

            buttonA.BackColor = Color.White;
            buttonB.BackColor = Color.White;
            buttonC.BackColor = Color.White;
            buttonD.BackColor = Color.White;

            buttonA.Enabled = false;
            buttonB.Enabled = false;
            buttonC.Enabled = false;
            buttonD.Enabled = false;
        }

        private void buttonSelectFolder_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowDialog();
            // Set the initial directory to the desired folder path
            folderBrowserDialog.SelectedPath = textBoxFolderPath.Text.Trim();

            // Set the description (optional)
            folderBrowserDialog.Description = "Select a folder";

            // Show the FolderBrowserDialog and check if the user clicked the OK button
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                // Retrieve the selected folder path
                string selectedFolderPath = folderBrowserDialog.SelectedPath;

                // Do something with the selected folder path
                // For example, display it in a TextBox
                textBoxFolderPath.Text = selectedFolderPath.Trim();
            }
        }

        private async void buttonStudyNewWords_Click(object sender, EventArgs e)
        {
            #region only for input data

            //var file = File.ReadAllLines(@"C:\D\workspace\VSCode\WordsKiller\WordsKiller\Data\words.txt");
            //var wordsInput = new List<Words>();
            //foreach(var item in file)
            //{
            //    if(string.IsNullOrWhiteSpace(item)) continue;
            //    var wordArray = item.Split('|');
            //    var wordInput = new Words();
            //    wordInput.word = wordArray[0].Trim();
            //    wordInput.ChineseInterpretation = $"{wordArray[1]}|{wordArray[2]}".Trim();
            //    wordsInput.Add(wordInput);
            //}
            //var disctinct = wordsInput.Select(s=>s.word).Distinct().ToList();
            //var finalWordsInput = new List<Words>();
            //foreach(var word in disctinct)
            //{
            //    var addedItem = wordsInput.Where(s => s.word == word).FirstOrDefault();
            //    if (addedItem != null)
            //    {
            //        finalWordsInput.Add(addedItem);
            //    }
            //}
            //using(var db = new WordListdbContext())
            //{
            //    int count = db.Words.Count();
            //    if (count > 0) return;
            //    try
            //    {
            //        await db.Words.AddRangeAsync(finalWordsInput);
            //        await db.SaveChangesAsync();
            //    }
            //    catch(Exception ex)
            //    {
            //        Debug.WriteLine($"save database failed. {ex.Message}: {ex.StackTrace}");
            //    }
            //}

            #endregion only for input data

            studyType = StudyType.学习新单词;
            UpdateWordsList(studyType);
        }

        private void buttonReviewWords_Click(object sender, EventArgs e)
        {
            studyType = StudyType.复习答对的单词;
            UpdateWordsList(studyType);
        }

        private void buttonStudyWrongWords_Click(object sender, EventArgs e)
        {
            studyType = StudyType.学习答错的单词;
            UpdateWordsList(studyType);
        }

        private async void buttonImportData_Click(object sender, EventArgs e)
        {
            #region 查询音标

            /***
            using(var db = new WordListdbContext())
            {
                var wordList = db.Words.ToList();
                var config = AngleSharp.Configuration.Default.WithDefaultLoader();
                var context = BrowsingContext.New(config);
                foreach (var item in wordList)
                {
                    string url = $"https://www.youdao.com/result?word={item.word.Trim()}&lang=en";
                    var document = await context.OpenAsync(url);

                    var phone_con = document.QuerySelector("div.phone_con")?.Children;
                    foreach (var item1 in phone_con)
                    {
                        if (item1.OuterHtml.Contains("美"))
                        {
                            var phonetic = item1.QuerySelector("span.phonetic")?.InnerHtml;
                            phonetic = "[" + phonetic?.Substring(1, phonetic.Length - 2) + "]";

                            item.ChineseInterpretation = phonetic??"" + "|" + item.ChineseInterpretation;
                        }
                    }
                }

                await db.SaveChangesAsync();
            }
            ***/

            #endregion 查询音标

            #region update words' Examples

            /***
            for (int i = 0; i < 20; i++)
            {
                using (var db = new WordListdbContext())
                {
                    var allwords = db.Words.Where(s => s.Examples == null).Select(s => s);
                    var config = Configuration.Default.WithDefaultLoader();
                    var context = BrowsingContext.New(config);
                    int count = 0;
                    foreach (var item in allwords)
                    {
                        if (count > 200) break;
                        var address = @"https://www.dictionary.com/browse/" + item.word.Trim();
                        var document = await context.OpenAsync(address);
                        var examples = document.QuerySelector("#examples");
                        var sample = examples?.QuerySelectorAll("p");
                        //var sample = document.QuerySelectorAll("div.sentence");
                        string sentences = "";
                        if (sample != null)
                        {
                            foreach (var item1 in sample)
                            {
                                sentences += item1.InnerHtml.Replace("<em>", "").Replace("</em>", "") + Environment.NewLine;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(sentences))
                        {
                            item.Examples = sentences;
                        }
                        count++;
                    }

                    await db.SaveChangesAsync();
                }
            }
            ***/

            #endregion update words' Examples

            #region import bao's words
            /***
            using (var db = new WordListdbContext())
            {
                var list = db.Words.ToList();
                var httpclient = new HttpClient();
                string url = @"https://fanyi.youdao.com/translate?&doctype=json&type=AUTO&i=";
                var inputwords = new List<Words>();
                foreach (var line in list)
                {
                    {
                        if (!string.IsNullOrWhiteSpace(line.word))
                        {
                            var response = await httpclient.GetStringAsync(url + line.word.Trim());
                            var reult = JsonConvert.DeserializeObject<dynamic>(response);

                            string definition = reult == null ? "" : reult.translateResult[0][0]["tgt"].ToString();
                            if (!string.IsNullOrWhiteSpace(definition))
                            {
                                line.ChineseInterpretation = line.ChineseInterpretation.Replace(" ", "") + "|" + definition;
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                }
            }
            ***/
            #endregion import bao's words

            if (textBox1.Text.Trim().Length > 0)
            {
                using (var db = new WordListdbContext())
                {
                    var inputStr = textBox1.Text.Trim().Split(Environment.NewLine);
                    var words = new List<Word>();
                    foreach (var item in inputStr)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            var word = new Word();
                            word.word = item.Split('|')[0].Trim();
                            word.difinition = item.Split('|')[1].Trim();
                            words.Add(word);
                        }
                    }

                    await db.AddRangeAsync(words);
                    await db.SaveChangesAsync();
                }
            }
        }

        private void ButtonShowWrongWords_Click(object sender, EventArgs e)
        {
            using var db = new WordListdbContext();

            var wordList = db.Words.Where(s => s.TotalWrongTimes != null).OrderByDescending(s => s.TotalWrongTimes).ToList();

            string text = "";

            foreach (var item in wordList)
            {
                text += $"{item.word}: {item.ChineseInterpretation}" + Environment.NewLine + Environment.NewLine;
            }

            textBox1.Text = text;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == false)
            {
                textBox1.Text = "";
            }
            else
            {
                textBox1.Text = samples;
            }
        }
    }
}