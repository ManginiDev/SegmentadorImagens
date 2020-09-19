using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Linq;

namespace SegmentadorImagensMaldito
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SelecionarArquivos : Window
    {
        private readonly ObservableCollection<ImagemProcessar> arquivos = new ObservableCollection<ImagemProcessar>();
        private static readonly List<string> extensoesValidas = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };

        public SelecionarArquivos()
        {
            InitializeComponent();
            arquivosListBox.DataContext = arquivos;

            var argumentos = Environment.GetCommandLineArgs();
            if (argumentos.Length > 0)
            {
                foreach (var argumento in OrdernarPorNumeroFinal(argumentos.ToList()))
                {
                    if (extensoesValidas.Contains(Path.GetExtension(argumento).ToUpperInvariant()))
                    {
                        arquivos.Add(new ImagemProcessar(argumento));
                    }
                }
            }

            ArquivosAlterados();
        }

        private static List<string> OrdernarPorNumeroFinal(List<string> caminhos)
        {
            var novosCaminho = caminhos.Select(x => new { Caminho = x, Ordem = SepararNumeroFinalCaminho(x) });
            return novosCaminho.OrderBy(x => x.Ordem).Select(x => x.Caminho).ToList();
        }

        private static int SepararNumeroFinalCaminho(string caminho)
        {
            var caminhoSemExtensao = Path.GetFileNameWithoutExtension(caminho);
            var resultado = Regex.Match(caminhoSemExtensao, @"\d+$", RegexOptions.RightToLeft);
            if (resultado.Success) return Convert.ToInt32(resultado.Value);

            return 0;
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
        OpenFileDialog openFileDialog;
        private OpenFileDialog SelecionadorArquivo()
        {
            if(openFileDialog == null)
            {
                openFileDialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
                };
            }

            return openFileDialog;
        }

        private void EscolherArquivosClick(object sender, RoutedEventArgs e)
        {
            if (SelecionadorArquivo().ShowDialog() == true)
            {
                foreach (var caminhoArquivo in openFileDialog.FileNames)
                {
                    arquivos.Add(new ImagemProcessar(caminhoArquivo));
                }

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
                segmentarAutomaticoButton.IsEnabled = true;
            }
            else
            {
                arquivosListBox.Visibility = Visibility.Hidden;
                avisosText.Text = "Selecione os arquivos para começar";
                removerArquivosButton.IsEnabled = false;
                continuarButton.IsEnabled = false;
                segmentarAutomaticoButton.IsEnabled = false;
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

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }

        private void SegmentarClick(object sender, RoutedEventArgs e)
        {
            SegmentacaoImagens segmentacaoImagens = new SegmentacaoImagens(arquivos, Convert.ToInt32(quantiaQuadros.Text))
            {
                WindowState = WindowState.Maximized
            };
            segmentacaoImagens.Show();
            selecaoArquivosWindow.Close();
        }

        private void DropArquivos(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] novosArquivos = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var argumento in OrdernarPorNumeroFinal(novosArquivos.ToList()))
                {
                    if (extensoesValidas.Contains(Path.GetExtension(argumento).ToUpperInvariant()))
                    {
                        arquivos.Add(new ImagemProcessar(argumento));
                    }
                }

                ArquivosAlterados();
            }
        }
    }
}