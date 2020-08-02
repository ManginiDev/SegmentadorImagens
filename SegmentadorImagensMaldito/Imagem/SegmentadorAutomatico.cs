using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace SegmentadorImagensMaldito.Imagem
{
    public class SegmentadorAutomatico
    {
        public static double porcentagemToleranciaDiferente = 0.7;
        public static double porcentagemToleranciaIgual = 0.10;
        public static double porcentagemMaximaDaImagemParaCobrirAntesDeJuntar = 0.90;

        public static List<BitmapSource> Segmentar(List<ImagemProcessar> imagens, int quadrosPorPagina)
        {
            var linhaBase = BytesLinha(ProcessarImagens.BitmapSource2Bitmap(imagens.ElementAt(0).Imagem), 0);

            int indiceImagemAtual = 0;
            int alturaAtual = 0;
            int quadrosAtuais = 0;
            bool encontrouQuadro = false;
            while (indiceImagemAtual < imagens.Count)
            {
                Bitmap bitmapAtual = ProcessarImagens.BitmapSource2Bitmap(imagens.ElementAt(indiceImagemAtual).Imagem);
                for (; alturaAtual < bitmapAtual.Height && quadrosAtuais < quadrosPorPagina; alturaAtual++)
                {
                    if (alturaAtual / (double)bitmapAtual.Height > porcentagemMaximaDaImagemParaCobrirAntesDeJuntar) break;

                    var pixelAtual = bitmapAtual.GetPixel(0, alturaAtual);
                    var porcentagemIgualdade = PorcentagemIgualdadeLinhas(linhaBase, bitmapAtual, alturaAtual);
                    if (!encontrouQuadro && porcentagemIgualdade > porcentagemToleranciaDiferente)
                    {
                        encontrouQuadro = true;
                    }
                    else if (encontrouQuadro && porcentagemIgualdade < porcentagemToleranciaIgual)
                    {
                        encontrouQuadro = false;
                        quadrosAtuais++;
                    }
                }

                if (quadrosAtuais == quadrosPorPagina)
                {
                    var imagensSeparadas = ProcessarImagens.SepararImagem(imagens.ElementAt(indiceImagemAtual), alturaAtual);
                    imagens.RemoveAt(indiceImagemAtual);
                    imagens.Insert(indiceImagemAtual, new ImagemProcessar(imagensSeparadas.ElementAt(0)));

                    if (imagens.Count() - 1 == indiceImagemAtual)
                    {
                        imagens.Add(new ImagemProcessar(imagensSeparadas.ElementAt(1)));
                    }
                    else
                    {
                        var novaImagem = ProcessarImagens.CombinarImagens(imagensSeparadas.ElementAt(1), imagens.ElementAt(indiceImagemAtual + 1).Imagem);
                        imagens.RemoveAt(indiceImagemAtual + 1);
                        imagens.Insert(indiceImagemAtual + 1, new ImagemProcessar(novaImagem));
                    }

                    alturaAtual = 0;
                    quadrosAtuais = 0;
                    encontrouQuadro = false;

                    indiceImagemAtual++;
                } 
                else
                {
                    if (indiceImagemAtual + 1 < imagens.Count)
                    {
                        var novaImagem = ProcessarImagens.CombinarImagens(imagens.ElementAt(indiceImagemAtual), imagens.ElementAt(indiceImagemAtual + 1));
                        imagens.RemoveRange(indiceImagemAtual, 2);
                        imagens.Insert(indiceImagemAtual, new ImagemProcessar(novaImagem));

                        alturaAtual++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            
            return imagens.Select(x => x.Imagem).ToList();
        }

        private static byte[] BytesLinha(Bitmap bitmap, int altura)
        {
            int bytes = bitmap.Width * (Image.GetPixelFormatSize(bitmap.PixelFormat) / 8);
            byte[] bitmapBytes = new byte[bytes];

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, altura, bitmap.Width, 1), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            Marshal.Copy(bitmapData.Scan0, bitmapBytes, 0, bytes);
            bitmap.UnlockBits(bitmapData);

            return bitmapBytes;
        }

        private static double PorcentagemIgualdadeLinhas(byte[] linhaBase, Bitmap bitmap, int altura)
        {
            byte[] bitmapBytes = BytesLinha(bitmap, altura);

            int diferentes = 0;
            for (int indice = 0; indice < linhaBase.Length; indice++)
            {
                if (bitmapBytes[indice] != linhaBase[indice]) diferentes++;
            }

            return diferentes / (double)linhaBase.Length;
        }
    }
}
