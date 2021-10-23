using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePackets
{
    [Serializable]
    public class ChatPacket
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public string UserColor { get; set; }
    }

    [Serializable]
    public class PartidasPacket
    {
        
        public string Nome { get; set; }
        public string Jogo { get; set; }
        public string Emulador { get; set; }
        public string Ip { get; set; }
        public string[][] Jogadores { get; set; }
        public string Titulo { get; set; }
        public string TipoServidor { get; set; }
        public string Engine { get; set; }
        public string Key { get; set; }
    }

}
