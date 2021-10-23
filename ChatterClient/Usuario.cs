using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuladoresMultiplayer
{
    public class Usuario
    {
        public string Nome { get; set; }
        public string Senha { get; set; }
        public string Key { get; set; }
    }
    [Serializable]
    public class Partida
    {
        public string Nome { get; set; }
        public string Jogo { get; set; }
        public string Emulador { get; set; }
        public string Engine { get; set; }
        public string[][] Jogadores;
        public string TipoServidor { get; set; }
        public string Ip { get; set; }
        public string Titulo { get; set; }
        public string Key { get; set; }
    }
    [Serializable]
    public class RemoverPartida
    {
        public string Nome { get; set; }
        public string Jogo { get; set; }
        public string Emulador { get; set; }
        public string Engine { get; set; }
        public string[][] Jogadores;
        public string TipoServidor { get; set; }
        public string Ip { get; set; }
        public string Titulo { get; set; }
        public string Key { get; set; }
    }
}
