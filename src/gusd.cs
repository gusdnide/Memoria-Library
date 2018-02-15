using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
namespace Sasci
{
    public class Memoria
    {
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr handle, int address, ref IntPtr buffer, uint size, IntPtr numberofbytesread);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        /// <summary>
        /// Handle do processo
        /// </summary>
        public IntPtr handle { get; private set; }
        /// <summary>
        /// Processo atacado
        /// </summary>
        public Process Processo { get; private set; }
        /// <summary>
        /// Modulo principal ex: (Game.exe)
        /// </summary>
        public ProcessModule ModuloPrincpal { get; }
        /// <summary>
        /// Instancia um novo objeto
        /// </summary>
        /// <param name="p"> Processo a ser attached </param>
        public Memoria(Process p)
        {
            Processo = p;
            ModuloPrincpal = p.MainModule;
            handle = AbrirProc(p);
        }
        /// <summary>
        /// Ler endereço de um ponteiro
        /// </summary>
        /// <param name="endereco">Ponteiro</param>
        /// <returns>Retorna o endereço no ponteiro</returns>
        public int LerPonteiro(int endereco)
        {
            return Ler<int>(endereco);
        }
        /// <summary>
        /// Ler endereço de um ponteiro com offsets
        /// </summary>
        /// <param name="endereco">Ponteiro</param>
        /// <param name="offsets">Offsets</param>
        /// <returns>Retorna o endereço no ponteiro</returns>
        public int LerPonteiro(int endereco, int[] offsets)
        {
            int Retorno = Ler<int>(endereco + offsets[0]);
            for (int i = 1; i < offsets.Length; i++)
            {
                Retorno = Ler<int>(Retorno + offsets[i]);
            }
            return Retorno;
        }
        /// <summary>
        /// Ler o valor em algum endereço
        /// </summary>
        /// <typeparam name="T">Tipo de leitura (bool, int, byte[], char....) aceita estruturas também</typeparam>
        /// <param name="endereco">Endereço</param>
        /// <returns>Retorna o resultado da leitura</returns>
        public T Ler<T>(int endereco) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var buffer = Marshal.AllocHGlobal(size);
            var obj = default(T);
            if (ReadProcessMemory(handle, endereco, ref buffer, (uint)size, IntPtr.Zero))
            {
                var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                obj = (T)Marshal.PtrToStructure(bufferHandle.AddrOfPinnedObject(), typeof(T));
                bufferHandle.Free();
            }
            return obj;
        }
        /// <summary>
        /// Verifica se o processo está em execuçao
        /// </summary>
        /// <returns>Retorna se o processo está ou não aberto</returns>
        public bool ProcessoVivo()
        {
            return !Processo.HasExited;
        }
        /// <summary>
        /// Pega a base de um modulo
        /// </summary>
        /// <param name="m">Modulo</param>
        /// <returns>Retorna a base</returns>
        public int PegarBase(ProcessModule m)
        {
            return m.BaseAddress.ToInt32();
        }
        /// <summary>
        /// Pega o tamanho de um modulo
        /// </summary>
        /// <param name="m">Modulo</param>
        /// <returns>Retorna o tamanho</returns>
        public int PegarTamanho(ProcessModule m)
        {
            return m.ModuleMemorySize;
        }
        /// <summary>
        /// Verifica se o processo está em execuçao
        /// </summary>
        /// <param name="n">Nome do processo</param>
        /// <returns>Retorna se o processo está ou não aberto</returns>
        public static bool ProcessoVivo(string n)
        {
            if (Process.GetProcessesByName(n).Length > 0)
                return true;
            return false;
        }
        /// <summary>
        /// Busca um processo pelo nome
        /// </summary>
        /// <param name="n">Nome do processo </param>
        /// <returns>Retorna o processo</returns>
        public static Process BuscarProcesso(string n)
        {
            n = n.Replace(".exe", "");
            return Process.GetProcessesByName(n)[0];
        }
        /// <summary>
        /// Abre um processo para leitura e escrita de memoria
        /// </summary>
        /// <param name="p">Processo</param>
        /// <returns>Retorna a handle</returns>
        public IntPtr AbrirProc(Process p)
        {
            return OpenProcess(0x1F0FFF, false, p.Id);
        }
        /// <summary>
        /// Escreve uma sequencia de bytes em tal endereço
        /// </summary>
        /// <param name="endereco">Endereço</param>
        /// <param name="bytes">Bytes a escrever</param>
        /// <returns>Retorna se foi possivel ou nao a escrita</returns>
        public bool EscreverBytes(int endereco, byte[] bytes)
        {
            int saida = 0;
            byte[] buffer = bytes;
            return WriteProcessMemory(handle, endereco, buffer, buffer.Length, ref saida);
        }

        private bool Compare(int end, byte[] bytes, int[] locs)
        {
            Func<int, byte> LerByte = (adr) => { return Ler<byte>(adr); };
            for (int i = 0; i < bytes.Length; i++)
            {
                if (locs.Contains(i))
                    continue;
                if (LerByte(end + (i)) != bytes[i])
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Busca uma assinatura uasndo a pattern a mascara
        /// </summary>
        /// <param name="pattern">Pattern</param>
        /// <param name="mask">Mascara</param>
        /// <param name="modulo">Modulo para fazer o scan</param>
        /// <returns>Retorna uma lista com o resultado</returns>
        public List<int> BuscarPatternMask(string pattern, string mask, ProcessModule modulo)
        {
            int Base = modulo.BaseAddress.ToInt32();
            int Tamanho = modulo.ModuleMemorySize;
            return BuscarPatternMask(pattern, mask, Base, Tamanho);
        }

        /// <summary>
        /// Faz uma busca por uma Array Of Bytes
        /// </summary>
        /// <param name="aob">AOB String</param>
        /// <param name="modulo">Modulo</param>
        /// <returns>Retorna uma lista com o resultado.</returns>
        public List<int> BuscarAOB(string aob, ProcessModule modulo)
        {
            int Base = PegarBase(modulo);
            int Tamanho = PegarTamanho(modulo);
            return BuscarAOB(aob, Base, Tamanho);
        }
        /// <summary>
        /// Busca uma assinatura uasndo a pattern a mascara
        /// </summary>
        /// <param name="pattern">Pattern</param>
        /// <param name="mask">Mascara</param>
        /// <param name="inicio">Endereco de inicio</param>
        /// <param name="fim">Endereco do fim de scan</param>
        /// <returns>Retorna uma lista com o resultado.</returns>
        public List<int> BuscarPatternMask(string pattern, string mask, int inicio, int fim)
        {
            List<int> Retorno = new List<int>();
            int Base = inicio;
            int Tamanho = fim;
            mask = mask.Replace(" ", ""); //Remover espaços desnecessarios
            pattern = pattern.Replace(" ", ""); //Remover espaços desnecessarios
            List<byte> Bytes = new List<byte>();
            List<int> Locs = new List<int>();
            string[] patrnDiv = pattern.Remove(0, 2).Split(new string[] { @"\x" }, StringSplitOptions.None);
            for (int j = 0; j < mask.Length; j++)
            {
                if (mask[j] == 'x')
                {

                    Bytes.Add(Convert.ToByte(patrnDiv[j], 16));
                }
                else if (mask[j] == '?')
                {
                    Locs.Add(j);
                    Bytes.Add(0x00);
                }
            }
            for (int i = Base; i < (Base + Tamanho); i++)
            {
                bool Resultado = Compare(i, Bytes.ToArray(), Locs.ToArray());
                if (Resultado)
                {
                    Retorno.Add(i);
                }
            }
            return Retorno;
        }
        /// <summary>
        /// Buscar modulo pelo nome do arquivo!
        /// </summary>
        /// <param name="nome">Nome do modulo</param>
        /// <returns>Retorna o modulo!</returns>
        public ProcessModule BuscarModulo(string nome)
        {
            foreach (ProcessModule p in Processo.Modules)
            {
                if (p.FileName == nome)
                    return p;
            }
            return null;
        }
        /// <summary>
        /// Retorna modulos do processo!
        /// </summary>
        public List<ProcessModule> Modulos
        {
            get
            {
                List<ProcessModule> Retorno = new List<ProcessModule>();
                foreach (ProcessModule p in Processo.Modules)
                {
                    Retorno.Add(p);
                }
                return Retorno;
            }
        }
        /// <summary>
        /// Faz uma busca por uma Array Of Bytes
        /// </summary>
        /// <param name="aob">AOB String</param>
        /// <param name="inicio">Endereco inicio do scan</param>
        /// <param name="fim">Endereco fim do scan</param>
        /// <returns>Retorna uma lista com o resultado.</returns>
        public List<int> BuscarAOB(string aob, int inicio, int fim)
        {
            List<int> Retorno = new List<int>();
            int Base = inicio;
            int Tamanho = fim;
            List<byte> Bytes = new List<byte>();
            List<int> Locs = new List<int>();
            string[] aobDiv = aob.Split(' ');
            for (int j = 0; j < aobDiv.Length; j++)
            {
                string s = aobDiv[j];
                if (s == "??" || s == "?")
                {
                    Locs.Add(j);
                    Bytes.Add(0x00);
                }
                else if (s.Length == 2)
                {
                    Bytes.Add(Convert.ToByte(s, 16));
                }
            }
            for (int i = Base; i < (Base + Tamanho); i++)
            {
                bool Resultado = Compare(i, Bytes.ToArray(), Locs.ToArray());
                if (Resultado)
                {
                    Retorno.Add(i);
                }
            }
            return Retorno;
        }
        /// <summary>
        /// Escreve na memoria 
        /// </summary>
        /// <typeparam name="T">Tipo</typeparam>
        /// <param name="endereco">Endereco</param>
        /// <param name="value">Valor</param>
        /// <returns>Retorna se foi possivel ou nao escrever na memoria</returns>
        public bool Escrever<T>(int endereco, T value) where T : struct
        {
            int saida = 0;
            int rawSize = Marshal.SizeOf(value);
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(value, buffer, false);
            byte[] rawDatas = new byte[rawSize];
            Marshal.Copy(buffer, rawDatas, 0, rawSize);
            var ret = WriteProcessMemory(handle, endereco, rawDatas, rawSize, ref saida);
            Marshal.FreeHGlobal(buffer);
            return ret;
        }
    }
}