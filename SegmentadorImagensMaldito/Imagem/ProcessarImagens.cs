using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SegmentadorImagensMaldito.Imagem
{
    public class ProcessarImagens
    {
        public static List<BitmapSource> SepararImagem(ImagemProcessar imagemProcessar, int altura)
        {
            var imagem = BitmapSource2Bitmap(imagemProcessar.Imagem);
            var primeiraImagem = imagem.Clone(new Rectangle(0, 0, imagem.Width, altura), imagem.PixelFormat);
            var segundaImagem = imagem.Clone(new Rectangle(0, altura, imagem.Width, imagem.Height - altura), imagem.PixelFormat);

            return new List<BitmapSource>() { Bitmap2BitmapSource(primeiraImagem), Bitmap2BitmapSource(segundaImagem) };
        }

        public static List<BitmapSource> SepararUnirImagem(ImagemProcessar imagemUnir, ImagemProcessar imagemSeparar, int altura)
        {
            var imagem = BitmapSource2Bitmap(imagemSeparar.Imagem);
            var primeiraImagem = imagem.Clone(new Rectangle(0, 0, imagem.Width, altura), imagem.PixelFormat);
            var segundaImagem = imagem.Clone(new Rectangle(0, altura, imagem.Width, imagem.Height - altura), imagem.PixelFormat);
            var novaImagem = CombinarImagens(imagemUnir, primeiraImagem);

            return new List<BitmapSource>() { novaImagem, Bitmap2BitmapSource(segundaImagem) };
        }

        public static BitmapSource CombinarImagens(ImagemProcessar primeiraImagem, ImagemProcessar segundaImagem)
        {
            return CombinarImagens(BitmapSource2Bitmap(primeiraImagem.Imagem), BitmapSource2Bitmap(segundaImagem.Imagem));
        }

        public static BitmapSource CombinarImagens(List<ImagemProcessar> imagens)
        {
            var imagemAtual = imagens.ElementAt(0).Imagem;
            for (int indice = 1; indice < imagens.Count; indice++)
            {
                imagemAtual = CombinarImagens(imagemAtual, imagens.ElementAt(indice).Imagem);
            }

            return imagemAtual;
        }

        public static BitmapSource CombinarImagens(Bitmap primeiraImagem, ImagemProcessar segundaImagem)
        {
            return CombinarImagens(primeiraImagem, BitmapSource2Bitmap(segundaImagem.Imagem));
        }
        public static BitmapSource CombinarImagens(ImagemProcessar primeiraImagem, Bitmap segundaImagem)
        {
            return CombinarImagens(BitmapSource2Bitmap(primeiraImagem.Imagem), segundaImagem);
        }
        public static BitmapSource CombinarImagens(BitmapSource primeiraImagem, BitmapSource segundaImagem)
        {
            return CombinarImagens(BitmapSource2Bitmap(primeiraImagem), BitmapSource2Bitmap(segundaImagem));
        }

        public static BitmapSource CombinarImagens(Bitmap primeiraImagem, Bitmap segundaImagem)
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
                        distanciaLargura = (largura - image.Width) / 2;
                    }

                    graphics.DrawImage(image, new Rectangle(distanciaLargura, distanciaAltura, image.Width, image.Height));
                    distanciaAltura += image.Height;
                }

                return Bitmap2BitmapSource(imagemFinal);
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

        public static Bitmap BitmapSource2Bitmap(BitmapSource bitmapSource)
        {
            Bitmap bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, PixelFormat.Format32bppPArgb);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            bitmapSource.CopyPixels(Int32Rect.Empty, bitmapData.Scan0, bitmapData.Height * bitmapData.Stride, bitmapData.Stride);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource Bitmap2BitmapSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();

            try
            {
                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                bitmapSource.Freeze();

                return bitmapSource;
            }
            finally
            {
                DeleteObject(hBitmap);
            }
        }
    }
}
