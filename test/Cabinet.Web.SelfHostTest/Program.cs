using Microsoft.Owin.Hosting;
using System;

namespace Cabinet.Web.SelfHostTest {
    class Program {
        static void Main(string[] args) {
            string url = args[0];
            
            using (WebApp.Start<Startup>(url)) {
                Console.WriteLine("Listening at: " + url);
                Console.ReadLine();
            }
        }
    }
}
