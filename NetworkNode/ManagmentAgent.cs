using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using NetworkingTools;
using System.Diagnostics;
using System.Collections.Specialized;

namespace NetworkNode
{
    class ManagmentAgent
    {
        private NameValueCollection mySettings = System.Configuration.ConfigurationManager.AppSettings;

        private object _syncRoot = new object();

        private static List<Data> tmp = new List<Data>();

        public delegate Socket SocketDelegate(Socket socket);

        public static SocketDelegate sd;

        private SocketListener sl = new SocketListener();

        private SocketSending sS = new SocketSending();

        private static bool Listening = true;

        private static bool Last = true;

        private OperationConfiguration oc = new OperationConfiguration();


        /// <summary>
        /// Tablica z zajetymi pasmami
        /// </summary>
        public EONTable eonTable;

        /// <summary>
        /// Tablica komutacji - dla wszystkich rodzajow wezlow sieciowych
        /// </summary>
        public CommutationTable commutationTable;

        /// <summary>
        /// Tablica komutacji dla wezlow sieciowych na brzegu sieci. Najpierw router zaglada w pakiet
        /// i sprawdza, czy jest tam jakas czestotliwosc tunelu. Jak nie ma, to wlasnie w tej tablicy
        /// bedzie napisane, co dalej robic. Jak jest - to zwykla commutationTable.
        /// </summary>
        public BorderNodeCommutationTable borderNodeCommutationTable;

        /// <summary>
        /// Funckja uruchamiajaca agenta zarzadzania.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="commutationTable"></param>
        /// <param name="borderNodeCommutationTable"></param>
        /// <param name="eonTable"></param>
        public void Run(string number, ref CommutationTable commutationTable,
             ref BorderNodeCommutationTable borderNodeCommutationTable,
           ref EONTable eonTable)
        {

            this.eonTable = eonTable;
            this.commutationTable = commutationTable;
            this.borderNodeCommutationTable = borderNodeCommutationTable;
            //Zmienna do przechowywania klucza na adres wychodzacy powiazany z socketem sluchaczem
            string settingsString = "";


            //pobranie wlasnosci zapisanych w pliku konfiguracyjnym
            tmp = OperationConfiguration.ReadAllSettings(mySettings);

            CreateConnect(ConfigurationManager.AppSettings[number + "NMS"], number + "NMS");

            ConsoleKeyInfo cki;
            while (true)
            {
                cki = Console.ReadKey();
                if (cki.Key == ConsoleKey.Escape)
                {


                    break;
                }
            }

        }

        public void CreateConnect(string addressConnectIP, string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            Socket socket = null;
            Socket listenerAgent = null;
            Socket agentSending = null;


            try
            {
                byte[] bytes = new Byte[128];

                string numberOfRouter = key.Substring(0, 1);

                //Sklejenie czesci wspolnej klucza dla socketu OUT oraz indeksu 
                string settingsString = numberOfRouter + "Agent";

                IPAddress ipAddress =
                         ipAddress = IPAddress.Parse(OperationConfiguration.getSetting(settingsString, mySettings));
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP socket.  
                listenerAgent = new Socket(ipAddress.AddressFamily,
                   SocketType.Stream, ProtocolType.Tcp);

                if (!listenerAgent.IsBound)
                {
                    //zabindowanie na sokecie punktu koncowego
                    listenerAgent.Bind(localEndPoint);
                    listenerAgent.Listen(100);
                }
                int milliseconds = 100;
                //Nasluchujemy bez przerwy
                while (Last)
                {

                    if (Listening)
                    {
                        //Dodanie socketu do listy socketow OUT
                        agentSending = sS.ConnectToEndPoint(addressConnectIP);
                        if (agentSending == null)
                        {
                            Console.WriteLine("agentSending is null");
                            continue;
                        }
                        Thread.Sleep(milliseconds);
                        SendingNodeIsUpMessage(agentSending, OperationConfiguration.getSetting(settingsString, mySettings), Int16.Parse(numberOfRouter));

                        //oczekiwanie na polaczenie
                        socket = listenerAgent.Accept();

                        

                        SendingKeepAliveMessage(OperationConfiguration.getSetting(settingsString, mySettings), agentSending);

                        Listening = false;


                        Console.WriteLine("Connect on port  " + NetworkNode.takingAddresListenerSocket(socket));
                        byte[] msg;


                        if (socket.Connected)
                        {
                            //Oczekiwanie w petli na przyjscie danych
                            while (true)
                            {
                                string from = string.Empty;
                                //Odebranie tablicy bajtow na obslugiwanym w watku sluchaczu

                                msg = sl.ProcessRecivedBytes3(socket);                                

                                if (msg == null) { break; };
                                NMSPackage package = new NMSPackage();
                                string usableMessage = NMSPackage.extractUsableMessage(msg, NMSPackage.extractUsableInfoLength(msg));
                                //Console.WriteLine(usableMessage);
                                fillingTable(usableMessage, agentSending, settingsString);

                            }
                        }


                    }

                }
            }
            catch (SocketException se)
            {
                Console.WriteLine($"Socket Exception: {se}");
            }
            finally
            {
                // StopListening();
            }
            if (socket == null)
            {

            }

        }

        public void SendingNodeIsUpMessage(Socket socketout, string addressIP, short WhichRouter)
        {
            IPEndPoint ippoint = socketout.RemoteEndPoint as IPEndPoint;
            Console.WriteLine(ippoint.Address.ToString());

            try
            {
               
                    // wiadomośc przesyłana do komunikacji z NMSem
                    string message = "Network node is up";
                    short length = (Int16)message.Length;
                    NMSPackage nmspackage = new NMSPackage(addressIP, WhichRouter, message, length);



                    byte[] bytemessage = nmspackage.toBytes();

                    // Send the data through the socket.  
                    socketout.Send(bytemessage);
               
            }
            catch (Exception)
            {

                Console.WriteLine("Lock connection with NMS");
            }

        }

        public void SendingKeepAliveMessage(string address, Socket socketout)
        {
            Task.Run(async () =>
            {
                try
                {

                    while (true)
                    {
                        if (!socketout.Connected)
                        {
                            Console.WriteLine("Lock connection NMS");
                            break;

                        }
                        string keep_alive_message = "Keep Alive";
                        short length = (Int16)keep_alive_message.Length;
                        NMSPackage nmspackage = new NMSPackage(address, keep_alive_message, length);

                        byte[] bytemessage = nmspackage.toBytes();

                        // Send the data through the socket.  
                        socketout.Send(bytemessage);

                        var delay = await Task.Run(async () =>
                        {
                            Stopwatch sw = Stopwatch.StartNew();
                            await Task.Delay(2000);
                            sw.Stop();
                            return sw.ElapsedMilliseconds;
                        });
                    }
                }
                catch (Exception err)
                {

                    Console.WriteLine("Lock connection with NMS");
                }

            });


        }

        public void sendingMessageCommunication(string address, string communication, Socket socket)
        {
            try
            {
                short length = (Int16)communication.Length;
                NMSPackage nmspackage = new NMSPackage(address, communication, length);

                byte[] bytemessage = nmspackage.toBytes();

                // Send the data through the socket.  
                socket.Send(bytemessage);
            }
            catch (Exception)
            {
                Console.WriteLine("Lack of connection with NMS");
            }
        }

        public void fillingTable(string line, Socket socketsending, string key)
        {
            //Znaki oddzielające poszczególne części żądania klienta.
            char[] delimiterChars = { '#' };
            //Podzielenie żądania na tablicę stringów.
            string[] words;


            words = line.Split(delimiterChars);
            switch (words[0])
            {
                case "ADD":
                    switch (Int32.Parse(words[1]))
                    {
                        //Dodawanie wpisu do BorderComutationTable
                        case 1:
                            BorderNodeCommutationTableRow newRow = new BorderNodeCommutationTableRow(
                              words[2], Convert.ToInt16(words[3]), Convert.ToInt16(words[4]), Convert.ToInt16(words[5]), Convert.ToInt16(words[6]),
                              Convert.ToInt16(words[7]), words[8], Convert.ToInt16(words[9]), Convert.ToInt16(words[10]));
                            borderNodeCommutationTable.Table.Add(newRow);
                            stateReceivedMessageFromNMS("BorderNodeCommutationTable", "ADD");
                            break;
                        //Dodanie wpisu do EONTable
                        case 2:
                            EONTableRowIN eonIN = new EONTableRowIN(Convert.ToInt16(words[2]), Convert.ToInt16(words[3]));
                            EONTableRowOut eonOut = new EONTableRowOut(Convert.ToInt16(words[4]), Convert.ToInt16(words[5]));
                            bool eoninbool = eonTable.addRow(eonIN);
                            bool eonoutbool = eonTable.addRow(eonOut);
                            if (eoninbool == false || eonoutbool == false)
                            {
                                sendingMessageCommunication(OperationConfiguration.getSetting(key, mySettings), "ERROR", socketsending);
                            }
                            else
                            {
                                stateReceivedMessageFromNMS("EONTable", "ADD");
                            }

                            break;
                        //Dodanie wpisu do CommutationTable
                        case 3:
                            CommutationTableRow commuteRow = new CommutationTableRow(Convert.ToInt16(words[2]),
                                Convert.ToInt16(words[3]), Convert.ToInt16(words[4]), Convert.ToInt16(words[5]));
                            commutationTable.Table.Add(commuteRow);

                            stateReceivedMessageFromNMS("CommutationTable", "ADD");
                            break;
                        default:
                            break;

                    }
                    break;
                case "DELETE":
                    switch (Int32.Parse(words[1]))
                    {
                        //Dodawanie wpisu do BorderComutationTable
                        case 1:
                            BorderNodeCommutationTableRow newRow = new BorderNodeCommutationTableRow();
                            newRow = borderNodeCommutationTable.FindRow(words[2], Convert.ToInt16(words[3]), words[4]);
                            borderNodeCommutationTable.Table.Remove(newRow);
                            stateReceivedMessageFromNMS("BorderNodeCommutationTable", "DELETE");
                            break;
                        //Dodanie wpisu do EONTable
                        case 2:
                            EONTableRowIN eonIN = new EONTableRowIN(Convert.ToInt16(words[2]), Convert.ToInt16(words[3]));
                            EONTableRowOut eonOut = new EONTableRowOut(Convert.ToInt16(words[4]), Convert.ToInt16(words[5]));
                            bool eoninbool = eonTable.deleteRow(eonIN);
                            bool eonoutbool = eonTable.deleteRow(eonOut);

                            if (eoninbool == false || eonoutbool == false)
                            {
                                sendingMessageCommunication(OperationConfiguration.getSetting(key, mySettings), "ERROR", socketsending);
                            }
                            else
                            {
                                stateReceivedMessageFromNMS("EONTable", "DELETE");
                            }

                            break;
                        //Dodanie wpisu do CommutationTable
                        case 3:
                            CommutationTableRow rowToDelete = new CommutationTableRow();
                            rowToDelete = commutationTable.FindRow(Convert.ToInt16(words[2]), Convert.ToInt16(words[3]));
                            commutationTable.Table.Remove(rowToDelete);
                            stateReceivedMessageFromNMS("CommutationTable", "DELETE");
                            break;
                        default:
                            break;
                    }
                    break;
                case "TOPOLOGY":
                    switch (Int32.Parse(words[1]))
                    {

                        case 1:
                            //Dodawanie wpisu do BorderComutationTable
                            for (int i = 0; i < borderNodeCommutationTable.Table.Count; i++)
                            {
                                byte[] table_in_bytes = null;
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
                                command = "TOPOLOGY";
                                IP_IN = borderNodeCommutationTable.Table[i].IP_IN.ToString();
                                port_in = borderNodeCommutationTable.Table[i].port_in.ToString();
                                band_out = borderNodeCommutationTable.Table[i].band.ToString();
                                Frequency_out = borderNodeCommutationTable.Table[i].frequency.ToString();
                                Modulation = borderNodeCommutationTable.Table[i].modulationPerformance.ToString();
                                BitRate = borderNodeCommutationTable.Table[i].bitRate.ToString();
                                destination_IP = borderNodeCommutationTable.Table[i].IP_Destination.ToString();
                                port_out = borderNodeCommutationTable.Table[i].Port.ToString();
                                Hops = borderNodeCommutationTable.Table[i].hopsNumber.ToString();

                                builder = command + "#" + "1" + "#" + IP_IN + "#" + port_in + "#" + band_out + "#" + Frequency_out + "#" +
                                    Modulation + "#" + BitRate + "#" + destination_IP + "#" + port_out + "#" + Hops;

                                sendingMessageCommunication(OperationConfiguration.getSetting(key, mySettings), builder, socketsending);

                            }
                            break;
                        //Dodanie wpisu do EONTable
                        case 2:
                            for (int i = 0; i < eonTable.TableIN.Count; i++)
                            {
                                byte[] table_in_bytes = null;
                                string builder = string.Empty;
                                ;
                                string command = string.Empty;
                                string band_in = string.Empty;
                                string frequency_out = string.Empty;
                                string band_out = string.Empty;
                                string frequency_in = string.Empty;
                                command = "TOPOLOGY";

                                frequency_in = eonTable.TableIN[i].busyFrequency.ToString();
                                band_in = eonTable.TableIN[i].busyBandIN.ToString();
                                frequency_out = eonTable.TableOut[i].busyFrequency.ToString();
                                band_out = eonTable.TableOut[i].busyBandOUT.ToString();


                                builder = command + "#" + "2" + "#" + frequency_in + "#" + band_in + "#" + frequency_out + "#" + band_out;
                                sendingMessageCommunication(OperationConfiguration.getSetting(key, mySettings), builder, socketsending);
                            }

                            break;
                        //Dodanie wpisu do CommutationTable
                        case 3:
                            for (int i = 0; i < commutationTable.Table.Count; i++)
                            {
                                byte[] table_in_bytes = null;
                                string command = string.Empty;
                                string builder = string.Empty;
                                string port_in = string.Empty;
                                string port_out = string.Empty;
                                string Frequency_in = string.Empty;
                                string frequency_out = string.Empty;
                                command = "TOPOLOGY";
                                port_in = commutationTable.Table[i].port_in.ToString();

                                frequency_out = commutationTable.Table[i].frequency_out.ToString();
                                Frequency_in = commutationTable.Table[i].frequency_in.ToString();
                                port_out = commutationTable.Table[i].port_out.ToString();


                                builder = command + "#" + "3" + "#" + Frequency_in + "#" + port_in + "#" + frequency_out + "#" + port_out;

                                sendingMessageCommunication(OperationConfiguration.getSetting(key, mySettings), builder, socketsending);

                            }
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            // Console.ReadKey();
        }
        public static void stateReceivedMessageFromNMS(string table, string type)
        {

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[" + DateTime.Now + "] Message from NMS {0,8} {1} ", type, table);
            Console.ResetColor();
        }


        /* public static void stateSendingMessageToNMS( Socket socket)
         {
             if (socket != null)
             {

                 Console.ForegroundColor = ConsoleColor.DarkMagenta;
                 Console.WriteLine("[" + DateTime.Now + "] Message about ID: {0,5} and {1,2} sent on port: " + numberOfLink, ID, messageNumber);
                 Console.ResetColor();
             }
             else
             {
                 Console.ForegroundColor = ConsoleColor.DarkGray;
                 Console.WriteLine("CableCloud is not responding - link is not available");
                 Console.ResetColor();
             }

         }*/

    }
}
