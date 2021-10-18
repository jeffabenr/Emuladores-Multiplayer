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
        public string Username { get; set; }
        public string Jogo { get; set; }
        public string Emulador { get; set; }
    }
}
