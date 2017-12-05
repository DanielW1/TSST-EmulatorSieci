using NetworkingTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.Collections.Specialized;
using System.IO;

namespace NewNMS
{
    public partial class Application : Form
    {
        //Zmienna, która posłuży do wysyłania wiadomości od użytkownika do chmury
        public static SocketSending sS = new SocketSending();

        // public Application _Application;

        public static CancellationTokenSource _cts = new CancellationTokenSource();

        public static Listening ls = new Listening();

        private OperationConfiguration oc = new OperationConfiguration();

        //odwoływanie się do funkcji czytających z pliku konfiguracyjnego
        public static Reading rd = new Reading();

        // socket służacy do dodawania go do listy socketów w celu dalszej komunikacji
        public Socket send;

        // socket służący do nasłuchiwania
        public Socket listener;

        //socket służący do przekazywania go do funkcji wysyłającej w zależności od wyboru do którego routera ma pójść wiadomość
        public Socket sending;

        public Application _Application;

        //lista socketów, które będą łączone w momencie przyjścia wiadomości Node is up
        public List<Socket> sends_socket;

        //lista socketów z zestawionym polaczeniem 
        public List<Socket> listening_socket = new List<Socket>();

        private object _syncRoot = new object();

        // lista zawierająca adresy IP network nodów, pochodzące z wiadomości keep alive
        public List<string> routers_IP;

        // tablica bajtów żądania przesyłanego od NMSa 
        public byte[] message_bytes, bytes_to_send;

        private static bool boolListening = true;

        //lista portów  do agentów 
        private List<string> interfaces;

        private List<string> paths;

        //wiersz tablicy komutacji do wysłania do agenta
        private List<string> CommutationTableRow;

        public List<string> list;

        private int constPort;

        private int numbersOfParameter;
        private Timestamp tp = new Timestamp();


        public Application()
        {
            //Ustawienie CultureInfo na en-US spowoduje wyświetlanie się wyłapanych Exceptions w języku angielskim
            // Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            _Application = this;
            InitializeComponent();
            routers_IP = new List<string>();
            sends_socket = new List<Socket>();
            //dodawanie opcji do comboboxów
            ShowComboBox();
            HidetextBoxes();
            readConfiguration();
        }




        /// <summary>
        ///  metoda zwracająca socket z listy socketów socket w zalezności od wybrania adresu w comboBocRouters
        /// </summary>
        /// <returns> Socket</returns>
        private Socket returnSocket()
        {
            int i;
            try
            {
                //pętla po wszystkich wartościach w comboBoxRouters
                for (i = 0; i < comboBoxRouters.Items.Count; i++)
                {
                    if (comboBoxRouters.SelectedIndex == i)
                    {
                        sending = sends_socket[i];
                    }
                }
            }
            catch (Exception e)
            {
                listBoxReceived.Items.Add("[ "+ generateTimestamp() + "]" + "Fail with access to the socket");
            }

            return sending;
        }

       
        /// <summary>
        /// Baardzo długa funkcja tworząca wiadomość wysyłaną do agenta w zalezności od miliona textboxów przez co ejst taka długa
        /// </summary>
        /// <returns> byte[] </returns>
        private byte[] returnBytes()
        {
            byte[] table_in_bytes = new byte[128];
            try
            {

                if (comboBoxTables.GetItemText(comboBoxTables.SelectedItem) == "Commutation Table")
                {

                    if (comboBoxActions.GetItemText(comboBoxActions.SelectedItem) == "DELETE")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty; ;
                        string command = string.Empty;
                        string freq_in = string.Empty;
                        string port_in = string.Empty;
                        command = this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem);
                        freq_in = (textBoxFrequencyIN.Text).ToString();
                        port_in = (textBox_Port_IN.Text).ToString();
                        builder = command + "#" + "3" + "#" + freq_in + "#" + port_in;
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);
                        if (freq_in == "" || port_in == "")
                        {
                            table_in_bytes = null;
                        }
                        else
                        {

                            table_in_bytes = commutation_table.toBytes();
                            UpdateListBoxReceived("[" + generateTimestamp() + "]  CommutationTableRow to Delete:");
                            UpdateListBoxReceived("Frequency_IN: " + freq_in);
                            UpdateListBoxReceived("Port_IN: " + port_in);
                        }
                    }
                    else if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "ADD")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty;
                        string command = string.Empty;
                        string freq_in = string.Empty;
                        string port_in = string.Empty;
                        string freq_out = string.Empty;
                        string port_out = string.Empty;
                        command = this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem);
                        freq_in = (textBoxFrequencyIN.Text).ToString();
                        port_in = (textBox_Port_IN.Text).ToString();
                        freq_out = (textBoxFrequencyOUT.Text).ToString();
                        port_out = (textBoxPort_OUT.Text).ToString();
                        builder = command + "#" + "3" + "#" + freq_in + "#" + port_in + "#" + freq_out + "#" + port_out;
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);

                        if (freq_in == "" || port_in == "" || freq_out == "" || port_out == "")
                        {
                            table_in_bytes = null;
                        }
                        else
                        {

                            table_in_bytes = commutation_table.toBytes();
                            UpdateListBoxReceived("[" + generateTimestamp() + "]  CommutationTableRow to Add: ");
                            UpdateListBoxReceived("Frequency_IN: " + freq_in);
                            UpdateListBoxReceived("Port_IN: " + port_in);
                            UpdateListBoxReceived("Frequency_OUT: " + freq_out);
                            UpdateListBoxReceived("Port_OUT " + port_out);
                        }
                    }
                    else if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "TOPOLOGY")
                    {
                        string builder = string.Empty;
                        string tee = string.Empty;
                        tee = "TOPOLOGY";
                        builder = tee + "#" + "3";
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);
                        table_in_bytes = commutation_table.toBytes();
                        UpdateListBoxReceived("[" + generateTimestamp() + "]  CommutationTableRow");
                        UpdateListBoxReceived("Sending request of topology to the node agent");

                    }
                }
                if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "Border Node Commutation Table")
                {

                    if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "ADD")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty;
                        string port_in = string.Empty;
                        string port_out = string.Empty;
                        string Hops = string.Empty;
                        string command = string.Empty;
                        string band_out = string.Empty;
                        string destination_IP = string.Empty;
                        string Modulation = string.Empty;
                        string BitRate = string.Empty;
                        string IP_IN = string.Empty;
                        string Frequency_out = string.Empty;
                        command = this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem);
                        IP_IN = (textBox_IP_IN.Text).ToString();
                        port_in = (textBox_Port_IN.Text).ToString();
                        band_out = (textBoxBand_OUT.Text).ToString();
                        Frequency_out = (textBoxFrequencyOUT.Text).ToString();
                        Modulation = (textBoxModulation.Text).ToString();
                        BitRate = (textBoxBitrate.Text).ToString();
                        destination_IP = (textBoxDestination_IP.Text).ToString();
                        port_out = (textBoxPort_OUT.Text).ToString();
                        Hops = (textBoxHops.Text).ToString();

                        builder = command + "#" + "1" + "#" + IP_IN + "#" + port_in + "#" + band_out + "#" + Frequency_out + "#" +
                            Modulation + "#" + BitRate + "#" + destination_IP + "#" + port_out + "#" + Hops;
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);


                        if (IP_IN == "" || port_in == "" || band_out == "" || port_out == "" || Frequency_out == "" || Modulation == "" || BitRate == "" || destination_IP == "" || Hops == "")
                        {
                            table_in_bytes = null;
                        }
                        else
                        {
                            table_in_bytes = commutation_table.toBytes();
                            UpdateListBoxReceived("[" + generateTimestamp() + "]  BorderNodeCommutationTableRow to Add:");
                            UpdateListBoxReceived("IP_IN:" + IP_IN);
                            UpdateListBoxReceived("Port_IN: " + port_in);
                            UpdateListBoxReceived("Band: " + band_out);
                            UpdateListBoxReceived("Frequency: " + Frequency_out);
                            UpdateListBoxReceived("Modulation: " + Modulation);
                            UpdateListBoxReceived("Bitrate: " + BitRate);
                            UpdateListBoxReceived("destiantion_IP: " + destination_IP);
                            UpdateListBoxReceived("Port_OUT: " + port_out);
                            UpdateListBoxReceived("Hops: " + Hops);
                        }

                    }
                    if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "DELETE")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty;
                        string command = string.Empty;
                        string IP_IN = string.Empty;
                        string port_in = string.Empty;
                        string destination_IP = string.Empty;
                        command = this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem);
                        IP_IN = (textBox_IP_IN.Text).ToString();
                        port_in = (textBox_Port_IN.Text).ToString();
                        destination_IP = (textBoxDestination_IP.Text).ToString();

                        builder = command + "#" + "1" + "#" + IP_IN + "#" + port_in + "#" + destination_IP;
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);
                        if (IP_IN == "" || port_in == "" || destination_IP == "")
                        {
                            table_in_bytes = null;
                        }
                        else
                        {

                            table_in_bytes = commutation_table.toBytes();
                            UpdateListBoxReceived("[" + generateTimestamp() + "]  BorderNodeCommutationTableRow to Delete: ");
                            UpdateListBoxReceived("IP_IN:" + IP_IN);
                            UpdateListBoxReceived("Port_IN: " + port_in);
                            UpdateListBoxReceived("destiantion_IP: " + destination_IP);
                        }
                    }
                    else if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "TOPOLOGY")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty;
                        string tee = string.Empty;
                        tee = "TOPOLOGY";
                        builder = tee + "#" + "1";
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);
                        table_in_bytes = commutation_table.toBytes();
                        UpdateListBoxReceived("[" + generateTimestamp() + "]  BorderNodeCommutationTable: ");
                        UpdateListBoxReceived("Sending request of topology to the node agent");
                    }
                }
                if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "EON Table")
                {

                    if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "ADD")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty;
                        string command = string.Empty;
                        string Band_out = string.Empty;
                        string Band_in = string.Empty;
                        string frequency_in = string.Empty;
                        string Frequency_out = string.Empty;
                        command = this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem);
                        Band_out = (textBoxBand_OUT.Text).ToString();
                        Frequency_out = (textBoxFrequencyOUT.Text).ToString();
                        Band_in = (textBoxBand_IN.Text).ToString();
                        frequency_in = (textBoxFrequencyIN.Text).ToString();

                        builder = command + "#" + "2" + "#" + Band_in + "#" + frequency_in + "#" + Band_out + "#" + Frequency_out;
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);
                        if (Band_out == "" || Frequency_out == "" || Band_in == "" || frequency_in == "")
                        {
                            table_in_bytes = null;
                        }
                        else
                        {

                            table_in_bytes = commutation_table.toBytes();
                            UpdateListBoxReceived("[" + generateTimestamp() + "]  EONTableRow to Add:");
                            UpdateListBoxReceived("Band_IN: " + Band_in);
                            UpdateListBoxReceived("Frequency_IN: " + frequency_in);
                            UpdateListBoxReceived("Band_OUT: " + Band_OUT);
                            UpdateListBoxReceived("Frequency_OUT: " + Frequency_out);
                        }

                    }
                    else if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "DELETE")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty;
                        string command = string.Empty;
                        string Band_out = string.Empty;
                        string Band_in = string.Empty;
                        string frequency_in = string.Empty;
                        string Frequency_out = string.Empty;
                        command = this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem);
                        Band_out = (textBoxBand_OUT.Text).ToString();
                        Frequency_out = (textBoxFrequencyOUT.Text).ToString();
                        Band_in = (textBoxBand_IN.Text).ToString();
                        frequency_in = (textBoxFrequencyIN.Text).ToString();

                        builder = command + "#" + "2" +  "#" + Band_in + "#" + frequency_in + "#" +  Band_out + "#" +  Frequency_out;
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);
                        if (Band_out == "" || Frequency_out == "" || Band_in == "" || frequency_in == "")
                        {
                            table_in_bytes = null;
                        }
                        else
                        {

                            table_in_bytes = commutation_table.toBytes();
                            UpdateListBoxReceived("[" + generateTimestamp() + "]  EONTableRow to Delete: ");
                            UpdateListBoxReceived("Band_IN: " + Band_in);
                            UpdateListBoxReceived("Frequency_IN: " + frequency_in);
                            UpdateListBoxReceived("Band_OUT: " + Band_OUT);
                            UpdateListBoxReceived("Frequency_OUT: " + Frequency_out);
                        }
                    }
                    else if (this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "TOPOLOGY")
                    {
                        table_in_bytes = null;
                        string builder = string.Empty;
                        string tee = string.Empty;
                        tee = "TOPOLOGY";
                        builder = tee + "#" + "2";
                        short length = (Int16)builder.Length;
                        NMSPackage commutation_table = new NMSPackage(interfaces.ElementAt(1 - 1), builder, length);
                        table_in_bytes = commutation_table.toBytes();
                        UpdateListBoxReceived("[" + generateTimestamp() + "] : EONTable ");
                        UpdateListBoxReceived("Sending request of topology to the node agent");
                    }
                }
            }
            catch (Exception e)
            {
                listBoxReceived.Items.Add("[" + generateTimestamp() + "]" + "Error occured with getting data for the agent");
                table_in_bytes = null;
            }
            return table_in_bytes;
        }

        /// <summary>
        /// funkcja odpalana po naciśnięciu przycisku Run
        /// </summary>
        private void ListenForConnections()
        {
          
                // petla po wszystkich wartościach portów agentów appconfig
                foreach (var address in interfaces)
                {
                    Socket socketClient = null;
                    Socket listener = null;
                    try
                    {
                        byte[] bytes = new Byte[64];

                        IPAddress ipAddress = IPAddress.Parse(address);
                        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, constPort);
                        // Create a TCP/IP socket.  
                        listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        listener.Bind(localEndPoint);
                        listener.Listen(100);
                        try
                        {
                            Task.Run(() =>
                            {
                                while (true)
                                {
                                    socketClient = listener.Accept();
                                    listening_socket.Add(socketClient);
                                    ReceivedMessage(socketClient);
                                }
                            });
                        }
                        catch (OperationCanceledException)
                        {
                            listBoxReceived.Items.Add("[" + generateTimestamp() + "]" + "Exception during listetning");
                        }
                    }
                    catch (SocketException se)
                    {
                        UpdateListBoxReceived("[" + generateTimestamp() + "]"+  "Exception during connecting with network node");
                    }
                }
           
        }

        // funkcja dodajaca wiadomości do ListBoxa wraz z invokiem 
        public void UpdateListBoxReceived(string message)
        {
            _Application.listBoxReceived.Invoke(new Action(delegate ()
            {
                _Application.listBoxReceived.Items.Add(message);
                listBoxReceived.SelectedIndex = listBoxReceived.Items.Count - 1;
            }));
        }

        /// <summary>
        /// funkcja pobierająca adres IP z socketa 
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static string getIPAddressRemote(Socket socket)
        {
            //string address;
            IPEndPoint ipend = socket.RemoteEndPoint as IPEndPoint;
            return ipend.Address.ToString();
        }

        /// <summary>
        /// funkcja obsługująca przychodzącą wiadomość
        /// </summary>
        /// <param name="listener"></param>
        private void ReceivedMessage(Socket listener)
        {
            string usableMessage = "";

            while (true)
            {

                // wyczyszczenie paczki przed jej wysłaniem oraz przed ponownym wysyłaniem 
                usableMessage = string.Empty;

                //przypisywanie do paczki wiadomości przesłanej przez klienta, w tym przypadku przez agenta
                byte[] nmspackage = new byte[128];

                // tylko jeden wątek moze wykonywac ten kod w danym czasie
                //  lock (_syncRoot)
                //   {
                nmspackage = ls.ProcessRecivedByteMessage(listener);
                //   }
                // wykonuje się tylko wtedy jeśli agent nie  jest podłączony
                if (!listener.Connected)
                {
                    // pobranie indeksu socketu z listy soecketów
                    int index = listening_socket.IndexOf(listener);
                    UpdateListBoxReceived("[" + generateTimestamp() + "]" + "Network Node" + getIPAddressRemote(sends_socket.ElementAt(index)) + " is disconnected");
                    // rozłączanie obu socketów
                    listener.Disconnect(true);
                    //send.Disconnect(true);

                    //usuwanie socketów, adresów z list by przy ponownym połączeniu dodawać je na ponownie
                    listening_socket.RemoveAt(index);
                    sends_socket.RemoveAt(index);
                    routers_IP.RemoveAt(index);

                    // wyświetlanie pobranego adresu IP z list podłączonych agentów
                    _Application.comboBoxRouters.Invoke(new Action(delegate ()
                    {

                        comboBoxRouters.Items.RemoveAt(index);

                    }));
                    break;
                }
                // wykonuje się tylko wtedy jeśli agent jest podłączony
                else
                {
                    string sourceip;
                    //tylko jesli paczka nie jest nullem
                    if (nmspackage != null)
                    {
                        usableMessage = NMSPackage.extractUsableMessage(nmspackage, NMSPackage.extractUsableInfoLength(nmspackage));
                        //tylko w przypadku pierwszej wiadomości od danego agenta
                        if (usableMessage == "Network node is up")
                        {
                            sourceip = NMSPackage.exctractSourceIP(nmspackage).ToString();
                            //jesli lista z adresami IP routerów nie zawiera danego IP to je dodaje a następnie wyśwuietlam komunikat 
                            if (!routers_IP.Contains(sourceip))
                            {
                                //dodanie do listy adresów IP agenta który wysłał wiadomosc
                                routers_IP.Add(sourceip);
                                UpdateListBoxReceived("[" + generateTimestamp() + "]" + " Network Node: " + sourceip + " is up");
                                //UpdateListBoxReceived(listening_socket.Count.ToString());

                            }
                            //tworze połączenie z socketem routera, który wysłał do mnie wiadomość
                            send = sS.ConnectToEndPoint(NMSPackage.exctractSourceIP(nmspackage).ToString());
                            // a następnie dodaje ten socket do listy socketów, by potem móc z nich korzystać
                            sends_socket.Add(send);

                            short numberOfRouter = NMSPackage.extractNumberOfRouterNumber(nmspackage);

                            // po kazdej wiadomosci Node is Up wysyłana jest mu jego tablica, zczytywany jest sciezka z pliku konfiguracyjenego i wysyłanie linijka po linijce
                            List<string> configurationRouter = ReadingFromFile(paths.ElementAt(numberOfRouter - 1));
                            foreach (var line in configurationRouter)
                            {
                                short length = (Int16)line.Length;
                                NMSPackage tablePackage = new NMSPackage(interfaces.ElementAt(numberOfRouter - 1), line, length);
                                byte[] tablePackageInBytes = tablePackage.toBytes();
                                send.Send(tablePackageInBytes);
                                Task.Delay(10);
                            }



                            /* z kazdą wiadomością "Network node is up" dodaje IP routera do checkboca w celu mozliwości wybrania
                             docelwoego punktu komunikacji */
                            _Application.comboBoxRouters.Invoke(new Action(delegate ()
                            {

                                _Application.comboBoxRouters.Items.Add(sourceip);

                            }));

                        }
                        //Obsułga wiadomosci Keep Alive od agenta
                        else if (usableMessage == "Keep Alive")
                        {
                           
                             //UpdateListBoxReceived("["+generateTimestamp()+"] "+ usableMessage+ "from"+ NMSPackage.exctractSourceIP(nmspackage).ToString());
                            
                        }
                        else if (usableMessage == "ERROR")
                        {
                            UpdateListBoxReceived("[" + generateTimestamp() + "] Cannot send Table to the node agent" + NMSPackage.exctractSourceIP(nmspackage).ToString());
                        }
                        /// obsługa wiadomosci TOPOLOGY od agenta
                        else if (usableMessage.StartsWith("TOPOLOGY"))
                        {
                            int index = listening_socket.IndexOf(listener);
                            char[] delimiterChars = { '#' };
                            string[] words;
                            words = usableMessage.Split(delimiterChars);
                            int size = words.Length;
                            switch (Int32.Parse(words[1]))
                            {
                                case 1:
                                    

                                        UpdateListBoxReceived("[" + generateTimestamp() + "]  BorderNodeCommutationTableRow from: " + getIPAddressRemote(sends_socket.ElementAt(index)));
                                        UpdateListBoxReceived("IP_IN:" + words[2]);
                                        UpdateListBoxReceived("Port_IN: " + words[3]);
                                        UpdateListBoxReceived("Band: " + words[4]);
                                        UpdateListBoxReceived("Frequency: " + words[5]);
                                        UpdateListBoxReceived("Modulation: " + words[6]);
                                        UpdateListBoxReceived("Bitrate: " + words[7]);
                                        UpdateListBoxReceived("destiantion_IP: " + words[8]);
                                        UpdateListBoxReceived("Port_OUT: " + words[9]);
                                        UpdateListBoxReceived("Hops: " + words[10]);
                                   
                                    
                                    break;
                                case 2:

                                        UpdateListBoxReceived("[" + generateTimestamp() + "] : EONTableRow from: " + getIPAddressRemote(sends_socket.ElementAt(index)));
                                        UpdateListBoxReceived("Band_IN: " + words[2]);
                                        UpdateListBoxReceived("Frequency_IN: " + words[3]);
                                        UpdateListBoxReceived("Band_OUT: " + words[4]);
                                        UpdateListBoxReceived("Frequency_OUT: " + words[5]);
                                      
                                    break;
                                case 3:
                                    

                                        UpdateListBoxReceived("[" + generateTimestamp() + "] : CommutationTableRow from: " + getIPAddressRemote(sends_socket.ElementAt(index)));
                                        UpdateListBoxReceived("Frequency_IN: " + words[2]);
                                        UpdateListBoxReceived("Port_IN: " + words[3]);
                                        UpdateListBoxReceived("Frequency_OUT: " + words[4]);
                                        UpdateListBoxReceived("Port_OUT " + words[5]);

                                    
                                    break;
                                default:
                                    break;
                            }


                        }

                    }
                    //jesli paczka jest nullem
                    else
                    {
                        int index = listening_socket.IndexOf(listener);
                        // stwierdzam, że agent nie odpowiada, a potem go rozłączam
                        UpdateListBoxReceived(generateTimestamp() + " Network Node" + 
                            getIPAddressRemote(sends_socket.ElementAt(index)) + " is not responding");
                        UpdateListBoxReceived(generateTimestamp() + " Network Node" +
                            getIPAddressRemote(sends_socket.ElementAt(index)) + " is disconnected");

                        // usuniecie danego agenta z comboboxe'a, a takze socketów służacych do komunikacji z danym agentem
                        listening_socket.RemoveAt(index);
                        sends_socket.RemoveAt(index);
                        routers_IP.RemoveAt(index);

                        // usunięcie rozłączonego agenta z comboboce'a z dostepnymi agentami
                        _Application.comboBoxRouters.Invoke(new Action(delegate ()
                        {

                            comboBoxRouters.Items.RemoveAt(index);

                        }));
                        //odłączanie
                        listener.Disconnect(true);
                        //send.Disconnect(true);
                        //usuwanie socketów, adresów z list by przy ponownym połączeniu dodawać je na ponownie

                        break;

                    }

                }

            }

        }


        /// <summary>
        /// metoda generująca TimeStamp do logów
        /// </summary>
        /// <returns></returns>
        private string generateTimestamp()
        {
            return  Timestamp.generateTimestamp();
        }


        /// <summary>
        /// Funkcja rozpoczynająca nasłuchiwanie NMSa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonListen_Click_1(object sender, EventArgs e)
        {
            _Application.listBoxReceived.Items.Add("Running");
            buttonListen.Enabled = false;
            ListenForConnections();

        }


        /// <summary>
        /// obsługa przycisku "SEND" korzystająca z dwóch metod i pobierająca z nich Socket i tablicę bajtów do wysłania
        /// przed wysłaniem trzeba wybrać agenta oraz rodzaj operacji jaki chcemy wykonac
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (comboBoxRouters.SelectedItem != null)
            {

                Socket socket = null;
                byte[] bytes = null;
                socket = returnSocket();
                bytes = returnBytes();
                if (bytes != null)
                {
                    socket.Send(bytes);
                    HidetextBoxes();
                }
                else
                {
                    MessageBox.Show("[" + generateTimestamp() + "]" + "Fill in all textBoxes");
                }
            }
        }

        /// <summary>
        /// funkca zczytujaca parmaetry konfiguracyjne NMSa, tj interfejsy polaczneiowe z agentami,
        ///  port na którym NMS słucha i wysyla, liczbe agentow oraz sciezki do plikow z tablicami routerow
        /// </summary>
        private void readConfiguration()
        {
            List<Data> data = new List<Data>();
            interfaces = new List<string>();
            paths = new List<string>();
            NameValueCollection readSettings = ConfigurationManager.AppSettings;
            data = OperationConfiguration.ReadAllSettings(readSettings);
            foreach (var key in data)
            {
                if (key.Keysettings.StartsWith("interface"))
                {
                    interfaces.Add((key.SettingsValue));
                }
                else if (key.Keysettings.StartsWith("port"))
                {
                    constPort = Int32.Parse(key.SettingsValue);
                }
                else if (key.Keysettings.StartsWith("number"))
                {
                    numbersOfParameter = Int32.Parse(key.SettingsValue);
                }
                else if (key.Keysettings.StartsWith("path"))
                {
                    paths.Add((key.SettingsValue));
                }
            }

        }
        /// <summary>
        /// funkcja czytająca z pliku konfiguracyjnego zawierajace tablice routerów, zwracajaca tekst w postaci listy stringów, jeden string jedna linia tekstu - jeden wiersz tablicy
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<string> ReadingFromFile(string path)
        {
            List<string> listLine = new List<string>();
            string line;
            try
            {
                using (StreamReader file = new StreamReader(path))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        listLine.Add(line);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return listLine;

        }
       
        private void button1_Click(object sender, EventArgs e)
        {
            EnableTextBoxes();
        }

        private void buttonHiding_Click_1(object sender, EventArgs e)
        {
            HidetextBoxes();
        }


        /// <summary>
        /// funkcja pokazująca wybrane textboxy labele w zależności od tabeli i operacji na niej wykonywanej 
        /// </summary>
        private void EnableTextBoxes()
        {
            if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "Commutation Table" &&
                comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "ADD")
            {
                textBoxFrequencyIN.Visible = true;
                label7.Visible = true;

                textBox_Port_IN.Visible = true;
                label5.Visible = true;

                textBoxPort_OUT.Visible = true;
                label11.Visible = true;

                textBoxFrequencyOUT.Visible = true;
                label2.Visible = true;
            }
            else if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "Commutation Table" &&
                this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "DELETE")
            {

                textBoxFrequencyIN.Visible = true;
                label7.Visible = true;

                textBox_Port_IN.Visible = true;
                label5.Visible = true;
            }


            if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "EON Table" &&
                this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "ADD")
            {
                textBoxBand_OUT.Visible = true;
                Band_OUT.Visible = true;

                textBoxFrequencyOUT.Visible = true;
                label2.Visible= true;

                textBoxBand_IN.Visible = true;
                label6.Visible = true;

                textBoxFrequencyIN.Visible = true;
                label7.Visible = true;
            }

            else if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "EON Table" && 
                this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "DELETE")
            {
                textBoxBand_OUT.Visible = true;
                Band_OUT.Visible = true;

                textBoxFrequencyOUT.Visible = true;
                label2.Visible = true;

                textBoxBand_IN.Visible = true;
                label6.Visible = true;

                textBoxFrequencyIN.Visible = true;
                label7.Visible = true;
            }


            if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "Border Node Commutation Table" 
                && this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "ADD")
            {
                textBox_IP_IN.Visible = true;
                label4.Visible = true;

                textBox_Port_IN.Visible = true;
                label5.Visible = true;

                textBoxBand_OUT.Visible = true;
                Band_OUT.Visible = true;

                textBoxFrequencyOUT.Visible = true;
                label2.Visible = true;

                textBoxModulation.Visible = true;
                label8.Visible = true;

                textBoxBitrate.Visible = true;
                label9.Visible = true;

                textBoxDestination_IP.Visible = true;
                label10.Visible = true;

                textBoxPort_OUT.Visible = true;
                label11.Visible = true;

                textBoxHops.Visible = true;
                label12.Visible = true;
            }
            else if (this.comboBoxTables.GetItemText(this.comboBoxTables.SelectedItem) == "Border Node Commutation Table" 
                && this.comboBoxActions.GetItemText(this.comboBoxActions.SelectedItem) == "DELETE")
            {
                textBox_IP_IN.Visible = true;
                label4.Visible = true;

                textBoxDestination_IP.Visible = true;
                label10.Visible = true;

                textBox_Port_IN.Visible = true;
                label5.Visible = true;

            }

        }

        /// <summary>
        /// funkca chowająca wszytskie labele i textboxy, uzywana po wsyłąaniu wiadomości w celu odjrycia wybranych parametrów do wpisania
        /// </summary>
        private void HidetextBoxes()
        {
            textBox_IP_IN.Visible = false;
            Band_OUT.Visible = false;
            textBoxBand_IN.Visible = false;
            label2.Visible = false;
            textBoxModulation.Visible = false;
            label4.Visible = false;
            textBoxBitrate.Visible = false;
            label5.Visible = false;
            textBoxDestination_IP.Visible = false;
            label6.Visible = false;
            textBoxHops.Visible = false;
            label7.Visible = false;
            textBoxBand_OUT.Visible = false;
            label8.Visible = false;
            textBoxFrequencyOUT.Visible = false;
            label9.Visible = false;
            textBox_Port_IN.Visible = false;
            label10.Visible = false;
            textBoxFrequencyIN.Visible = false;
            label11.Visible = false;
            textBoxPort_OUT.Visible = false;
            label12.Visible = false;
        }

        /// <summary>
        /// funkcja dodająca statyczne elementy do comboboxów, nie zmieniające się w czasie
        /// </summary>
        private void ShowComboBox()
        {
            // elementy comboboxe'a wybierające operacje do wykonania
            comboBoxActions.Items.Add("DELETE");
            comboBoxActions.Items.Add("ADD");
            comboBoxActions.Items.Add("TOPOLOGY");

            comboBoxTables.Items.Add("Commutation Table");
            comboBoxTables.Items.Add("EON Table");
            comboBoxTables.Items.Add("Border Node Commutation Table");

        }

    }
}







