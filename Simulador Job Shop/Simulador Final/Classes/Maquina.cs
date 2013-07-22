using System;
using System.Windows;   //Style
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;   //ArrayList
using System.Windows.Controls;  //botoes e etc
using System.Windows.Input; //mouseEvent no eventHandler
namespace Simulador_Final
{

    class Maquina
    {
        private int id;
        private bool estaProcessando;
        private Tarefa emProcessamento;

        private double mediaTempoDeVidaDasTarefas;  /*Tempo médio que cada tarefa ficou na máquina, incluindo processamento e fila*/
        private int tarefasConcluidas;
        private int estado; //Utilizado no aprendizado

        //Vetores com dados para saída em arquivo
        private ArrayList menorSPT;
        private ArrayList menorWINQ;
        private ArrayList menorLWR;
        private ArrayList menorEDD;
        private ArrayList regra;
        public ArrayList tamanhoFila;
        public ArrayList trabalhoFila;


        //Elementos Visuais
        private GroupBox caixaMaquina = new GroupBox();
        private Grid gridMaquina = new Grid();
        private WrapPanel fluxoMaquina = new WrapPanel();
        private ScrollViewer scroll = new ScrollViewer();
        private StackPanel painelDetalhes;
        private Label[] Detalhes;
        private Label TituloStackPanel;

        //Fila de tarefas na maquina
        private ArrayList fila = new ArrayList();

        Aprendiz aprendiz;

        public Maquina(int id, Aprendiz aprendiz)
        {
            //Os IDs das maquinas variam de 0 a 4;
            this.id = id;
            estaProcessando = false;
            tarefasConcluidas = 0;
            mediaTempoDeVidaDasTarefas = 0;

            this.aprendiz = aprendiz;

            //Dados adicionados ao array a cada decisão
            //Trata-se de um crosscutting concern. Possibilidade de utilização de aspectos para controlar a saída em relatórios
            menorSPT = new ArrayList();
            menorLWR = new ArrayList();
            menorWINQ = new ArrayList();
            menorEDD = new ArrayList();
            regra = new ArrayList();
            tamanhoFila = new ArrayList();
            trabalhoFila = new ArrayList();

            //Atribuição de parâmetros da GROUPBOX
            caixaMaquina.Content = scroll;
            caixaMaquina.Header = "Maquina " + this.id;
            caixaMaquina.Style = (Style)App.Current.Resources["estiloCaixaMaquina"];
            if (this.id == 0)
                caixaMaquina.Margin = new Thickness(20, 40, 20, 20);
            Grid.SetRow(caixaMaquina, id);

            //Atribuição de parâmetros da PAINEL DE DETALHES
            painelDetalhes = new StackPanel();
            painelDetalhes.Style = (Style)App.Current.Resources["estiloStackPanel"];


            //Atribuição dos parâmetros do TITULO DO STACKPANEL
            TituloStackPanel = new Label();
            TituloStackPanel.Style = (Style)App.Current.Resources["estiloTituloStackPanel"];
            painelDetalhes.Children.Add(TituloStackPanel);

            //Atribuição de parâmetros das ETIQUETAS DE DETALHE
            Detalhes = new Label[3];
            for (int i = 0; i < 3; i++)
            {
                Detalhes[i] = new Label();
                painelDetalhes.Children.Add(Detalhes[i]);
                if(i != 0)
                    Detalhes[i].Style = (Style)App.Current.Resources["estiloDetalhe"];
            }
            Detalhes[0].Content = "Maquina " + id + ":";
            setDetalhes();
            Detalhes[0].Style = (Style)App.Current.Resources["estiloTituloDetalhes"];

            //Atribuição de parâmetros do PAINEL DE FLUXO
            fluxoMaquina.Style = (Style)App.Current.Resources["estiloFluxoMaquina"];

            //Atribuição de parâmetros do SCROLL DO PAINEL DE FLUXO
            scroll.Content = gridMaquina;
            scroll.Style = (Style)App.Current.Resources["estiloScrollMaquina"];

            //Atribuição dos parâmetros do GRID DA MAQUINA
            estiloGridMaquina();

            //Eventos de clique da máquina
            caixaMaquina.MouseDown += maquinaClick;
            caixaMaquina.MouseDown += maquinaFocada;

        }


        private void estiloGridMaquina()
        {
            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = System.Windows.GridLength.Auto;
            gridMaquina.ColumnDefinitions.Add(cd);
            ColumnDefinition cd1 = new ColumnDefinition();
            cd1.Width = System.Windows.GridLength.Auto;
            gridMaquina.ColumnDefinitions.Add(cd1);
            gridMaquina.ShowGridLines = true;
            Grid.SetColumn(fluxoMaquina, 1);
            gridMaquina.Children.Add(fluxoMaquina);
            gridMaquina.Background = System.Windows.Media.Brushes.DarkSeaGreen;
            gridMaquina.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            Label proc = new Label();
            proc.FontSize = 11;
            proc.FontWeight = FontWeights.SemiBold;
            proc.Content = "Em processamento:";
            gridMaquina.Children.Add(proc);
        }

        public ScrollViewer getScroll()
        {
            return scroll;
        }

        public System.Windows.Controls.GroupBox getCaixa()
        {
            return caixaMaquina;
        }

        public int getId()
        {
            return this.id;
        }

        public bool getEstaProcessando()
        {
            return this.estaProcessando;
        }



        //EVENTOS DE CLICK da groupbox da máquina
        public void maquinaClick(object sender, RoutedEventArgs e)
        {
            if (Focada != null)
                Focada(this, new PainelArgs(this.painelDetalhes, this.caixaMaquina, this.id));
        }
        private void maquinaFocada(object sender, RoutedEventArgs e)
        {
            caixaMaquina.BorderBrush = System.Windows.Media.Brushes.Red;
        }



        public void adicionarTarefa(Tarefa t)
        {
            fila.Add(t);

            //Adicionando botão da tarefa na máquina
            t.getBotaoTarefa().Margin = new Thickness(2, 12, 2, 0);
            fluxoMaquina.Children.Add(t.getBotaoTarefa());
        }

        //------------------------------------------------------
        //-----------------REGRAS DE DESPACHO-------------------
        //------------------------------------------------------
        private int menorTempoProc(out double menor)
        {
            int indice = -1;
            menor = Double.MaxValue;
            Tarefa T;

            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];
                if (T.getTempoDeProcessamentoMaq(this.id) < menor)
                {
                    menor = T.getTempoDeProcessamentoMaq(this.id);
                    indice = i;
                }
            }

            return indice;
        }

        private int menorTrabRestante(out double menor)
        {
            int indice = -1;
            Tarefa T;
            double trabalhoRestante = 0;
            double menorTrabRestanteParcial = Double.MaxValue;
            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];
                //soma do trabalho total restante
                for (int j = T.getOperacoesCompletas(); j < T.getQtdMaquina(); j++)
                {
                    trabalhoRestante += T.getTempoDeProcessamento(j);
                }

                if (trabalhoRestante < menorTrabRestanteParcial)
                {
                    menorTrabRestanteParcial = trabalhoRestante;
                    indice = i;
                }
                trabalhoRestante = 0;
            }
            menor = menorTrabRestanteParcial;
            return indice;
        }

        private int menorDataEntrega(out double menor)
        {
            int indice = -1;
            Tarefa T;
            double menorData = Double.MaxValue;
            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];
                if (T.getDataEntrega() < menorData)
                {
                    menorData = T.getDataEntrega();
                    indice = i;
                }
            }

            menor = menorData;
            return indice;
        }

        private int menorDE_mais_TR(out double menor)
        {
            int indice = -1;
            Tarefa T;
            double menorSoma = Double.MaxValue;
            double trabalhoRestante = 0;

            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];

                //soma do trabalho total restante
                for (int j = T.getOperacoesCompletas(); j < T.getQtdMaquina(); j++)
                {
                    trabalhoRestante += T.getTempoDeProcessamento(j);
                }

                if (T.getDataEntrega() + trabalhoRestante < menorSoma)
                {
                    menorSoma = T.getDataEntrega() + trabalhoRestante;
                    indice = i;
                }

                trabalhoRestante = 0;
            }

            menor = menorSoma;
            return indice;
        }

        private int menorTrabProxFila(out double menor)
        { 
            int indice = -1;
            Tarefa T;
            double menorTrabProxFila = Double.MaxValue;
            double trabProxFila;
            var main = App.Current.MainWindow as MainWindow;

            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];

                //soma do trabalho total na próxima fila
                if (T.getOperacoesCompletas() == T.getQtdMaquina()-1)
                {
                    trabProxFila = 0;
                }
                else
                {
                    trabProxFila = main.calcularTrabalhoMaquina(T.getMaquina(T.getOperacoesCompletas() + 1));
                }

                if(trabProxFila < menorTrabProxFila)
                {
                    menorTrabProxFila = trabProxFila;
                    indice = i;
                }
            }
            
            menor = menorTrabProxFila;
            return indice;
        }

        private int maisTrabFilasAnteriores(out double maior)
        {
            int indice = -1;
            Tarefa T;
            double maiorTrabFilasAnteriores = -1;
            double trabFilasAnteriores;
            var main = App.Current.MainWindow as MainWindow;

            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];

                //soma do trabalho total na próxima fila
                if (T.getOperacoesCompletas() == 0)
                {
                    trabFilasAnteriores = 0;
                }
                else
                {
                    trabFilasAnteriores = 0;
                    for(int m=0; m<T.getOperacoesCompletas(); m++)
                        trabFilasAnteriores += main.calcularTrabalhoMaquina(T.getMaquina(m));
                }

                if (trabFilasAnteriores > maiorTrabFilasAnteriores)
                {
                    maiorTrabFilasAnteriores = trabFilasAnteriores;
                    indice = i;
                }
            }

            maior = maiorTrabFilasAnteriores;
            return indice;
        }

        private int menorVelocidade(double tempoatual, out double menor)
        {
            int indice = -1;
            Tarefa T;
            double menorVelocidade = 2;
            double trabalhoRealizado = 0;

            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];

                //soma do trabalho já realizado na tarefa
                for (int j = 0 ; j < T.getOperacoesCompletas(); j++)
                {
                    trabalhoRealizado += T.getTempoDeProcessamento(j);
                }

                if (trabalhoRealizado/ (tempoatual-T.getTempoChegada()+0.0001) < menorVelocidade)
                {
                    menorVelocidade = (trabalhoRealizado + 0.0001) / (tempoatual - T.getTempoChegada() + 0.0001);
                    indice = i;
                }

                trabalhoRealizado = 0;
            }

            menor = menorVelocidade;
            return indice;
        }

        private int maiorVelocidade(double tempoatual, out double maior)
        {
            int indice = -1;
            Tarefa T;
            double maiorVelocidade = 0;
            double trabalhoRealizado = 0;

            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];

                //soma do trabalho total restante
                for (int j = 0; j < T.getOperacoesCompletas(); j++)
                {
                    trabalhoRealizado += T.getTempoDeProcessamento(j);
                }

                if ((trabalhoRealizado + 0.0001) / (tempoatual - T.getTempoChegada() + 0.0001) > maiorVelocidade)
                {
                    maiorVelocidade = (trabalhoRealizado + 0.0001) / (tempoatual - T.getTempoChegada() + 0.0001);
                    indice = i;
                }

                trabalhoRealizado = 0;
            }

            maior = maiorVelocidade;
            return indice;
        }

        private int menorFolgaBaseadoEmVelocidadeTarefa(double tempoatual, out double menor)
        {
            int indice = -1;
            Tarefa T;
            double menorFolga = Double.MaxValue;
            double trabalhoRealizado = 0;
            double trabalhoRestante = 0;
            double velocidade = 0, tempoParaPrazo = 0;

            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];

                //soma do trabalho já realizado na tarefa
                for (int j = 0; j < T.getOperacoesCompletas(); j++)
                {
                    trabalhoRealizado += T.getTempoDeProcessamento(j);
                }

                //soma do trabalho restante a ser realizado na tarefa
                for(int j =T.getOperacoesCompletas(); j<T.getQtdMaquina(); j++)
                {
                    trabalhoRestante += T.getTempoDeProcessamento(j);
                }

                velocidade = trabalhoRealizado / (tempoatual - T.getTempoChegada() + 0.0001) + 0.0001;    /*Adiciona 0.0001 para evitar divisão por zero*/
                tempoParaPrazo = T.getDataEntrega() - tempoatual;

                if (trabalhoRestante/velocidade - tempoParaPrazo < menorFolga)
                {
                    menorFolga = trabalhoRestante / velocidade - tempoParaPrazo;
                    indice = i;
                }

                trabalhoRealizado = 0;
                trabalhoRestante = 0;
            }

            menor = menorFolga;
            return indice;
        }

        private int maiorFolgaBaseadoEmVelocidadeTarefa(double tempoatual, out double maior)
        {
            int indice = -1;
            Tarefa T;
            double maiorFolga = Double.MinValue;
            double trabalhoRealizado = 0;
            double trabalhoRestante = 0;
            double velocidade = 0, tempoParaPrazo = 0;

            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];

                //soma do trabalho já realizado na tarefa
                for (int j = 0; j < T.getOperacoesCompletas(); j++)
                {
                    trabalhoRealizado += T.getTempoDeProcessamento(j);
                }
                //soma do trabalho restante a ser realizado na tarefa
                for(int j =T.getOperacoesCompletas(); j<T.getQtdMaquina(); j++)
                {
                    trabalhoRestante += T.getTempoDeProcessamento(j);
                }

                velocidade = trabalhoRealizado / (tempoatual - T.getTempoChegada() + 0.0001) + 0.0001;
                tempoParaPrazo = T.getDataEntrega() - tempoatual;

                if (trabalhoRestante/velocidade - tempoParaPrazo > maiorFolga)
                {
                    maiorFolga = trabalhoRestante / velocidade - tempoParaPrazo;
                    indice = i;
                }

                trabalhoRealizado = 0;
                trabalhoRestante = 0;
            }

            maior = maiorFolga;
            return indice;
        }

        private int menorFolgaBaseadaEmTempoDeMaquinas(double tempoatual, out double menor)
        {
            int indice = -1;
            Tarefa T;
            double menorTempo = Double.MaxValue;
            double somaTempoProximasMaquinas = 0;
            double tempoParaPrazo = 0;
            var main = App.Current.MainWindow as MainWindow;

            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];

                //soma do tempo estimado que a tarefa ficará nas próximas máquinas, baseado no tempo de vida médio nas máquinas
                for (int j = T.getOperacoesCompletas(); j < T.getQtdMaquina(); j++)
                {
                    somaTempoProximasMaquinas += main.tempoDeVidaMedioNaMaquina(T.getMaquina(j));
                }
                tempoParaPrazo = T.getDataEntrega() - tempoatual;

                if (somaTempoProximasMaquinas - tempoParaPrazo < menorTempo)
                {
                    menorTempo = somaTempoProximasMaquinas - tempoParaPrazo;
                    indice = i;
                }
                somaTempoProximasMaquinas = 0;
            }

            menor = menorTempo;
            return indice;
        }

        private int maiorFolgaBaseadaEmTempoDeMaquinas(double tempoatual, out double maior)
        {
            int indice = -1;
            Tarefa T;
            double maiorTempo = Double.MinValue;
            double somaTempoProximasMaquinas = 0;
            double tempoParaPrazo = 0;
            var main = App.Current.MainWindow as MainWindow;

            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];

                //soma do tempo estimado que a tarefa ficará nas próximas máquinas, baseado no tempo de vida médio nas máquinas
                for (int j = T.getOperacoesCompletas(); j < T.getQtdMaquina(); j++)
                {
                    somaTempoProximasMaquinas += main.tempoDeVidaMedioNaMaquina(T.getMaquina(j));
                }
                tempoParaPrazo = T.getDataEntrega() - tempoatual;

                if (somaTempoProximasMaquinas - tempoParaPrazo > maiorTempo)
                {
                    maiorTempo = somaTempoProximasMaquinas - tempoParaPrazo;
                    indice = i;
                }
                somaTempoProximasMaquinas = 0;
            }

            maior = maiorTempo;
            return indice;
        }

        private int maiorTempoDasMaquinasSeguintes(out double maior)
        {
            int indice = -1;
            Tarefa T;
            double maiorTempo = Double.MinValue;
            double somaTempoProximasMaquinas = 0;
            var main = App.Current.MainWindow as MainWindow;

            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];

                //soma do tempo estimado que a tarefa ficará nas próximas máquinas, baseado no tempo de vida médio nas máquinas
                for (int j = T.getOperacoesCompletas(); j < T.getQtdMaquina(); j++)
                {
                    somaTempoProximasMaquinas += main.tempoDeVidaMedioNaMaquina(T.getMaquina(j));
                }
                if (somaTempoProximasMaquinas > maiorTempo)
                {
                    maiorTempo = somaTempoProximasMaquinas;
                    indice = i;
                }
                somaTempoProximasMaquinas = 0;
            }

            maior = maiorTempo;
            return indice;
        }

        private void setSaidaArq()
        {
            double mSPT, mLWR, mEDD, mWINQ;

            menorTempoProc(out mSPT);
            menorTrabRestante(out mLWR);
            menorDataEntrega(out mEDD);
            menorTrabProxFila(out mWINQ);

            menorSPT.Add(mSPT);
            menorLWR.Add(mLWR);
            menorEDD.Add(mEDD);
            menorWINQ.Add(mWINQ);
        }

        //Shortest Processing Time
        public void SPT()
        {
            double mSPT;
            int indice = menorTempoProc(out mSPT);

            pósRegraDespacho(indice, 0);
        }

        //Work in Next Queue
        public void WINQ()
        {
            double menor;
            int indice = menorTrabProxFila(out menor);
 
            pósRegraDespacho(indice, 1);
        }

        //Trabalho nas Filas Anteriores
        public void TFA()
        {
            double maior;
            int indice = maisTrabFilasAnteriores(out maior);

            pósRegraDespacho(indice, 2);
        }

        //Maior Folga Baseada em Tempo de Vida na Máquina
        public void maiorFTM(double tempo)
        {
            double maior;
            int indice = maiorFolgaBaseadaEmTempoDeMaquinas(tempo, out maior);

            pósRegraDespacho(indice, 3);
        }

        public void EDD_LWR()
        {
            double menor;
            int indice = menorDE_mais_TR(out menor);

            pósRegraDespacho(indice, 4);
        }

        public void menorVelocidade(double tempo)
        {
            double menor;
            int indice = menorVelocidade(tempo, out menor);

            pósRegraDespacho(indice, 5);
        }

        public void maiorVelocidade(double tempo)
        {

            double maior;
            int indice = maiorVelocidade(tempo, out maior);

            pósRegraDespacho(indice, 6);  
        }


        //Least Work Remaining
        public void LWR()
        {
            double menor;
            int indice = menorTrabRestante(out menor);

            pósRegraDespacho(indice, 7);
        }


        //Menor Folga Baseada em Velocidade da Tarefa
        public void menorFVT(double tempo)
        {
            double menor;
            int indice = menorFolgaBaseadoEmVelocidadeTarefa(tempo, out menor);

            pósRegraDespacho(indice, 8);
        }

        //Maior Folga Baseada em Velocidade da Tarefa
        public void maiorFVT(double tempo)
        {
            double maior;
            int indice = maiorFolgaBaseadoEmVelocidadeTarefa(tempo, out maior);

            pósRegraDespacho(indice, 9);
        }


        //Menor Folga Baseada em Tempo de Vida na Máquina
        public void menorFTM(double tempo)
        {
            double menor;
            int indice = menorFolgaBaseadaEmTempoDeMaquinas(tempo, out menor);

            pósRegraDespacho(indice, 10);
        }

        //Earliest Due Date
        public void EDD()
        {
            double menor;
            int indice = menorDataEntrega(out menor);

            pósRegraDespacho(indice, 11);
        }

        //Maior tempo de vida nas máquinas seguintes
        public void maiorTVMS()
        {
            double maior;
            int indice = maiorTempoDasMaquinasSeguintes(out maior);

            pósRegraDespacho(indice, 12);
        }

        public void aprendizadoReforco(double tempo)
        {
            var main = App.Current.MainWindow as MainWindow;
            int[] estado = main.getEstadoSistema(this.id);
            int acao;
            Random rand = new Random();
            if (fila.Count == 1)
            {
                acao = rand.Next(4);
            }
            else
            {//SOH FUNCIONA PRA 5 MAQUINAS
                int s = estado[0] + 3 * estado[1] +
                            9 * estado[2] + 27 * estado[3] + 81 * estado[4];
                acao = aprendiz.selecionarAcao(s);
            }
            switch(acao){
                case 0:SPT();
                    break;
                case 1:
                    WINQ();
                    break;
                case 2:
                    TFA();
                    break;
                case 3:
                    maiorFTM(tempo);
                    break;
            }
        }

        public void regraAleatoria(double tempo)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int x = rnd.Next(0, 4);

            switch (x)
            {
                case 0:
                    SPT();
                    break;
                case 1:
                    WINQ();
                    break;
                case 2:
                    TFA();
                    break;
                case 3:
                    maiorFTM(tempo);
                    break;
            }
        }

        public void tarefaAleatoria()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int indice = rnd.Next(0, fila.Count - 1);

            pósRegraDespacho(indice, 100);
        }

        public void pósRegraDespacho(int indice, int acao)
        {
            //Ajusta vetores de saída e painel
            regra.Add(acao);
            setDetalhes();
            setSaidaArq();

            //Coloca informações de estado (s) na tarefa que será retirada
            //da fila, no momento em que esta ainda está na fila
            Tarefa tarefaSelecionada = (Tarefa)fila[indice];
            tarefaSelecionada.setEstados(acao);

            //Move da fila para o processamento
            estaProcessando = true;
            emProcessamento = (Tarefa)fila[indice];
            fila.RemoveAt(indice);

            //Ajusta elementos gráficos
            emProcessamento.getBotaoTarefa().Margin = new Thickness(5, 22, 5, 0);
            fluxoMaquina.Children.Remove(emProcessamento.getBotaoTarefa());
            Grid.SetColumn(emProcessamento.getBotaoTarefa(), 0);
            gridMaquina.Children.Add(emProcessamento.getBotaoTarefa());

            //Coloca informações de estado (s') e acao na tarefa
            emProcessamento.setProximoEstado();
        }

        public bool isFilaVazia()
        {
            if (fila.Count == 0)
                return true;
            else
                return false;
        }

        public void operacaoFinalizada(double tempo)
        {
            gridMaquina.Children.Remove(this.emProcessamento.getBotaoTarefa());
            emProcessamento.operacaoFinalizada(tempo);
            tarefasConcluidas++;
            mediaTempoDeVidaDasTarefas = (mediaTempoDeVidaDasTarefas * (tarefasConcluidas - 1) + tempo - emProcessamento.getTempoChegadaNaUltimaMaquina())/tarefasConcluidas;
            setDetalhes();
            emProcessamento = null;
            this.estaProcessando = false;
        }

        public void setDetalhes()
        {
            Detalhes[1].Content = "Número de operações completas: " + this.tarefasConcluidas;

            Detalhes[2].Content = "Sequência de regras de despacho: ";
            for (int i = 0; i < regra.Count; i++)
            {
                Detalhes[2].Content = Detalhes[2].Content + this.regra[i].ToString() + ", ";
                if ((i+1) % 10 == 0)
                    Detalhes[2].Content += System.Environment.NewLine;
            }
        }

        private void setEstado()
        {
            double somaDoTrabalho = 0;
            Tarefa T;

            for (int i = 0; i < fila.Count; i++)
            {
                T = (Tarefa)fila[i];
                somaDoTrabalho += T.getTempoDeProcessamentoMaq(this.id);
            }
            if (somaDoTrabalho < 5)
                this.estado = 0;
            else if (somaDoTrabalho < 35)
                this.estado = 1;
            else
                this.estado = 2;
        }

        public void adicionarTamFila()
        {
            tamanhoFila.Add(this.fila.Count);
        }

        public void adicionarTrabFila()
        {
            double trabTotal = 0;
            Tarefa t;
            for (int i = 0; i < fila.Count; i++)
            {
                t = (Tarefa)fila[i];
                trabTotal += t.getTempoDeProcessamentoMaq(this.id);
            }
            trabalhoFila.Add(trabTotal);
        }

        /*----------------------------------------------------
         * ------------------Getters--------------------------
         * -------------------------------------------------*/
        public int getEstado() { setEstado(); return this.estado; }
        public ArrayList getMenorSPT()
        {
            return this.menorSPT;
        }

        public ArrayList getMenorLWR()
        {
            return this.menorLWR;
        }

        public ArrayList getMenorEDD()
        {
            return this.menorEDD;
        }

        public ArrayList getMenorWINQ()
        {
            return this.menorWINQ;
        }

        public ArrayList getRegra()
        {
            return this.regra;
        }

        public double getTempoMedioDeVida()
        {
            return this.mediaTempoDeVidaDasTarefas;
        }

        public ArrayList getFila()
        {
            return this.fila;
        }

        public Tarefa getProcessando()
        {
            return this.emProcessamento;
        }

        public event Simulador_Final.Tarefa.PainelEventHandler Focada;


    }
}
