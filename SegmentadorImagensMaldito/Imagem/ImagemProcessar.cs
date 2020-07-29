using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SegmentadorImagensMaldito
{
    public class ImagemProcessar
    {
        public string Nome { get; private set; }
        public string CaminhoCompleto { get; private set; }
        public BitmapSource Thumbnail { get; private set; }
        public BitmapImage Imagem { get; private set; }
        
        public ImagemProcessar(string caminhoCompleto)
        {
            CaminhoCompleto = caminhoCompleto;
            Nome = Path.GetFileNameWithoutExtension(caminhoCompleto);
        }
        public ImagemProcessar(BitmapImage imagem)
        {
            Imagem = imagem;
            Thumbnail = new TransformedBitmap(Imagem, new ScaleTransform(0.25, 0.25));
        }

        public void CarregarImagem()
        {
            Imagem = new BitmapImage();
            var stream = File.OpenRead(CaminhoCompleto);

            Imagem.BeginInit();
            Imagem.CacheOption = BitmapCacheOption.OnLoad;
            Imagem.StreamSource = stream;
            Imagem.EndInit();

            stream.Close();
            stream.Dispose();

            Thumbnail = new TransformedBitmap(Imagem, new ScaleTransform(0.25, 0.25));

        }

        public void AlterarImagem(BitmapImage imagem)
        {
            Imagem = imagem;
            Thumbnail = new TransformedBitmap(Imagem, new ScaleTransform(0.25, 0.25));
        }
    }
}
