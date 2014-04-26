using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Reinforcement_Simulator
{
    class Aprendiz
    {
        private double[][] Q;
        private int numeroDeReplicacoes;
        private double mi,      /*probabilidade de escolher acao aleatória*/
            alfa,   /*fator de atualizacao em relacao à próxima estimativa de Q*/
            gama;   /*fator de atualizacao em relacao ao prox estado*/
        private Random rand = new Random();


        public Aprendiz(int nroReps)
        {
            numeroDeReplicacoes = nroReps;
            mi = 1;
            alfa = 0.1;
            gama = 0.5;

            //s = 3^5 (3 estados possíveis em cada uma das 5 máquinas)
            Q = new double[243][];
            for (int i = 0; i < 243; i++)
                Q[i] = new double[4];
        }

        public double[][] getQ() { return Q; }

        public void atualizarQ(int s, int a, double r, int new_s, int new_a)
        {
            Q[s][a] = Q[s][a] + alfa*(r + gama*Q[new_s][new_a] - Q[s][a]);
        }


        public int selecionarAcao(int s)
        {
            double maior;
            int acao;

            // Se número aleatório entre 0.0 e 1.0 for menor que mi
            if (rand.NextDouble() < mi)
                return rand.Next(4);    /*seleciona regra aleatoria*/
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

        public void atualizarTaxaExploracao(int replicacaoAtual)
        {
            if (replicacaoAtual - 1 < numeroDeReplicacoes / 2)
                mi = -((double)2*(replicacaoAtual-1)/numeroDeReplicacoes) + 1;
            else
                mi = 0.0;
        }

    }
}
