using SegmentadorImagensMaldito.Imagem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Globalization;

namespace SegmentadorImagensMaldito
{
    /// <summary>
    /// Interaction logic for SegmentacaoImagens.xaml
    /// </summary>
    public partial class SegmentacaoImagens : Window
    {
        private readonly ObservableCollection<ImagemProcessar> arquivos = new ObservableCollection<ImagemProcessar>();
#if DEBUG
        public SegmentacaoImagens()
        {
            List<ImagemProcessar> fixos = new List<ImagemProcessar>();

            FileInfo[] filesInfo = new DirectoryInfo($@"D:\Downloads\14\14").GetFiles("*.jpg");
            foreach (var arquivo in filesInfo)
            {
                fixos.Add(new ImagemProcessar(arquivo.FullName));
            }

            Inicializar(new ObservableCollection<ImagemProcessar>(OrdernarPorNumeroFinal(fixos)));
            SegmentarAutomatico(5);
        }

        private static List<ImagemProcessar> OrdernarPorNumeroFinal(List<ImagemProcessar> caminhos)
        {
            var novosCaminho = caminhos.Select(x => new { Caminho = x, Ordem = SepararNumeroFinalCaminho(x.CaminhoCompleto) });
            return novosCaminho.OrderBy(x => x.Ordem).Select(x => x.Caminho).ToList();
        }
        private static int SepararNumeroFinalCaminho(string caminho)
        {
            var caminhoSemExtensao = Path.GetFileNameWithoutExtension(caminho);
            var resultado = Regex.Match(caminhoSemExtensao, @"\d+$", RegexOptions.RightToLeft);
            if (resultado.Success) return Convert.ToInt32(resultado.Value);

            return 0;
        }
#endif

        public SegmentacaoImagens(ObservableCollection<ImagemProcessar> imagensBase, int quadrosPorPagina = 0, double porcentagemToleranciaDiferente = 0, double porcentagemToleranciaIgual = 0, double porcentagemMaximaDaImagemParaCobrirAntesDeJuntar = 0)
        {
            Properties.Settings.Default.quadrosPorPagina = quadrosPorPagina;
            Properties.Settings.Default.porcentagemToleranciaDiferente = porcentagemToleranciaDiferente;
            Properties.Settings.Default.porcentagemToleranciaIgual = porcentagemToleranciaIgual;
            Properties.Settings.Default.porcentagemMaximaDaImagemParaCobrirAntesDeJuntar = porcentagemMaximaDaImagemParaCobrirAntesDeJuntar;
            Properties.Settings.Default.Save();

            Inicializar(imagensBase);
            if (quadrosPorPagina > 0) SegmentarAutomatico(quadrosPorPagina);
        }

        private void Inicializar(ObservableCollection<ImagemProcessar> imagensBase)
        {
            foreach (var arquivo in imagensBase)
            {
                arquivo.CarregarImagem();
                arquivos.Add(arquivo);
            }

            InitializeComponent();
            imagensMenoresListView.DataContext = arquivos;
            imagensMaioresListView.DataContext = arquivos;

            quantiaQuadros.Text = Properties.Settings.Default.quadrosPorPagina.ToString();
            diferenca.Value = Properties.Settings.Default.porcentagemToleranciaDiferente * 100;
            semelhanca.Value = Properties.Settings.Default.porcentagemToleranciaIgual * 100;
            cobrir.Value = Properties.Settings.Default.porcentagemMaximaDaImagemParaCobrirAntesDeJuntar * 100;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        #region Segmentador Automatico

        private void SalvarPropriedadesSegmentadorAutomatico()
        {
            Properties.Settings.Default.quadrosPorPagina = Convert.ToInt32(quantiaQuadros.Text);
            Properties.Settings.Default.porcentagemToleranciaDiferente = diferenca.Value / 100;
            Properties.Settings.Default.porcentagemToleranciaIgual = semelhanca.Value / 100;
            Properties.Settings.Default.porcentagemMaximaDaImagemParaCobrirAntesDeJuntar = cobrir.Value / 100;
            Properties.Settings.Default.Save();
        }

        private void SegmentarClick(object sender, RoutedEventArgs e)
        {
            SalvarPropriedadesSegmentadorAutomatico();
            SegmentarAutomatico(Convert.ToInt32(quantiaQuadros.Text));
        }

        private SegmentadorAutomatico segmentadorAutomatico;
        private async void SegmentarAutomatico(int quadrosPorPagina)
        {
            overlayGrid.Visibility = Visibility.Visible;

            segmentadorAutomatico ??= new SegmentadorAutomatico();
            segmentadorAutomatico.RecarregarPorSettings();
            await Task.Factory.StartNew(() => {
                var imagensNovas = segmentadorAutomatico.Segmentar(arquivos.ToList(), quadrosPorPagina);
                Dispatcher.Invoke(() => {
                    arquivos.Clear();
                    imagensNovas.ForEach(x => arquivos.Add(new ImagemProcessar(x)));
                });
            });

            overlayGrid.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Salvar Arquivos
        private async void Concluir_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(nomeBaseTextBox.Text))
            {
                MessageBox.Show("Necessario definir um nome para os arquivos a serem salvos", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                nomeBaseTextBox.Focus();
                return;
            }

            var dialog = new CommonOpenFileDialog
            {
                Title = "Selecione onde as imagens serão salvas",
                IsFolderPicker = true,

                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                overlayGrid.Visibility = Visibility.Visible;

                var diretorio = dialog.FileName + "/SegmentadorImagensMaldito";
                Directory.CreateDirectory(diretorio);
                await SalvarArquivos(diretorio);

                overlayGrid.Visibility = Visibility.Hidden;
                MessageBox.Show("Arquivos Salvos", "Concluido", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private Task SalvarArquivos(string pasta)
        {
            overlayGrid.Visibility = Visibility.Visible;

            int[] indices = Enumerable.Range(0, arquivos.Count).ToArray();
            return Task.WhenAll(indices.Select(indice => Salvar(indice, pasta, nomeBaseTextBox.Text)));
        }

        private Task Salvar(int indice, string pasta, string nomeArquivo)
        {
            return new TaskFactory().StartNew(() =>
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(arquivos.ElementAt(indice).Imagem));

                using var fileStream = new FileStream($@"{pasta}/{nomeArquivo}_{indice:D3}.png", FileMode.Create);
                encoder.Save(fileStream);
            });
        }
        #endregion
        #region Listas
        private void MostrarItemSelecionado_SelecaoListaMenor(object sender, SelectionChangedEventArgs e)
        {
            imagensMaioresListView.ScrollIntoView(imagensMenoresListView.SelectedItem);
        }

        private void MostrarItemSelecionado_SelecaoListaMaior(object sender, SelectionChangedEventArgs e)
        {
            imagensMenoresListView.ScrollIntoView(imagensMaioresListView.SelectedItem);
        }
        private async void UnirTodas_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Deseja realmente unitor todas as imagens? Isso pode demorar bastante tempo.", "Unir Todas", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No) return;

            overlayGrid.Visibility = Visibility.Visible;
            await Task.Factory.StartNew(() => {
                var imagemUnida = ProcessarImagens.CombinarImagens(arquivos.ToList());
                Dispatcher.Invoke(() => {
                    arquivos.Clear();
                    arquivos.Add(new ImagemProcessar(imagemUnida));
                });
            });

            overlayGrid.Visibility = Visibility.Hidden;
        }

        private async void UnirAbaixo_Click(object sender, RoutedEventArgs e)
        {
            Button botao = (Button)sender;
            if (botao.DataContext is ImagemProcessar imagemProcessar)
            {
                int indiceSelecionado = arquivos.IndexOf(imagemProcessar);
                if (indiceSelecionado == arquivos.Count - 1) MessageBox.Show("Não há imagem para unir", "Alerta", MessageBoxButton.OK, MessageBoxImage.Error);

                overlayGrid.Visibility = Visibility.Visible;

                BitmapSource novaImagem = null;
                await Task.Factory.StartNew(() => {
                    novaImagem = ProcessarImagens.CombinarImagens(arquivos.ElementAt(indiceSelecionado), arquivos.ElementAt(indiceSelecionado + 1));
                    Dispatcher.Invoke(() => {
                        arquivos.RemoveAt(indiceSelecionado + 1);
                        arquivos.RemoveAt(indiceSelecionado);
                        arquivos.Insert(indiceSelecionado, new ImagemProcessar(novaImagem));
                    });
                });

                overlayGrid.Visibility = Visibility.Hidden;
            }
        }

        private async void DividirImagen_ClickEsquerdo(object sender, MouseButtonEventArgs e)
        {
            Image imagem = (Image)sender;
            if (imagem.DataContext is ImagemProcessar imagemProcessar)
            {
                int indiceSelecionado = arquivos.IndexOf(imagemProcessar);
                overlayGrid.Visibility = Visibility.Visible;

                int altura = (int)(e.GetPosition(imagem).Y * (imagemProcessar.Imagem.PixelHeight / imagem.ActualHeight));
                List<BitmapSource> novasImagens = null;
                await Task.Factory.StartNew(() => {
                    novasImagens = ProcessarImagens.SepararImagem(arquivos.ElementAt(indiceSelecionado), altura);
                    Dispatcher.Invoke(() => {
                        arquivos.RemoveAt(indiceSelecionado);
                        arquivos.Insert(indiceSelecionado, new ImagemProcessar(novasImagens.ElementAt(1)));
                        arquivos.Insert(indiceSelecionado, new ImagemProcessar(novasImagens.ElementAt(0)));
                    });
                });

                overlayGrid.Visibility = Visibility.Hidden;
            }
        }

        private async void DividirImagen_ClickDireito(object sender, MouseButtonEventArgs e)
        {
            Image imagem = (Image)sender;
            if (imagem.DataContext is ImagemProcessar imagemProcessar)
            {
                int indiceSelecionado = arquivos.IndexOf(imagemProcessar);
                if (indiceSelecionado == 0)
                {
                    MessageBox.Show("Não é possivel realizar essa operação na primeira imagem", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                overlayGrid.Visibility = Visibility.Visible;

                int altura = (int)(e.GetPosition(imagem).Y * (imagemProcessar.Imagem.PixelHeight / imagem.ActualHeight));
                List<BitmapSource> novasImagens = null;
                await Task.Factory.StartNew(() => {
                    novasImagens = ProcessarImagens.SepararUnirImagem(arquivos.ElementAt(indiceSelecionado - 1), arquivos.ElementAt(indiceSelecionado), altura);
                    Dispatcher.Invoke(() => {
                        arquivos.RemoveAt(indiceSelecionado - 1);
                        arquivos.RemoveAt(indiceSelecionado - 1);
                        arquivos.Insert(indiceSelecionado - 1, new ImagemProcessar(novasImagens.ElementAt(1)));
                        arquivos.Insert(indiceSelecionado - 1, new ImagemProcessar(novasImagens.ElementAt(0)));
                    });
                });

                overlayGrid.Visibility = Visibility.Hidden;
            }
        }

        private async void DividirImagen_ClickMeio(object sender, MouseButtonEventArgs e)
        {
            if (!(e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Middle)) return;

            Image imagem = (Image)sender;
            if (imagem.DataContext is ImagemProcessar imagemProcessar)
            {
                int indiceSelecionado = arquivos.IndexOf(imagemProcessar);
                if (indiceSelecionado == 0 || indiceSelecionado == arquivos.Count - 1)
                {
                    MessageBox.Show("Não é possivel realizar essa operação na primeira ou ultima imagem", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                overlayGrid.Visibility = Visibility.Visible;

                int altura = (int)(e.GetPosition(imagem).Y * (arquivos.ElementAt(indiceSelecionado).Imagem.Height / imagem.ActualHeight));
                List<BitmapSource> novasImagens = null;

                await Task.Factory.StartNew(() => {
                    novasImagens = ProcessarImagens.SepararUnirImagem(arquivos.ElementAt(indiceSelecionado - 1), arquivos.ElementAt(indiceSelecionado), altura);
                    var imagemUnificada = ProcessarImagens.CombinarImagens(novasImagens.ElementAt(1), arquivos.ElementAt(indiceSelecionado + 1).Imagem);
                    Dispatcher.InvokeAsync(() => {
                        arquivos.RemoveAt(indiceSelecionado - 1);
                        arquivos.RemoveAt(indiceSelecionado - 1);
                        arquivos.RemoveAt(indiceSelecionado - 1);
                        arquivos.Insert(indiceSelecionado - 1, new ImagemProcessar(imagemUnificada));
                        arquivos.Insert(indiceSelecionado - 1, new ImagemProcessar(novasImagens.ElementAt(0)));
                    });
                });

                overlayGrid.Visibility = Visibility.Hidden;
            }
        }
        #endregion

        private void Voltar_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Deseja retornar, arquivos não salvos serão perdidos!", "Retornar", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                SelecionarArquivos selecionarArquivos = new SelecionarArquivos();
                selecionarArquivos.Show();
                segmentacaoWindow.Close();
            }
        }
    }
}
