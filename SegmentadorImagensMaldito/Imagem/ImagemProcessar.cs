using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace SegmentadorImagensMaldito
{
    public class ImagemProcessar
    {
        public string Nome { get; private set; }
        public string CaminhoCompleto { get; private set; }
        public BitmapImage Imagem { get; private set; }
        
        public ImagemProcessar(string caminhoCompleto)
        {
            CaminhoCompleto = caminhoCompleto;
            Nome = Path.GetFileNameWithoutExtension(caminhoCompleto);
        }
        public ImagemProcessar(BitmapImage imagem)
        {
            Imagem = imagem;
        }

        public void CarregarImagem()
        {
            Imagem = new BitmapImage(new Uri(CaminhoCompleto));
        }
    }
}
