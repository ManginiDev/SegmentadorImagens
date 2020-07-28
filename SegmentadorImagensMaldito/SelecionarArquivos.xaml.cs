using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SegmentadorImagensMaldito
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<ImagemProcessar> arquivos = new ObservableCollection<ImagemProcessar>();

        public MainWindow()
        {
            InitializeComponent();
            arquivosListBox.Visibility = Visibility.Hidden;
        }

        #region ListBox
        private void RemoverItem_Click(object sender, RoutedEventArgs e)
        {
            Button botao = (Button)sender;
            if (botao.DataContext is ImagemProcessar imagemProcessar)
            {
                arquivos.Remove(imagemProcessar);
                ArquivosAlterados();
            }
        }
        #endregion

        #region Arquivos
        private void EscolherArquivosClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
#if DEBUG
            openFileDialog.InitialDirectory = $@"C:\Users\Mauricio\Desktop\14";
#else
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
#endif
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var caminhoArquivo in openFileDialog.FileNames)
                {
                    arquivos.Add(new ImagemProcessar(caminhoArquivo));
                }
                arquivosListBox.DataContext = arquivos;

                ArquivosAlterados();
            }
        }

        private void RemoverArquivosClick(object sender, RoutedEventArgs e)
        {
            arquivos.Clear();
            ArquivosAlterados();
        }

        private void ArquivosAlterados()
        {
            if (arquivos.Count > 0)
            {
                arquivosListBox.Visibility = Visibility.Visible;
                avisosText.Text = "Arraste os itens da lista para reordenar, quando estiver tudo certo, clique em 'Continuar'";
                removerArquivosButton.IsEnabled = true;
                continuarButton.IsEnabled = true;
            }
            else
            {
                arquivosListBox.Visibility = Visibility.Hidden;
                avisosText.Text = "Selecione os arquivos para começar";
                removerArquivosButton.IsEnabled = false;
                continuarButton.IsEnabled = false;
            }
        }
        #endregion

        private void ContinuarClick(object sender, RoutedEventArgs e)
        {
            SegmentacaoImagens segmentacaoImagens = new SegmentacaoImagens(arquivos)
            {
                WindowState = WindowState.Maximized
            };
            segmentacaoImagens.Show();
            selecaoArquivosWindow.Close();
        }
    }
}
