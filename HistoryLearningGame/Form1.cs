using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace HistoryLearningGame
{

    public partial class Form1 : Form
    {
        ScenarioRoot scenarioRoot;
        HistoryNetwork historyNetwork;
        Dictionary<string, ScenarioNode> scenarioDict;
        string currentNodeId;

        // フォーム全体で使いたいのでここで定義
        private TextBox txtDescription;
        private Panel panelChoices;
        private Label lblStatus;
        private Button btnStart;


        public Form1()
        {
            InitializeComponent();
            CreateCustomUI();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            LoadScenario();
            LoadNetwork();
            currentNodeId = "start";
            panelChoices.Enabled = true;
            ShowCurrentScenario();
        }


        private void LoadScenario()
        {
            string json = File.ReadAllText("scenario.json");
            scenarioRoot = JsonConvert.DeserializeObject<ScenarioRoot>(json);
            scenarioDict = scenarioRoot.scenarios.ToDictionary(scenarioRoot => scenarioRoot.id);
        }

        private void LoadNetwork()
        {
            string json = File.ReadAllText("history_network.json");
            historyNetwork = JsonConvert.DeserializeObject<HistoryNetwork>(json);
        }

        private void ShowCurrentScenario()
        {
            panelChoices.Controls.Clear();

            var current = scenarioDict[currentNodeId];
            txtDescription.Text = current.description;

            var rnd = new Random();
            var shuffledChoices = current.choices.OrderBy(c => rnd.Next()).ToList();

            int y = 10;
            foreach (var choice in shuffledChoices)
            {
                var btn = new Button();
                btn.Text = choice.text;
                btn.Tag = choice;
                btn.Size = new Size(950, 50);
                btn.Location = new Point(10, y);
                btn.Click += ChoiceButton_Click;
                btn.Font = new Font("メイリオ", 11);
                btn.BackColor = Color.LightBlue;
                panelChoices.Controls.Add(btn);
                y += 60;
            }
        }

        private void ChoiceButton_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            var choice = btn.Tag as Choice;

            // 判定
            bool isCorrect = IsChoiceCorrect(choice.semanticNodeId);

            // 色変更：選ばれたボタンだけに適用
            btn.BackColor = isCorrect ? Color.LightGreen : Color.IndianRed;

            // 他の選択肢は無効化（色は変えない）
            foreach (Control c in panelChoices.Controls)
            {
                c.Enabled = false;
            }

            if (!isCorrect)
            {
                lblStatus.Text = "間違った選択です。ゲームオーバー！";
                return;
            }

            lblStatus.Text = "正しい選択です！";

            if (string.IsNullOrEmpty(choice.nextNodeId))
            {
                lblStatus.Text += "\nゲームクリア！";
                return;
            }

            currentNodeId = choice.nextNodeId;

            // 少し待ってから次へ
            var timer = new Timer();
            timer.Interval = 2000;
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                ShowCurrentScenario();
            };
            timer.Start();
        }


        private bool IsChoiceCorrect(string semanticNodeId)
        {
            if (string.IsNullOrEmpty(semanticNodeId)) return false;

            var correctNodes = new HashSet<string>
            {
                "sakamoto", "satsucho", "saigo", "okubo", "iwakura",
                "restoration", "oath", "abolition"
            };

            return correctNodes.Contains(semanticNodeId);
        }




        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "歴史学習システム（意味ネットワーク連携）";
            this.MaximumSize = this.MinimumSize = new Size(1024, 768);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.LightYellow;
        }
        private void CreateCustomUI()
        {
            // 説明テキストボックス
            txtDescription = new TextBox();
            txtDescription.Multiline = true;
            txtDescription.ReadOnly = true;
            txtDescription.Size = new Size(980, 120);
            txtDescription.Location = new Point(20, 20);
            txtDescription.Font = new Font("メイリオ", 11);
            this.Controls.Add(txtDescription);

            // 選択肢パネル
            panelChoices = new Panel();
            panelChoices.AutoScroll = true;
            panelChoices.Size = new Size(980, 250);
            panelChoices.Location = new Point(20, 160);
            this.Controls.Add(panelChoices);

            // ステータス表示ラベル
            lblStatus = new Label();
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(20, 430);
            lblStatus.Font = new Font("メイリオ", 10);
            this.Controls.Add(lblStatus);

            // スタートボタン
            btnStart = new Button();
            btnStart.Text = "ゲームを開始";
            btnStart.Size = new Size(200, 40);
            btnStart.Location = new Point(20, 470);
            btnStart.Click += btnStart_Click;
            btnStart.BackColor = Color.LightGreen;
            this.Controls.Add(btnStart);
        }

    }

    public class Choice
    {
        public string text { get; set; }
        public string nextNodeId { get; set; }
        public string semanticNodeId { get; set; }
    }

    public class ScenarioNode
    {
        public string id { get; set; }
        public string description { get; set; }
        public List<Choice> choices { get; set; }
    }

    public class ScenarioRoot
    {
        [JsonProperty("nodes")]
        public List<ScenarioNode> scenarios { get; set; }
    }

    public class Node
    {
        public string id { get; set; }
        public string label { get; set; }
        public string type { get; set; }
    }

    public class Edge
    {
        public string source { get; set; }
        public string target { get; set; }
        public string relation { get; set; }
    }

    public class HistoryNetwork
    {
        public List<Node> nodes { get; set; }
        public List<Edge> edges { get; set; }
    }

}
