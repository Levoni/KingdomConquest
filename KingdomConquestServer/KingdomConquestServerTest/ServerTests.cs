using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using KingdomConquestServer;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace KingdomConquestServerTest
{
   [TestClass]
   public class ServerTests
   {
      private const int PORT_NUM = 5756;
      private const string IP_ADDRESS = "127.0.0.1";
      private const int BUF_SIZE = 2048;
      Thread runThread;

      TcpClient client1;
      TcpClient client2;

      NetworkStream stream1;
      NetworkStream stream2;

      [TestMethod]
      public void Run_EstablishConnection()
      {
         Server serverTest = new Server();

         runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);

         stream1 = client1.GetStream();

         stream1.Close();
         client1.Close();

         serverTest.Abort();
         //runThread.Abort();
      }

      [TestMethod]
      public void Run_RequestUsername_Received()
      {
         Server serverTest = new Server();

         runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         stream1 = client1.GetStream();

         byte[] byteBuffer = new byte[BUF_SIZE];

         int size = 0;

         Thread.Sleep(1000);

         if (stream1.DataAvailable)
            size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         stream1.Close();
         client1.Close();

         serverTest.Abort();

         string message = Encoding.ASCII.GetString(byteBuffer, 0, size);
         Assert.AreEqual("REQUEST_USERNAME|", message);
      }

      [TestMethod]
      public void Run_RequestUsername_Accepted()
      {
         string message = "";

         Server serverTest = new Server();

         runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         stream1 = client1.GetStream();

         byte[] byteBuffer = new byte[BUF_SIZE];
         int size = 0;

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);
         message = Encoding.ASCII.GetString(byteBuffer, 0, size);
         byteBuffer = new byte[BUF_SIZE];
         if (message.Equals("REQUEST_USERNAME|"))
         {
            byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname|");
            stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname|"));
         }

         Thread.Sleep(1000);

         byteBuffer = new byte[BUF_SIZE];

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         stream1.Close();
         client1.Close();

         serverTest.Abort();

         message = Encoding.ASCII.GetString(byteBuffer, 0, size);
         Assert.AreEqual("REQUEST_MATCH_TYPE|", message);
      }

      [TestMethod]
      public void Run_RequestUsername_Rejected()
      {
         string message = "";

         Server serverTest = new Server();

         runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         client2 = new TcpClient(IP_ADDRESS, PORT_NUM);

         stream1 = client1.GetStream();
         stream2 = client2.GetStream();

         Thread.Sleep(1000);

         byte[] byteBuffer = new byte[BUF_SIZE];
         int size = 0;

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);
         message = Encoding.ASCII.GetString(byteBuffer, 0, size);

         byteBuffer = new byte[BUF_SIZE];
         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname|");

         if (message.Equals("REQUEST_USERNAME|"))
            stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname|"));

         byteBuffer = new byte[BUF_SIZE];

         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message = Encoding.ASCII.GetString(byteBuffer, 0, size);

         byteBuffer = new byte[BUF_SIZE];
         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname|");

         if (message.Equals("REQUEST_USERNAME|"))
            stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         message = Encoding.ASCII.GetString(byteBuffer, 0, size);

         stream1.Close();
         client1.Close();

         stream2.Close();
         client2.Close();

         serverTest.Abort();

         Assert.AreEqual("USERNAME_TAKEN|", message);
      }

      [TestMethod]
      public void Run_RequestMatchType_Random()
      {
         string message = "";
         int size = 0;
         byte[] byteBuffer = new byte[BUF_SIZE];

         Server serverTest = new Server();

         Thread runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         stream1 = client1.GetStream();

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         message = Encoding.ASCII.GetString(byteBuffer, 0, size);

         stream1.Close();
         client1.Close();

         serverTest.Abort();

         Assert.AreEqual("WAITING_RANDOM|", message);
      }

      [TestMethod]
      public void Run_RequestMatchType_Friend()
      {
         string message = "";
         int size = 0;
         byte[] byteBuffer = new byte[BUF_SIZE];

         Server serverTest = new Server();

         Thread runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         stream1 = client1.GetStream();

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|FRIEND|friend|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|friend|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         message = Encoding.ASCII.GetString(byteBuffer, 0, size);

         stream1.Close();
         client1.Close();

         serverTest.Abort();

         Assert.AreEqual("WAITING_FRIEND|", message);
      }

      [TestMethod]
      public void Run_MatchOpponents_Random()
      {
         string message1 = "";
         string message2 = "";
         int size = 0;
         byte[] byteBuffer = new byte[BUF_SIZE];

         Server serverTest = new Server();

         Thread runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         client2 = new TcpClient(IP_ADDRESS, PORT_NUM);

         stream1 = client1.GetStream();
         stream2 = client2.GetStream();

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname1|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname1|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         //

         while (!stream2.DataAvailable)
         {
            ;
         }

         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname2|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname2|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         Thread.Sleep(1000);

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);
         message1 = Encoding.ASCII.GetString(byteBuffer, 0, size);

         /*
         while (!stream2.DataAvailable)
         {
            ;
         }
         
         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);
         */

         stream1.Close();
         client1.Close();

         stream2.Close();
         client2.Close();

         serverTest.Abort();

         Assert.AreEqual("OPPONENT_FOUND|0|", message1);
         Assert.AreEqual("WAITING_RANDOM|OPPONENT_FOUND|1|", message2);
      }

      [TestMethod]
      public void Run_MatchOpponents_Friend()
      {
         string message1 = "";
         string message2 = "";
         int size = 0;
         byte[] byteBuffer = new byte[BUF_SIZE];

         Server serverTest = new Server();

         Thread runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         client2 = new TcpClient(IP_ADDRESS, PORT_NUM);

         stream1 = client1.GetStream();
         stream2 = client2.GetStream();

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname1|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname1|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|FRIEND|testname2|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|FRIEND|testname2|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         //

         while (!stream2.DataAvailable)
         {
            ;
         }

         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname2|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname2|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|FRIEND|testname1|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|FRIEND|testname1|"));

         Thread.Sleep(1000);

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);


         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);
         message1 = Encoding.ASCII.GetString(byteBuffer, 0, size);


         /*
         while (!stream2.DataAvailable)
         {
            ;
         }
         
         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);
         */

         stream1.Close();
         client1.Close();

         stream2.Close();
         client2.Close();

         serverTest.Abort();

         Assert.AreEqual("OPPONENT_FOUND|0|", message1);
         Assert.AreEqual("WAITING_FRIEND|OPPONENT_FOUND|1|", message2);
      }

      [TestMethod]
      public void Run_GameInitialization()
      {
         string message1 = "";
         string message2 = "";

         int size = 0;
         byte[] byteBuffer = new byte[BUF_SIZE];

         Server serverTest = new Server();

         Thread runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         client2 = new TcpClient(IP_ADDRESS, PORT_NUM);

         stream1 = client1.GetStream();
         stream2 = client2.GetStream();

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname1|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname1|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         //

         while (!stream2.DataAvailable)
         {
            ;
         }

         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname2|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname2|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         Thread.Sleep(1000);

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         /*
         while (!stream2.DataAvailable)
         {
            ;
         }
         
         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);
         */

         byteBuffer = Encoding.ASCII.GetBytes("INITIALIZE|2|TestUnit1:1:1:true:true:true:100:100:2:3:4:5:6:70:melee|TestUnit2:2:2:true:true:true:101:101:7:8:9:10:11:71:melee|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("INITIALIZE|2|TestUnit1:1:1:true:true:true:100:100:2:3:4:5:6:70:melee|TestUnit2:2:2:true:true:true:101:101:7:8:9:10:11:71:melee|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message1 = Encoding.ASCII.GetString(byteBuffer, 0, size);

         byteBuffer = Encoding.ASCII.GetBytes("INITIALIZE|2|TestUnit3:3:3:true:true:true:102:102:12:13:14:15:16:72:ranged|TestUnit4:4:4:true:true:true:103:103:17:18:19:20:21:73:magic|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("INITIALIZE|2|TestUnit3:3:3:true:true:true:102:102:12:13:14:15:16:72:ranged|TestUnit4:4:4:true:true:true:103:103:17:18:19:20:21:73:magic|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);

         stream1.Close();
         client1.Close();

         stream2.Close();
         client2.Close();

         serverTest.Abort();

         Assert.AreEqual("INITIALIZE|2|TestUnit1:1:1:true:true:true:100:100:2:3:4:5:6:70:melee|TestUnit2:2:2:true:true:true:101:101:7:8:9:10:11:71:melee|", message1);
         Assert.AreEqual("INITIALIZE|2|TestUnit3:3:3:true:true:true:102:102:12:13:14:15:16:72:ranged|TestUnit4:4:4:true:true:true:103:103:17:18:19:20:21:73:magic|", message2);
      }

      [TestMethod]
      public void Run_Move()
      {
         string message1 = "";
         string message2 = "";
         int size = 0;
         byte[] byteBuffer = new byte[BUF_SIZE];

         Server serverTest = new Server();

         Thread runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         client2 = new TcpClient(IP_ADDRESS, PORT_NUM);

         stream1 = client1.GetStream();
         stream2 = client2.GetStream();

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname1|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname1|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         //

         while (!stream2.DataAvailable)
         {
            ;
         }

         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname2|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname2|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         Thread.Sleep(1000);

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         /*
         while (!stream2.DataAvailable)
         {
            ;
         }
         
         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);
         */

         byteBuffer = Encoding.ASCII.GetBytes("MOVE|1|1|2|2|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MOVE|1|1|2|2|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message1 = Encoding.ASCII.GetString(byteBuffer, 0, size);

         byteBuffer = Encoding.ASCII.GetBytes("MOVE|2|2|3|3|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MOVE|2|2|3|3|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);

         stream1.Close();
         client1.Close();

         stream2.Close();
         client2.Close();

         serverTest.Abort();

         Assert.AreEqual("MOVE|1|1|2|2|", message1);
         Assert.AreEqual("MOVE|2|2|3|3|", message2);
      }

      [TestMethod]
      public void Run_Attack()
      {
         string message1 = "";
         string message2 = "";

         int size = 0;
         byte[] byteBuffer = new byte[BUF_SIZE];

         Server serverTest = new Server();

         Thread runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         client2 = new TcpClient(IP_ADDRESS, PORT_NUM);

         stream1 = client1.GetStream();
         stream2 = client2.GetStream();

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname1|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname1|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         //

         while (!stream2.DataAvailable)
         {
            ;
         }

         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname2|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname2|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         Thread.Sleep(1000);

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         /*
         while (!stream2.DataAvailable)
         {
            ;
         }
         
         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);
         */

         byteBuffer = Encoding.ASCII.GetBytes("ATTACK|1|1|2|1|10|5|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("ATTACK|1|1|2|1|10|5|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message1 = Encoding.ASCII.GetString(byteBuffer, 0, size);

         byteBuffer = Encoding.ASCII.GetBytes("ATTACK|3|3|4|3|11|6|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("ATTACK|3|3|4|3|11|6|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);

         stream1.Close();
         client1.Close();

         stream2.Close();
         client2.Close();

         serverTest.Abort();

         Assert.AreEqual("ATTACK|1|1|2|1|10|5|", message1);
         Assert.AreEqual("ATTACK|3|3|4|3|11|6|", message2);
      }

      [TestMethod]

      public void Run_TurnEnd()
      {
         string message1 = "";
         string message2 = "";

         int size = 0;
         byte[] byteBuffer = new byte[BUF_SIZE];

         Server serverTest = new Server();

         Thread runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         client2 = new TcpClient(IP_ADDRESS, PORT_NUM);

         stream1 = client1.GetStream();
         stream2 = client2.GetStream();

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname1|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname1|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         //

         while (!stream2.DataAvailable)
         {
            ;
         }

         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname2|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname2|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         Thread.Sleep(1000);

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         /*
         while (!stream2.DataAvailable)
         {
            ;
         }
         
         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);
         */

         byteBuffer = Encoding.ASCII.GetBytes("TURN_END|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("TURN_END|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message1 = Encoding.ASCII.GetString(byteBuffer, 0, size);

         byteBuffer = Encoding.ASCII.GetBytes("TURN_END|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("TURN_END|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);

         stream1.Close();
         client1.Close();

         stream2.Close();
         client2.Close();

         serverTest.Abort();

         Assert.AreEqual("TURN_END|", message1);
         Assert.AreEqual("TURN_END|", message2);
      }

      [TestMethod]
      public void Run_LevelUp()
      {
         string message1 = "";
         string message2 = "";

         int size = 0;
         byte[] byteBuffer = new byte[BUF_SIZE];

         Server serverTest = new Server();

         Thread runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         client2 = new TcpClient(IP_ADDRESS, PORT_NUM);

         stream1 = client1.GetStream();
         stream2 = client2.GetStream();

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname1|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname1|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         //

         while (!stream2.DataAvailable)
         {
            ;
         }

         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname2|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname2|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         Thread.Sleep(1000);

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         /*
         while (!stream2.DataAvailable)
         {
            ;
         }
         
         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);
         */

         byteBuffer = Encoding.ASCII.GetBytes("LEVEL_UP|1|1|2|3|4|5|6|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("LEVEL_UP|1|1|2|3|4|5|6|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message1 = Encoding.ASCII.GetString(byteBuffer, 0, size);

         byteBuffer = Encoding.ASCII.GetBytes("LEVEL_UP|2|2|7|8|9|10|11|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("LEVEL_UP|2|2|7|8|9|10|11|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);

         stream1.Close();
         client1.Close();

         stream2.Close();
         client2.Close();

         serverTest.Abort();

         Assert.AreEqual("LEVEL_UP|1|1|2|3|4|5|6|", message1);
         Assert.AreEqual("LEVEL_UP|2|2|7|8|9|10|11|", message2);
      }

      [TestMethod]
      public void Run_ExpectedDisconnect_P1()
      {
         string message = "";

         int size = 0;
         byte[] byteBuffer = new byte[BUF_SIZE];

         Server serverTest = new Server();

         Thread runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         client2 = new TcpClient(IP_ADDRESS, PORT_NUM);

         stream1 = client1.GetStream();
         stream2 = client2.GetStream();

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname1|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname1|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         //

         while (!stream2.DataAvailable)
         {
            ;
         }

         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname2|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname2|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         Thread.Sleep(1000);

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         /*
         while (!stream2.DataAvailable)
         {
            ;
         }
         
         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);
         */

         byteBuffer = Encoding.ASCII.GetBytes("DISCONNECT|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("DISCONNECT|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message = Encoding.ASCII.GetString(byteBuffer, 0, size);

         Thread.Sleep(1000);

         stream1.Close();
         //stream1.Dispose();
         client1.Close();
         //bool test = client1.Connected;
         //Assert.IsFalse(test);

         stream2.Close();
         //stream2.Dispose();
         client2.Close();

         //bool test = stream1.DataAvailable;

         Thread.Sleep(1000);

         serverTest.Abort();

         Assert.AreEqual("DISCONNECT|", message);
      }

      [TestMethod]
      public void Run_ExpectedDisconnect_P2()
      {
         string message = "";

         int size = 0;
         byte[] byteBuffer = new byte[BUF_SIZE];

         Server serverTest = new Server();

         Thread runThread = new Thread(new ThreadStart(serverTest.Run));
         runThread.Start();

         Thread.Sleep(1000);

         client1 = new TcpClient(IP_ADDRESS, PORT_NUM);
         client2 = new TcpClient(IP_ADDRESS, PORT_NUM);

         stream1 = client1.GetStream();
         stream2 = client2.GetStream();

         while (!stream1.DataAvailable)
         {
            ;
         }

         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname1|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname1|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream1.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         //

         while (!stream2.DataAvailable)
         {
            ;
         }

         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("USERNAME|testname2|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("USERNAME|testname2|"));

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         byteBuffer = Encoding.ASCII.GetBytes("MATCH_TYPE|RANDOM|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("MATCH_TYPE|RANDOM|"));

         Thread.Sleep(1000);

         while (!stream2.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);

         /*
         while (!stream2.DataAvailable)
         {
            ;
         }
         
         byteBuffer = new byte[BUF_SIZE];
         size = stream2.Read(byteBuffer, 0, BUF_SIZE);
         message2 = Encoding.ASCII.GetString(byteBuffer, 0, size);
         */

         byteBuffer = Encoding.ASCII.GetBytes("DISCONNECT|");
         stream2.Write(byteBuffer, 0, Encoding.ASCII.GetByteCount("DISCONNECT|"));

         while (!stream1.DataAvailable)
         {
            ;
         }

         byteBuffer = new byte[BUF_SIZE];
         size = stream1.Read(byteBuffer, 0, BUF_SIZE);
         message = Encoding.ASCII.GetString(byteBuffer, 0, size);

         Thread.Sleep(1000);

         stream1.Close();
         client1.Close();

         stream2.Close();
         client2.Close();

         Thread.Sleep(1000);

         serverTest.Abort();

         Assert.AreEqual("DISCONNECT|", message);
      }

      [TestMethod]
      public void Run_UnexpectedDisconnect()
      {

      }
   }
}
