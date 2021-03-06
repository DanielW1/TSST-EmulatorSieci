﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using NetworkingTools;
using System.Configuration;
using System.Runtime.Remoting.Messaging;

namespace SubNetwork
{

   public class MessageCC
    {
        public string addressCC { get; set; }
        public string addressStart { get; set; }
        public string startPort { get; set; }
        public string addressEnd { get; set; }
        public string endPort { get; set; }

        public MessageCC(string addressCC,string addressS,string portA,string addressE,string portB)
        {
            this.addressCC = addressCC;
            this.addressStart = addressS;
            this.startPort = portA;
            this.addressEnd = addressE;
            this.endPort = portB;

        }

    }

    public class ControlConnection
    {
        string numberCC;
        List<MessageStruct> listOfMessages;
        List<MessageCC> messagesCC;
        MessageStruct response;
        MessageCC transponderStart, transponderEnd;
        SubNetworkPoint startSNP, endSNP;
        string numberOfHops;
        string frequency;

        string connectionBitrate;

        //określa, czy ten CC jest centralnym CC dla tej sieci operatorskiej, będący najwyżej w hierarchii
        bool mainCC = false;

        public ControlConnection(string numberCC)
        {
            this.numberCC = numberCC;
            listOfMessages = new List<MessageStruct>();
            messagesCC = new List<MessageCC>();


        }

        private void SendingMessage(string ipaddress, string message)
        {
            byte[] data = new byte[64];

            UdpClient newsock = new UdpClient();
            IPEndPoint sender = new IPEndPoint(IPAddress.Parse(ipaddress), 11000);

            try
            {
                data = Encoding.ASCII.GetBytes(message);
                newsock.Send(data, data.Length, sender);

            }
            catch (Exception)
            {
            }

            newsock.Close();
        }

        /// <summary>
        /// Funkcja obsluguje odbior wiadomosci, korzystamy z polaczenia UDP
        /// </summary>
        public void ReceivedMessage()
        {
            byte[] data = new byte[64];
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //CC przyjmuje adres IP w zależności od wybranego numeru posieci 
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ConfigurationManager.AppSettings["CC" + numberCC]), 11000);
            UdpClient newsock = new UdpClient(ipep);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (true)
                {
                    data = newsock.Receive(ref sender);
                    string receivedMesage = Encoding.ASCII.GetString(data);
                    char separator = '#';
                    //Zczytanie ustalonych wedlug wlasnego protokolu wartosci oddzielonych znakiem #
                    string[] words = receivedMesage.Split(separator);

                    Task.Run(() => chooiceAction(words));
                }
            }
            catch (Exception)
            {

            }
        }


        private void ConnectionRequest(SubNetworkPoint startPoint, SubNetworkPoint endPoint)
        {

        }


        /// <summary>
        /// Generuje wykonwywanie pewnej sekwencji zadan w zaleznosci od typu operacji i komponety ktory sie z CC komunikuje
        /// </summary>
        /// <param name="message">lista stringow bedacych parametrami danego zapytania</param>
        private void chooiceAction(string[] message)
        {
            string action = message[1];
            string address = message[0];

            if (action == MessageNames.CONNECTION_REQUEST)
            {
                ConnectionRequestHandling(message);
            }
            else if (action == MessageNames.LINK_CONNECTION_DEALLOCATION)
            {
                connectionDeallocationHandling(message);
            }
            else if (action == MessageNames.LINK_CONNECTION_REQUEST)
            {
                messageLinkConnectionHandling(message);
            }
            else if (action == MessageNames.ROUTE_TABLE_QUERY)
            {

                //USUNĄC !!! DO TESTOW !!!!!!!!!!!!!!!!!!
                /*listOfMessages.Add(new MessageStruct()
                {
                    function = MessageNames.CONNECTION_REQUEST,
                    ip = ConfigurationManager.AppSettings["CC" + numberCC]
                });
                numberOfHops = "1";
                mainCC = true;*/
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                generateLogReceived("RC", MessageNames.ROUTE_TABLE_QUERY + "RESPONSE");
                string band = message[3];
                string token = "A";// RandomMessageGenerator.generateRandomMessage(3);
                frequency = message[2];

                //transponderStart = new MessageCC("", "127.0.0.2", "", "127.0.0.4", "");
                //struktura potrzebna, aby kontrolowac czy wszystkie wiadomosci otrzymaly potwierdzenie
                MessageStruct lrmMessage = new MessageStruct();
                lrmMessage.function = MessageNames.LINK_CONNECTION_REQUEST;
                lrmMessage.ip = ConfigurationManager.AppSettings["LRM" + numberCC];
                lrmMessage.tocken = token;
                //liczba wiadomosci wymagajacych potwierdzenia
                //Otrzymana liczbe SNPow dziele na pojedyncze i wysyłam prosbe o alokacje na kazdy SNP
                if (numberCC == "1")
                {
                    lrmMessage.count = (message.Length - 8); //(message.Length - 4) / 4 * 2;
                }
                else
                {
                    //w podstawowym wariancie zawsze są przynajmniej 2 SNP i z każdym dodanym węzełem ta liczba wzrasta o 2
                    //-4 to 5 - parametry +1 
                    lrmMessage.count = 2+(message.Length-5 - 3);
                }
                listOfMessages.Add(lrmMessage);
                int index = listOfMessages.FindIndex(x => x.function == MessageNames.CONNECTION_REQUEST);
                MessageStruct CCMessage = listOfMessages.ElementAt(index);
                if(numberCC=="1")
                {
                    //-8 bo 5 wartosci to parametry i wartosc zero na koncu (oddzielająca), -2 to wezly klienckie, -1 to krawedź
                    CCMessage.count = (message.Length - 8) / 2;
                }
                else
                {
                    //-4 bo 5 wartosci to parametry, +1 bo rozpoczyna się i kończy węzłem
                    CCMessage.count = (message.Length - 4) / 2;
                }
                  
               // CCMessage.count = (message.Length - 4) / 4;
                listOfMessages[index] = CCMessage;
                //Jezeli RC jest nadrzędnym na sieć wtedy zaczynamy indeksownie od 5-ego parametru sesji 
                int readIndex = 5;
                //Przypuszczam że w naszym algorytmie i przy wyznaczaniu ścieżek przez RC powstanie problem niezarezerwowania skrajnych SNP 
                //oraz trudnosc z wpisdami do tablic commutationTable, dlatego w przypadku pod sieci podejmuje inny schemat działania, 
                //Zapamiętuje skrajne SNP, dla których ma być zestawione połączenie i przetrzymuje je w celu przekazania do kolejnych komponentów
                if (!mainCC)
                {
                    string addressSNP1 = translateAddress(message[5], message[6]);
                    

                    messagesCC.Add(new MessageCC(ConfigurationManager.AppSettings[message[5]], startSNP.ipaddress.ToString(), startSNP.portIN.ToString(),
                        addressSNP1,
                        ConfigurationManager.AppSettings["RC2"+message[6]]));

                    //adres CC okreslany na podstawie trnaslacji z pliku konfiguracyjnego, otrzymuje adres wezla, sprawdzamjaki obsluguje go CC
                  /*  MessageCC firstPair = new MessageCC(ConfigurationManager.AppSettings[message[5]],
                        startSNP.ipaddress.ToString(), startSNP.portIN.ToString(), message[5], message[6]);
                    messagesCC.Add(firstPair);*/

                    string messageToLRMOUT = ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                             MessageNames.LINK_CONNECTION_REQUEST + "#" + band +
                                             "#" + token + "#" + numberOfHops + "#" + "OUT" + "#" + addressSNP1 + "#" +
                                             ConfigurationManager.AppSettings["RC2"+message[6]] + "#" + frequency + "#";

                    SendingMessage(ConfigurationManager.AppSettings["LRM" + numberCC], messageToLRMOUT);
                    generateLogSend("LRM", MessageNames.LINK_CONNECTION_REQUEST);
                    

                    string addressSNP2 = translateAddress(message[message.Length-2], message[message.Length-3]);


                    messagesCC.Add(new MessageCC(ConfigurationManager.AppSettings[addressSNP2], endSNP.ipaddress.ToString(),endSNP.portIN.ToString(),
                        addressSNP2,
                        ConfigurationManager.AppSettings["RC2"+message[message.Length-3]]));

                   /* MessageCC lastPair = new MessageCC(ConfigurationManager.AppSettings[message[message.Length - 2]],
                        message[message.Length - 2], message[message.Length - 1], endSNP.ipaddress.ToString(),
                        endSNP.portOUT.ToString());
                    messagesCC.Add(lastPair);*/

                    string messageToLRMOUT2 = ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                              MessageNames.LINK_CONNECTION_REQUEST + "#" +band +
                                              "#" + token + "#" + numberOfHops + "#" + "IN" + "#" +
                                              addressSNP2 + "#" + ConfigurationManager.AppSettings["RC2"+message[message.Length - 3]] + "#" +
                                              frequency + "#";

                    SendingMessage(ConfigurationManager.AppSettings["LRM" + numberCC], messageToLRMOUT2);
                   
                    generateLogSend("LRM", MessageNames.LINK_CONNECTION_REQUEST);

                    readIndex = 5;
                }


                // bitrate na sztwno do porawy !!!!!!!!!!
                //
                //
                //
                //
                if (mainCC)
                {
                    for (int i = readIndex; i < message.Length-3; i += 2)
                    {
                        string addressSNP1 = translateAddress(message[i + 1], message[i]);
                        string addressSNP2 = translateAddress((message[i + 1]), message[i + 2]);

                        messagesCC.Add(new MessageCC(ConfigurationManager.AppSettings["RC1"+message[i+1]], addressSNP1,
                            ConfigurationManager.AppSettings["RC1"+message[i]], addressSNP2,
                            ConfigurationManager.AppSettings["RC1"+message[i + 2]]));

                        string messageToLRMIN = ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                                MessageNames.LINK_CONNECTION_REQUEST + "#" + 2 +
                                                "#" + token + "#" + numberOfHops +
                                                "#" + "IN" + "#" + addressSNP1 + "#" +
                                                ConfigurationManager.AppSettings["RC1"+message[i]] + "#" + frequency +
                                                "#";
                        SendingMessage(ConfigurationManager.AppSettings["LRM" + numberCC], messageToLRMIN);
                        
                        generateLogSend("LRM", MessageNames.LINK_CONNECTION_REQUEST);

                        string messageToLRMOUT = ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                                 MessageNames.LINK_CONNECTION_REQUEST + "#" + 2 +
                                                 "#" + token + "#" + numberOfHops + "#" + "OUT" + "#" + addressSNP2 +
                                                 "#" + ConfigurationManager.AppSettings["RC1"+message[i + 2]] + "#" +
                                                 frequency + "#";

                        SendingMessage(ConfigurationManager.AppSettings["LRM" + numberCC], messageToLRMOUT);
                        
                        generateLogSend("LRM", MessageNames.LINK_CONNECTION_REQUEST);
                    }
                }
                else
                {
                    for (int i = readIndex; i < message.Length-3
                        ; i += 2)
                    {
                        string addressSNP1 = translateAddress(message[i + 1], message[i]);
                        string addressSNP2 = translateAddress((message[i + 1]), message[i + 2]);

                        messagesCC.Add(new MessageCC(ConfigurationManager.AppSettings[addressSNP1], addressSNP1,
                            ConfigurationManager.AppSettings["RC2"+message[i]], addressSNP2,
                            ConfigurationManager.AppSettings["RC2"+message[i + 2]]));

                        string messageToLRMIN = ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                                MessageNames.LINK_CONNECTION_REQUEST + "#" + 2 +
                                                "#" + token + "#" + numberOfHops +
                                                "#" + "IN" + "#" + addressSNP1 + "#" +
                                                ConfigurationManager.AppSettings["RC2"+message[i]] + "#" + frequency +
                                                "#";
                        SendingMessage(ConfigurationManager.AppSettings["LRM" + numberCC], messageToLRMIN);
                        lrmMessage.count++;
                        generateLogSend("LRM", MessageNames.LINK_CONNECTION_REQUEST);

                        string messageToLRMOUT = ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                                 MessageNames.LINK_CONNECTION_REQUEST + "#" + 2 +
                                                 "#" + token + "#" + numberOfHops + "#" + "OUT" + "#" + addressSNP2 +
                                                 "#" + ConfigurationManager.AppSettings["RC2"+message[i + 2]] + "#" +
                                                 frequency + "#";

                        SendingMessage(ConfigurationManager.AppSettings["LRM" + numberCC], messageToLRMOUT);
                        lrmMessage.count++;
                        generateLogSend("LRM", MessageNames.LINK_CONNECTION_REQUEST);
                    }
                }
                



            }
            else if (action == MessageNames.PEER_COORDINATION)
            {
                string code = message[2];
                string typeObject = message[3];



                //Otrzymalismy wiadomosc peer coordination z CC z innej sieci w celu zestawienia polaczenia
                if (code == "PUT")
                {
                    //Jeżeli otrzymało PERR COORDINATION to jest oto główne CC na sieć operatorską
                    mainCC = true;
                    string startAddress = message[4];
                    string endAddress = message[5];
                    string bitrate = message[6];
                    connectionBitrate = bitrate;
                    frequency = message[7];
                    string hops = "2";
                    numberOfHops = hops;
                    response = new MessageStruct();
                    response.ip = message[0];
                    response.function = message[1];

                    listOfMessages.Add(new MessageStruct()
                    {
                        function = MessageNames.CONNECTION_REQUEST,
                        ip = ConfigurationManager.AppSettings["CC" + numberCC]
                    });
                    // W drugiej sieci trzeba przepuścić sygnał przez transponder na końcu ścieżki 
                    transponderEnd = new MessageCC("", startAddress, "", endAddress, "");

                    generateLogReceived("CC", MessageNames.PEER_COORDINATION);
                    string messageToSend = ConfigurationManager.AppSettings["CC" + numberCC] + "#"
                                                                                             + MessageNames
                                                                                                 .ROUTE_TABLE_QUERY +
                                                                                             "#" + startAddress + "#" +
                                                                                             endAddress + "#" +
                                                                                             bitrate + "#" + hops +
                                                                                             "#" + frequency + "#";
                    SendingMessage(ConfigurationManager.AppSettings["RC" + numberCC], messageToSend);
                    generateLogSend("CC", MessageNames.ROUTE_TABLE_QUERY);
                }

            }
            else if (action == MessageNames.CONNECTION_CONFIRMED)
            {

                connectionConfirmed();
            }
            else if (action == MessageNames.CONNECTION_DELETE)
            {
                ConnectionDeleteHandling(message);
            }
            else if (action == MessageNames.GET_PATH)
            {
                getPathHandling(message);
            }
        }

        public void connectionDeallocationHandling(string[] message)
        {
            string address = message[0];
            string action = message[1];

            generateLogReceived("LRM", MessageNames.LINK_CONNECTION_DEALLOCATION + "RESPONSE");
            string token = message[2];
            string code = message[3];

            //Wiadomosci nadalismy tocken i po jego wartosci mozemy sprawdzic czy w liscie jest i gdzie jest nasza wiadomosc oczekujaca potwierdznia
            int index = listOfMessages.FindIndex(x => x.tocken == token);
            MessageStruct msg = listOfMessages.ElementAt(index);
            //Przyszła odpowiedź więc zmniejszamy o 1 liczbę wiadomosci potrzebujących potwierdzenia
            msg.count = msg.count - 1;
            listOfMessages[index] = msg;
            //Gdy wszystkie wiadomosci potwierdzone mozemy rozpoczac zestawienie polaczenia na nizszych poziomach
            if (msg.count == 0)
            {
                if (mainCC == true)
                {
                    //Według przyjętej konwencji jeżeli hops=1 to połącznie odbywa się w ramach jednej sieci operatorskiej 
                    if (Int32.Parse(numberOfHops) == 1)
                    {
                        transponderStart.addressCC = messagesCC[0].addressCC;
                        transponderStart.startPort = messagesCC[0].startPort;

                        //Uzupelnienie tablicy border node commutation table
                        borderTableFillingMessage(transponderStart, MessageNames.CONNECTION_DELETE);

                        transponderEnd.addressCC = messagesCC[messagesCC.Count - 1].addressCC;
                        transponderEnd.endPort = messagesCC[messagesCC.Count - 1].endPort;

                        //Uzupelnienie tablicy border node commutation table
                        borderTableFillingMessage(transponderEnd, MessageNames.CONNECTION_DELETE);

                    }
                    else
                    {
                        //Ten warunek pozwala określić czy jest to sieć pierwsz czy druga operatorska
                        //Jeżeli pierwsza- przy połaczeniu przez dwie to uzupełniamy border node comutation na poczatku sciezki
                        //Jezlei druga -przy łaczneniu uzupełniamy tablice transpondujące na końcu ścieżki
                        if (response.function == MessageNames.PEER_COORDINATION)
                        {

                            transponderEnd.addressCC = messagesCC[messagesCC.Count - 1].addressCC;
                            transponderEnd.endPort = messagesCC[messagesCC.Count - 1].endPort;
                            //Uzupelnienie tablicy border node commutation table
                            borderTableFillingMessage(transponderEnd, MessageNames.CONNECTION_DELETE);
                        }
                        else
                        {
                            transponderStart.addressCC = messagesCC[0].addressCC;
                            transponderStart.startPort = messagesCC[0].startPort;
                            //Uzupelnienie tablicy border node commutation table
                            borderTableFillingMessage(transponderStart, MessageNames.CONNECTION_DELETE);
                        }


                    }
                }

                //Wysłanie snp wejsciowego i wyjsciowego do podsieci(odpowiedzialnego za nią CC) w celu dalszego zestawienia połączenia
                foreach (var item in messagesCC)
                {

                    string MessageToCC = ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                         MessageNames.CONNECTION_DELETE + "#" +
                                         "PUT" + "#" + "CC" + "#" + item.addressStart + "#" + item.startPort + "#" +
                                         item.addressEnd + "#" + item.endPort + "#" +
                                         connectionBitrate + "#" + numberOfHops + "#" + frequency + "#";
                    SendingMessage(item.addressCC, MessageToCC);
                }

                listOfMessages.RemoveAt(index);
                messagesCC.Clear();
                frequency = string.Empty;
                numberOfHops = string.Empty;
                connectionBitrate = string.Empty;
            }


        }

        /// <summary>
        /// W przypadku usuwania sciezki, deallokowania lub modyfikowania, pozyskujemy sciezke od RC
        /// </summary>
        /// <param name="message"></param>
        public void getPathHandling(string[] message)
        {
            //USUNĄC !!! DO TESTOW !!!!!!!!!!!!!!!!!!
            listOfMessages.Add(new MessageStruct()
            {
                function = MessageNames.CONNECTION_DELETE,
                ip = ConfigurationManager.AppSettings["CC" + numberCC]
            });
            numberOfHops = "1";
            mainCC = true;
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            generateLogReceived("RC", MessageNames.GET_PATH + "RESPONSE");
            string token = message[2];
            frequency = message[3];

            //struktura potrzebna, aby kontrolowac czy wszystkie wiadomosci otrzymaly potwierdzenie
            MessageStruct lrmMessage = new MessageStruct();
            lrmMessage.function = MessageNames.LINK_CONNECTION_DEALLOCATION;
            lrmMessage.ip = ConfigurationManager.AppSettings["LRM" + numberCC];
            lrmMessage.tocken = token;
            //liczba wiadomosci wymagajacych potwierdzenia
            //Otrzymana liczbe SNPow dziele na pojedyncze i wysyłam prosbe o alokacje na kazdy SNP
            lrmMessage.count = (message.Length - 5) / 4 * 2;
            listOfMessages.Add(lrmMessage);
            int index = listOfMessages.FindIndex(x => x.function == MessageNames.CONNECTION_DELETE);
            MessageStruct CCMessage = listOfMessages.ElementAt(index);
            CCMessage.count = (message.Length - 5) / 4;
            listOfMessages[index] = CCMessage;
            //Jezeli RC jest nadrzędnym na sieć wtedy zaczynamy indeksownie od 5-ego parametru sesji 
            int readIndex = 5;
            //Przypuszczam że w naszym algorytmie i przy wyznaczaniu ścieżek przez RC powstanie problem niezarezerwowania skrajnych SNP 
            //oraz trudnosc z wpisdami do tablic commutationTable, dlatego w przypadku pod sieci podejmuje inny schemat działania, 
            //Zapamiętuje skrajne SNP, dla których ma być zestawione połączenie i przetrzymuje je w celu przekazania do kolejnych komponentów
            if (!mainCC)
            {
                //adres CC okreslany na podstawie trnaslacji z pliku konfiguracyjnego, otrzymuje adres wezla, sprawdzamjaki obsluguje go CC
                MessageCC firstPair = new MessageCC(ConfigurationManager.AppSettings[message[4]],
                    startSNP.ipaddress.ToString(), startSNP.portIN.ToString(), message[4], message[5]);
                messagesCC.Add(firstPair);

                string messageToLRMOUT = ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                         MessageNames.LINK_CONNECTION_DEALLOCATION +
                                         "#" + token + "#" + "OUT" + "#" + message[4] + "#" + message[5] + "#" +
                                         frequency + "#";

                SendingMessage(ConfigurationManager.AppSettings["LRM" + numberCC], messageToLRMOUT);
                generateLogSend("LRM", MessageNames.LINK_CONNECTION_DEALLOCATION);

                MessageCC lastPair = new MessageCC(ConfigurationManager.AppSettings[message[message.Length - 2]],
                    message[message.Length - 2], message[message.Length - 1], endSNP.ipaddress.ToString(),
                    endSNP.portOUT.ToString());
                messagesCC.Add(lastPair);

                string messageToLRMOUT2 = ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                          MessageNames.LINK_CONNECTION_DEALLOCATION +
                                          "#" + token + "#" + numberOfHops + "#" + "IN" + "#" +
                                          message[message.Length - 2] + "#" + message[message.Length - 1] + "#" +
                                          frequency + "#";

                SendingMessage(ConfigurationManager.AppSettings["LRM" + numberCC], messageToLRMOUT2);
                generateLogSend("LRM", MessageNames.LINK_CONNECTION_DEALLOCATION);

                readIndex = 7;
            }

            for (int i = readIndex; i < message.Length; i += 4)
            {

                messagesCC.Add(new MessageCC(ConfigurationManager.AppSettings[message[i]], message[i], message[i + 1],
                    message[i + 2], message[i + 3]));

                string messageToLRMIN = ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                        MessageNames.LINK_CONNECTION_DEALLOCATION +
                                        "#" + token + "#" + numberOfHops + "#" + "IN" + "#" + message[i] + "#" +
                                        message[i + 1] + "#" + frequency + "#";

                SendingMessage(ConfigurationManager.AppSettings["LRM" + numberCC], messageToLRMIN);
                generateLogSend("LRM", MessageNames.LINK_CONNECTION_REQUEST);

                string messageToLRMOUT = ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                         MessageNames.LINK_CONNECTION_DEALLOCATION +
                                         "#" + token + "#" + numberOfHops + "#" + "OUT" + "#" + message[i + 2] + "#" +
                                         message[i + 3] + "#" + frequency + "#";

                SendingMessage(ConfigurationManager.AppSettings["LRM" + numberCC], messageToLRMOUT);
                generateLogSend("LRM", MessageNames.LINK_CONNECTION_REQUEST);
            }
        }

        /// <summary>
        /// Obsluga wiadomosci zwizanych ze stykiem Connection Delete, żądania jak i odpowiedzi
        /// </summary>
        /// <param name="message">lista parametrow przesylanych miedzy komponentami wedlug zaprojektowanego protokolu</param>
        public void ConnectionDeleteHandling(string[] message)
        {
            string action = message[1];
            string address = message[0];
            string code = message[2];
            string typeObject = message[3];


            if (typeObject.StartsWith("NCC"))
            {
                //Jest to CC najwyzsze w hierarchii bo kontaktuje się z nim NCC
                mainCC = true;

                string startAddress = message[4];
                string endAddress = message[5];
                string hops = message[6];
                numberOfHops = hops;

                if (code == "PUT")
                {
                    generateLogReceived(typeObject, MessageNames.CONNECTION_DELETE);
                    response = new MessageStruct();
                    response.ip = message[0];
                    response.function = message[1];


                    //Według przyjętej konwencji jeżeli hops=1 to połącznie odbywa się w ramach jednej sieci operatorskiej 
                    if (Int32.Parse(hops) == 1)
                    {
                        //Oznacza to że wezeł poczatkowy i końcowy na tym poziomie są węzłami dostępowymi z transponderami
                        //Zamieniamy z szarej częstotliwości na inną(wejście), lub z innej na szarą(wyjście)
                        transponderStart = new MessageCC("", startAddress, "", endAddress, "");
                        transponderEnd = new MessageCC("", startAddress, "", endAddress, "");

                        listOfMessages.Add(new MessageStruct()
                        {
                            function = MessageNames.CONNECTION_DELETE,
                            ip = ConfigurationManager.AppSettings["CC" + numberCC]
                        });


                    }
                    else
                    {
                        //Dodajemy strukture wiadomosci do listy wiadomosci oczekujacych na potwierdznie
                        //Po przyjsciu odpowiedzi bedziemy usuwac je z listy i gdy wszystko przebiegnie poprawnie zwrocimy pozytywna wiadomosc wyzej
                        listOfMessages.Add(new MessageStruct()
                        {
                            function = MessageNames.CONNECTION_REQUEST,
                            ip = ConfigurationManager.AppSettings["CC" + numberCC]
                        });
                        transponderStart = new MessageCC("", startAddress, "", endAddress, "");
                    }

                    string messageToSend = ConfigurationManager.AppSettings["CC" + numberCC] + "#"
                                                                                             + MessageNames.GET_PATH +
                                                                                             "#" + startAddress + "#" +
                                                                                             endAddress + "#" + hops +
                                                                                             "#" + frequency + "#";
                    SendingMessage(ConfigurationManager.AppSettings["RC" + numberCC], messageToSend);
                }
            }

            else
            {
                if (code == "PUT")
                {
                    generateLogReceived(typeObject, MessageNames.CONNECTION_DELETE);
                    response = new MessageStruct();
                    response.ip = message[0];
                    response.function = message[1];

                    string startAddress = message[4];
                    string startPort = message[5];
                    string endAddress = message[6];
                    string endPort = message[7];
                    string hops = message[9];
                    string connectionFrequency = message[10];
                    numberOfHops = hops;
                    frequency = connectionFrequency;
                    //SNP startowy i koncowy dla sciezki zestawianej na tym poziomie
                    startSNP = new SubNetworkPoint(IPAddress.Parse(startAddress), Int32.Parse(startPort), 0);
                    endSNP = new SubNetworkPoint(IPAddress.Parse(endAddress), 0, Int32.Parse(endPort));

                    listOfMessages.Add(new MessageStruct()
                    {
                        function = MessageNames.CONNECTION_DELETE,
                        ip = ConfigurationManager.AppSettings["CC" + numberCC]
                    });

                    string messageToSend = ConfigurationManager.AppSettings["CC" + numberCC] + "#"
                                                                                             + MessageNames.GET_PATH +
                                                                                             "#" + startAddress + "#" +
                                                                                             endAddress + "#" + hops +
                                                                                             "#" + frequency + "#";

                    SendingMessage(ConfigurationManager.AppSettings["RC" + numberCC], messageToSend);

                }
                else if (code == "OK")
                {
                    generateLogReceived(typeObject, MessageNames.CONNECTION_DELETE + "RESPONSE");

                    int index = listOfMessages.FindIndex(x => x.function == MessageNames.CONNECTION_DELETE);
                    MessageStruct msg = listOfMessages.ElementAt(index);
                    msg.count = msg.count - 1;
                    listOfMessages[index] = msg;
                    if (msg.count == 0)
                    {
                        listOfMessages.RemoveAt(index);

                        if (listOfMessages.Count == 0)
                        {
                            if (mainCC == true)
                            {
                                if (numberOfHops == "1")
                                {
                                    string responseMessageToUp =
                                        ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                        MessageNames.CONNECTION_DELETE + "#" + "OK" + "#";
                                    SendingMessage(response.ip, responseMessageToUp);
                                }
                                else //Gdy numberOfHops jest 2 to musimy rozważyć dwa przypadki, gdy będzie to sprawdzane w domenie inicjującej
                                    //i gdy będzie to rozważane w domenie drugiej
                                    //po przyjściu wszystkich odpowiedzi z warstwy niższej, albo wysyłamy PEER COORDINATION, albo wysyłamy odpowiedź na PEER COORDINATION
                                {
                                    if (response.function == MessageNames.CONNECTION_DELETE)
                                    {
                                        MessageStruct PC = new MessageStruct();
                                        PC.ip = ConfigurationManager.AppSettings["CCDomain" + numberCC];
                                        PC.function = MessageNames.PEER_COORDINATION;
                                        listOfMessages.Add(PC);

                                        string messagePeerCoordination =
                                            ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                            MessageNames.PEER_COORDINATION + "#" + "DELETE" + "#" + "CC" + "#" +
                                            transponderStart.addressStart + "#" + transponderStart.addressEnd +
                                            "#" + frequency + "#";
                                        SendingMessage(ConfigurationManager.AppSettings["CCDomain" + numberCC],
                                            messagePeerCoordination);
                                        generateLogSend("CC", response.function);
                                    }
                                    else
                                    {
                                        string messageResponseOnPeerCoordination =
                                            ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                            MessageNames.CONNECTION_DELETE + "#" + "OK" + "#" + "CC" + "#";

                                        SendingMessage(response.ip, messageResponseOnPeerCoordination);
                                        generateLogSend("CC", MessageNames.CONNECTION_DELETE + "RESPONSE");
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Funkcja obsługująca wiadomość Connection Confirmed
        /// </summary>
        private void connectionConfirmed()
        {
            generateLogReceived("CC", MessageNames.CONNECTION_CONFIRMED);
            int index = listOfMessages.FindIndex(x => x.function == MessageNames.PEER_COORDINATION);
            MessageStruct msg = listOfMessages.ElementAt(index);


            listOfMessages.RemoveAt(index);
            //Jeżeli wszystkie wiadomości zostały potwierdzone to wysyłamy wiadomość do góry
            if (listOfMessages.Count == 0)
            {
                string responseMessageToUp = ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                             MessageNames.CONNECTION_REQUEST + "#" + "OK" + "#";
                SendingMessage(response.ip, responseMessageToUp);
            }
        }

        /// <summary>
        /// Obsluga wiadomosci zwizanych ze stykiem Connection Request, żądania jak i odpowiedzi
        /// </summary>
        /// <param name="message">lista parametrow przesylanych miedzy komponentami wedlug zaprojektowanego protokolu</param>
        private void ConnectionRequestHandling(string[] message)
        {

            string action = message[1];
            string address = message[0];
            string code = message[2];
            string typeObject = message[3];

            if (typeObject.StartsWith("NCC"))
            {
                //Jest to CC najwyzsze w hierarchii bo kontaktuje się z nim NCC
                mainCC = true;

                string startAddress = message[4];
                string endAddress = message[5];
                string bitrate = message[6];
                string hops = message[7];
                numberOfHops = hops;
                connectionBitrate = bitrate;

                if (code == "PUT")
                {
                    generateLogReceived(typeObject, MessageNames.CONNECTION_REQUEST);
                    response = new MessageStruct();
                    response.ip = message[0];
                    response.function = message[1];


                    //Według przyjętej konwencji jeżeli hops=1 to połącznie odbywa się w ramach jednej sieci operatorskiej 
                    if (Int32.Parse(hops) == 1)
                    {
                        //Oznacza to że wezeł poczatkowy i końcowy na tym poziomie są węzłami dostępowymi z transponderami
                        //Zamieniamy z szarej częstotliwości na inną(wejście), lub z innej na szarą(wyjście)
                        transponderStart = new MessageCC("", startAddress, "", endAddress, "");
                        transponderEnd = new MessageCC("", startAddress, "", endAddress, "");

                        listOfMessages.Add(new MessageStruct()
                        {
                            function = MessageNames.CONNECTION_REQUEST,
                            ip = ConfigurationManager.AppSettings["CC" + numberCC]
                        });

                    }
                    else
                    {
                        //Dodajemy strukture wiadomosci do listy wiadomosci oczekujacych na potwierdznie
                        //Po przyjsciu odpowiedzi bedziemy usuwac je z listy i gdy wszystko przebiegnie poprawnie zwrocimy pozytywna wiadomosc wyzej
                        listOfMessages.Add(new MessageStruct()
                        {
                            function = MessageNames.CONNECTION_REQUEST,
                            ip = ConfigurationManager.AppSettings["CC" + numberCC]
                        });

                        transponderStart = new MessageCC("", startAddress, "", endAddress, "");
                    }

                    string messageToSend = ConfigurationManager.AppSettings["CC" + numberCC] + "#"
                                                                                             + MessageNames
                                                                                                 .ROUTE_TABLE_QUERY +
                                                                                             "#" + startAddress + "#" +
                                                                                             endAddress + "#" +
                                                                                             bitrate + "#" + hops + "#";
                    SendingMessage(ConfigurationManager.AppSettings["RC" + numberCC], messageToSend);
                }
            }
            else
            {
                if (code == "PUT")
                {
                    generateLogReceived(typeObject, MessageNames.CONNECTION_REQUEST);
                    response = new MessageStruct();
                    response.ip = message[0];
                    response.function = message[1];

                    string startAddress = message[4];
                    string startPort = message[5];
                    string endAddress = message[6];
                    string endPort = message[7];
                    string bitrate = message[8];
                    string hops = message[9];
                    string connectionFrequency = message[10];
                    numberOfHops = hops;
                    connectionBitrate = bitrate;
                    frequency = connectionFrequency;
                    //SNP startowy i koncowy dla sciezki zestawianej na tym poziomie
                    startSNP = new SubNetworkPoint(IPAddress.Parse(startAddress), Int32.Parse(startPort), 0);
                    endSNP = new SubNetworkPoint(IPAddress.Parse(endAddress), 0, Int32.Parse(endPort));

                    listOfMessages.Add(new MessageStruct()
                    {
                        function = MessageNames.CONNECTION_REQUEST,
                        ip = ConfigurationManager.AppSettings["CC" + numberCC]
                    });

                    string messageToSend = ConfigurationManager.AppSettings["CC" + numberCC] + "#"
                                                                                             + MessageNames
                                                                                                 .ROUTE_TABLE_QUERY +
                                                                                             "#" + startAddress + "#" +
                                                                                             endAddress + "#" +
                                                                                             bitrate + "#" + hops +
                                                                                             "#" + frequency + "#";

                    SendingMessage(ConfigurationManager.AppSettings["RC" + numberCC], messageToSend);

                }
                else if (code == "OK")
                {
                    generateLogReceived(typeObject, MessageNames.CONNECTION_REQUEST + "RESPONSE");

                    int index = listOfMessages.FindIndex(x => x.function == MessageNames.CONNECTION_REQUEST);
                    MessageStruct msg = listOfMessages.ElementAt(index);
                    msg.count = msg.count - 1;
                    listOfMessages[index] = msg;
                    if (msg.count == 0)
                    {
                        listOfMessages.RemoveAt(index);

                        if (listOfMessages.Count == 0)
                        {
                            if (mainCC == true)
                            {
                                if (numberOfHops == "1")
                                {
                                    string responseMessageToUp =
                                        ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                        MessageNames.CONNECTION_REQUEST + "#" + "OK" + "#";
                                    SendingMessage(response.ip, responseMessageToUp);
                                }
                                else //Gdy numberOfHops jest 2 to musimy rozważyć dwa przypadki, gdy będzie to sprawdzane w domenie inicjującej
                                    //i gdy będzie to rozważane w domenie drugiej
                                    //po przyjściu wszystkich odpowiedzi z warstwy niższej, albo wysyłamy PEER COORDINATION, albo wysyłamy odpowiedź na PEER COORDINATION
                                {
                                    if (response.function == MessageNames.CONNECTION_REQUEST)
                                    {
                                        MessageStruct PC = new MessageStruct();
                                        PC.ip = ConfigurationManager.AppSettings["CCDomain" + numberCC];
                                        PC.function = MessageNames.PEER_COORDINATION;
                                        listOfMessages.Add(PC);

                                        string messagePeerCoordination =
                                            ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                            MessageNames.PEER_COORDINATION + "#" + "PUT" + "#" + "CC" + "#" +
                                            transponderStart.addressStart + "#" + transponderStart.addressEnd +
                                            "#" + connectionBitrate + "#" + frequency + "#";
                                        SendingMessage(ConfigurationManager.AppSettings["CCDomain" + numberCC],
                                            messagePeerCoordination);
                                        generateLogSend("CC", MessageNames.PEER_COORDINATION);
                                    }
                                    else
                                    {
                                        string messageResponseOnPeerCoordination =
                                            ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                            MessageNames.CONNECTION_CONFIRMED + "#" + "OK" + "#" + "CC" + "#";

                                        SendingMessage(response.ip, messageResponseOnPeerCoordination);
                                        generateLogSend("CC", MessageNames.CONNECTION_CONFIRMED);
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Obsluga wiadomosci zwizanych ze stykiem Link Connection, żądania jak i odpowiedzi
        /// </summary>
        /// <param name="message">lista parametrow przesylanych miedzy komponentami wedlug zaprojektowanego protokolu</param>
        private void messageLinkConnectionHandling(string[] message)
        {
            string address = message[0];
            string action = message[1];

            generateLogReceived("LRM", MessageNames.LINK_CONNECTION_REQUEST + "RESPONSE");
            string token = message[2];
            string code = message[3];
            /*if (code == "LAMBDA")
            {
                frequency = message[4];
                int index = listOfMessages.FindIndex(x => x.tocken == token);
                MessageStruct msg = listOfMessages.ElementAt(index);
                msg.count = msg.count - 1;

                listOfMessages[index] = msg;

            }
            else */
            if (code == "OK")
            {
                //Wiadomosci nadalismy tocken i po jego wartosci mozemy sprawdzic czy w liscie jest i gdzie jest nasza wiadomosc oczekujaca potwierdznia
                int index = listOfMessages.FindIndex(x => x.tocken == token);
                MessageStruct msg = listOfMessages.ElementAt(index);
                //Przyszła odpowiedź więc zmniejszamy o 1 liczbę wiadomosci potrzebujących potwierdzenia
                msg.count = msg.count - 1;
                listOfMessages[index] = msg;
                //Gdy wszystkie wiadomosci potwierdzone mozemy rozpoczac zestawienie polaczenia na nizszych poziomach
                if (msg.count == 0)
                {
                    if (mainCC == true)
                    {
                        //Według przyjętej konwencji jeżeli hops=1 to połącznie odbywa się w ramach jednej sieci operatorskiej 
                        if (Int32.Parse(numberOfHops) == 1)
                        {
                            transponderStart.addressCC = messagesCC[0].addressCC;
                            transponderStart.startPort = messagesCC[0].startPort;

                            //Uzupelnienie tablicy border node commutation table
                            borderTableFillingMessage(transponderStart, MessageNames.CONNECTION_REQUEST);

                            transponderEnd.addressCC = messagesCC[messagesCC.Count - 1].addressCC;
                            transponderEnd.endPort = messagesCC[messagesCC.Count - 1].endPort;

                            //Uzupelnienie tablicy border node commutation table
                            borderTableFillingMessage(transponderEnd, MessageNames.CONNECTION_REQUEST);

                        }
                        else
                        {
                            //Ten warunek pozwala określić czy jest to sieć pierwsz czy druga operatorska
                            //Jeżeli pierwsza- przy połaczeniu przez dwie to uzupełniamy border node comutation na poczatku sciezki
                            //Jezlei druga -przy łaczneniu uzupełniamy tablice transpondujące na końcu ścieżki
                            if (response.function == MessageNames.PEER_COORDINATION)
                            {

                                transponderEnd.addressCC = messagesCC[messagesCC.Count - 1].addressCC;
                                transponderEnd.endPort = messagesCC[messagesCC.Count - 1].endPort;
                                //Uzupelnienie tablicy border node commutation table
                                borderTableFillingMessage(transponderEnd, MessageNames.CONNECTION_REQUEST);
                            }
                            else
                            {
                                transponderStart.addressCC = messagesCC[0].addressCC;
                                transponderStart.startPort = messagesCC[0].startPort;
                                //Uzupelnienie tablicy border node commutation table
                                borderTableFillingMessage(transponderStart, MessageNames.CONNECTION_REQUEST);
                            }


                        }
                    }

                    //Wysłanie snp wejsciowego i wyjsciowego do podsieci(odpowiedzialnego za nią CC) w celu dalszego zestawienia połączenia
                    foreach (var item in messagesCC)
                    {

                        string MessageToCC = ConfigurationManager.AppSettings["CC" + numberCC] + "#" +
                                             MessageNames.CONNECTION_REQUEST + "#" +
                                             "PUT" + "#" + "CC" + "#" + item.addressStart + "#" + item.startPort + "#" +
                                             item.addressEnd + "#" + item.endPort + "#" +
                                             connectionBitrate + "#" + numberOfHops + "#" + frequency + "#";
                        SendingMessage(item.addressCC, MessageToCC);
                    }

                    listOfMessages.RemoveAt(index);
                    messagesCC.Clear();
                    frequency = string.Empty;
                    numberOfHops = string.Empty;
                    connectionBitrate = string.Empty;
                }
            }


        }

        /// <summary>
        /// Funkcja wysyla prosbe o uzupelnienie tabeli border node commutation table na interfejsie na poczatku lub koncu sciezki 
        /// </summary>
        /// <param name="transponder"></param>
        public void borderTableFillingMessage(MessageCC transponder, string operation)
        {

            string MessageToCC = ConfigurationManager.AppSettings["CC" + numberCC] + "#" + operation + "#" +
                                 "PUT" + "#" + "BORDERTABLE" + "#" + transponder.addressStart + "#" +
                                 transponder.startPort +
                                 "#" + transponder.addressEnd + "#" + frequency + "#";

            //Zwiekszamy zapamietana liczbe wyslanych wiadomosci o 1 oczywiscie po to aby wiedziec czy otrzymamy wszystkie odpowiedzi
            int ind = listOfMessages.FindIndex(x => x.function == operation);
            MessageStruct CCMessage = listOfMessages.ElementAt(ind);
            CCMessage.count++;
            listOfMessages[ind] = CCMessage;

            SendingMessage(transponderStart.addressCC, MessageToCC);
        }


        public static void generateLogReceived(string nameRemoteObject, string function)
        {
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "]" + "CC received message " + function + " from " +
                              nameRemoteObject);
        }

        public static void generateLogSend(string nameRemoteObject, string function)
        {
            Console.WriteLine("[" + Timestamp.generateTimestamp() + "]" + "CC send message " + function + " from " +
                              nameRemoteObject);
        }

        public string translateAddress(string adres, string numberOfLink)
        {
            if (adres == "127.0.0.31" && numberOfLink == "2311")
            {
                return "127.0.0.31";
            }
            else if (adres == "127.0.0.31" && numberOfLink == "3171")
            {
                return "127.0.0.31";
            }
            else if (adres == "127.0.0.7" && numberOfLink == "3171")
            {
                return "127.0.0.3";
            }
            else if (adres == "127.0.0.7" && numberOfLink == "472")
            {
                return "127.0.0.5";
            }
            else if (adres == "127.0.0.3" && numberOfLink == "352")
            {
                return "127.0.0.3";
            }
            else if (adres == "127.0.0.5" && numberOfLink == "352")
            {
                return "127.0.0.5";
            }

            return "127.0.0.1";

        }
    }
}
