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


namespace ScripterDesctop
{
    class Menu_Customize : Scripter
    {
        private Bitmap Scene;

        public int StartPosition { get; set; }

        public Menu_Customize(Bitmap scene)
        {
            Scene = scene;
        }

        public void FindStartPosition()
        {

            Point R1 = new Point(177, 225);
            Point R2 = new Point(372, 225);
            Point R3 = new Point(538, 225);
            Point R4 = new Point(725, 225);
            Point R5 = new Point(177, 393);
            Point R6 = new Point(372, 393);
            Point R7 = new Point(538, 393);
            Point R8 = new Point(725, 393);

            if (IsRed(Scene.GetPixel(R1.X, R1.Y)))
            {
                StartPosition = 1;
            }
            else if (IsRed(Scene.GetPixel(R2.X, R2.Y)))
            {
                StartPosition = 2;
            }
            else if (IsRed(Scene.GetPixel(R3.X, R3.Y)))
            {
                StartPosition = 3;
            }
            else if (IsRed(Scene.GetPixel(R4.X, R4.Y)))
            {
                StartPosition = 4;
            }
            else if (IsRed(Scene.GetPixel(R5.X, R5.Y)))
            {
                StartPosition = 5;
            }
            else if (IsRed(Scene.GetPixel(R6.X, R6.Y)))
            {
                StartPosition = 6;
            }
            else if (IsRed(Scene.GetPixel(R7.X, R7.Y)))
            {
                StartPosition = 7;
            }
            else if (IsRed(Scene.GetPixel(R8.X, R8.Y)))
            {
                StartPosition = 8;
            }
            else
            {
                StartPosition = 0;
            }

        }

        public List<string> ShortWay(int Start, int Finish)
        {
            List<string> alg = new List<string>();

            int StepsCounter = 0;
            if (Start <= 4 && Finish >= 5)
            {
                alg.Add("<Go bot>");
                StepsCounter++; Start += 4;
            }
            else if (Finish <= 4 && Start >= 5)
            {
                alg.Add("<Go top>");
                StepsCounter++;
                Start -= 4;
            }
            while (Start != Finish)
            {
                if (Start < Finish)
                {
                    alg.Add("<Go right>");
                    StepsCounter++;
                    Start++;
                }
                else
                {
                    alg.Add("<Go left>");
                    StepsCounter++;
                    Start--;
                }
            }
            return alg;
        }
    }
}
