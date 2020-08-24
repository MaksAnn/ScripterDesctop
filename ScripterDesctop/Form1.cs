using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ScripterDesctop
{
    public partial class Form1 : Form
    {
        private string GetPictureURL = "http://185.80.129.249:4222/getImage";
        private Menu_Customize menu_Customize;
        private Menu_NewItem menu_NewItem;

        public Form1()
        {
            InitializeComponent();
        }

        //Get Picture
        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            panel1.Hide();

            Bitmap SceneBitmap = SetBitmapScene();
            pictureBox1.Image = SceneBitmap;

            //Searching tmp
            Bitmap CustomiseTmpBitmap = new Bitmap(Properties.Resources.CustomiseTmp);
            Bitmap NewItemTmpBitmap = new Bitmap(Properties.Resources.keepItem_tmp);

            if (TmpMatching(SceneBitmap, CustomiseTmpBitmap))
            {
                panel1.Show();
                label1.Text = "CUSTOMIZE";
                
                menu_Customize = new Menu_Customize(SceneBitmap);
                menu_Customize.FindStartPosition();
                textBox1.Text = menu_Customize.StartPosition.ToString();

            }
            else if (TmpMatching(SceneBitmap, NewItemTmpBitmap))
            {
                label1.Text = "NEW ITEM";
                menu_NewItem = new Menu_NewItem(SceneBitmap);

                int ActivPosition = menu_NewItem.FindPosition();
                switch (ActivPosition)
                {
                    case 1:
                        listBox1.Items.Add("SEND TO TRANSFER LIST");
                        listBox1.Items.Add("<Go bot>");
                        break;
                    case 2:
                        listBox1.Items.Add("NEW ITEM");

                        menu_NewItem.CardsRecognizer();

                        listBox1.Items.Add(menu_NewItem.CardNumbers.ToString() + " Items");
                        listBox1.Items.Add("");
                        listBox1.Items.Add("Number " + (menu_NewItem.ActiveCardNumb + 1).ToString() + " is active");
                        listBox1.Items.Add("");

                        menu_NewItem.CardLabelsRecognizer();

                        listBox1.Items.Add("Card labels:");
                        int i = 0;
                        foreach (string s in menu_NewItem.CardLabels)
                        {
                            listBox1.Items.Add((i+1).ToString() + ". " + s);
                            i++;                     
                        }

                        listBox1.Items.Add("");

                        if (menu_NewItem.ActiveCardIsPlayer())
                        {
                            listBox1.Items.Add("Active card is PLAYER");
                        }
                        else
                        {
                            listBox1.Items.Add("Active card is NOT a PLAYER");
                        }
                        pictureBox1.Image = DrawRectangles(SceneBitmap, menu_NewItem.CardAreas, Color.Yellow);
                        break;
                    case 3:
                        listBox1.Items.Add("QUICK SELL NOW");
                        listBox1.Items.Add("<Go top>");
                        break;
                    default:
                        break;
                }
            }
            else
            {
                label1.Text = "Not for analysis";
            }
            label1.Show();

            pictureBox1.Image = SceneBitmap;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Hide();
            panel1.Hide();
            button1_Click(sender, e);
        }

        //Jump to position (CUSTOMIZE)
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                int ActivPosition = Int32.Parse(textBox1.Text);
                int NewPosition = Int32.Parse(comboBox1.Text);
                if (NewPosition < 1 || NewPosition > 8)
                {
                    throw new Exception("Error. New Position out of range!");
                }
                listBox1.Items.Add("Active panel is " + ActivPosition.ToString());
                listBox1.Items.Add("Go to position " + NewPosition);

                List<string> alg = menu_Customize.ShortWay(ActivPosition, NewPosition);

                foreach (string s in alg)
                {
                    listBox1.Items.Add(s);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private Image DownloadImageByURL(string fromUrl)
        {
            using (System.Net.WebClient webClient = new System.Net.WebClient())
            {
                using (Stream stream = webClient.OpenRead(fromUrl))
                {
                    return Image.FromStream(stream);
                }
            }
        }

        private bool TmpMatching(Bitmap Scene, Bitmap Tmp)
        {

            Image<Bgr, byte> imgScene = new Image<Bgr, byte>(Scene);
            Image<Bgr, byte> ingTmp = new Image<Bgr, byte>(Tmp);

            Mat imgOut = new Mat();
            CvInvoke.MatchTemplate(imgScene, ingTmp, imgOut, Emgu.CV.CvEnum.TemplateMatchingType.Sqdiff);

            double minval = 0;
            double maxval = 0;

            Point minloc = new Point();
            Point maxloc = new Point();

            CvInvoke.MinMaxLoc(imgOut, ref minval, ref maxval, ref minloc, ref maxloc);

            if (minval < 50000)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private Bitmap DrawRectangles(Bitmap Scene, List<Rectangle> rectangles, Color color)
        {
            using (Graphics graphics = Graphics.FromImage(Scene))
            {
                using (Pen pen = new Pen(color, 1))
                {
                    foreach (Rectangle r in rectangles)
                    {
                        graphics.DrawRectangle(pen, r);
                    }
                    graphics.DrawRectangle(pen, menu_NewItem.textRect);
                   
                }
            }
            return Scene;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                int start = Int32.Parse(textBox1.Text);
                int finish = Int32.Parse(comboBox1.Text);
                if (finish < 1 || finish > 8)
                {
                    throw new Exception("Error! New position out of range");
                }

                List<string> alg = menu_Customize.ShortWay(start, finish);
                foreach (string s in alg)
                {
                    listBox1.Items.Add(s);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private Bitmap SetBitmapScene()
        {
            Bitmap SceneBitmap;
            if (radioButton2.Checked)
            {
                SceneBitmap = new Bitmap(Properties.Resources.keep_2);
            }
            else if (radioButton1.Checked)
            {
                SceneBitmap = new Bitmap(Properties.Resources.keep_item);
            }
            else if (radioButton3.Checked)
            {
                SceneBitmap = new Bitmap(Properties.Resources.keep_item_3);
            }
            else if (radioButton5.Checked)
            {
                SceneBitmap = new Bitmap(Properties.Resources.cust_1);
            }
            else if (radioButton6.Checked)
            {
                SceneBitmap = new Bitmap(Properties.Resources.cust_2);
            }
            else if (radioButton7.Checked)
            {
                SceneBitmap = new Bitmap(Properties.Resources.cust_3);
            }
            else
            {
                radioButton4.Checked = true;
                SceneBitmap = new Bitmap(DownloadImageByURL(GetPictureURL));
            }

            return SceneBitmap;
        }

    }
}
