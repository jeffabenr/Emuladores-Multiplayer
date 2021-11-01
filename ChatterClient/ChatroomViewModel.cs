using EmuladoresMultiplayer;
using Firebase.Database;
using Firebase.Database.Query;
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

namespace ChatterClient
{
    public class ChatroomViewModel : BaseViewModel
    {
        private readonly FirebaseClient _fbClient = new FirebaseClient("https://emuladoresbr-94d9d-default-rtdb.firebaseio.com/");
        private ObservableCollection<Partida> _dbPartidas = new ObservableCollection<Partida>();
        private ObservableCollection<string> _dbJogadores = new ObservableCollection<string>();
        public ObservableCollection<ChatPacket> Messages { get; set; }
        public ObservableCollection<PartidasPacket> Partidas { get; set; }
        public ObservableCollection<string> Users { get; set; }
        public ObservableCollection<Partida> DbPartidas
        {
            get { return _dbPartidas; }
            set
            {
                if (value != _dbPartidas)
                {
                    _dbPartidas = value;
                    OnPropertyChanged("DbPartidas");
                }
            }
        }
        public ObservableCollection<string> DbJogadores
        {
            get { return _dbJogadores; }
            set
            {
                if (value != _dbJogadores)
                {
                    _dbJogadores = value;
                    OnPropertyChanged("DbJogadores");
                }
            }
        }

        public FirebaseClient FBClient
        {
            get { return _fbClient; }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set { OnPropertyChanged(ref _status, value); }
        }
       
        private bool _isRunning = false;
        public bool IsRunning
        {
            get { return _isRunning; }
            set { OnPropertyChanged(ref _isRunning, value); }
        }
        private bool _logar;
        public bool Logar
        {
            get { return _logar; }
            set { OnPropertyChanged(ref _logar, value); }
        }
        private SimpleClient _client;

        private Task<bool> _listenTask;
        private Task _updateTask;
        private Task _connectionTask;

        private DateTime _pingSent;
        private DateTime _pingLastSent;
        private bool _pinged = false;
       

        public ChatroomViewModel()
        {
            Messages = new ObservableCollection<ChatPacket>();
            Partidas = new ObservableCollection<PartidasPacket>();
            Users = new ObservableCollection<string>();
        }

        public async Task Connect(string username, string address, int port)
        {
            Status = "Conectando...";

            if (SetupClient(username, address, port))
            {
                var packet = await GetNewConnectionPacket(username);
                await InitializeConnection(packet);
            }
        }

        private async Task InitializeConnection(PersonalPacket connectionPacket)
        {
            _pinged = false;
           
            if (IsRunning)
            {
                _updateTask = Task.Run(() => Update());
                await _client.SendObject(connectionPacket);
                _connectionTask = Task.Run(() => MonitorConnection());
                Status = "Conectado";
                
            }
            else
            {
               
                Status = "Conexão falhou";
                await Disconnect();
                Logar = true;
            }
        }

        private async Task<PersonalPacket> GetNewConnectionPacket(string username)
        {
            _listenTask = Task.Run(() => _client.Connect());

            IsRunning = await _listenTask;

            var notifyServer = new UserConnectionPacket
            {
                Username = username,
                IsJoining = true,
                UserGuid = _client.ClientId.ToString()
            };

            var personalPacket = new PersonalPacket
            {
                GuidId = _client.ClientId.ToString(),
                Package = notifyServer
            };

            return personalPacket;
        }

        private bool SetupClient(string username, string address, int port)
        {
            _client = new SimpleClient(address, port);
            return true;
        }

        public async Task Disconnect()
        {
            if(IsRunning)
            {
                IsRunning = false;
                await _connectionTask;
                await _updateTask;

                _client.Disconnect();
            }

            Status = "Desconectado";

            App.Current.Dispatcher.Invoke(delegate
            {
                Logar = true;
                Messages.Add(new ChatPacket
                {
                    Username = string.Empty,
                    Message = "Você se desconectou do servidor.",
                    UserColor = "black"
                });
            });
        }

        public async Task Send(string username, string message, string colorCode)
        {
            ChatPacket cap = new ChatPacket
            {
                Username = username,
                Message = message,
                UserColor = colorCode
            };

            await _client.SendObject(cap);
        }
        public async         Task
GetJogadores(string Username)
        {

            try
            {


                //if (DbUsers.Count>0) {
                var temp = await FBClient
                    .Child("Partidas")
                    .OnceAsync<Partida>();

                await App.Current.Dispatcher.BeginInvoke((Action)delegate () { DbJogadores.Clear(); });



                foreach (var e in temp)
                {

                    // MessageBox.Show(e.Object.Jogadores[0][1] + " - "+ Username);

                    if (e.Object.Jogadores[0][1] == Username)
                    {
                        await App.Current.Dispatcher.BeginInvoke((Action)delegate ()
                        {


                            for (int i = 0; i < e.Object.Jogadores.Length; i++)
                            {
                                DbJogadores.Add(e.Object.Jogadores[i][0] + " - " + e.Object.Jogadores[i][1]);

                            }





                        });
                    }
                }

                //await App.Current.Dispatcher.BeginInvoke((Action)delegate () { StrCollection.Clear(); });

                //foreach (var e in temp)
                //{
                //    await App.Current.Dispatcher.BeginInvoke((Action)delegate ()
                //     {
                //         StrCollection.Add(e.Object.Name);
                //     });
                //}
                // }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async         Task
GetPartidas()
        {
            var temp = await FBClient
                .Child("Partidas")
                .OrderByKey()
                .OnceAsync<Partida>();

            await App.Current.Dispatcher.BeginInvoke((Action)delegate () { DbPartidas.Clear(); });

            foreach (var e in temp)
            {
                await App.Current.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    DbPartidas.Add(new Partida { Key = e.Key, Nome = e.Object.Nome, Jogo = e.Object.Jogo, Titulo = e.Object.Titulo, Engine = e.Object.Engine, TipoServidor = e.Object.TipoServidor, Ip = e.Object.Ip });
                });
            }

            //await App.Current.Dispatcher.BeginInvoke((Action)delegate () { StrCollection.Clear(); });

            //foreach (var e in temp)
            //{
            //    await App.Current.Dispatcher.BeginInvoke((Action)delegate ()
            //     {
            //         StrCollection.Add(e.Object.Name);
            //     });
            //}

        }

        //public async Task IniciarPartida(string username, string jogo, string emulador,string servidor,string ip,string engine)
        public async Task IniciarPartida(string username, string jogo, string emulador, string ip,string tipoServidor,string engine)
        {
            string[][] Jogadores = new string[][] {
                    new string[] {"Host", username } };

            //teste.SetValue("abner",0);

            Partida cap2 = new Partida
            {
                
                Nome = username,
                Jogo = jogo,
                Emulador = emulador,
                Ip = ip,
                Jogadores = Jogadores,
                TipoServidor = tipoServidor,
                Engine = engine,
                Titulo = "Host:" + username + " Jogo:" + emulador +" Jogadores:"+ Jogadores.Length + "/5  Servidor:"+tipoServidor+"  Engine:"+engine
               

            };
                await _client.SendObject(cap2);
            
           
           // ChatPacket cap = new ChatPacket
            //{
            //   Username = username,
            //    Message = jogo,
             //   UserColor = emulador
           // };

          
        }
        public async Task RemoverPartida(string username, string jogo, string emulador, string ip, string tipoServidor, string engine)
        {
            string[][] Jogadores = new string[][] {
                    new string[] {"Host", username } };

            //teste.SetValue("abner",0);

            RemoverPartida cap2 = new RemoverPartida
            {

                Nome = username,
                Jogo = jogo,
                Emulador = emulador,
                Ip = ip,
                Jogadores = Jogadores,
                TipoServidor = tipoServidor,
                Engine = engine,
                Titulo = "Host:" + username + " Jogo:" + emulador + " Jogadores:" + Jogadores.Length + "/5  Servidor:" + tipoServidor + "  Engine:" + engine


            };
            await _client.SendObject(cap2);


            // ChatPacket cap = new ChatPacket
            //{
            //   Username = username,
            //    Message = jogo,
            //   UserColor = emulador
            // };


        }
        //public void ApagarPartida()
        //{
           // Partidas.RemoveAt(0);
         //   Partidas.Clear();

       // }
        private async Task Update()
        {
            while (IsRunning)
            {
                Thread.Sleep(1);
                var recieved = await MonitorData();

                if (recieved)
                    Console.WriteLine(recieved);
            }
        }

        private async Task MonitorConnection()
        {
            _pingSent = DateTime.Now;
            _pingLastSent = DateTime.Now;

            while (IsRunning)
            {
                Thread.Sleep(1);
                var timePassed = (_pingSent.TimeOfDay - _pingLastSent.TimeOfDay);
                if (timePassed > TimeSpan.FromSeconds(5))
                {
                    if (!_pinged)
                    {
                        var result = await _client.PingConnection();
                        _pinged = true;

                        Thread.Sleep(5000);

                        if (_pinged)
                            await Task.Run(() => Disconnect());
                    }
                }
                else
                {
                    _pingSent = DateTime.Now;
                }
            }
        }

        private async Task<bool> MonitorData()
        {
            var newObject = await _client.RecieveObject();

            App.Current.Dispatcher.Invoke(delegate
            {
                return ManagePacket(newObject);
            });

            return false;
        }

        private  bool ManagePacket(object packet)
        {
            if (packet != null)
            {
                if (packet is ChatPacket chatP)
                {
                    Messages.Add(chatP);
                   
                }
                if (packet is PartidasPacket playP)
                {
                    if(playP.Nome!="Servidor")
                    Partidas.Add(playP);
                    


                }

                if (packet is Partida playP2)
                {
                    //MessageBox.Show(playP2.Jogo);
                    DbPartidas.Add(playP2);
                    foreach (var jogadores in playP2.Jogadores)
                    {
                        DbJogadores.Add(jogadores[0]+" - "+ jogadores[1]) ;
                    }

                }
                if (packet is RemoverPartida playRemover)
                {
                   


                   
                    //MessageBox.Show(DbPartidas.IndexOf(partida).ToString());
                    DbPartidas.Clear();
                    DbJogadores.Clear();
                    GetPartidas();
                    GetJogadores(playRemover.Nome);
                }
                    if (packet is UserConnectionPacket connectionP)
                {
                    Users.Clear();
                    foreach (var user in connectionP.Users)
                    {
                        Users.Add(user);
                    }
                }

                if (packet is PingPacket pingP)
                {
                    _pingLastSent = DateTime.Now;
                    _pingSent = _pingLastSent;
                    _pinged = false;
                }

                return true;
            }

            return false;
        }

        public void Clear()
        {
            Messages.Clear();
            Partidas.Clear();
            Users.Clear();
            
        }
    }
}
