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
using System.Windows.Shapes;

namespace Reinforcement_Simulator
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class novaSimulacao : Window
    {

        public novaSimulacao()
        {
            InitializeComponent();
            this.Title = "Iniciar Nova Simulação";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

            if (validar(textBox1.Text, "\"Máquina\"") && validar(textBox2.Text, "\"Tarefa\"") && validar(textBox3.Text, "\"Replicações\"") && validar(textBox4.Text, "\"Replicações a exibir\""))
            {
                var main = App.Current.MainWindow as MainWindow;

                int nroMaquinas = Convert.ToInt32(textBox1.Text);
                int nroTarefas = Convert.ToInt32(textBox2.Text);
                int nroReplicacoes = Convert.ToInt32(textBox3.Text);
                int nroReplicacoesExibir = Convert.ToInt32(textBox4.Text);

                int[] exibir = new int[nroReplicacoesExibir];
                ReplicacoesAExibir janelaReplicacoes = new ReplicacoesAExibir(nroReplicacoesExibir, nroReplicacoes);


                if (nroReplicacoes == nroReplicacoesExibir || nroReplicacoesExibir==0)
                {
                    janelaReplicacoes.Close();
                    for (int i = 0; i < nroReplicacoesExibir; i++)
                        exibir[i] = i + 1;
                }
                else
                {
                    janelaReplicacoes.ShowDialog();
                    exibir = janelaReplicacoes.getRepExibir();
                }


                if ((bool)SPT.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 0, nroReplicacoes, exibir);
                if ((bool)WINQ.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 1, nroReplicacoes, exibir);
                if ((bool)TFA.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 2, nroReplicacoes, exibir);
                if ((bool)maiorFTVM.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 3, nroReplicacoes, exibir);
                if ((bool)EDD_LWR.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 4, nroReplicacoes, exibir);
                if ((bool)menorVelocidade.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 5, nroReplicacoes, exibir);
                if ((bool)maiorVelocidade.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 6, nroReplicacoes, exibir);
                if ((bool)LWR.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 7, nroReplicacoes, exibir);
                if ((bool)menorFVT.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 8, nroReplicacoes, exibir);
                if ((bool)maiorFVT.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 9, nroReplicacoes, exibir);
                if ((bool)menorFTVM.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 10, nroReplicacoes, exibir);
                if ((bool)EDD.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 11, nroReplicacoes, exibir);
                if ((bool)maiorTVMS.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 12, nroReplicacoes, exibir);

                if ((bool)regraAleatoria.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 20, nroReplicacoes, exibir);

                if ((bool)tarefaAleatoria.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 100, nroReplicacoes, exibir);

                if ((bool)aprendizadoReforco.IsChecked)
                    main.iniciarSimulacao(nroTarefas, nroMaquinas, 13, nroReplicacoes, exibir);
            }
        }



        private bool validar(string texto, string campo)
        {
            bool invalido = false;
            for (int i = 0; i < texto.Length; i++)
                if (texto[i] < '0' || texto[i] > '9')
                {
                    invalido = true;
                }
            if (invalido)
            {
                MessageBox.Show("Apenas números são aceitos no campo "+campo);
            }

            if(texto.Length ==0)
            {
                MessageBox.Show("Não são aceitos campos em branco");
                invalido = true;
            }

            return !invalido;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

 
    }
}
