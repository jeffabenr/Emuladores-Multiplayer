using SimplePackets;
using SimpleTcp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatterServer
{
    public class MainWindowViewModel : BaseViewModel
    {
        private string _externalAddress;
        public string ExternalAddress
        {
            get { return _externalAddress; }
            set { OnPropertyChanged(ref _externalAddress, value); }
        }

        private string _port = "8000";
        public string Port
        {
            get { return _port; }
            set { OnPropertyChanged(ref _port, value); }
        }

        private string _status = "Idle";
        public string Status
        {
            get { return _status; }
            set { OnPropertyChanged(ref _status, value); }
        }

        private int _clientsConnected;
        public int ClientsConnected
        {
            get { return _clientsConnected; }
            set { OnPropertyChanged(ref _clientsConnected, value); }
        }

        public ObservableCollection<string> Outputs { get; set; }
        public Dictionary<string,string> Usernames = new Dictionary<string, string>();

        public ICommand RunCommand { get; set; }
        public ICommand StopCommand { get; set; }

        private SimpleServer _server;
        private bool _isRunning;

        private Task _updateTask;
        private Task _listenTask;

        public MainWindowViewModel()
        {
            Outputs = new ObservableCollection<string>();

            RunCommand = new AsyncCommand(Run);
            StopCommand = new AsyncCommand(Stop);
        }

        private async Task Run()
        {
            Status = "Conectando...";
            await SetupServer();
            _server.Open();
            _listenTask = Task.Run(() => _server.Start());
            _updateTask = Task.Run(() => Update());
            _isRunning = true;
        }

        private async Task SetupServer()
        {
            Status = "Validando soquete...";
            int socketPort = 0;
            var isValidPort = int.TryParse(Port, out socketPort);

            if (!isValidPort)
            {
                DisplayError("O valor da porta não é válido.");
                return;
            }

            Status = "Obtendo IP...";
            await Task.Run(() => GetExternalIp());
            Status = "Configurando o servidor...";
            _server = new SimpleServer(IPAddress.Any, socketPort);

            Status = "Configurando eventos...";
            _server.OnConnectionAccepted += Server_OnConnectionAccepted;
            _server.OnConnectionRemoved += Server_OnConnectionRemoved;
            _server.OnPacketSent += Server_OnPacketSent;
            _server.OnPersonalPacketSent += Server_OnPersonalPacketSent;
            _server.OnPersonalPacketReceived += Server_OnPersonalPacketReceived;
            _server.OnPacketReceived += Server_OnPacketReceived;
        }

        private void Update()
        {
            while(_isRunning)
            {
                Thread.Sleep(5);
                if (!_server.IsRunning)
                {
                    Task.Run(() => Stop());
                    return;
                }

                ClientsConnected = _server.Connections.Count;
                Status = "Online";
            }
        }

        private async Task Stop()
        {
            try
            {
                ExternalAddress = string.Empty;
                _isRunning = false;
                ClientsConnected = 0;
                _server.Close();

                await _listenTask;
                await _updateTask;
                Status = "Parado";
            }
            catch (Exception ex)
            {
                WriteOutput(ex.Message);
                MessageBox.Show(ex.Message);
            }
           
        }


        private void GetExternalIp()
        {
            try
            {
                string externalIP;
                externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                             .Matches(externalIP)[0].ToString();
                ExternalAddress = externalIP;
            }
            catch { ExternalAddress = "Erro ao receber endereço IP."; }
        }

        private void DisplayError(string message)
        {
            MessageBox.Show(message, "Uau aí!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Server_OnPacketSent(object sender, PacketEvents e)
        {

        }

        private void Server_OnPacketReceived(object sender, PacketEvents e)
        {

        }

        private void Server_OnPersonalPacketSent(object sender, PersonalPacketEvents e)
        {
            WriteOutput("Pacote Pessoal Enviado");
        }
        private void Server_OnPersonalPacketReceived(object sender, PersonalPacketEvents e)
        {
            if(e.Packet.Package is UserConnectionPacket ucp)
            {
                var notification = new ChatPacket
                {
                    Username = "Servidor",
                    Message = "Um novo usuário entrou no chat",
                    UserColor = Colors.Purple.ToString()
                };
                var notification2 = new PartidasPacket
                {
                    Username = "Servidor",
                    Jogo = "Super Bomberman 4",
                    Emulador = "Super Nintendo"
                };

                if (Usernames.Keys.Contains(ucp.UserGuid))
                    Usernames.Remove(ucp.UserGuid);
                else
                    Usernames.Add(ucp.UserGuid, ucp.Username);

                ucp.Users = Usernames.Values.ToArray();

                Task.Run(() => _server.SendObjectToClients(ucp)).Wait();
                Thread.Sleep(500);
                Task.Run(() => _server.SendObjectToClients(notification)).Wait();
                Task.Run(() => _server.SendObjectToClients(notification2)).Wait();
            }
            WriteOutput("Pacote Pessoal Recebido");
        }

        private void Server_OnConnectionAccepted(object sender, PacketEvents e)
        {
            WriteOutput("Cliente conectado: " + e.Sender.Socket.RemoteEndPoint.ToString());
        }

        private void Server_OnConnectionRemoved(object sender, PacketEvents e)
        {
            if(!Usernames.ContainsKey(e.Sender.ClientId.ToString()))
            {
                return;
            }

            var notification = new ChatPacket
            {
                Username = "Servidor",
                Message = "Um usuário saiu do chat",
                UserColor = Colors.Purple.ToString()
            };

            var userPacket = new UserConnectionPacket
            {
                UserGuid = e.Sender.ClientId.ToString(),
                Username = Usernames[e.Sender.ClientId.ToString()],
                IsJoining = false
            };

            if(Usernames.Keys.Contains(userPacket.UserGuid))
                Usernames.Remove(userPacket.UserGuid);

            userPacket.Users = Usernames.Values.ToArray();

            if(_server.Connections.Count != 0)
            {
                Task.Run(() => _server.SendObjectToClients(userPacket)).Wait();
                Task.Run(() => _server.SendObjectToClients(notification)).Wait();
            }
            WriteOutput("Cliente desconectado: " + e.Sender.Socket.RemoteEndPoint.ToString());
        }

        private void WriteOutput(string message)
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                Outputs.Add(message);
            });
        }
    }
}
