using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using System.Timers;

namespace KrumpfBot
{
    class Bot
    {
        private TcpClient twitch;  //Our connection to twitch server
        private StreamWriter output;  //Outgoing message stream
        private StreamReader input; //Incoming message stream
        private Thread readerThread; //Thread to read messages directly from twitch server
        private Thread parser;  //Thread that handles message and command responses
        private Boolean exitflag = false;  //If true, program will close
        private Queue<String> messages;  //Our queue of unhandled messages from server.
        private Mutex mex;  //Mutex to handle the message queue without thread mishaps
        private System.Timers.Timer autoMessages;   //timer for our scripted messages

        /// <summary>
        /// Constructor initializes the mutex and the message queue for our bot to handle.
        /// </summary>
        public Bot()
        {
            mex = new Mutex();
            messages = new Queue<String>();
        }

        /// <summary>
        /// The main program loop for the bot. Call this once, and let this function handle everything.
        /// </summary>
        public void RunBot()
        {
            if (!Connect()) return;

            readerThread = new Thread(ReadingLoop);
            readerThread.Start();

            StartTimer();

            parser = new Thread(ParseLoop);
            parser.Start();

            Authenticate();
            ConnectToChannel();

            HaltProgramLoop();

        }

        /// <summary>
        /// Since the main program runs in a loop, this will return and permanently end the program if it is called.
        /// </summary>
        public void HaltProgramLoop()
        {

            while (true)
            {
                System.Threading.Thread.Sleep(1000);


                if (exitflag) return;
            }
        }

        /// <summary>
        /// Connects to the specified channel in the Globals file.
        /// </summary>
        private void ConnectToChannel()
        {
            SendToIRC("JOIN #" + Globals.CHANNEL);
            SendToChat("I have returned... Kappa");
        }

        /// <summary>
        /// Connects to twitch and initializes input and output streams for network writing.
        /// </summary>
        /// <returns></returns>
        private Boolean Connect()
        {
            twitch = new TcpClient(Globals.SERVER, Globals.PORT);
            output = new StreamWriter(twitch.GetStream());
            input = new StreamReader(twitch.GetStream());
            return true;
        }

        /// <summary>
        /// Passes the username and password for connection authentication.
        /// </summary>
        private void Authenticate()
        {
            output.WriteLine("PASS " + Globals.PASSWORD);
            output.WriteLine("NICK " + Globals.USERNAME);
            output.Flush();
        }

        /// <summary>
        /// This program will read all incoming traffic from Twitch and place it into the message queue on demand.
        /// </summary>
        private void ReadingLoop()
        {
            String message = "";
            while (true)
            {
                message = input.ReadLine();
                Console.WriteLine(message);

                mex.WaitOne();
                messages.Enqueue(message);
                mex.ReleaseMutex();

                message = "";
            }
        }

        /// <summary>
        /// Writes messages to the twitch IRC server. Messages will be echoed to console.
        /// </summary>
        /// <param name="a_message">Message to send to twitch.</param>
        public void SendToIRC(String a_message)
        {
            output.WriteLine(a_message);
            Console.WriteLine(">>>> " + a_message);
            output.Flush();
        }

        /// <summary>
        /// Sends a message to the current chatroom channel.
        /// </summary>
        /// <param name="a_message"></param>
        public void SendToChat(String a_message)
        {
            //do not change this format, or messages will not be sent.
            SendToIRC(":" + Globals.USERNAME + "!" + Globals.USERNAME + "@" + Globals.USERNAME
                + ".tmi.twitch.tv PRIVMSG #" + Globals.CHANNEL + " :" + a_message);
        }

        /// <summary>
        /// This function is called to start and run a timer. 
        /// </summary>
        private void StartTimer()
        {
            autoMessages = new System.Timers.Timer(5 * 60 * 1000);
            autoMessages.AutoReset = true;
            autoMessages.Elapsed += HandleTimerElapsed;
            autoMessages.Enabled = true;
        }

        /// <summary>
        /// This will trigger when the timer reaches 0. The timer is set to re-start.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            CycleAutoMessages();
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Cycles messages in order when timer is elapsed.
        /// </summary>
        int MessageNum = 1;
        private void CycleAutoMessages()
        {
            switch ( MessageNum)
            {
                case 1:
                    SendToChat("Don't forget to follow me on twitter https://twitter.com/KrumpfGG/");
                    MessageNum += 1;
                    break;
                case 2:
                    SendToChat("KappaPride");
                    MessageNum += 1;
                    break;
                case 3:
                    SendToChat("PogChamp");
                    MessageNum += 1;
                    break;

                case 4:
                    SendToChat("WutFace");
                    MessageNum += 1;
                    break;
                case 5:
                    SendToChat("BibleThump");
                    MessageNum += 1;
                    break;
                default:
                    SendToChat("Be sure to follow to be notified when I'm streaming!");
                    MessageNum = 1;
                    break;
            }
           
            
        }
        

        /// <summary>
        /// This pulls messages from our mutex'd queue for our bot to parse.
        /// </summary>
        private void ParseLoop()
        {
            while (true)
            {
                while (messages.Count != 0)
                {
                    String NextMessage;
                    mex.WaitOne();
                    NextMessage = messages.Dequeue();
                    mex.ReleaseMutex();

                    RespondToMessage(NextMessage);

                }
            }
        }
        
        /// <summary>
        /// Any messages or commands we want to respond to are sent here.
        /// </summary>
        /// <param name="message"></param>
        private void RespondToMessage(String message)
        {
            //INCLUDE THIS. Without replying to this, twitch.tv will disconnect our bot.
            if (message.Contains("PING :tmi.twitch.tv") && !message.Contains("PRIVMSG")) SendToIRC("PONG :tmi.twitch.tv");
        }
    }
}
