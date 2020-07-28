using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace SegmentadorImagensMaldito.Imagem
{
    public class ProcessarImagens
    {
        public static List<BitmapImage> SepararImagem(ImagemProcessar imagemProcessar, int altura)
        {
            var imagem = BitmapImage2Bitmap(imagemProcessar.Imagem);
            var primeiraImagem = imagem.Clone(new Rectangle(0, 0, imagem.Width, altura), imagem.PixelFormat);
            var segundaImagem = imagem.Clone(new Rectangle(0, altura, imagem.Width, imagem.Height - altura), imagem.PixelFormat);

            return new List<BitmapImage>() { Bitmap2BitmapImage(primeiraImagem), Bitmap2BitmapImage(segundaImagem) };
        }

        public static BitmapImage CombinarImagens(ImagemProcessar primeiraImagem, ImagemProcessar segundaImagem)
        {
            List<Bitmap> imagens = new List<Bitmap>() { BitmapImage2Bitmap(primeiraImagem.Imagem), BitmapImage2Bitmap(segundaImagem.Imagem) };
            Bitmap imagemFinal = null;

            try
            {
                int largura = imagens.Max(x => x.Width);
                int altura = imagens.Sum(x => x.Height);

                imagemFinal = new Bitmap(largura, altura);

                using Graphics graphics = Graphics.FromImage(imagemFinal);
                graphics.Clear(Color.Transparent);

                int distanciaAltura = 0;
                foreach (Bitmap image in imagens)
                {
                    graphics.DrawImage(image, new Rectangle(0, distanciaAltura, image.Width, image.Height));
                    distanciaAltura += image.Height;
                }

                return Bitmap2BitmapImage(imagemFinal);
            }
            catch (Exception)
            {
                if (imagemFinal != null) imagemFinal.Dispose();
                throw;
            }
            finally
            {
                foreach (Bitmap image in imagens) image.Dispose();
            }
        }

        private static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using MemoryStream outStream = new MemoryStream();

            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapImage));
            enc.Save(outStream);

            Bitmap bitmap = new Bitmap(outStream);
            return new Bitmap(bitmap);
        }

        public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using var memory = new MemoryStream();

            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}
