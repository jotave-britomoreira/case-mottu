using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CaseMottu.Cruzamentos
{
    public class SolucaoCase
    {
        // Método para ler os dados de mecanicos.csv
        public List<Mecanico> LerMecanicos(string caminhoArquivo)
        {
            var mecanicos = new List<Mecanico>();

            var linhas = File.ReadAllLines(caminhoArquivo);
            foreach (var linha in linhas.Skip(1)) // Ignora a linha do cabeçalho
            {
                var colunas = linha.Split(',');
                mecanicos.Add(new Mecanico
                {
                    mecanicoId = int.Parse(colunas[0]),
                    nome = colunas[1],
                    idade = int.Parse(colunas[2]),
                    tempoPorDia = int.Parse(colunas[3]),
                    nivelComplexidade = int.Parse(colunas[4])
                });
            }
            return mecanicos;
        }

        // Método para ler os dados de consertoDeMotos.csv
        public List<Conserto> LerConsertos(string caminhoArquivo)
        {
            var consertos = new List<Conserto>();

            var linhas = File.ReadAllLines(caminhoArquivo);
            foreach (var linha in linhas.Skip(1)) // Ignora a linha do cabeçalho
            {
                var colunas = linha.Split(',');

                // Verifica se o valor de tempoReal e mecanicoId são "NULL" e define como 0 e -1, respectivamente
                int tempoReal = colunas[3] == "NULL" ? 0 : int.Parse(colunas[3]);
                int mecanicoId = colunas[5] == "NULL" ? -1 : int.Parse(colunas[5]);

                consertos.Add(new Conserto
                {
                    motoId = int.Parse(colunas[0]),
                    complexidadeDoConserto = int.Parse(colunas[1]),
                    tipoConsertoId = int.Parse(colunas[2]),
                    tempoReal = tempoReal,
                    dataEntrada = colunas[4],
                    mecanicoId = mecanicoId
                });
            }
            return consertos;
        }

        // Método para ler os dados de tipoConserto.csv
        public List<TipoConserto> LerTiposDeConserto(string caminhoArquivo)
        {
            var tiposDeConserto = new List<TipoConserto>();

            var linhas = File.ReadAllLines(caminhoArquivo);
            foreach (var linha in linhas.Skip(1)) // Ignora a linha do cabeçalho
            {
                var colunas = linha.Split(',');
                tiposDeConserto.Add(new TipoConserto
                {
                    id = int.Parse(colunas[0]),
                    tempoEstimado = int.Parse(colunas[1])
                });
            }
            return tiposDeConserto;
        }

        // Método para identificar as motos que excederam o tempo de conserto e calcular o mecânico mais eficiente
        public void ProcessarDados(List<Conserto> consertos, List<TipoConserto> tiposDeConserto, List<Mecanico> mecanicos)
        {
            // Lista para armazenar as motos que excederam o tempo de conserto
            List<int> motosExcederamTempo = new List<int>();

            // Dicionário para armazenar a eficiência de cada mecânico
            Dictionary<int, (double horasEficientes, double horasExcedidas)> eficienciaPorMecanico = new Dictionary<int, (double, double)>();

            // Processar cada conserto
            foreach (var conserto in consertos)
            {
                // Ignorar consertos que não possuem um mecânico associado (mecanicoId = -1)
                if (conserto.mecanicoId == -1)
                {
                    continue;
                }

                // Encontrar o tipo de conserto
                var tipoConserto = tiposDeConserto.Find(t => t.id == conserto.tipoConsertoId);
                if (tipoConserto != null)
                {
                    double diferencaTempo = tipoConserto.tempoEstimado - conserto.tempoReal;

                    // Verifica se o mecânico existe na lista de mecânicos
                    var mecanico = mecanicos.Find(m => m.mecanicoId == conserto.mecanicoId);
                    if (mecanico != null)
                    {
                        // Inicializar as horas do mecânico, se necessário
                        if (!eficienciaPorMecanico.ContainsKey(conserto.mecanicoId))
                        {
                            eficienciaPorMecanico[conserto.mecanicoId] = (0, 0);
                        }

                        if (diferencaTempo > 0) // Eficiência
                        {
                            eficienciaPorMecanico[conserto.mecanicoId] =
                                (eficienciaPorMecanico[conserto.mecanicoId].horasEficientes + diferencaTempo, eficienciaPorMecanico[conserto.mecanicoId].horasExcedidas);
                        }
                        else if (diferencaTempo < 0) // Excedeu o tempo
                        {
                            eficienciaPorMecanico[conserto.mecanicoId] =
                                (eficienciaPorMecanico[conserto.mecanicoId].horasEficientes, eficienciaPorMecanico[conserto.mecanicoId].horasExcedidas - diferencaTempo);

                            // Adicionar a moto à lista de motos que excederam o tempo
                            motosExcederamTempo.Add(conserto.motoId);
                        }
                    }
                }
            }

            // Exibir o número de motos que excederam o tempo
            Console.WriteLine($"Número de motos que excederam o tempo de conserto: {motosExcederamTempo.Count}");

            // Exibir os IDs das motos que excederam o tempo
            Console.WriteLine("Moto IDs que excederam o tempo de conserto: " + string.Join(", ", motosExcederamTempo));

            // Encontrar o mecânico mais eficiente (horasEficientes - horasExcedidas)
            var mecanicoMaisEficiente = eficienciaPorMecanico
                .Select(kv => new { MecId = kv.Key, EfTotal = kv.Value.horasEficientes - kv.Value.horasExcedidas })
                .OrderByDescending(m => m.EfTotal)
                .FirstOrDefault();

            // Exibir o nome do mecânico mais eficiente
            if (mecanicoMaisEficiente != null)
            {
                var mecanico = mecanicos.Find(m => m.mecanicoId == mecanicoMaisEficiente.MecId);
                if (mecanico != null)
                {
                    Console.WriteLine($"O mecânico mais eficiente é: {mecanico.nome}");
                }
            }
        }
    }

    // Classes auxiliares
    public class Conserto
    {
        public int motoId { get; set; }
        public int complexidadeDoConserto { get; set; }
        public int tipoConsertoId { get; set; }
        public int tempoReal { get; set; }
        public string dataEntrada { get; set; } = string.Empty; // Inicializa com string vazia
        public int mecanicoId { get; set; }
    }

    public class TipoConserto
    {
        public int id { get; set; }
        public int tempoEstimado { get; set; }
    }

    public class Mecanico
    {
        public int mecanicoId { get; set; }
        public string nome { get; set; } = string.Empty;
        public int idade { get; set; }
        public int tempoPorDia { get; set; }
        public int nivelComplexidade { get; set; }
    }
}
