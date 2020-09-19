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
        double porcentagemToleranciaDiferente { get; set; }
        double porcentagemToleranciaIgual { get; set; }
        double porcentagemMaximaDaImagemParaCobrirAntesDeJuntar { get; set; }
        int numeroLinhasParaAmostragemLinhaBase { get; set; }

        public static SegmentadorAutomatico InicializarPorSettings()
        {
            var segmentador = new SegmentadorAutomatico();
            segmentador.RecarregarPorSettings();

            return segmentador;
        }

        public void RecarregarPorSettings()
        {
            porcentagemToleranciaDiferente = Properties.Settings.Default.porcentagemToleranciaDiferente;
            porcentagemToleranciaIgual = Properties.Settings.Default.porcentagemToleranciaIgual;
            porcentagemMaximaDaImagemParaCobrirAntesDeJuntar = Properties.Settings.Default.porcentagemMaximaDaImagemParaCobrirAntesDeJuntar;
            numeroLinhasParaAmostragemLinhaBase = Properties.Settings.Default.numeroLinhasParaAmostragemLinhaBase;
        }

        public List<BitmapSource> Segmentar(List<ImagemProcessar> imagens, int quadrosPorPagina)
        {
            var linhaBase = LinhaBase(ProcessarImagens.BitmapSource2Bitmap(imagens.ElementAt(0).Imagem));

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
                    if(alturaAtual < imagens.ElementAt(indiceImagemAtual).Imagem.Height)
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

        private byte[] LinhaBase(Bitmap bitmap)
        {
            Dictionary<byte[], double> linhas = new Dictionary<byte[], double>();
            for (int indice = 0; indice < numeroLinhasParaAmostragemLinhaBase; indice++) linhas.Add(BytesLinha(bitmap, indice), 0D);
            for (int indicePrincipal = 0; indicePrincipal < linhas.Count; indicePrincipal++)
            {
                double totalDiferenca = 0D;
                for (int indiceSecundario = 0; indiceSecundario < linhas.Count; indiceSecundario++)
                {
                    if (indicePrincipal != indiceSecundario)
                    {
                        totalDiferenca += PorcentagemIgualdadeByteArray(linhas.ElementAt(indicePrincipal).Key, linhas.ElementAt(indiceSecundario).Key);
                    }

                    linhas[linhas.ElementAt(indicePrincipal).Key] = totalDiferenca;
                }
            }

            return linhas.OrderBy(x => x.Value).FirstOrDefault().Key;
        }

        private static double PorcentagemIgualdadeByteArray(byte[] valor1, byte[] valor2)
        {
            int diferentes = 0;
            for (int indice = 0; indice < valor1.Length; indice++)
            {
                if (valor1[indice] != valor2[indice]) diferentes++;
            }

            return diferentes / (double)valor1.Length;
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
