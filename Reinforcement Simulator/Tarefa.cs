using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Simulador_Final;

namespace Simulador
{
    class Tarefa
    {
        //Identificador da Tarefa: 1-200
        private int id;
        //Ordem das máquinas: 0-4, sem repetição
        private int[] ordem = new int[5];
        //Tempo de processamento em cada máquina: aleatório 1-20
        private float[] tempoDeProcessamento = new float[5];

        private Button botaoTarefa;

        public Tarefa(int id)
        {
            this.id = id;
            int x, temp;
            Random rnd = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < 5; i++)
                ordem[i] = i;

            for (int i = 0; i < 5; i++)
            {
                x = rnd.Next(0, 5);
                temp = ordem[x];
                ordem[x] = ordem[i];
                ordem[i] = temp;
            }

            //----------------------------------VERIFICAR NO TRAB  SE EH ISSO MSM----
            for (int i = 0; i < 5; i++)
                tempoDeProcessamento[i] = (float)rnd.NextDouble() * 19 + 1;


            botaoTarefa = new Button();
            botaoTarefa.Content = "Tarefa " + id;
            //--------------O ESTILO DEVE ESTAR EM APP.XAML--------------
            botaoTarefa.Style = (Style)(App.Current.Resources["estiloTarefa"]);
        }

        public int getId()
        {
            return this.id;
        }

        public int[] getOrdem()
        {
            return this.ordem;
        }

        public float[] getTempoDeProcessamento()
        {
            return this.tempoDeProcessamento;
        }

        public Button getBotaoTarefa()
        {
            return this.botaoTarefa;
        }
    }
}
