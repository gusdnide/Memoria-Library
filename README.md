# Memoria Library
Simples biblioteca contendo varias funçoes para criar trainers e varios outros cheats

## Funcoes
![classes](https://2cheat.net/uploads/monthly_2018_02/image.png.303c64f3a1ecced676e59423d463f37b.png)

* Scan AOB
* Scan Patter & Mask ( Sig )
* Escrever Array de Bytes
* Escrever na memoria (tipos (int, float, bool, string, char, double, long ...) e tbm objetos ou estruturas)
* Ler na memoria (tipos (int, float, bool, string, char, double, long...) e tbm objetos ou estruturas)
* PegarBase de algum modulo
* PegarTamanho de algum modulo
* Verificar se o processo está rodando
* Buscar um processo pelo nome
* Ler ponteiros com ou sem offsets...
* Buscar um modulo pelo nome do arquivo ex: ("CShell.dll", "Game.exe"....)
* Pegar todos os modulos do processo


### Exemplo de uso:
```
   using Sasci;
   //...
   Process p = Memoria.BuscarProcesso("ac_client"); //Busca o processo
   Memoria m = new Memoria(p); // Instancia o objeto
   int Saude = m.Ler<int>(0x151515); //Le no 0x151515 com um valor do tipo inteiro
   Console.WriteLine("Saude: {0}", Saude); //Mostra na tela a Saude
   m.Escrever(0x151515, 999); // Escreve no 0x151515  o valor 999 
```

## Releases
[DOWNLOAD](https://github.com/gusdnide/Memoria-Library/releases/tag/1.0)

Deixe os devidos creditos, se você é uma pessoa respeitavel!