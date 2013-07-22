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

namespace Simulador_Final
{
    /// <summary>
    /// Interaction logic for ReplicacoesAExibir.xaml
    /// </summary>
    public partial class ReplicacoesAExibir : Window
    {
        int nroRepExibir;
        int nroRepTotal;
        int[] exibir;
        Label[] l;
        TextBox[] t;
        Button Ok;

        public ReplicacoesAExibir(int nroRepExibir, int nroRepTotal)
        {
            InitializeComponent();

            this.nroRepTotal = nroRepTotal;
            this.nroRepExibir = nroRepExibir;
            l = new Label[nroRepExibir];
            t = new TextBox[nroRepExibir];
            exibir = new int[nroRepExibir];

            int i;
            for (i = 0; i < nroRepExibir; i++)
            { 
                l[i] = new Label();
                l[i].Content = (i + 1)+ "ª replicação a exibir:";
                l[i].Width = 150;
                l[i].Height = 30;
                l[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                l[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                l[i].Margin = new System.Windows.Thickness(5, 50 + 50 * i, 0, 0);
                baseGrid.Children.Add(l[i]);

                t[i] = new TextBox();
                t[i].Width = 100;
                t[i].Height = 30;
                t[i].HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                t[i].VerticalAlignment = System.Windows.VerticalAlignment.Top;
                t[i].Margin = new System.Windows.Thickness(160, 50 + 50 * i, 0, 0);
                baseGrid.Children.Add(t[i]);
            }
            Ok = new Button();
            Ok.Content = "Ok";
            Ok.VerticalAlignment = VerticalAlignment.Top;
            Ok.HorizontalAlignment = HorizontalAlignment.Right;
            Ok.Margin = new Thickness(0, 50+50*i, 20, 15);
            Ok.Click += botaoOK_Click;
            Ok.Width = 70;
            baseGrid.Children.Add(Ok);


        }

        public int[] getRepExibir()
        {
            return this.exibir;
        }

        private void botaoOK_Click(object sender, RoutedEventArgs e)
        {
            bool todosValidos = true;
            for (int i = 0; i < nroRepExibir; i++)
            { 
                if(!validar(t[i].Text, i.ToString()))
                    todosValidos = false;
            }

            if (todosValidos)
            {
                for (int i = 0; i < nroRepExibir; i++)
                {
                    exibir[i] = Convert.ToInt32(t[i].Text);
                }
                this.Close();
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
                MessageBox.Show("Apenas números são aceitos no campo " + campo);
            }

            if (texto.Length == 0)
            {
                MessageBox.Show("Não são aceitos campos em branco");
                invalido = true;
            }
            else 
            {
                int teste = Convert.ToInt32(texto);
                if (teste > nroRepTotal)
                {
                    MessageBox.Show("O número da replicação a ser exibida deve ser menor que o número total de replicações.");
                    invalido = true;
                }
            }


            return !invalido;
        }
    }
}
