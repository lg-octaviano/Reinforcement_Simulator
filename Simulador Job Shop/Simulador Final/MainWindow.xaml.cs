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

namespace Simulador_Final
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
            /*
            Thread worker = new Thread( () => sim1.iniciarReplicacao(this));
            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();
             */

            /*TUDO ISSO SÓ SE APLICA PARA CASO DE NAO MOSTRAR SIMS*/
            while(sim1.existemMaisReplicacoes())
                sim1.iniciarReplicacao();
            baseGrid.Children.Remove(sim1.nroReplicacao);
            toolbar.IsEnabled = false;
            MessageBox.Show("Todas as replicações foram concluídas.");
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

        //Calcula o trabalho total em uma máquina (utilizado na Maquina.WINQ())
        public double calcularTrabalhoMaquina(int m)
        {
            double trabalho = 0;
            for (int i = 0; i < sim1.getMaquinas()[m].getFila().Count; i++)
            {
                Tarefa T = (Tarefa)sim1.getMaquinas()[m].getFila()[i];
                trabalho += T.getTempoDeProcessamento(T.getOperacoesCompletas());
            }
            return trabalho;
        }

        //Retorna estado do sistema (vetor do estado das máquinas)
        public int[] getEstadoSistema(int m)
        {
            int[] estado = new int[sim1.getMaquinas().Length];
            for (int i = 0; i < sim1.getMaquinas().Length; i++)
                estado[i] = sim1.getMaquinas()[i].getEstado();

            int temp = estado[m];//VERIFICAR O INDICE
            estado[m] = estado[0];
            estado[0] = temp;

            String str = "";
            //Array.Sort(estado);
            Array.Sort(estado, 1, estado.Length - 1);
            for (int i = 0; i < sim1.getMaquinas().Length; i++)
                str += estado[i] + " - ";

            //MessageBox.Show("Maquina "+m+" : "+str);
            return estado;
        }

        public double tempoDeVidaMedioNaMaquina(int m)
        {
            return sim1.getMaquinas()[m].getTempoMedioDeVida();
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
            }
        }

        private void botaoPlay_Click(object sender, RoutedEventArgs e)
        {
            if (sim1 != null)
            {
                if (botaoPlay.Content == FindResource("Play"))
                {
                    botaoPlay.Content = FindResource("Pause");
                    sim1.getTimer().Start();
                }
                else
                {
                    botaoPlay.Content = FindResource("Play");
                    sim1.getTimer().Stop();
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
            {
                //Gerenciamento do tempo
                switch ((int)sliderTempo.Value)
                {
                    case 0:
                        sim1.getTimer().Interval = TimeSpan.FromMilliseconds(0.1);
                        break;
                    case 1:
                        sim1.getTimer().Interval = TimeSpan.FromMilliseconds(1);
                        break;
                    case 2:
                        sim1.getTimer().Interval = TimeSpan.FromMilliseconds(50);
                        break;
                    case 3:
                        sim1.getTimer().Interval = TimeSpan.FromMilliseconds(100);
                        break;
                    default:
                        sim1.getTimer().Interval = TimeSpan.FromMilliseconds(0.1);
                        break;
                }
            }
        }

        private void novaSim_Click(object sender, RoutedEventArgs e)
        {
            novaSimulacao novaSim = new novaSimulacao();
            novaSim.ShowDialog();
        }
    }
}
