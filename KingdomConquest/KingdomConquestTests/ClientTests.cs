using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KingdomConquest_Shared;

namespace KingdomConquestTests
{
   [TestClass]
   public class ClientTests
   {
      private const int PORT_NUM = 5756;
      private const string SERVER_IP = "127.0.0.1";
      private const string USERNAME_1 = "testname1";
      private const string USERNAME_2 = "testname2";

      [TestMethod]
      public void Connect_RandomOpponent_Success()
      {
         string connectionResult = "";
         Client client1 = new Client(SERVER_IP);

         connectionResult = client1.Connect(USERNAME_1);

         client1.Abort();

         Assert.AreEqual("WAITING_RANDOM", connectionResult);
      }

      [TestMethod]
      public void Connect_Failure_UsernameTaken()
      {
         string connectionResult = "";
         Client client1 = new Client(SERVER_IP);
         Client client2 = new Client(SERVER_IP);

         client1.Connect(USERNAME_1);
         connectionResult = client2.Connect(USERNAME_1);

         client1.Abort();
         client2.Abort();

         Assert.AreEqual("USERNAME_TAKEN", connectionResult);
      }

      [TestMethod]
      public void Connect_FriendOpponent_Success()
      {
         string connectionResult = "";
         Client client1 = new Client(SERVER_IP);

         connectionResult = client1.Connect(USERNAME_1, USERNAME_2);

         client1.Abort();

         Assert.AreEqual("WAITING_FRIEND", connectionResult);
      }

      
   }
}
