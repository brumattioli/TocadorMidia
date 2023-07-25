using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.WebRequestMethods;

namespace TocadorMidia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class File
    {
        public string name { get; set; }
        public string path { get; set; }
    }
    public partial class MainWindow : Window
    {
        private bool userIsDraggingSlider = false;
        string[] files;
        List<File> arquivos = new List<File>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        public void Abrir_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Video and Music Files (*.asf, *.wma, *.wmv, *.wm, *.asx, *.wax, *.wvx, *.wmx, *.wpl, *.dvr-ms, *.wmd, *.avi, *.mpg, *.mpeg, *.m1v, *.mp2, *.mp3, *.mpa, *.mpe, *.m3u, *.mid, *.midi, *.rmi. *.aif, *.aifc, *.aiff, *.au, *.snd, *.wav, *.cda, *.ivf, *.wmz, *.wms, *.mov, *.m4a, *.mp4, *.m4v, *.mp4v, *.3g2, *.3gp2, *.3gp, *.3gpp, *.aac, *.adt, *.adts, *.m2ts)|*.asf;*.wma;*.wmv;*.wm;*.asx;*.wax;*.wvx;*.wmx;*.wpl;*.dvr-ms;*.wmd;*.avi;*.mpg;*.mpeg;*.m1v;*.mp2;*.mp3;*.mpa;*.mpe;*.m3u;*.mid;*.midi;*.rmi;*.aif;*.aifc;*.aiff;*.au;*.snd;*.wav;*.cda;*.ivf;*.wmz;*.wms;*.mov;*.m4a;*.mp4;*.m4v;*.mp4v;*.3g2;*.3gp2;*.3gp;*.3gpp;*.aac;*.adt;*.adts;*.m2ts";
            openFile.Multiselect = true;
            openFile.ShowDialog();

            Tocador.LoadedBehavior = MediaState.Manual;
            Tocador.Source = new Uri(openFile.FileName);
            Tocador.MediaOpened += Tocador_MediaOpened;

            Nome_arquivo.Content = openFile.SafeFileName;
            Título.Content = "Título: " + openFile.SafeFileName;

            files = openFile.FileNames;

            foreach (string filename in openFile.FileNames)
            {
                File f = new File();
                f.name = System.IO.Path.GetFileName(filename);
                f.path = filename;
                arquivos.Add(f);
                listBox.Items.Add(f.name);
            }

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            Tocador.Play();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (Tocador.NaturalDuration.HasTimeSpan == true) lblStatus.Content = String.Format("{0} / {1}", Tocador.Position.ToString(@"mm\:ss"), Tocador.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
            timelineSlider.Minimum = 0;
            if (Tocador.NaturalDuration.HasTimeSpan == true) timelineSlider.Maximum = Tocador.NaturalDuration.TimeSpan.TotalSeconds;
            timelineSlider.Value = Tocador.Position.TotalSeconds;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            Tocador.Play();
        }

        private void listBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            foreach (File f in arquivos) 
            {
                if (listBox.SelectedValue.ToString() == f.name)
                {
                    Tocador.Source = new Uri(f.path);
                    Tocador.Play();
                    Nome_arquivo.Content = (f.name);
                    Título.Content = "Título: " + (listBox.SelectedValue);
                    break;
                }
            } 
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Tocador.Stop();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            Tocador.Pause();
        }

        private void Pular_Click(object sender, RoutedEventArgs e)
        {
            Tocador.Position += TimeSpan.FromSeconds(15);
        }

        private void Voltar_Click(object sender, RoutedEventArgs e)
        {
            Tocador.Position -= TimeSpan.FromSeconds(15);
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Tocador.Volume = (double)volumeSlider.Value;
        }

        private void ChangeMediaSpeedRatio(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Tocador.SpeedRatio = (double)speedRatioSlider.Value;
        }

        public void Tocador_MediaOpened(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Método que faz o controle da reprodução Automática
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Tocador_MediaEnded(object sender, EventArgs e)
        {
             for (int i = 0; i < files.Length - 1; i++)
             {
                 Uri current = new Uri(files[i]);
                 if (Tocador.Source == current)
                 {
                     Tocador.Source = new Uri(files[i + 1]);
                     break;
                 }
             }
        }
        private void timelineSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void timelineSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Tocador.Position = TimeSpan.FromSeconds(timelineSlider.Value);
        }

        private void timelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblProgressStatus.Text = TimeSpan.FromSeconds(timelineSlider.Value).ToString(@"hh\:mm\:ss");
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Criar_Playlist_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Compartilhar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            MoveItem(-1);
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            MoveItem(1);
        }

        public void MoveItem(int direction)
        {
            //listBox.SelectionMode = SelectionMode.Multiple;
            // Checking selected item
            if (listBox.SelectedItem == null || listBox.SelectedIndex < 0)
                return; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = listBox.SelectedIndex + direction;

            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= listBox.Items.Count)
                return; // Index out of range - nothing to do

            object selected = listBox.SelectedItem;

            // Removing removable element
            listBox.Items.Remove(selected);
            // Insert it in new position
            listBox.Items.Insert(newIndex, selected);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Tocador.LoadedBehavior = MediaState.Manual;
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Video and Music Files (*.asf, *.wma, *.wmv, *.wm, *.asx, *.wax, *.wvx, *.wmx, *.wpl, *.dvr-ms, *.wmd, *.avi, *.mpg, *.mpeg, *.m1v, *.mp2, *.mp3, *.mpa, *.mpe, *.m3u, *.mid, *.midi, *.rmi. *.aif, *.aifc, *.aiff, *.au, *.snd, *.wav, *.cda, *.ivf, *.wmz, *.wms, *.mov, *.m4a, *.mp4, *.m4v, *.mp4v, *.3g2, *.3gp2, *.3gp, *.3gpp, *.aac, *.adt, *.adts, *.m2ts)|*.asf;*.wma;*.wmv;*.wm;*.asx;*.wax;*.wvx;*.wmx;*.wpl;*.dvr-ms;*.wmd;*.avi;*.mpg;*.mpeg;*.m1v;*.mp2;*.mp3;*.mpa;*.mpe;*.m3u;*.mid;*.midi;*.rmi;*.aif;*.aifc;*.aiff;*.au;*.snd;*.wav;*.cda;*.ivf;*.wmz;*.wms;*.mov;*.m4a;*.mp4;*.m4v;*.mp4v;*.3g2;*.3gp2;*.3gp;*.3gpp;*.aac;*.adt;*.adts;*.m2ts";
            openFile.Multiselect = true;
            openFile.ShowDialog();


            foreach (string filename in openFile.FileNames)
            {
                File f = new File();
                f.name = System.IO.Path.GetFileName(filename);
                f.path = filename;
                arquivos.Add(f);
                listBox.Items.Add(f.name);
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            listBox.Items.RemoveAt(listBox.Items.IndexOf(listBox.SelectedItem));
        }
    }
}