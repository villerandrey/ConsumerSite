using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.DrawingCore.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BE24Services.App_Code
{
    public class CaptchaImage
    {
        /// <summary>
        /// Amount of distortion required.
        /// Default value = 18
        /// </summary>
        public static int Distortion { get; set; } = 18;
        const int Height = 96;
        const int Width = 150;
        const string Fontfamily = "Arial";
        const int Fontsize = 25;

        /// <summary>
        /// Background color to be used.
        /// Default value = Color.Wheat
        /// </summary>
        public static Color BackgroundColor { get; set; } = Color.Wheat;


        /// <summary>
        /// Generates the image with default image properties(150px X 96px) and distortion
        /// </summary>
        /// <param name="captchaCode">Captcha code for which the image has to be generated</param>
        /// <returns>Generated jpeg image as a MemoryStream object</returns>
        public static MemoryStream GenerateImage(string captchaCode)
        {
            return CaptchaImage.BuildImage(captchaCode, Height, Width, Fontsize, Distortion);
        }

        /// <summary>
        /// Generates the image with given image properties
        /// </summary>
        /// <param name="captchaCode">Captcha code for which the image has to be generated</param>
        /// <param name="imageHeight">Height of the image to be generated</param>
        /// <param name="imageWidth">Width of the image to be generated</param>
        /// <param name="fontSize">Font size to be used</param>
        /// <param name="distortion">Distortion required</param>
        /// <returns>Generated jpeg image as a MemoryStream object</returns>
        public static MemoryStream GenerateImage(string captchaCode, int imageHeight, int imageWidth, int fontSize, int distortion)
        {
            return CaptchaImage.BuildImage(captchaCode, imageHeight, imageWidth, fontSize, distortion);
        }


        /// <summary>
        /// Generates the image with given image properties
        /// </summary>
        /// <param name="captchaCode">Captcha code for which the image has to be generated</param>
        /// <param name="imageHeight">Height of the image to be generated</param>
        /// <param name="imageWidth">Width of the image to be generated</param>
        /// <param name="fontSize">Font size to be used</param>
        /// <returns>Generated jpeg image as a MemoryStream object</returns>
        public static MemoryStream GenerateImage(string captchaCode, int imageHeight, int imageWidth, int fontSize)
        {
            return CaptchaImage.BuildImage(captchaCode, imageHeight, imageWidth, fontSize, Distortion);
        }

        /// <summary>
        /// Actual image generator. Internally used.
        /// </summary>
        /// <param name="captchaCode">Captcha code for which the image has to be generated</param>
        /// <param name="imageHeight">Height of the image to be generated</param>
        /// <param name="imageWidth">Width of the image to be generated</param>
        /// <param name="fontSize">Font size to be used</param>
        /// <param name="distortion">Distortion required</param>
        /// <returns>Generated jpeg image as a MemoryStream object</returns>
        private static MemoryStream BuildImage(string captchaCode, int imageHeight, int imageWidth, int fontSize, int distortion)
        {
            int newX, newY;
            MemoryStream memoryStream = new MemoryStream();
            Bitmap captchaImage = new Bitmap(imageWidth, imageHeight, PixelFormat.Format64bppArgb);
            Bitmap cache = new Bitmap(imageWidth, imageHeight, PixelFormat.Format64bppArgb);


            Graphics graphicsTextHolder = Graphics.FromImage(captchaImage);
            graphicsTextHolder.Clear(BackgroundColor);
            graphicsTextHolder.DrawString(captchaCode, new Font(Fontfamily, fontSize, FontStyle.Italic), new SolidBrush(Color.Gray), new PointF(8.4F, 20.4F));

            //Distort the image with a wave function
            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    newX = (int)(x + (distortion * Math.Sin(Math.PI * y / 64.0)));
                    newY = (int)(y + (distortion * Math.Cos(Math.PI * x / 64.0)));
                    if (newX < 0 || newX >= imageWidth) newX = 0;
                    if (newY < 0 || newY >= imageHeight) newY = 0;
                    cache.SetPixel(x, y, captchaImage.GetPixel(newX, newY));
                }
            }

            cache.Save(memoryStream, ImageFormat.Jpeg);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
