using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reinforcement_Simulator
{
    class Aprendiz
    {
        private double[][] Q;
        private int numeroReplicacao;
        private double mi,      /*probabilidade de escolher acao aleatória*/
            alfa,   /*fator de atualizacao em relacao ao proximo estado e reforco*/
            gama;   /*fator de atualizacao em relacao ao prox estado*/
        private Random rand = new Random();


        public Aprendiz()
        {
            numeroReplicacao = 0;
            mi = 1;
            alfa = 0.1;
            gama = 0.5;

            //s = 3^5 (3 estados possíveis em cada uma das 5 máquinas)
            Q = new double[243][];
            for (int i = 0; i < 243; i++)
                Q[i] = new double[4];
        }

        public void atualizarQ(int s, int a, double r, int new_s, int new_a)
        {
            Q[s][a] = Q[s][a] + alfa*(r + gama*Q[new_s][new_a] - Q[s][a]);
        }


        public int selecionarAcao(int s)
        {
            int epsilon = Convert.ToInt32(1 / mi);
            double maior;
            int acao;

            if (rand.Next(epsilon) == 0)    /*seleciona regra aleatoria*/
                return rand.Next(4);
            else        /*seleciona regra baseado em Q[s][a]*/
            {
                maior = Q[s][0];
                acao = 0;
                for (int i = 1; i < 4; i++)
                {
                    if (Q[s][i] > maior)
                    {
                        maior = Q[s][i];
                        acao = i;
                    }
                    else
                        if (Q[s][i] == maior)
                        {
                            if (rand.Next(2) == 0)
                            {
                                maior = Q[s][i];
                                acao = i;
                            }
                        }
                }
                return acao;
            }
        }

        public void incrementarReplicacao()
        {
            numeroReplicacao++;
            if (numeroReplicacao < 300)
                mi = 1 - (95 * numeroReplicacao) / 30000;
            else
                mi = 0.05;
        }

    }
}
