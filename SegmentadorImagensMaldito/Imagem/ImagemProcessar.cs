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
        public BitmapSource Imagem { get; private set; }
        
        public ImagemProcessar(string caminhoCompleto)
        {
            CaminhoCompleto = caminhoCompleto;
            Nome = Path.GetFileNameWithoutExtension(caminhoCompleto);
        }
        public ImagemProcessar(BitmapSource imagem)
        {
            Imagem = imagem;
            Thumbnail = new TransformedBitmap(Imagem, new ScaleTransform(0.25, 0.25));
        }

        public void CarregarImagem()
        {
            BitmapImage novaImagem = new BitmapImage();
            var stream = File.OpenRead(CaminhoCompleto);

            novaImagem.BeginInit();
            novaImagem.CacheOption = BitmapCacheOption.OnLoad;
            novaImagem.StreamSource = stream;
            novaImagem.EndInit();

            stream.Close();
            stream.Dispose();

            novaImagem.Freeze();
            Imagem = novaImagem;
            Thumbnail = new TransformedBitmap(Imagem, new ScaleTransform(0.25, 0.25));

        }

        public void AlterarImagem(BitmapSource imagem)
        {
            Imagem = imagem;
            Thumbnail = new TransformedBitmap(Imagem, new ScaleTransform(0.25, 0.25));
        }
    }
}
