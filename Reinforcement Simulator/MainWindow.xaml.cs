using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;

namespace Reinforcement_Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        //Controle focado e o idMaquina
        private Control controleFocado;
        private int idFocado;
        private int IndicePainel = -1;

        Simulador sim1;
        Thread threadWorker;

        public MainWindow()
        {

            InitializeComponent();

            //Adiciona o EventHandler para o resize
            mainGrid.LayoutUpdated += chaoDeFabricaSizeChanged;
        }

        /*-------------------------------------------------------------------------
         * --------------------------------MÉTODOS---------------------------------
         * -----------------------------------------------------------------------*/

        //Insere as máquinas no chão de fábrica
        public void renderizarMaquinas(int qtdMaquinas)
        {
            //Insere as linhas de cada maquina na grid chaodeFabrica
            for (int i = 0; i < qtdMaquinas; i++)
                addRow();

            for (int i = 0; i < qtdMaquinas; i++)
            {
                chaoDeFabrica.Children.Add(sim1.getMaquinas()[i].getCaixa());
            }
        }


        public void iniciarSimulacao(int qtdTarefas, int qtdMaquinas, int acao, int rep, int[] exibirRep)
        {
            sim1 = new Simulador(qtdTarefas, qtdMaquinas, acao, rep, exibirRep);
            saida.Children.RemoveRange(1, saida.Children.Count);
            botaoPlay.Content = FindResource("Pause");

            //O correto seria fazer o processamento da simulação em uma thread distinta
            //e comunicar a main thread sobre atualizacao de UI
            
            threadWorker = new Thread( () => sim1.iniciarSimulacao(this));
            //worker.SetApartmentState(ApartmentState.STA);
            threadWorker.Start();
             
            
            /*TUDO ISSO SÓ SE APLICA PARA CASO DE NAO MOSTRAR SIMS*/
            /*
            while(sim1.existemMaisReplicacoes())
                sim1.iniciarReplicacao();
            baseGrid.Children.Remove(sim1.nroReplicacao);
            toolbar.IsEnabled = false;
            MessageBox.Show("Todas as replicações foram concluídas.");
             * */
        }

        //Adiciona uma coluna à Grid, usada para inserir número indeterminado de máquinas no chão de fábrica
        private void addRow()
        {
            RowDefinition rowdef;
            rowdef = new RowDefinition();
            chaoDeFabrica.RowDefinitions.Add(rowdef);
        }

        
        //PainelEventHandler adicionado ao evento do clique na tarefa e máquina
        public void colocarDetalhe(object sender, PainelArgs e)
        {


            if (controleFocado != null)
            {
                if (controleFocado.GetType().ToString() == "System.Windows.Controls.GroupBox")
                    sim1.getMaquinas()[idFocado].getCaixa().BorderBrush = System.Windows.Media.Brushes.Blue;
            }
            scrollDetalhes.Content = e.painel;
            IndicePainel = mainGrid.Children.IndexOf(e.painel);
            controleFocado = e.c;

            //idFocado tornou-se desnecessario com a adicao do scrollDetalhes
            idFocado = e.id;
        }
    
        //EventHandler para o resize das colunas
        private void chaoDeFabricaSizeChanged(object sender, EventArgs e)
        {
            if (mainGrid.ColumnDefinitions[0].ActualWidth < 400)
                chaoDeFabrica.MaxWidth = 400;
            else
                chaoDeFabrica.MaxWidth = mainGrid.ColumnDefinitions[0].ActualWidth - 20;
            /*
            if(mainGrid.Children.OfType<StackPanel>().Count<StackPanel>() > 0)
            {
                mainGrid.Children.OfType<StackPanel>().ElementAt<StackPanel>(0).Width = 
            }
             * */
        }

        /*----------------------EVENTHANDLERS---------------------*/
        private void sobre_Click(object sender, RoutedEventArgs e)
        {
            InformacoesProd a = new InformacoesProd();
            a.ShowDialog();
        }
        
        private void botaoParar_Click(object sender, RoutedEventArgs e)
        {
            //aviso de cancelar simulacao
            MessageBoxResult avisoCancelamento = MessageBox.Show("Deseja cancelar a simulação? Os dados da replicação atual e das posteriores não serão guardados em disco.", "Cancelar Simulação", MessageBoxButton.YesNo, MessageBoxImage.Question);

            //Cancelamento da simulação
            if (avisoCancelamento == MessageBoxResult.Yes)
            {
                chaoDeFabrica.Children.Clear();
                scrollDetalhes.Content = null;
                baseGrid.Children.Remove(sim1.nroReplicacao);
                threadWorker.Abort();
            }
        }

        private void botaoPlay_Click(object sender, RoutedEventArgs e)
        {
            if (sim1 != null)
            {
                if (botaoPlay.Content == FindResource("Play"))
                {
                    botaoPlay.Content = FindResource("Pause");
                    sim1.pausado = false;
                }
                else
                {
                    botaoPlay.Content = FindResource("Play");
                    sim1.pausado = true;
                }
            }
        }

        private void botaoAvancar_Click(object sender, RoutedEventArgs e)
        {
            if (sim1 != null)
                sim1.avancarReplicacao();
            chaoDeFabrica.Children.Clear();
        }

        //EventHandler para Exibir->Saida do Sistema
        private void saida_Click(object sender, RoutedEventArgs e)
        {
            if (baseGrid.RowDefinitions[3].Height == new System.Windows.GridLength(70))
            {
                baseGrid.RowDefinitions[3].Height = new System.Windows.GridLength(0);
            }
            else
            {
                baseGrid.RowDefinitions[3].Height = new System.Windows.GridLength(70);
            }
        }

        //EventHandler para mudar a velocidade do simulador pelo Slider
        private void slider_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (sim1 != null)
                if(sim1.mostrar)
                    sim1.tempoDeEspera = (int)sliderTempo.Value;
        }

        private void novaSim_Click(object sender, RoutedEventArgs e)
        {
            novaSimulacao novaSim = new novaSimulacao();
            novaSim.ShowDialog();
        }

        private void closing_handler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(threadWorker!=null)
                if (threadWorker.ThreadState!=ThreadState.Unstarted)
                    threadWorker.Abort();
        }
    }
}
