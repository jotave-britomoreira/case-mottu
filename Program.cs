using System;
using CaseMottu.Cruzamentos;

class Program
{
    static void Main(string[] args)
    {
        SolucaoCase solucao = new SolucaoCase();

    // Processar os dados de consertos e mecânicos
        var mecanicos = solucao.LerMecanicos("mecanicos.csv");
        var consertos = solucao.LerConsertos("consertoDeMotos.csv");
        var tiposDeConserto = solucao.LerTiposDeConserto("tiposDeConserto.csv");

        solucao.ProcessarDados(consertos, tiposDeConserto, mecanicos);
    }
}
