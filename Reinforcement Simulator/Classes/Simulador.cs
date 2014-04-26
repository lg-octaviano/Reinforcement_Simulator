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


namespace Reinforcement_Simulator
{
    class Simulador
    {

        private delegate void UpdateProgressBarDelegate(
        System.Windows.DependencyProperty dp, Object value);

        //Define qual o espaço de estados.
        //Deve ser modificado no construtor diretamente.
        private int espacoDeEstados;

        //Objetos para controle do tempo
        public int tempoDeEspera;
        public bool pausado;
        private double tempo = 0;
        private double dt = 0.01;
        private Label labelTempo;

        //PARA TESTE--------------
        public int regraAleatoria, regraSelecionada;

        //Número de replicações totais e replicações já concluídas
        public int replicacoesTotais;
        private int replicacaoAtual;

        //Replicações a serem mostradas
        public bool mostrar;
        int[] exibirRep;

        //Características escolhidas pelo usuário para a simulação
        public int acao;
        int qtdMaquinas;
        int qtdTarefas;

        //Objetos do sistema de manufatura
        Tarefa[] tarefa;
        Maquina[] maquina;

        int tarefasConcluidas;

        Aprendiz aprendiz;

        //Objeto gráfico que diz o número da replicação atual
        public Label nroReplicacao;


        public Simulador(int qTarefas, int qMaquinas, int acao, int rep, int[] exibirRep)
        {
            var main = App.Current.MainWindow as MainWindow;

            espacoDeEstados = 3;
            regraAleatoria = 0;
            regraSelecionada = 0;

            tempoDeEspera = (int)main.sliderTempo.Value;
            pausado = false;

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
                aprendiz = new Aprendiz(replicacoesTotais);
            else
                aprendiz = null;
        }

        public int selecionarAcao(int s)
        {
            return this.aprendiz.selecionarAcao(s);
        }

        public bool isModoAprendizado()
        {
            return acao == 13;
        }

        public void atualizarQ(int s, int a, double r, int new_s, int new_a)
        {
            this.aprendiz.atualizarQ(s, a, r, new_s, new_a);
        }

        /* Como o Threading Model do WPF exige que os elementos gráficos sejam criados
         * na mesma Thread STA que irá manipulá-las, criaremos os objetos Maquina e 
         * Tarefa na Thread Principal. Para isso, utilizaremos instanciarMaquinas() e
         * instanciarTarefas(), passando-as pelo Dispatcher para a Thread associada a main.
         */
        private void instanciarMaquinas()
        {
            var main = App.Current.MainWindow as MainWindow;
            this.maquina = new Maquina[qtdMaquinas];
            for (int i = 0; i < qtdMaquinas; i++)
            {
                this.maquina[i] = new Maquina(i, aprendiz, this);
                this.maquina[i].Focada += new Reinforcement_Simulator.Tarefa.PainelEventHandler(main.colocarDetalhe);
            }
        }

        private void instanciarTarefas()
        {
            var main = App.Current.MainWindow as MainWindow;

            tarefa = new Tarefa[qtdTarefas];
            Exponential expDist = new Exponential(0.090);
            double ultimaChegada = 0;

            for (int i = 0; i < qtdTarefas; i++)
            {
                ultimaChegada += expDist.Sample();
                ultimaChegada = Math.Round(ultimaChegada, 2);
                tarefa[i] = new Tarefa(i, qtdMaquinas, ultimaChegada, this);
                tarefa[i].Focada += new Reinforcement_Simulator.Tarefa.PainelEventHandler(main.colocarDetalhe);
            }
        }

        public double tempoDeVidaMedioNaMaquina(int m)
        {
            return maquina[m].getTempoMedioDeVida();
        }

        public double calcularTrabalhoMaquina(int m)
        {
            double trabalho = 0;
            for (int i = 0; i < maquina[m].getFila().Count; i++)
            {
                Tarefa T = (Tarefa)maquina[m].getFila()[i];
                trabalho += T.getTempoDeProcessamento(T.getOperacoesCompletas());
            }
            return trabalho;
        }

        public int getEstadoSistema(int m)
        {
            int[] estado;   //Espaço de estados multidimensional 
            int s;  //Adaptação de estado[] para uma dimensao
            double trabalhoOutrasMaquinas;

            switch (espacoDeEstados)
            {
                case 1:            /* Espaco de estados 1*/
                    estado = new int[maquina.Length];
                    for (int i = 0; i < maquina.Length; i++)
                        estado[i] = maquina[i].getEstado();

                    int temp = estado[m];
                    estado[m] = estado[0];
                    estado[0] = temp;

                    String str = "";
                    Array.Sort(estado, 1, estado.Length - 1);
                    for (int i = 0; i < maquina.Length; i++)
                        str += estado[i] + " - ";
                    s = estado[0] + 3 * estado[1] +
                            9 * estado[2] + 27 * estado[3] + 81 * estado[4];

                    //MessageBox.Show("Maquina "+m+" : "+str);
                    break;
                case 2: 
                    estado = new int[2];
                    trabalhoOutrasMaquinas = 0;
                    estado[0] = maquina[m].getEstado();

                    for (int i = 0; i < maquina.Length; i++)
                    {
                        if (i != m)
                            trabalhoOutrasMaquinas += maquina[i].getTrabalhoNaFila();
                    }

                    if (trabalhoOutrasMaquinas < 0.0000001)  //TrabalhoOutrasMaquinas == 0
                        estado[1] = 0;
                    else if (trabalhoOutrasMaquinas < 14)
                        estado[1] = 1;
                    else if (trabalhoOutrasMaquinas < 28)
                        estado[1] = 2;
                    else if (trabalhoOutrasMaquinas < 54)
                        estado[1] = 3;
                    else
                        estado[1] = 4;

                    s = estado[0] + 3 * estado[1];
                    break;
                case 3:
                    estado = new int[2];
                    trabalhoOutrasMaquinas = 0;
                    estado[0] = maquina[m].getEstado();

                    for (int i = 0; i < maquina.Length; i++)
                    {
                        if (i != m)
                            trabalhoOutrasMaquinas += maquina[i].getTrabalhoNaFila();
                    }

                    if (trabalhoOutrasMaquinas < 0.0000001)  //TrabalhoOutrasMaquinas == 0
                        estado[1] = 0;
                    else if (trabalhoOutrasMaquinas < 56)
                        estado[1] = 1;
                    else if (trabalhoOutrasMaquinas < 112)
                        estado[1] = 2;
                    else if (trabalhoOutrasMaquinas < 216)
                        estado[1] = 3;
                    else
                        estado[1] = 4;

                    s = estado[0] + 3 * estado[1];
                    break;
                default:
                    MessageBox.Show("Espaco de Estados Indefinido. Cancele a Simulação.");
                    return 0;
            }

            return s;
        }

        public void iniciarSimulacao(MainWindow main)
        {
            //Mudança na cultura desta thread Worker permite que a impressão
            //nos arquivos se façam no padrão americano, utilizando "." como
            //separador decimal.
            var culture = new System.Globalization.CultureInfo("en-us");
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;

            while (replicacoesTotais > replicacaoAtual)
            {
                iniciarReplicacao(main);
                GC.Collect();
            }
            main.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                main.baseGrid.Children.Remove(this.nroReplicacao);
                main.toolbar.IsEnabled = false;
            }));

            if (isModoAprendizado())
                escreverFuncaoQ(); 

            MessageBox.Show("Todas as replicações foram concluídas.");
        }

        public void iniciarReplicacao(MainWindow main)
        {
            //var main = (MainWindow)param;
            //var main = App.Current.MainWindow as MainWindow;

            tarefasConcluidas = 0;

            main.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                //Valor maximo da barra de progresso é o número de operações que terão de ser completadas
                main.barraProgresso.Maximum = qtdTarefas * qtdMaquinas;
                //Ativação dos botões da toolbar
                main.toolbar.IsEnabled = true;
                main.saida.Children.RemoveRange(1, qtdTarefas);
                main.chaoDeFabrica.Children.Clear();
                main.barraProgresso.Value = 0;
                nroReplicacao.Content = "Replicação nº " + replicacaoAtual;
            }));

            //Define se serão mostrados os dados da replicação
            mostrar = false;
            foreach (int exibir in exibirRep)
            {
                if (exibir == replicacaoAtual)
                    mostrar = true;
            }
            if (mostrar)
            {
                main.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    main.chaoDeFabrica.Children.Add(labelTempo);
                    main.menuSaida.IsEnabled = true;
                    main.menuSaida.IsChecked = true;
                    main.baseGrid.RowDefinitions[3].Height = new System.Windows.GridLength(70);
                }));
            }
            else
                tempoDeEspera = 0;

            //Reset dos dados de replicações anteriores
            tempo = 0;

            //Construtor das máquinas
            main.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(instanciarMaquinas ));

            //Adiciona as máquinas ao chão de fábrica somente se replicação for mostrada
            if (mostrar)
            {
                App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    main.renderizarMaquinas(qtdMaquinas);
                }));
            }

            /*Construtor das tarefas: Aqui se calculam os instantes de chegada da tarefa
              A thread Main deverá criar os objetos tarefa, pois estes possuem elementos gráficos,
              que devem estar obrigatoriamente em uma STA (ver Threading Model do WPF).
            */
            main.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(instanciarTarefas));

            while (tarefasConcluidas < qtdTarefas)
            {
                avancaTempo(main);
                System.Threading.Thread.Sleep(tempoDeEspera);
            }

            GC.Collect();

            escreverRelatorios();
            replicacaoAtual++;
            if (isModoAprendizado())
                aprendiz.atualizarTaxaExploracao(replicacaoAtual);
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

        private void avancaTempo(MainWindow main)
        {
                while (pausado)
                    Thread.Sleep(200);

                tempo = Math.Round(tempo + dt, 2);

                if (!mostrar)
                {
                    main.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        main.menuSaida.IsChecked = false;
                        main.menuSaida.IsEnabled = false;
                        main.baseGrid.RowDefinitions[3].Height = new System.Windows.GridLength(0);
                    }));
                }

                //Imprime um numero determinado de digitos da variavel tempo
                main.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    labelTempo.Content = "TEMPO: " + tempo.ToString("00.00");
                }));

                for (int m = 0; m < qtdMaquinas; m++)
                {
                    //maquina[m].adicionarTamFila();  /*CROSS-CUTTING CONCERN: adiciona o tam. da fila ao vetor para depois colocar no relatório*/
                    //maquina[m].adicionarTrabFila();

                    if (!maquina[m].getEstaProcessando())
                        if (maquina[m].getFila().Count > 0)
                            regraDespacho(m, this.acao, this.tempo);
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
                            main.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                main.barraProgresso.Value++;
                            }));

                            if (temp.getOperacoesCompletas() < qtdMaquinas)
                            {
                                maquina[temp.getMaquina(temp.getOperacoesCompletas())].adicionarTarefa(temp);
                            }
                            else
                            {
                                temp.finalizada(tempo);
                                main.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                                    main.saida.Children.Add(temp.getBotaoTarefa());
                                }));
                                tarefasConcluidas++;
                            }
                        }
                    }
                }

                chegadaTarefas();
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
                    maquina[m].menorFTM(tempo);
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
                    maquina[m].maiorFTM(tempo);
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

        private void escreverFuncaoQ()
        {
             string relatorio = caminhoRelatorio("FuncaoQ.txt");
             double[][] Q = aprendiz.getQ();
             using (System.IO.StreamWriter file = new System.IO.StreamWriter(relatorio))
             {
                 for (int i = 0; i < 243; i++)
                 {
                     for (int j = 0; j < 4; j++)
                         file.Write(Q[i][j] + ", ");
                     file.WriteLine();
                 }
             }
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
            tempoDeEspera = 0;
        }
    }
}
