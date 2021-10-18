using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EmuladoresMultiplayer
{
    /// <summary>
    /// Lógica interna para ModalWindow.xaml
    /// </summary>
    public partial class ModalWindow : Window
    {
        string Path = Directory.GetCurrentDirectory();
        public Process MednafenInstance = null;
        public ModalWindow()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProcessStartInfo MednafenInfo = new ProcessStartInfo();
                MednafenInfo.FileName = Path + @"\mednafen\mednafen.exe";
                string GameKey = " -netplay.gamekey \"\"";
                MednafenInfo.Arguments = " -connect -netplay.host " + "\"emuladores-br.ddns.net\"" + GameKey + " -netplay.nick \"Abner" + "\" ";
                MednafenInfo.Arguments += "\"" + Path + @"\jogos\snes\Super Bomberman 4 (Japan).sfc" + "\"";
                Console.WriteLine("Comando: " + MednafenInfo.Arguments);
                MednafenInstance = Process.Start(MednafenInfo);
                MednafenInstance.EnableRaisingEvents = true;
                this.Close();
            }
            catch (Exception ex)
            {

                MessageBox.Show("Emulador não encontrado! Erro:" + ex.Message);
            }

        }
    }
}
