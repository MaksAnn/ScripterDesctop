using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.OCR;
using Emgu.CV.Util;
using Emgu.Util;
using System.Drawing.Imaging;

namespace ScripterDesctop
{
    class Menu_NewItem : Scripter
    {
        private Bitmap Scene;

        private Image<Bgr, byte> imgScene;
        public int CardNumbers { get; set; }
        public string CardNStr { get; set; }
        public int ActiveCardNumb { get; set; }
        public Rectangle textRect { get; set; }
        public List<Rectangle> CardAreas { get; set; }
        public List<Rectangle> CardLabelsAreas { get; set; }
        public List<string> CardLabels { get; set; }

        private string tessdataPath = ( System.IO.Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().Location) )
            + "\\tessdata";

        public List<Bitmap> imgSceneRects;

        public Menu_NewItem(Bitmap scene)
        {
            Scene = scene;
            imgScene = new Image<Bgr, byte>(Scene);
        }

        public int FindPosition()
        {
            List<string> alg = new List<string>();
            int position;

            Point R1 = new Point(400, 100);
            Point R2 = new Point(400, 170);
            Point R3 = new Point(400, 330);

            if (IsRed(Scene.GetPixel(R1.X, R1.Y)))
            {
                position = 1;
            }
            else if (IsRed(Scene.GetPixel(R2.X, R2.Y)))
            {
                position = 2;
            }
            else if (IsRed(Scene.GetPixel(R3.X, R3.Y)))
            {
                position = 3;
            }
            else
            {
                position = 0;
            }

            return position;
        }

        private void ItemCountRecogniseByText()
        {
            int recogniseNumb;

            //recognition area
            textRect = new Rectangle(770, 155, 97, 26);

            UMat imgSceneRect = FilterPlate(imgScene.GetSubRect(textRect));

            //text recognizing
            Tesseract ocr = new Tesseract(tessdataPath, "eng", OcrEngineMode.TesseractLstmCombined);

            CardNStr = TextRecognize(imgSceneRect, ocr);

            int.TryParse(string.Join("", CardNStr.Where(c => char.IsDigit(c))), out recogniseNumb);

            CardNumbers = recogniseNumb;
        }

        public void CardsRecognizer()
        {
            ItemCountRecogniseByText();
            List<Rectangle> Rectal = new List<Rectangle>();

            Image<Bgr, byte> imgScene = new Image<Bgr, byte>(Scene);
            Image<Bgr, byte> ingTmp = new Image<Bgr, byte>(Properties.Resources.active_tmp);

            Mat imgOut = new Mat();

            CvInvoke.MatchTemplate(imgScene, ingTmp, imgOut, Emgu.CV.CvEnum.TemplateMatchingType.Sqdiff);

            double minval = 0;
            double maxval = 0;

            Point minloc = new Point();
            Point maxloc = new Point();

            CvInvoke.MinMaxLoc(imgOut, ref minval, ref maxval, ref minloc, ref maxloc);

            Point ActivCardCenter = new Point(minloc.X + 22 - 56, minloc.Y + 23 - 80);
            Point searchPoint = ActivCardCenter;

           
            /* We found center of active card
             * go left and right from this card
             * count steps
             * and remembered ragion cards as rectangles list
             * look like
             *... <-- active --> ...
             */
            int countRigthStep = 0; 
            int countLeftStep = 0; 

            int cardHeight = 142; // Height from center to top point of card
            int cardWidth = 62; 


            searchPoint.X -= cardWidth;
            //Add Active
            Rectal.Add(new Rectangle(ActivCardCenter.X - 56, ActivCardCenter.Y - 80, 110, 160));

            //Go left
            while (true)
            {
                //If red
                if (IsRed(Scene.GetPixel(searchPoint.X - 5, searchPoint.Y)))
                {
                    break;
                }
                //If duplicate
                else if (IsGray(Scene.GetPixel(searchPoint.X - 5, searchPoint.Y)))
                {
                    searchPoint.X -= 5;
                    while (IsGray(Scene.GetPixel(searchPoint.X, searchPoint.Y)))
                    {
                        searchPoint.X -= 2;
                    }

                }
                //If out of range
                else if (searchPoint.X < 50)
                {
                    Rectal.RemoveAt(Rectal.Count - 1);
                    countLeftStep--;
                    break;
                }
                else
                {
                    Rectal.Add(new Rectangle(searchPoint.X - cardWidth, searchPoint.Y - (cardHeight/2), cardWidth, cardHeight));
                    searchPoint.X -= cardWidth;
                    countLeftStep++;
                }
            }

            ActiveCardNumb = countLeftStep;

            //Go to Activ
            searchPoint = ActivCardCenter;
            searchPoint.X += cardWidth;

            //Go right
            while (true)
            {
                Color p = Scene.GetPixel(searchPoint.X, searchPoint.Y);
                if (IsRed(Scene.GetPixel(searchPoint.X+5, searchPoint.Y)))
                {
                    break;
                }
                //If duplicate
                else if (IsGray(Scene.GetPixel(searchPoint.X + 5, searchPoint.Y)))
                {
                    searchPoint.X += 5;
                    while (IsGray(Scene.GetPixel(searchPoint.X, searchPoint.Y)))
                    {
                        searchPoint.X += 2;
                    }
                    
                }
                //If out of range
                else if (searchPoint.X > 889)
                {
                    Rectal.RemoveAt(Rectal.Count - 1);
                    countRigthStep--;
                    break;

                }
                else
                {
                    Rectal.Add(new Rectangle(searchPoint.X, searchPoint.Y - (cardHeight / 2), cardWidth, cardHeight));
                    searchPoint.X += cardWidth;
                    countRigthStep++;
                }
            }

            

            // Sorted cards
            CardAreas = new List<Rectangle>();

            //From last left step
            for (int i = countLeftStep; i > 0; i--)
            {
                //CardAreas.Add(Rectal[ActiveCardNumb - 1 - (countLeftStep - i)]);
                CardAreas.Add(Rectal[i]);
            }

            CardAreas.Add(Rectal[0]);

            //To last right step
            for (int i = 1; i <= countRigthStep; i++)
            {
                CardAreas.Add(Rectal[ActiveCardNumb + i]);
            }

        }

        public void CardLabelsRecognizer()
        {
            CardLabels = new List<string>();

            int CardNumb = CardAreas.Count();

            CardLabelsAreas = new List<Rectangle>();
            imgSceneRects = new List<Bitmap>();

            for (int i = 0; i < CardNumb; i++)
            {
                
                Point TopLeftPointCardArea = new Point();
                int TextAreaWidth;
                int TextAreaHeight;
                int SecondStringHieght;

                Rectangle CardLabelArea;

                
                //Search Text Map Area
                if (i == ActiveCardNumb)
                {
                    TopLeftPointCardArea.Y = 304;
                    TopLeftPointCardArea.X = CardAreas[i].Left + 6;
                    TextAreaWidth = CardAreas[i].Right - 6 - TopLeftPointCardArea.X;
                    SecondStringHieght = 325;
                    
                    //Search second line
                    Point searchPoint = new Point(TopLeftPointCardArea.X, SecondStringHieght);

                    TextAreaHeight = 30;

                    CardLabelArea = new Rectangle(TopLeftPointCardArea.X, TopLeftPointCardArea.Y, TextAreaWidth, TextAreaHeight);

                }
                else
                {
                    TopLeftPointCardArea.Y = 290;
                    TopLeftPointCardArea.X = CardAreas[i].Left;
                    TextAreaWidth = 62;
                    SecondStringHieght = 306;

                    Point searchPoint = new Point(TopLeftPointCardArea.X, SecondStringHieght);

                    TextAreaHeight = 24;

                    CardLabelArea = new Rectangle(TopLeftPointCardArea.X, TopLeftPointCardArea.Y, TextAreaWidth, TextAreaHeight);

                }

                CardLabelsAreas.Add(CardLabelArea);

                UMat imgSceneRect = FilterPlate(imgScene.GetSubRect(CardLabelArea));

                imgSceneRects.Add(imgSceneRect.ToImage<Bgr, byte>().ToBitmap());
                //imgSceneRects.Add(Scene.Clone(CardLabelArea, PixelFormat.Format16bppRgb555));

                Tesseract ocr = new Tesseract(tessdataPath, "eng", OcrEngineMode.TesseractLstmCombined);

                CardLabels.Add(TextRecognize(imgSceneRect, ocr));
            }

        }

        public bool ActiveCardIsPlayer()
        {
            Bitmap PlayerTmpBitmap = new Bitmap(Properties.Resources.player_tmp);
            Bitmap ActivCardScaleBitmap = imgScene.GetSubRect(CardAreas[ActiveCardNumb]).ToBitmap();

            if (TmpMatching(ActivCardScaleBitmap, PlayerTmpBitmap))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
