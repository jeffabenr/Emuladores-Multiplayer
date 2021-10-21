using SimplePackets;
using SimpleTcp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ChatterClient
{
    public class MainWindowViewModel : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get { return _username; }
            set { OnPropertyChanged(ref _username, value); }
        }

        private object[] _jogadores;
        public object[] Jogadores
        {
            get { return _jogadores; }
            set { OnPropertyChanged(ref _jogadores, value); }
        }

        private string _address;
        public string Address
        {
            get { return _address; }
            set { OnPropertyChanged(ref _address, value); }
        }

        private string _port = "8000";
        public string Port
        {
            get { return _port; }
            set { OnPropertyChanged(ref _port, value); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { OnPropertyChanged(ref _message, value); }
        }

        private string _partida;
        public string Partida
        {
            get { return _partida; }
            set { OnPropertyChanged(ref _partida, value); }
        }

        private string _colorCode;
        public string ColorCode
        {
            get { return _colorCode; }
            set { OnPropertyChanged(ref _colorCode, value); }
        }

        private string _emuladores;
        public string Emuladores
        {
            get { return _emuladores; }
            set { OnPropertyChanged(ref _emuladores, value); }
        }

        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand SendCommand { get; set; }
        public ICommand SendPartida { get; set; }

        private ChatroomViewModel _chatRoom;
        public ChatroomViewModel ChatRoom
        {
            get { return _chatRoom; }
            set { OnPropertyChanged(ref _chatRoom, value); }
        }

        public MainWindowViewModel()
        {
            ChatRoom = new ChatroomViewModel();
          
            ConnectCommand = new AsyncCommand(Connect, CanConnect);
            DisconnectCommand = new AsyncCommand(Disconnect, CanDisconnect);
            SendCommand = new AsyncCommand(Send, CanSend);
            SendPartida = new AsyncCommand(iniciarPartida, CanPartida);
        }

        private async Task Connect()
        {
            if (ChatRoom != null)
                await ChatRoom.Disconnect();


            ChatRoom = new ChatroomViewModel();
            string url = "127.0.0.1";
            //string url = "emuladores-br.ddns.net";
            Port = "8000";
            int socketPort = 0;
            var validPort = int.TryParse(Port, out socketPort);

            if (!validPort)
            {
                DisplayError("Forneça uma porta válida.");
                return;
            }

            if (String.IsNullOrWhiteSpace(url))
            {
                DisplayError("Forneça um endereço válido.");
                return;
            }

            if (String.IsNullOrWhiteSpace(Username))
            {
                DisplayError("Forneça um nome de usuário.");
                return;
            }
            

            ChatRoom.Clear();
         
            await Task.Run(() => ChatRoom.Connect(Username, url, socketPort));
        }

        private async Task Disconnect()
        {
            if (ChatRoom == null)
                DisplayError("Você não está conectado a um servidor.");

            await ChatRoom.Disconnect();
        }

        private async Task Send()
        {
            if (ChatRoom == null)
                DisplayError("Você não está conectado a um servidor.");
            if (String.IsNullOrWhiteSpace(Message))
            {
                DisplayError("Forneça uma mensagem!");
                return;
            }

            if (ColorCode == "Vermelho")
                ColorCode = "Red";
            else if (ColorCode == "Azul")
                ColorCode = "Blue";
            else if (ColorCode == "Verde")
                ColorCode = "green";
            else if (ColorCode == "Laranja")
                ColorCode = "Orange";
            else if (ColorCode == "Preto")
                ColorCode = "Black";



            await ChatRoom.Send(Username, Message, ColorCode);
            Message = string.Empty;
        }
        private async Task iniciarPartida()
        {
            if (ChatRoom == null)
                DisplayError("Você não está conectado a um servidor.");






            //await ChatRoom.IniciarPartida(Username, Message, ColorCode);


            // await ChatRoom.IniciarPartida(Username,"Super Bomberman 4","Super Nintendo","Público", "emuladores-br.ddns.net", "Mednafem");
            await ChatRoom.IniciarPartida(Username, "Super Bomberman 4", "Super Nintendo", "emuladores-br.ddns.net","Público","Mednafem");
            Partida = string.Empty;
        }

        private bool CanConnect() => !ChatRoom.IsRunning;
        private bool CanDisconnect() => ChatRoom.IsRunning;
        private bool CanSend() => !String.IsNullOrWhiteSpace(Message) && ChatRoom.IsRunning;
        private bool CanPartida() =>  ChatRoom.IsRunning;

        private void DisplayError(string message) => 
            MessageBox.Show(message, "Uau aí!", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
