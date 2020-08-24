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
    abstract class Scripter
    {
        public bool TmpMatching(Bitmap Scene, Bitmap Tmp)
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

        public bool IsRed(Color color)
        {

            if ((color.R >= 200 && color.R <= 255) &&
                 (color.G >= 30 && color.G <= 80) &&
                 (color.B >= 80 && color.B <= 120))
            {

                return true;
            }
            else
            {

                return false;
            }
        }

        public bool IsGray(Color color)
        {
            if ((color.R >= 60 && color.R <= 90) &&
                 (color.G >= 70 && color.G <= 100) &&
                 (color.B >= 80 && color.B <= 120))
            {

                return true;
            }
            else
            {

                return false;
            }
        }

        public bool IsDark(Color color)
        {
            //68 53 32
            if ((color.R >= 63 && color.R <= 73) &&
                 (color.G >= 48 && color.G <= 66) &&
                 (color.B >= 37 && color.B <= 37))
            {

                return true;
            }
            else
            {

                return false;
            }
        }


        public string TextRecognize(Image<Bgr, byte> imgSceneRect, Tesseract ocr)
        {

            string str;
            //ocr.SetImage(FilterPlate(imgSceneRect));
            ocr.SetImage(imgSceneRect);
            ocr.Recognize();

            str = ocr.GetUTF8Text();
            ocr.Dispose();

            return str;
        }
        public string TextRecognize(UMat plate, Tesseract ocr)
        {

            string str;
            //ocr.SetImage(FilterPlate(imgSceneRect));
            ocr.SetImage(plate);
            ocr.Recognize();

            str = ocr.GetUTF8Text();
            ocr.Dispose();

            return str;
        }

        public UMat FilterPlate(Image<Bgr, byte> imgSceneRect)
        {
            UMat plate = imgSceneRect.ToUMat();
            UMat thresh = new UMat();
            CvInvoke.Threshold(plate, thresh, 120, 255, ThresholdType.BinaryInv);

            return thresh;
        }

    }
}
