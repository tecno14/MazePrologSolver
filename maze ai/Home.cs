using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SbsSW.SwiPlCs;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace maze_ai
{
    public partial class Home : Form
    {
        public Home()
        {
            InitializeComponent();
        }

        public readonly int StartSizeRow = 4;
        public readonly int StartSizeCol = 4;

        public readonly string maze_source_code_path = "maze_source_for_app.pl";
        public readonly string maze_code_path = "maze.pl";

        public readonly int maxSize = 20;
        public readonly int minSize = 2;
        public static string Goal = "GOAL";
        public static string Start = "START";

        public static Color closeBlockColor = Color.FromArgb(38, 38, 38);
        public static Color openBlockColor = Color.FromArgb(222, 222, 222);
        public static Color StartTextColor = Color.FromArgb(237, 28, 36);
        public static Color GoalTextColor = Color.FromArgb(0, 32, 96);
        public static Color PathTextColor = Color.Green;

        public static List<Button> MazeBlocks;
        public static Button StartBlock = null;
        public static Button GoalBlock = null;

        public static int MazeRowsCount = 4;
        public static int MazeColumsCount = 4;

        public readonly string Breadth = "Breadth-First Search Algorithm";
        public readonly string Depth = "Depth-First Search Algorithm";

        public void CreateMaze()
        {
            lbStatus.Text = "";
            try
            {
                //get size
                int Y = int.Parse(udRowSize.Value.ToString());//rows
                int X = int.Parse(udcloumSize.Value.ToString());//coloms 

                if (((Y < minSize) || (Y > maxSize)) || ((X < minSize) || (X > maxSize)))
                    throw new Exception("min " + minSize + " max " + maxSize);

                MazeColumsCount = X;
                MazeRowsCount = Y;
                PrintMazeMap();

                cbStartGoalRelodItems();

                cbStartBlock.SelectedItem = cbStartBlock.Items[0];
                cbGoalBlock.SelectedItem = cbGoalBlock.Items[cbGoalBlock.Items.Count - 1];

                btnChangeSG_Click(new object(), new EventArgs());

                lbStatus.Text = "Done";
            }
            catch (Exception ex)
            { lbStatus.Text = "Error : " + ex.Message; }
        }
        public void cbStartGoalRelodItems()
        {
            cbStartBlock.Items.Clear();
            cbStartBlock.Text = "";
            cbGoalBlock.Items.Clear();
            cbGoalBlock.Text = "";

            foreach (Button b in MazeBlocks)
            {
                cbGoalBlock.Items.Add(b.Name);
                cbStartBlock.Items.Add(b.Name);
            }
        }
        public void PrintMazeMap()
        {
            MazeBlocks = new List<Button>();
            TableLayoutPanel table = new TableLayoutPanel()
            {
                ColumnCount = MazeColumsCount,
                RowCount = MazeRowsCount
            };
            table.Location = new Point(3, 16);
            table.Dock = DockStyle.Fill;
            Button btn;
            for (int x = 0; x < MazeColumsCount; x++)
            {
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                for (int y = 0; y < MazeRowsCount; y++)
                {
                    table.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
                    btn = CreateNewButton(string.Format("[ {0},{1} ]", y, x));
                    table.Controls.Add(btn, x, y);
                    MazeBlocks.Add(btn);
                }
            }
            this.groupBox.Controls.Clear();
            this.groupBox.Controls.Add(table);
        }
        public Button CreateNewButton(string name)
        {
            Button btn = new Button()
            {
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(0, 0),
                ForeColor = Color.Black,
                BackColor = closeBlockColor,
                Margin = new Padding(0),
                TabIndex = 0,
                Name = name,
                //Text = name,
                Font = new Font("Tahoma", 8F, FontStyle.Bold),
                UseVisualStyleBackColor = false
            };
            btn.Click += new EventHandler(Block_Click);
            return btn;
        }
        public void Block_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if ((b.Text == Start) || (b.Text == Goal))
                return;
            b.ForeColor = (b.BackColor == closeBlockColor ? openBlockColor : Color.Black);
            b.BackColor = (b.BackColor == closeBlockColor ? openBlockColor : closeBlockColor);
        }
        private void btnChangeSG_Click(object sender, EventArgs e)
        {
            if (StartBlock != null)
            {
                StartBlock.Text = "";
                StartBlock.ForeColor = openBlockColor;
                StartBlock = null;
            }
            if (GoalBlock != null)
            {
                GoalBlock.Text = "";
                GoalBlock.ForeColor = openBlockColor;
                GoalBlock = null;
            }

            foreach (Button b in MazeBlocks)
                if (b.Name == cbStartBlock.Text)
                { b.BackColor = openBlockColor; b.ForeColor = StartTextColor; b.Text = Start; StartBlock = b; break; }

            foreach (Button b in MazeBlocks)
                if (b.Name == cbGoalBlock.Text)
                { b.BackColor = openBlockColor; b.ForeColor = GoalTextColor; b.Text = Goal; GoalBlock = b; break; }

           
        }
        private void btnEnter_Click(object sender, EventArgs e)
        {
            btnEnter.Enabled = false;
            CreateMaze();
            btnEnter.Enabled = true;
        }
        private void Home_Load(object sender, EventArgs e)
        {
            Environment.SetEnvironmentVariable("SWI_HOME_DIR", @"prolog");
            Environment.SetEnvironmentVariable("Path", @"prolog");
            Environment.SetEnvironmentVariable("Path", @"prolog\\bin");

            cbMethod.Items.Clear();
            cbMethod.Items.Add(Depth);
            cbMethod.Items.Add(Breadth);
            cbMethod.SelectedItem = cbMethod.Items[0];

            udcloumSize.Maximum = maxSize;
            udRowSize.Maximum = maxSize;
            udcloumSize.Minimum = minSize;
            udRowSize.Minimum = minSize;

            udcloumSize.Value = StartSizeCol;
            udRowSize.Value = StartSizeRow;
            CreateMaze();
        }
        private void btnSolve_Click(object sender, EventArgs e)
        {
            btnSolve.Enabled = false;
            foreach (Button b in MazeBlocks)
            { if ((b.Name != cbStartBlock.Text) && (b.Name != cbGoalBlock.Text)) b.Text = ""; b.ForeColor = (b.BackColor == closeBlockColor ? Color.Black : openBlockColor); }

            if (((StartBlock == null) || (GoalBlock == null)) || (StartBlock == GoalBlock))
            {
                lbStatus.Text = "please select start and goal block";
                return;
            }
            lbStatus.Text = "";
            SolveMaze();

            btnChangeSG_Click(sender, e);
            btnSolve.Enabled = true;
        }
        public void SolveMaze()
        {
            //get solv
            //print the solv

            try
            {
                WritePLFile(NewMazeCode());
                //Solv
                string solv = "";

                if (PlEngine.IsInitialized)
                    PlEngine.PlCleanup(); 

                string[] p = { "-q", "-f", maze_code_path };
                PlEngine.Initialize(p);

                PlQuery q = new PlQuery("search(R).");
               
                foreach (PlQueryVariables v in q.SolutionVariables)
                    solv += v["R"];

                if (solv == "")
                { lbStatus.Text = "There are no answer"; return; }

                PrintSol(solv);
            }
            catch (Exception ex)
            {
                lbStatus.Text = ex.Message;
            }
            if(PlEngine.IsInitialized)
                PlEngine.PlCleanup();
        }
        public void PrintSol(string sol)
        {
            sol = sol.Replace('"', ' ');
            sol = sol.Replace(" ","");
            sol = sol.Remove(0, 1);
            sol = sol.Remove(sol.Length-1,1);

            Regex r = new Regex(@"block[(]([^)]*)[)]([^)]*)[)]");
            MatchCollection moves = r.Matches(sol);//moves

            Regex pos = new Regex(@"[[][0-9]+[,][0-9]+\]");//[0-9]+[,][0-9]+

            Regex cost = new Regex(@"[)][,][0-9]*[)]");

            for(int i=0;i<moves.Count;i++) //(string i in moves)
            {
                foreach (Button b in MazeBlocks)
                {
                    if (b.Name.Replace(" ","") == pos.Match(moves[i].ToString()).ToString())
                    {
                        b.Text = i.ToString(); b.Font = new Font("Tahoma", 8F, FontStyle.Bold); b.ForeColor = PathTextColor;
                    }
                }

            }
            lbStatus.Text = string.Format("Cost : {0} ", moves.Count - 1);
            
        }
        public string NewMazeCode()
        {
            string Source_Code = MazeSource_Code();
            Source_Code = Source_Code.Replace(@"/*MapHere*/", GetMazeMap_code());
            Source_Code = Source_Code.Replace(@"/*StartHere*/", GetStartGoalBlock_Code("start"));
            Source_Code = Source_Code.Replace(@"/*GoalHere*/", GetStartGoalBlock_Code("goal"));
            Source_Code = Source_Code.Replace(@"/*AlgorithmHere*/", GetAlgorithm());

            return Source_Code;
        }
        public string GetAlgorithm()
        {
            if (cbMethod.Text == Depth)
                return @"insert_all(NewStates,Fringe,NewFringe):-append(NewStates,Fringe,NewFringe).";
            else if (cbMethod.Text == Breadth)
                return @"insert_all(NewStates,Fringe,NewFringe):-append(Fringe,NewStates,NewFringe).";
            else
                throw new Exception("please select search algorithm");
        }

        public string GetStartGoalBlock_Code(string str)
        {
            string[] Site;
            if (str == "start")
                Site = StartBlock.Name.Replace("[", "").Replace("]", "").Replace(" ", "").Split(',');
            else
                Site = GoalBlock.Name.Replace("[", "").Replace("]", "").Replace(" ", "").Split(',');

            return string.Format("{0}Block([X,Y]):-X is {1},Y is {2}.", str, Site[0], Site[1]);
        }
        public void WritePLFile(string NewCode)
        {
            if (File.Exists(maze_code_path))
                File.Delete(maze_code_path);
            FileStream fs = new FileStream(maze_code_path, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(NewCode);
            sw.Close();
            fs.Close();

        }
        public string GetMazeMap_code()
        {
            //openedBlocks([row,col]).
            string MazeMap = "";
            string tmp;
            string rtmp, ctmp;
            foreach (Button b in MazeBlocks)
            {
                if (b.BackColor != openBlockColor)
                    continue;
                tmp = b.Name.Replace(" ", "").Replace("[", "").Replace("]", "");
                rtmp = tmp.Split(',')[0];
                ctmp = tmp.Split(',')[1];
                MazeMap += string.Format("openedBlocks([{0},{1}]). ", rtmp, ctmp);
            }
            return MazeMap;
        }
        private void Home_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (PlEngine.IsInitialized)
                PlEngine.PlCleanup();
        }//////
        public string MazeSource_Code()
        {
            FileStream fs = new FileStream(maze_source_code_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader sr = new StreamReader(fs);
            string code = sr.ReadToEnd();
            sr.Close();
            fs.Close();
            return code;
        }
    }
}
