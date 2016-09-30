// Copyright (C) 2016 by Barend Erasmus, David Jeske and donated to the public domain

using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

using SimpleHttpServer;
using SimpleHttpServer.Models;
using SimpleHttpServer.RouteHandlers;

namespace SimpleHttpServer.App
{
    class Program
    {

        static public void IESetupPrinter()
        {

            string strKey = "Software\\Microsoft\\Internet Explorer\\PageSetup";
            bool bolWritable = true;
            object oValue = "";
            RegistryKey oKey = Registry.CurrentUser.OpenSubKey(strKey, bolWritable);
            Console.Write(strKey);
            oKey.SetValue("footer", oValue);
            oKey.SetValue("header", oValue);
           // oKey.SetValue("margin_bottom", "0");
           // oKey.SetValue("margin_left", "0");
           // oKey.SetValue("margin_top", "0");
            oKey.SetValue("Print_Background", "no");
 
            oKey.Close();
        }
        static private void runBrowserThread(string text)
        {
            var th = new Thread(() => {
                var br = new WebBrowser();
                br.DocumentCompleted += browser_DocumentCompleted;
                br.DocumentText = text;
                Application.Run();
              ;
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

       static void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            IESetupPrinter();
            var br = sender as WebBrowser;
                br.Print();
        }
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var route_config = new List<Models.Route>() {
                new Route {
                    Name = "Print Handler",
                    UrlRegex = @"^/",
                    Method = "POST",
                    Callable = (Models.HttpRequest request) => {
                var html_data = request.Content.Replace("\n", string.Empty);
                       
                   runBrowserThread(html_data);
                        return new Models.HttpResponse()
                        {
                            ContentAsUTF8 = "Printered",
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                },
                new Route {
                    Name = "allow",
                    UrlRegex = @"^/",
                    Method = "OPTIONS",
                    Callable = (Models.HttpRequest request) => {
                    
                
                        return new Models.HttpResponse()
                        {
                            ContentAsUTF8 = "Allowed",
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                }, 
                //new Route {   
                //    Name = "FileSystem Static Handler",
                //    UrlRegex = @"^/Static/(.*)$",
                //    Method = "GET",
                //    Callable = new FileSystemRouteHandler() { BasePath = @"C:\Tmp", ShowDirectories=true }.Handle,
                //},
            };

            HttpServer httpServer = new HttpServer(8080, route_config);
            
            Thread thread = new Thread(new ThreadStart(httpServer.Listen));
            thread.Start();
        }
    }
}
