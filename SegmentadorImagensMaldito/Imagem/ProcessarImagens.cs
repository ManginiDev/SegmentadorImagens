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

        public static List<BitmapImage> SepararUnirImagem(ImagemProcessar imagemUnir, ImagemProcessar imagemSeparar, int altura)
        {
            var imagem = BitmapImage2Bitmap(imagemSeparar.Imagem);
            var primeiraImagem = imagem.Clone(new Rectangle(0, 0, imagem.Width, altura), imagem.PixelFormat);
            var segundaImagem = imagem.Clone(new Rectangle(0, altura, imagem.Width, imagem.Height - altura), imagem.PixelFormat);
            var novaImagem = CombinarImagens(imagemUnir, primeiraImagem);

            return new List<BitmapImage>() { novaImagem, Bitmap2BitmapImage(segundaImagem) };
        }

        public static BitmapImage CombinarImagens(ImagemProcessar primeiraImagem, ImagemProcessar segundaImagem)
        {
            return CombinarImagens(BitmapImage2Bitmap(primeiraImagem.Imagem), BitmapImage2Bitmap(segundaImagem.Imagem));
        }

        public static BitmapImage CombinarImagens(ImagemProcessar primeiraImagem, Bitmap segundaImagem)
        {
            return CombinarImagens(BitmapImage2Bitmap(primeiraImagem.Imagem), segundaImagem);
        }
        public static BitmapImage CombinarImagens(BitmapImage primeiraImagem, BitmapImage segundaImagem)
        {
            return CombinarImagens(BitmapImage2Bitmap(primeiraImagem), BitmapImage2Bitmap(segundaImagem));
        }

        public static BitmapImage CombinarImagens(Bitmap primeiraImagem, Bitmap segundaImagem)
        {
            List<Bitmap> imagens = new List<Bitmap>() { primeiraImagem, segundaImagem };
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
                    int distanciaLargura = 0;
                    if (image.Width < largura)
                    {
                        distanciaLargura = (largura - image.Width)/ 2;
                    }

                    graphics.DrawImage(image, new Rectangle(distanciaLargura, distanciaAltura, image.Width, image.Height));
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
