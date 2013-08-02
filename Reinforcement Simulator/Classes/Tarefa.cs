using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MathNet.Numerics.Distributions;
using System.Windows.Threading;

namespace Reinforcement_Simulator
{
    class Tarefa
    {
        public delegate void PainelEventHandler(object sender, PainelArgs e);

        private Simulador simulador;
        //Identificador da Tarefa: 0-199
        private int id;
        //Ordem das máquinas: 0-4, sem repetição
        private int[] ordem;
        //Tempo de processamento na i-ésima maquina (de acordo com a ordem): aleatório 1-19
        private double[] tempoDeProcessamento;
        //Tempo processado de cada operação
        private double[] tempoProcessado;
        //Instante de tempo de chegada na máquina
        private double[] tempoChegadaNaMaquina;

        //Tempo de chegada da tarefa no sistema
        private double tempoChegada;
        //Data de entrega para a peça. A partir deste momento, seu atraso passa a ser incrementado
        private double dataEntrega;
        //Atraso da Tarefa (só é calculado quando a tarefa completa todas as operações)
        private double atraso;

        //Número de maquinas pelas quais a tarefa já passou
        private int operacoesCompletas;

        //Número de máquinas
        private int qtdMaquinas;

        //Elementos Visuais da Tarefa
        private Button botaoTarefa;
        private StackPanel painelDetalhes;
        private Label[] Detalhes;
        private Label TituloStackPanel;

        //Estado do sistema (NroOperacoes x NroMaquinas)
        private int[][] estado;     /*estado no momento da tomada de decisão*/
        private int[][] proximoEstado;      /*estado após ação realizada*/

        //Ação que selecionou a tarefa na i-ésima operação
        private int[] acao;
        //Ação a tomar no estado após a seleção da tarefa
        private int[] proximaAcao;

        public Tarefa(int id, int qtdMaquinas, double instanteChegada, Simulador sim)
        {
            this.id = id;
            int x, temp;
            operacoesCompletas = 0;
            this.qtdMaquinas = qtdMaquinas;
            this.simulador = sim;

            Random rnd = new Random(DateTime.Now.Millisecond + id*3);//Sem id, as seeds ficam iguais
                                                                     //e os numeros tbm
            //Contruindo vetores de acordo com número de máquinas
            ordem = new int[qtdMaquinas];
            tempoDeProcessamento = new double[qtdMaquinas];
            tempoProcessado = new double[qtdMaquinas];
            tempoChegadaNaMaquina = new double[qtdMaquinas];

            tempoChegadaNaMaquina[0] = instanteChegada;    /*o tempo de chegada na primeira máquina é o tempo de chegada no sistema*/

            proximoEstado = new int[qtdMaquinas][];
            estado = new int[qtdMaquinas][];
            for (int i = 0; i < qtdMaquinas; i++)
            {
                proximoEstado[i] = new int[qtdMaquinas];
                estado[i] = new int[qtdMaquinas];
            }
            acao = new int[qtdMaquinas];
            proximaAcao = new int[qtdMaquinas];


            //Atribuição de valor aleatório à chegada da tarefa
            System.Threading.Thread.Sleep(1);
            tempoChegada = instanteChegada;

            //Atribuição de valores aleatórios à ordem da máquina
            for (int i = 0; i < qtdMaquinas; i++)
                ordem[i] = i;
            for (int i = 0; i < qtdMaquinas; i++)
            {
                x = rnd.Next(0, qtdMaquinas);
                temp = ordem[x];
                ordem[x] = ordem[i];
                ordem[i] = temp;
            }

            //Atribuição de valores aleatórios aos tempos de processamento
            for (int i = 0; i < qtdMaquinas; i++)
            {
                tempoDeProcessamento[i] = (float)rnd.NextDouble() * 18 + 1;
                tempoDeProcessamento[i] = Math.Round(tempoDeProcessamento[i], 2);
            }

            //Atribuição da data de entrega
            double processamentoTotal = 0;
            for (int i = 0; i < qtdMaquinas; i++)
                processamentoTotal += tempoDeProcessamento[i];
            dataEntrega = tempoChegada + 1.5 * processamentoTotal;
            dataEntrega = Math.Round(dataEntrega, 2);

            //Atribuição de parâmetros do BOTÃO
            botaoTarefa = new Button();
            botaoTarefa.Content = "Tarefa " + id;
            botaoTarefa.Click += tarefaClick;
            botaoTarefa.Style = (Style)(App.Current.Resources["estiloTarefa"]);


            //Atribuição de parâmetros do PAINEL
            painelDetalhes = new StackPanel();
            painelDetalhes.Style = (Style)App.Current.Resources["estiloStackPanel"];

            //Atribuição dos parâmetros do TITULO DO STACKPANEL
            TituloStackPanel = new Label();
            TituloStackPanel.Style = (Style)App.Current.Resources["estiloTituloStackPanel"];
            painelDetalhes.Children.Add(TituloStackPanel);
            
            //Atribuição dos parâmetros das ETIQUETAS DE DETALHE
            Detalhes = new Label[2*qtdMaquinas + 5];
            for (int i = 0; i < 2 * qtdMaquinas + 5; i++)
            {
                Detalhes[i] = new Label();
                painelDetalhes.Children.Add(Detalhes[i]);
                if (i != 0)
                    Detalhes[i].Style = (Style)App.Current.Resources["estiloDetalhe"];
            }
            Detalhes[0].Style = (Style)App.Current.Resources["estiloTituloDetalhes"];
            setDetalhes();


        }

        public double processar()
        {
            this.tempoProcessado[operacoesCompletas] = this.tempoProcessado[operacoesCompletas] + 0.01;
            tempoProcessado[operacoesCompletas] = Math.Round(tempoProcessado[operacoesCompletas], 2);

            return tempoProcessado[operacoesCompletas];
        }

        public int getOperacoesCompletas()
        {
            return this.operacoesCompletas;
        }

        private void setDetalhes()
        {
            Detalhes[0].Content = "Tarefa Número " + id;
            for (int i = 0; i < qtdMaquinas; i++)
            {
                Detalhes[i + 1].Content = (i+1) + "ª Maquina:" + ordem[i];
                Detalhes[i + qtdMaquinas + 1].Content = "Processamento na "+(i+1)+ "ª Maquina: " + tempoDeProcessamento[i];
            }
            Detalhes[2*qtdMaquinas + 1].Content = "Tempo de Chegada: " + this.tempoChegada;
            Detalhes[2*qtdMaquinas + 2].Content = "Data de entrega: " + this.dataEntrega;
            Detalhes[2*qtdMaquinas + 3].Content = "Número de operações finalizadas: " + this.operacoesCompletas;
        }

        public void setProximoEstadoAcao()
        {
            //var main = App.Current.MainWindow as MainWindow;
            int[] estadoSistema = simulador.getEstadoSistema(this.getMaquina(getOperacoesCompletas()));

            for (int i = 0; i < qtdMaquinas; i++)
            {
                this.proximoEstado[operacoesCompletas][i] = estadoSistema[i];
            }

            /*
            MessageBox.Show("Estado" + operacoesCompletas + " da tarefa " + this.id + 
             * "= " + estado[operacoesCompletas][0] + estado[operacoesCompletas][1]);
            MessageBox.Show("Acao ="+this.acao[operacoesCompletas]);
            MessageBox.Show("ProxEstado "+operacoesCompletas+" da tarefa "+this.id+"= "
             * +estadoSistema[0] +estadoSistema[1]);
            */

            if (simulador.isModoAprendizado() && qtdMaquinas == 5)
            {
                int estado = proximoEstado[operacoesCompletas][0] + 3 * proximoEstado[operacoesCompletas][1] +
                    9 * proximoEstado[operacoesCompletas][2] + 27 * proximoEstado[operacoesCompletas][3] +
                    81 * proximoEstado[operacoesCompletas][4];

                proximaAcao[operacoesCompletas] = simulador.selecionarAcao(estado);
            }
        }

        public void setEstados(int acao)
        {
            int[] estadoSistema = simulador.getEstadoSistema(getMaquina(getOperacoesCompletas()));

            for (int i = 0; i < qtdMaquinas; i++)
            {
                this.estado[operacoesCompletas][i] = estadoSistema[i];
                this.acao[operacoesCompletas] = acao;
            }
        }

        public int getId()
        {
            return this.id;
        }

        public double getAtraso()
        {
            return this.atraso;
        }

        public int getMaquina(int i)
        {
            return this.ordem[i];
        }

        public int getQtdMaquina()
        {
            return this.qtdMaquinas;
        }

        //Retorna o tempo de processamento na i-ésima máquina da rota
        public double getTempoDeProcessamento(int maquina)
        {
            return this.tempoDeProcessamento[maquina];
        }

        //Retorna o tempo de processamento da maquina de número i
        public double getTempoDeProcessamentoMaq(int maquina)
        {
            for (int i = 0; i < qtdMaquinas; i++)
            {
                if (this.ordem[i] == maquina)
                    return tempoDeProcessamento[i];
            }

            MessageBox.Show("Nao encontrado o tempo de processamento da maquina");
            return tempoDeProcessamento[0];
        }

        public double getDataEntrega()
        {
            return this.dataEntrega;
        }


        public double getTempoProcessado(int maquina)
        {
            return this.tempoProcessado[maquina];
        }

        public void operacaoFinalizada(double tempo)
        {
            operacoesCompletas++;
            if(operacoesCompletas < qtdMaquinas)
                tempoChegadaNaMaquina[operacoesCompletas] = tempo;  /*O tempo de chegada na próxima máquina é o tempo de conclusão na máquina anterior*/

            Detalhes[2 * qtdMaquinas + 3].Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            { Detalhes[2 * qtdMaquinas + 3].Content = "Número de operações finalizadas: " + this.operacoesCompletas; }));

            //DEPOIS QUE TIVER MAIS DETALHES TEM Q ATUALIZAR
            //this.setDetalhes();
        }

        //Chamar quando a tarefa tiver completado todas as operações
        public void finalizada(double tempo)
        {
            this.atraso = tempo - this.dataEntrega;
            if (atraso < 0) /*O atraso é zero caso a tarefa chegue antes da Due Date*/
                atraso = 0;

            botaoTarefa.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                botaoTarefa.Margin = new Thickness(0);
                botaoTarefa.VerticalAlignment = VerticalAlignment.Stretch;
                botaoTarefa.Margin = new Thickness(2, 0, 2, 0);
                Detalhes[2 * qtdMaquinas + 4].Content = "Atraso da Tarefa: " + this.atraso;
            }));


            /*Atualizações da tabela do aprendiz*/
            if (simulador.isModoAprendizado() && qtdMaquinas==5)
            {
                int s, new_s;
                for(int i=0; i<qtdMaquinas; i++)
                {
                    s = estado[i][0] + 3*estado[i][1] + 
                        9*estado[i][2] + 27*estado[i][3] + 81*estado[i][4];

                    new_s = proximoEstado[i][0] + 3*proximoEstado[i][1] + 
                        9*proximoEstado[i][2] + 27*proximoEstado[i][3] + 
                        81*proximoEstado[i][4];

                    simulador.atualizarQ(s, acao[i], -this.atraso, new_s, proximaAcao[i]);
                }
                    
            }

        }
        public Button getBotaoTarefa()
        {
            return this.botaoTarefa;
        }

        public double getTempoChegada()
        {
            return this.tempoChegada;
        }

        public double getTempoChegadaNaUltimaMaquina()
        {
            return this.tempoChegadaNaMaquina[operacoesCompletas-1];
        }

        public StackPanel getPainelDetalhes()
        {
            return this.painelDetalhes;
        }


        //Adiciona o evento público "Focada" ao evento do clique no botão da tarefa
        public void tarefaClick(object sender, RoutedEventArgs e)
        {
            setDetalhes();
            if (Focada != null)
            {
                Focada(this, new PainelArgs(this.painelDetalhes, this.botaoTarefa, this.id));
            }
        }

        //----EVENTO PUBLICO QUE MAINWINDOW USA (ADICIONANDO UM HANDLER A ELE)----
        public event PainelEventHandler Focada;


    }
}
