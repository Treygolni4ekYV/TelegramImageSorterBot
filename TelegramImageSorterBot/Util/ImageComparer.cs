using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TelegramImageSorterBot.Util
{
    internal class ImageComparer
    {
        public readonly Size CompareSize;
        private readonly int maxPixels;

        public ImageComparer(int height = 100, int width = 100) 
        {
            CompareSize = new Size(height, width);

            maxPixels = height*width;
        }


        //returns percentage of similarity of images (only jpeg)
        public double CompareImages(Image<Rgb24> sourseImage, Image<Rgb24> comparedImage)
        {
            int similarPixelsCount = 0;

            sourseImage.Mutate(x => x.Resize(CompareSize).Grayscale());
            comparedImage.Mutate(x => x.Resize(CompareSize).Grayscale());

            for (int i = 0; i < CompareSize.Height; i++)
            {
                for (int j = 0; j < CompareSize.Width; j++)
                {
                    if (CompareRgb24Pixels(sourseImage[i, j], comparedImage[i, j]))
                    {
                        similarPixelsCount ++;
                    }
                }
            }

            return (double) similarPixelsCount / (double) maxPixels;
        }

        public bool CompareRgb24Pixels(Rgb24 pixel1, Rgb24 pixel2)
        {
            var colors1 = Rgb24ToТormalizedColorsMass(pixel1);
            var colors2 = Rgb24ToТormalizedColorsMass(pixel2);

            if (colors1.Length != colors2.Length)
            {
                throw new Exception("Collor mass has wrong size");
            }

            if (colors1[0] != colors2[0] | colors1[1] != colors2[1] | colors1[2] != colors2[2])
            {
                return false;
            }

            return true;
        }

        public int[] Rgb24ToТormalizedColorsMass(Rgb24 pixel)
        {
            var colors = new int[3];

            colors[0] = Convert.ToInt32(Math.Round(pixel.R / 28.3));
            colors[1] = Convert.ToInt32(Math.Round(pixel.G / 28.3));
            colors[2] = Convert.ToInt32(Math.Round(pixel.B / 28.3));

            return colors;
        }
    }
}
