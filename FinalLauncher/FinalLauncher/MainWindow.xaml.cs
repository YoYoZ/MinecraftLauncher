using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace FinalLauncher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SHA512 sh = new SHA512Managed(); //Создаем общий SHA обработчик
        public const int LAUNCHER_VERSION = 1;  //Версия
        private string Minecraftsha = null; //SHA, подсчитывается потом, в методе CountSHA
        private static string SessionID; //ID, получаем от сервера
        private bool downoload_req = false; //Нужно ли обновится. Получаем от сервера
        private const string url = "http://test.ru/"; //Ссылка на сайт. Пока отсутсвует.
        private string path = (Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\.MinVersion"); //Путь к Нашему Minecraft'y.
        private string Linktodwnld; //Строка, указывающая на ссылку для закачки. (Сраные динамические линки).
        System.Text.ASCIIEncoding en = new System.Text.ASCIIEncoding();  //Энкодер
        DirectoryInfo dr = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\.MinVersion"); //Быдлокод, который создает два вида ДиректориИнфо, для обработки.

        public MainWindow()
        {
            InitializeComponent();
        }

        private bool checker()
        {
            DirectoryInfo drr = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\.MinVersion\bin");

            if (dr.Exists == true && drr.Exists == true) //Проверяем на существование папки.
            {
                FileInfo[] files = drr.GetFiles();
                for (int i = 0; i < files.Count(); i++) //Берем список файлов.
                {
                    if (files[i].Name == "minecraft.jar") //Считаем хеш
                    {
                        Minecraftsha = CountSHA(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\.MinVersion\bin\minecraft.jar", FileMode.Open, FileAccess.ReadWrite));
                    }

                }

            }
            else
            {
                return false;// Если что-то не то.
            }

            return true; //Если все удачно, возвращаем TRUE

        }

        private string[] Senddata()
        {


            try
            {
                Uri uri = new Uri(url + @"launcher/emulator.php/?" + getData()); //Вторая стадия. Отправляем данные на сервер.
                WebRequest http = HttpWebRequest.Create(uri); //Создаем запрос..
                HttpWebResponse response = (HttpWebResponse)http.GetResponse(); //И отправляем , после чего смиренно ждем ответа. Таймаут - 30 секунд.
                Stream stream = response.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                string[] arr =  sr.ReadToEnd().Split('&');
                return arr;
                //Вызываем парсер, передавая ответ
            }
            catch { return null; }
        }
        private string CountSHA(Byte[] pony)
        {

            string lol = BitConverter.ToString(sh.ComputeHash(pony)); //Первая из перегрузок. Есть массив байтов.
            lol = lol.Replace("-", ""); //Считает хеш. Возвращает строку.
            return lol;

        }
        private string CountSHA(FileStream pony)
        {

            string lol = BitConverter.ToString(sh.ComputeHash(pony)); //Вторая из перегрузок. Есть Поток. Вдруг надо будет прочитать хеш скачаного?
            lol = lol.Replace("-", "");//Считает хеш. Возвращает строку.
            pony.Close();
            return lol;

        }
        private void parser(string[] arr)
        {
             //Парсер. Ничего особенно сложного. С виду вроде неплохо, правда?
            if (exactpars(arr, "answer") == "yes") //Первая проверка. Ответил ли сайт да на нашу просьбу.
            {
                if (exactpars(arr, "code") == "0")
                {
                    exactpars(arr, "SessionID"); //Передача ID. Ее еще предстоит сделать. Это удачный вызод из проверки.
                }
                else if (exactpars(arr, "code") == "1")
                {
                     //Проверка провалилась. Узнаем линк на закачку и обновляем майн.
                    Linktodwnld = exactpars(arr, "link");
                    downoloader();
                }
                else if (exactpars(arr, "code") == "2")
                {
                    MessageBox.Show("Лаунчер устарел. Обновитесь."); /*Худший из выходов.*/ ///TODO: Вставить ссылку.
                }
                else if (exactpars(arr, "code") == "3")
                {
                                                    //SHA mineacraf'a не совпал. Обновляем.
                    Linktodwnld = exactpars(arr, "link");
                    downoloader();
                }
            }
            else { MessageBox.Show("Логин\\Пароль не опознаны."); }  //Тут все ясно.
        }

        private string exactpars(string[] parsed, string variable)
        {
            string[] temp = null;
            for (int i = 0; i < parsed.Count(); i++)
            {
                if (parsed[i].Contains(variable))
                {
                    
                    return parsed[i].Split('=')[1];
                }  
            }
            return temp[1];
        }
        public string getData()
        {

            Byte[] bytes = en.GetBytes(passwordBox1.Password); //Превращаем пасс в байты.
            string result = "playername=" + textBox1.Text + "&password=" + CountSHA(bytes) + "&game-build=";
            FileStream fs = new FileStream((path), FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            int var1 = Convert.ToInt32(sr.ReadLine());
            sr.Close();                                     //Вся эта телега просто формирует запрос.
            result += var1;
            result += "&launcher-version=" + LAUNCHER_VERSION;
            result += "&minecraft-SHA=" + Minecraftsha;
            return result;

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

                if (textBox1.Text != "" && passwordBox1.Password != "") //Ну клиент же не идиот, верно?
                {
                    if (checker() == true) //А вернули ли вы тру, сударь?
                    {
                       parser(Senddata()); //Отправка.
                    }
                    else
                    {
                        Senddata();
                        downoloader(); //А если что-то не так, то мы качаем. Полностью.
                    }
                }
                else
                {
                    MessageBox.Show("Неплохо было бы ввести имя или пароль.");
                }
            

        }

        private void downoloader()
        {
            try
            {
                dr.Create(); //Создаем папку.
                WebClient webClient = new WebClient();
                Debug.WriteLine("DOLCHE GABANE");
                webClient.DownloadFile(new Uri(exactpars(Senddata(),"link")), path); //Качаем файл
                Debug.WriteLine("OPACHKY");
            }
            catch (Exception exc) {Debug.WriteLine(exc.ToString()); }
            foreach (FileInfo fi in dr.GetFiles("*.df"))
            {
                Decompress(fi);
            }
        }
        public static void Decompress(FileInfo fi)
        {
            // Get the stream of the source file.
            using (FileStream inFile = fi.OpenRead())
            {
                // Get original file extension, for example
                // "doc" from report.doc.gz.
                string curFile = fi.FullName;
                string origName = curFile.Remove(curFile.Length -
                        fi.Extension.Length);

                //Create the decompressed file.
                using (FileStream outFile = File.Create(origName))
                {
                    using (DeflateStream Decompress = new DeflateStream(inFile,
                            CompressionMode.Decompress))
                    {
                        // Copy the decompression stream 
                        // into the output file.
                        Decompress.CopyTo(outFile);

                        Console.WriteLine("Decompressed: {0}", fi.Name);
                    }
                }
            }
        }

        private void rectangle1_GotMouseCapture(object sender, MouseEventArgs e)
        {
            rectangle1.Fill = Brushes.Brown;
        }

        private void button1_MouseLeave(object sender, MouseEventArgs e)
        {
            rectangle1.Fill = Brushes.Violet;
        }

        private void button1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            rectangle1.Fill = Brushes.Blue;
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            close_1.Opacity = 1;
        }

        private void close_0_MouseLeave(object sender, MouseEventArgs e)
        {
            close_1.Opacity = 0;
        }

        private void close_1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        private void hide_1_MouseEnter(object sender, MouseEventArgs e)
        {
            hide_1.Opacity = 1;
        }

        private void hide_1_MouseLeave(object sender, MouseEventArgs e)
        {
            hide_1.Opacity = 0;
        }

        private void hide_1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;

        }

        private void repair_1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            repair_1.Opacity = 1;
            downoload_req = true;

        }





    }
}
