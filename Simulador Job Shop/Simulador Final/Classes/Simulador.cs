using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using MathNet.Numerics.Distributions;
using System.Collections;
using System.Threading;


namespace Simulador_Final
{
    class Simulador
    {

        private delegate void UpdateProgressBarDelegate(
        System.Windows.DependencyProperty dp, Object value);

        //Objetos para controle do tempo
        private DispatcherTimer timer;
        private double tempo = 0;
        private double dt = 0.01;
        private Label labelTempo;

        //Número de replicações totais e replicações já concluídas
        private int replicacoesTotais;
        private int replicacaoAtual;

        //Replicações a serem mostradas
        private bool mostrar;
        int[] exibirRep;

        //Características escolhidas pelo usuário para a simulação
        int acao;
        int qtdMaquinas;
        int qtdTarefas;

        //Objetos do sistema de manufatura
        Tarefa[] tarefa;
        Maquina[] maquina;

        Aprendiz aprendiz;

        //Objeto gráfico que diz o número da replicação atual
        public Label nroReplicacao;


        public Simulador(int qTarefas, int qMaquinas, int acao, int rep, int[] exibirRep)
        {
            var main = App.Current.MainWindow as MainWindow;
            //Setando a quantidade de máquinas e tarefas
            qtdMaquinas = qMaquinas;
            qtdTarefas = qTarefas;
            if (this.qtdTarefas < 1)
                this.qtdTarefas = 1;
            if (this.qtdMaquinas < 1)
                this.qtdMaquinas = 1;

            labelTempo = new Label();
            labelTempo.Style = (Style)App.Current.Resources["estiloEtiquetaTempo"];

            replicacaoAtual = 1;
            replicacoesTotais = rep + 1;
            nroReplicacao = new Label();
            nroReplicacao.Style = (Style)App.Current.Resources["estiloEtiquetaRep"];
            main.baseGrid.Children.Add(nroReplicacao);

            this.exibirRep = exibirRep;
            //Setando as ações
            if (acao < 0 || acao > 101)
                this.acao = 4;
            else
            {
                this.acao = acao;
            }

            var culture = new System.Globalization.CultureInfo("en-us");
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;


            if (acao == 13) /*se simulação está em modo aprendizagem*/
                aprendiz = new Aprendiz();
            else
                aprendiz = null;
        }



        public void iniciarReplicacao()
        {
            //var main = (MainWindow)param;
            var main = App.Current.MainWindow as MainWindow;

            //App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
           // {
                //Valor maximo da barra de progresso é o número de operações que terão de ser completadas
                main.barraProgresso.Maximum = qtdTarefas * qtdMaquinas;
                //Ativação dos botões da toolbar
                main.toolbar.IsEnabled = true;
                main.saida.Children.RemoveRange(1, qtdTarefas);
                main.chaoDeFabrica.Children.Clear();
                main.barraProgresso.Value = 0;
                nroReplicacao.Content = "Replicação nº " + replicacaoAtual;
            //}));

            //Define se serão mostrados os dados da replicação
            mostrar = false;
            foreach (int exibir in exibirRep)
            {
                if (exibir == replicacaoAtual)
                    mostrar = true;
            }
            if (mostrar)
            {
                //App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
               // {
                    main.chaoDeFabrica.Children.Add(labelTempo);
                    main.menuSaida.IsEnabled = true;
                    main.menuSaida.IsChecked = true;
                    main.baseGrid.RowDefinitions[3].Height = new System.Windows.GridLength(70);
               // }));
            }

            //Reset dos dados de replicações anteriores
            tempo = 0;

            //Construtor das máquinas
            this.maquina = new Maquina[qtdMaquinas];
            for (int i = 0; i < qtdMaquinas; i++)
            {
                this.maquina[i] = new Maquina(i, aprendiz);
                this.maquina[i].Focada += new Simulador_Final.Tarefa.PainelEventHandler(main.colocarDetalhe);
            }

            //Adiciona as máquinas ao chão de fábrica somente se replicação for mostrada
            if (mostrar)
            {
                //App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                //  {
                main.renderizarMaquinas(qtdMaquinas);
                // }));
            }

            //Construtor das tarefas: Aqui se calculam os instantes de chegada da tarefa
            tarefa = new Tarefa[qtdTarefas];
            Exponential expDist = new Exponential(0.090);
            double ultimaChegada = 0;

            for (int i = 0; i < qtdTarefas; i++)
            {
                ultimaChegada += expDist.Sample();
                ultimaChegada = Math.Round(ultimaChegada, 2);
                tarefa[i] = new Tarefa(i, qtdMaquinas, ultimaChegada, aprendiz);
                tarefa[i].Focada += new Simulador_Final.Tarefa.PainelEventHandler(main.colocarDetalhe);
            }

            //Gerenciamento do tempo
            timer = new DispatcherTimer();
            switch ((int)main.sliderTempo.Value)
            {
                case 0:
                    timer.Interval = TimeSpan.FromMilliseconds(0.1);
                    break;
                case 1:
                    timer.Interval = TimeSpan.FromMilliseconds(1);
                    break;
                case 2:
                    timer.Interval = TimeSpan.FromMilliseconds(50);
                    break;
                case 3:
                    timer.Interval = TimeSpan.FromMilliseconds(100);
                    break;
                default:
                    timer.Interval = TimeSpan.FromMilliseconds(0.1);
                    break;
            }

            if (mostrar)/*só utiliza o timer se não for mostrar a replicação*/
            {
                timer.Tick += avancaTempo;
                timer.Start();
            }
           
            //Se a replicação não for mostrada, a UI trava mas processa mais rápido
            if (!mostrar)
            {
                //Teste de verificação de conclusão da simulação
                while (main.saida.Children.Count < qtdTarefas + 1)
                    avancaTempo(this, this);

                escreverRelatorios();
                replicacaoAtual++;

                /*
                //Verifica se há mais replicações a serem feitas
                if (replicacaoAtual < replicacoesTotais)
                {
                    iniciarReplicacao();
                }
                else
                {
                    main.baseGrid.Children.Remove(this.nroReplicacao);
                    main.toolbar.IsEnabled = false;
                    MessageBox.Show("Todas as replicações foram concluídas.");
                }
                 * */
            }
        }

        public bool existemMaisReplicacoes() { return (replicacaoAtual < replicacoesTotais); }

        public Maquina[] getMaquinas()
        {
            return this.maquina;
        }

        public double getTempo()
        {
            return this.tempo;
        }

        public DispatcherTimer getTimer()
        {
            return this.timer;
        }

        private void chegadaTarefas()
        {
            for (int t = 0; t < qtdTarefas; t++)
            {
                if (Math.Abs(tarefa[t].getTempoChegada() - tempo) <= 0.0001)
                {
                    maquina[tarefa[t].getMaquina(0)].adicionarTarefa(tarefa[t]);
                }
            }
        }

        private void avancaTempo(object sender, object e)
        {
            lock (this)
            {
                timer.Stop();
                tempo = Math.Round(tempo + dt, 2);
                var main = App.Current.MainWindow as MainWindow;

                if (!mostrar)
                {
                    main.menuSaida.IsChecked = false;
                    main.menuSaida.IsEnabled = false;
                    main.baseGrid.RowDefinitions[3].Height = new System.Windows.GridLength(0);
                }

                //Imprime um numero determinado de digitos da variavel tempo
                labelTempo.Content = "TEMPO: " + tempo.ToString("00.00");

                for (int m = 0; m < qtdMaquinas; m++)
                {
                    //maquina[m].adicionarTamFila();  /*CROSS-CUTTING CONCERN: adiciona o tam. da fila ao vetor para depois colocar no relatório*/
                    //maquina[m].adicionarTrabFila();

                    if (!maquina[m].getEstaProcessando())
                    {
                        if (maquina[m].getFila().Count > 0)
                            regraDespacho(m, this.acao, this.tempo);
                    }
                }

                for (int m = 0; m < qtdMaquinas; m++)
                {
                    if (maquina[m].getEstaProcessando())
                    {
                        if (maquina[m].getProcessando().getTempoProcessado(maquina[m].getProcessando().getOperacoesCompletas()) < maquina[m].getProcessando().getTempoDeProcessamento(maquina[m].getProcessando().getOperacoesCompletas()))
                            maquina[m].getProcessando().processar();
                        if (Math.Abs(maquina[m].getProcessando().getTempoProcessado(maquina[m].getProcessando().getOperacoesCompletas()) - maquina[m].getProcessando().getTempoDeProcessamento(maquina[m].getProcessando().getOperacoesCompletas())) < 0.001)
                        {
                            Tarefa temp = maquina[m].getProcessando();
                            maquina[m].operacaoFinalizada(tempo);
                            main.barraProgresso.Value++;

                            if (temp.getOperacoesCompletas() < qtdMaquinas)
                            {
                                maquina[temp.getMaquina(temp.getOperacoesCompletas())].adicionarTarefa(temp);
                            }
                            else
                            {
                                temp.finalizada(tempo, temp);
                                main.saida.Children.Add(temp.getBotaoTarefa());
                            }
                        }
                    }
                }

                chegadaTarefas();


                /*
                 //Teste de verificação de conclusão da simulação
                if (main.saida.Children.Count >= qtdTarefas + 1)
                {
                    escreverRelatorios();
                    replicacaoAtual++;


                    //Verifica se há mais replicações a serem feitas
                    if (replicacaoAtual < replicacoesTotais)
                    {
                        iniciarReplicacao();
                    }
                    else
                    {
                        this.timer.IsEnabled = false;
                        main.baseGrid.Children.Remove(this.nroReplicacao);
                        main.toolbar.IsEnabled = false;

                        MessageBox.Show("Todas as replicações foram concluídas.");
                    }
                }
                else
                { timer.Start(); }


                */

                if (mostrar)
                {
                    //Teste de verificação de conclusão da simulação
                    if (main.saida.Children.Count >= qtdTarefas + 1)
                    {
                        escreverRelatorios();
                        replicacaoAtual++;


                        //Verifica se há mais replicações a serem feitas
                        if (replicacaoAtual < replicacoesTotais)
                        {
                            iniciarReplicacao();
                        }
                        else
                        {
                            this.timer.IsEnabled = false;
                            main.baseGrid.Children.Remove(this.nroReplicacao);
                            main.toolbar.IsEnabled = false;

                            MessageBox.Show("Todas as replicações foram concluídas.");
                        }
                    }
                    else
                    {
                        timer.Start();
                    }
                }
            }
        }

        private void regraDespacho(int m, int acao, double tempo)
        {
            switch (acao)
            {
                case 0:
                    maquina[m].SPT();
                    break;
                case 1:
                    maquina[m].WINQ();
                    break;
                case 2:
                    maquina[m].TFA();
                    break;
                case 3:
                    maquina[m].maiorFTM(tempo);
                    break;
                case 4:
                    maquina[m].EDD_LWR();
                    break;
                case 5:
                    maquina[m].menorVelocidade(tempo);
                    break;
                case 6:
                    maquina[m].maiorVelocidade(tempo);
                    break;
                case 7:
                    maquina[m].LWR();
                    break;
                case 8:
                    maquina[m].menorFVT(tempo);
                    break;
                case 9:
                    maquina[m].maiorFVT(tempo);
                    break;
                case 10:
                    maquina[m].menorFTM(tempo);
                    break;
                case 11:
                    maquina[m].EDD();
                    break;
                case 12:
                    maquina[m].maiorTVMS();
                    break;
                case 13:
                    maquina[m].aprendizadoReforco(tempo);
                    break;
                case 20:
                    maquina[m].regraAleatoria(tempo);
                    break;
                case 100:
                    maquina[m].tarefaAleatoria();
                    break;
                default:
                    MessageBox.Show("Regra desconhecida. Usando regra de despacho aleatória.");
                    maquina[m].regraAleatoria(tempo);
                    break;
            }
        }


        /*--------------------------------------------------------------------------
         * ------MÉTODOS PARA ESCRITA DOS RESULTADOS DA SIMULAÇÃO EM ARQUIVO--------
         * ------------------------------------------------------------------------*/

        //Único método usado no código, se vale de outros métodos.
        private void escreverRelatorios()
        {
            relatorioReplicacoes();
            //relatorioMenorSPT();
            //relatorioMenorLWR();
            //relatorioMenorEDD();
            //relatorioMenorWINQ();
            //relatorioTamanhoFila();
            //relatorioMediaFila();
            //relatorioTrabalhoFila();

        }


        //Gera o caminho do arquivo de relatório
        private string caminhoRelatorio(string nomeRelatorio)
        {
            //Pega diretório da aplicação
            string diretorioBase = System.IO.Directory.GetCurrentDirectory();

            //Cria um subdiretório para relatórios
            string diretorioRelatorio = System.IO.Path.Combine(diretorioBase, "Relatório da Simulação");
            if (!System.IO.Directory.Exists(diretorioRelatorio))
            {
                System.IO.Directory.CreateDirectory(diretorioRelatorio);
            }

            //Cria um arquivo para os relatórios
            string caminhoRelatorio = System.IO.Path.Combine(diretorioRelatorio, nomeRelatorio);

            return caminhoRelatorio;
        }

        private void relatorioMediaFila()
        {
            string relatorio = caminhoRelatorio("Média Fila.txt");

            //Cria o arquivo .txt somente na primeira replicação
            bool append = true;
            if (replicacaoAtual == 1)
                append = false;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(relatorio, append))
            {
                if (!append)
                {
                    file.WriteLine("              RESULTADOS DA SIMULAÇÃO\r\n");
                    file.WriteLine("Média do tamanho da fila em cada replicação:");
                }

                file.WriteLine("----------------------REPLICAÇÃO " + replicacaoAtual + "-----------------------\n\n\n");
                for (int i = 0; i < qtdMaquinas; i++)
                {
                    double media = 0;
                    foreach (int x in maquina[i].tamanhoFila)
                        media += x;

                    media = media / maquina[i].tamanhoFila.Count;
                    file.WriteLine("Maquina " + i + ": " + media);
                }
            }
        }

        //Imprime a média da fila em casa segundo
        private void relatorioTamanhoFila()
        {
            string relatorio = caminhoRelatorio("Tamanho Fila.txt");

            ArrayList[] tamfilaSec = new ArrayList[qtdMaquinas];

            //Cria o arquivo .txt somente na primeira replicação
            bool append = true;
            if (replicacaoAtual == 1)
                append = false;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(relatorio, append))
            {
                if (!append)
                {
                    file.WriteLine("              RESULTADOS DA SIMULAÇÃO\r\n");
                    //file.WriteLine("Tamanho da fila a cada instante de tempo:");
                }

                //file.WriteLine("\r\n--------------REPLICACAO "+replicacaoAtual+"--------------------");

                //Calcula e imprimime a média da fila a cada 5 segundos para cada máquina individualmente
                for (int i = 0; i < qtdMaquinas; i++)
                {
                    tamfilaSec[i] = new ArrayList();
                    //file.WriteLine("\r\n---------------Maquina "+i);
                    for (int seg = 0; seg < maquina[i].tamanhoFila.Count - 500; seg = seg + 500)
                    {
                        double media = 0;
                        for (int quintoDeMilisec = 0; quintoDeMilisec < 500; quintoDeMilisec++)
                            media += Convert.ToInt32(maquina[i].tamanhoFila[seg + quintoDeMilisec]);

                        media = media / 500;
                        tamfilaSec[i].Add(media);
                    }

                    //foreach (double x in tamfilaSec[i])
                    //    file.Write(x + ",");

                }

                //Calcula e imprime a média do valor da fila em cada 5 segundo para todas as máquinas.
                file.WriteLine("\r\nMédia das Maquinas ");
                for (int i = 0; i < tamfilaSec[0].Count; i++)
                {
                    double mediaTamFila = 0;
                    for (int j = 0; j < qtdMaquinas; j++)
                    {
                        mediaTamFila += Convert.ToDouble(tamfilaSec[j][i]);
                    }
                    mediaTamFila = mediaTamFila / qtdMaquinas;
                    file.Write(mediaTamFila + ", ");
                }

                for (int i = 0; i < qtdMaquinas; i++)
                {
                    tamfilaSec[i].Clear();
                }
            }
        }

        //Imprime a média do trabalho na fila em casa segundo
        private void relatorioTrabalhoFila()
        {
            string relatorio = caminhoRelatorio("Trabalho Fila.txt");

            ArrayList[] trabfilaSec = new ArrayList[qtdMaquinas];

            //Cria o arquivo .txt somente na primeira replicação
            bool append = true;
            if (replicacaoAtual == 1)
                append = false;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(relatorio, append))
            {
                if (!append)
                {
                    file.WriteLine("              RESULTADOS DA SIMULAÇÃO\r\n");
                    //file.WriteLine("Tamanho da fila a cada instante de tempo:");
                }

                //file.WriteLine("\r\n--------------REPLICACAO "+replicacaoAtual+"--------------------");

                //Calcula e imprimime a média da fila a cada 5 segundos para cada máquina individualmente
                for (int i = 0; i < qtdMaquinas; i++)
                {
                    trabfilaSec[i] = new ArrayList();
                    //file.WriteLine("\r\n---------------Maquina "+i);
                    for (int seg = 0; seg < maquina[i].trabalhoFila.Count - 100; seg = seg + 100)
                    {
                        double media = 0;
                        for (int Milisec = 0; Milisec < 100; Milisec++)
                            media += (double)maquina[i].trabalhoFila[seg + Milisec];

                        media = media / 100;
                        trabfilaSec[i].Add(media);
                    }


                    foreach (double x in trabfilaSec[i])
                        file.Write(x + ",");
                    file.Write("\n");

                }

                //Calcula e imprime a média do valor da fila em cada segundo para todas as máquinas.
                file.WriteLine("\nMédia das Maquinas \n");
                for (int i = 0; i < trabfilaSec[0].Count; i++)
                {
                    double mediaTamFila = 0;
                    for (int j = 0; j < qtdMaquinas; j++)
                    {
                        mediaTamFila += Convert.ToDouble(trabfilaSec[j][i]);
                    }
                    mediaTamFila = mediaTamFila / qtdMaquinas;
                    file.Write(mediaTamFila + ", ");
                }

                file.Write("\n\n");

                for (int i = 0; i < qtdMaquinas; i++)
                {
                    trabfilaSec[i].Clear();
                }
            }
        }

        //Escreve no relatório dos menores tempos de processamento
        private void relatorioMenorSPT()
        {
            string relatorio = caminhoRelatorio("Menores Tempos de Processamento.txt");

            //Cria o arquivo .txt somente na primeira replicação
            bool append = true;
            if (replicacaoAtual == 1)
                append = false;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(relatorio, append))
            {
                if (!append)
                {
                    file.WriteLine("              RESULTADOS DA SIMULAÇÃO\r\n");
                    file.WriteLine("Menores Tempos de Processamento na fila da máquina a cada decisão:");
                }

                for (int i = 0; i < qtdMaquinas; i++)
                    foreach (double action in maquina[i].getMenorSPT())
                        file.Write(action + ",");
            }
        }

        //Escreve no relatório de menores trabalhos restantes
        private void relatorioMenorLWR()
        {
            string relatorio = caminhoRelatorio("Menores Trabalhos Totais Restantes.txt");

            //Cria o arquivo .txt somente na primeira replicação
            bool append = true;
            if (replicacaoAtual == 1)
                append = false;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(relatorio, append))
            {
                if (!append)
                {
                    file.WriteLine("              RESULTADOS DA SIMULAÇÃO\r\n");
                    file.WriteLine("Menores trabalhos totais restantes na fila da máquina a cada decisão:");
                }

                for (int i = 0; i < qtdMaquinas; i++)
                    foreach (double action in maquina[i].getMenorLWR())
                        file.Write(action + ",");
            }
        }


        //Escreve no relatório de menores datas de entrega
        private void relatorioMenorEDD()
        {
            string relatorio = caminhoRelatorio("Menores Datas de Entrega.txt");

            //Cria o arquivo .txt somente na primeira replicação
            bool append = true;
            if (replicacaoAtual == 1)
                append = false;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(relatorio, append))
            {
                if (!append)
                {
                    file.WriteLine("              RESULTADOS DA SIMULAÇÃO\r\n");
                    file.WriteLine("Menores datas de entrega na fila da máquina a cada decisão:");
                }

                for (int i = 0; i < qtdMaquinas; i++)
                    foreach (double action in maquina[i].getMenorEDD())
                        file.Write(action + ",");
            }
        }



        //Escreve no relatório de menores trabalhos na próxima fila
        private void relatorioMenorWINQ()
        {
            string relatorio = caminhoRelatorio("Menores Trabalhos Totais na Próxima Fila.txt");

            //Cria o arquivo .txt somente na primeira replicação
            bool append = true;
            if (replicacaoAtual == 1)
                append = false;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(relatorio, append))
            {
                if (!append)
                {
                    file.WriteLine("              RESULTADOS DA SIMULAÇÃO\r\n");
                    file.WriteLine("Menores trabalhos totais próxima fila na fila da máquina a cada decisão:");
                }

                for (int i = 0; i < qtdMaquinas; i++)
                    foreach (double action in maquina[i].getMenorWINQ())
                        file.Write(action + ",");
            }
        }

        //Gera um relatório com dados separados de cada replicação
        private void relatorioReplicacoes()
        {
            string relatorio = caminhoRelatorio("Replicacoes.txt");

            //Garante que, se estiver na primeira replicação, será criado um novo arquivo .txt.
            //Caso contrário, o conteúdo será adicionado ao final do arquivo.
            bool append = true;
            if (replicacaoAtual == 1)
                append = false;

            //--------CONSIDERAR POR UM AVISO FALANDO QUE JA EXISTE O ARQUIVO E SE QUER SOBRESCREVER!
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(relatorio, append))
            {
                if (append == false)
                    file.WriteLine("          RESULTADOS DA SIMULAÇÃO (atrasos)\r\n");


                /*------------------COMENTADO TEMPORARIO----------------------------------------------------
                file.WriteLine("----------------Replicação "+replicacaoAtual+":--------------");
                for (int i = 0; i < qtdMaquinas; i++)
                {
                    file.WriteLine("---Máquina "+i+":");
                    
                    //Escreve o menor tempo de processamento a cada decisão
                    file.Write("Menor Tempo de Processamento:");
                    foreach (double action in maquina[i].getMenorSPT())
                        file.Write(action + ",");
                    file.WriteLine();

                    //Escreve o menor trabalho restante a cada decisão
                    file.Write("Menor Trabalho Restante:");
                    foreach (double action in maquina[i].getMenorLWR())
                        file.Write(action + ",");
                    file.WriteLine();

                    //Escreve a menor data de entrega a cada decisão
                    file.Write("Menor Data de Entrega:");
                    foreach (double action in maquina[i].getMenorEDD())
                        file.Write(action + ",");
                    file.WriteLine();

                    //Escreve o menor trabalho na próxima fila a cada decisão
                    file.Write("Menor Trabalho na Próxima Fila:");
                    foreach (double action in maquina[i].getMenorWINQ())
                        file.Write(action + ",");
                    file.WriteLine();

                    //Escreve a sequência de ações da máquina
                    file.Write("Sequência de ações:");
                    foreach (int action in maquina[i].getRegra())
                        file.Write(action + ",");
                    file.WriteLine("\r\n");
                }
                -----------------------------------------------------------------*/

                //Calcula e escreve o atraso total
                double atrasototal = 0;
                for (int i = 0; i < qtdTarefas; i++)
                    if (tarefa[i].getAtraso() > 0)
                        atrasototal += tarefa[i].getAtraso();
                file.Write(atrasototal + ", ");
                //file.WriteLine("\r\n\r\n");
            }
        }

        //Utilizado no HANDLER do botão Avançar
        public void avancarReplicacao()
        {
            this.mostrar = false;
        }
    }
}
