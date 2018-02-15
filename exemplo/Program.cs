using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sasci;
namespace RPMWPM
{

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (!Memoria.ProcessoVivo("ac_client")) //Verifica se  o processo não está aberto!
            {
                Console.WriteLine("Processo nao foi aberto!");
                return;
            }

            Process p = Memoria.BuscarProcesso("ac_client"); //Busca o processo
            Memoria m = new Memoria(p); // Instancia o objeto

            int Saude = m.Ler<int>(0x151515); //Le no 0x151515 com um valor do tipo inteiro
            Console.WriteLine("Saude: {0}", Saude); //Mostra na tela a Saude
            m.Escrever(0x151515, 999); // Escreve no 0x151515  o valor 999 

            //Ler um ponteiro com 2 Offsets
            int EnderecoLocalPlayer = m.LerPonteiro(0x14514, new int[]{0x15, 0x2C });
            Console.WriteLine("LocalPlayer: 0x{0}", EnderecoLocalPlayer.ToString("X"));

            //Faz uma pesquisa com Pattern e Mask nao pode esquecer do "@" antes das aspas duplas!
            List<int> Resultado = m.BuscarPatternMask(@"\xFF\x0E\x57\x8B\x7C\x24\x14\x8D\x74\x24\x28\xE8\x00\x00\x00\x00\x5F\x5E\xB0\x01\x5B\x8B\xE5\x5D", "xxxxxxxxxxxx????xxxxxxxx", m.ModuloPrincpal);
            if (Resultado.Count <= 0)
            {
                Console.WriteLine("Nenhum resultado!");
                Console.ReadLine();
                return;
            }
           
            //Faz uma pesquisa com AOB

            List<int> ResultadoAOB = m.BuscarAOB("F3 D2 ?? 2D ?? F2 ?? F4", m.ModuloPrincpal);
            if (ResultadoAOB.Count <= 0)
            {
                Console.WriteLine("Nenhum resultado!");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Resultados da pattern: ");
            
            //Mostra resultados da pesqusia Pattern
            foreach (int i in Resultado)
            {
                Console.WriteLine($"Encontrado: 0x{i.ToString("X")}");
            }
            Console.WriteLine("Resultados da aob: ");
            //Mostra resultados da pesquisa AOB
            foreach (int i in ResultadoAOB)
            {
                Console.WriteLine($"Encontrado: 0x{i.ToString("X")}");
            }

            Console.ReadLine();
        }
    }
    
}
