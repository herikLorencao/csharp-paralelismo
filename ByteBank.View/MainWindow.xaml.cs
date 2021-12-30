using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using ByteBank.View.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            BtnProcessar.IsEnabled = false;

            var contas = r_Repositorio.GetContaClientes();

            LimparView();

            var inicio = DateTime.Now;
            var progress = new Progress<string>(value => PsgProgresso.Value++);
            // var byteBankProgress = new ByteBankProgress<string>(value => PsgProgresso.Value++);

            var resultado = await ConsolidarContas(contas, progress);

            var fim = DateTime.Now;
            AtualizarView(resultado, fim - inicio);
            BtnProcessar.IsEnabled = true;
        }

        private async Task<string[]> ConsolidarContas(IEnumerable<ContaCliente> contas, IProgress<string> progress)
        {
            var taskSchedulerGUI = TaskScheduler.FromCurrentSynchronizationContext();
            PsgProgresso.Maximum = contas.Count();

            var tasks = contas.Select(conta =>
                    Task.Factory.StartNew(() =>
                    {
                        var resultadoConsolidacao = r_Servico.ConsolidarMovimentacao(conta);
                        progress.Report(resultadoConsolidacao);
                        return resultadoConsolidacao;
                    })

                );
            return await Task.WhenAll(tasks);
        }

        private void LimparView()
        {
            LstResultados.ItemsSource = null;
            TxtProgresso.Text = null;
        }

        private void AtualizarView(IEnumerable<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count()} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }
    }
}
