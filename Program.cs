using System;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Linq;
using System.IO;

namespace GFWListUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            var ignoreLists = new []{"@@||fonts.googleapis.com","@@||storage.googleapis.com","@@||cn.gravatar.com","@@||csi.gstatic.com","@@||fonts.gstatic.com"};

            if(File.Exists("ignorelist.txt")){
                var ignorelist = string.Empty;
                using(var fs = File.OpenRead("ignorelist.txt")){
                    using(var streamReader = new StreamReader(fs)){
                        ignorelist = streamReader.ReadToEnd();
                    }
                }

                if(!string.IsNullOrEmpty(ignorelist)){
                    ignoreLists = ignorelist.Split('\n').ToArray();
                }
            }

             UpateGFWList(ignoreLists);  
        }

        static void UpateGFWList(string[] ignoreLists){
            var gfwListUrl = "https://raw.githubusercontent.com/gfwlist/gfwlist/master/gfwlist.txt";
            var content= string.Empty;

            var client = new HttpClient();
            try{
                var response =  client.GetStringAsync(gfwListUrl).Result;
                content = response;
            }catch(Exception e){
                Console.WriteLine(e.Message);
            }

            var list = System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(content));

            if(!string.IsNullOrEmpty(list)){
                var array = list.Split('\n').Where(x=>!ignoreLists.Contains(x));
                content = String.Join("\n", array);
            }

            if(string.IsNullOrEmpty(content)){
                Console.WriteLine("Failed");
                return;
            }

            using(var fs = new FileStream("gfwlist.txt", FileMode.Create, FileAccess.ReadWrite)){
                var r = Convert.ToBase64String(Encoding.ASCII.GetBytes(content));
                var sb = new StringBuilder(r);
                for (int i = (r.Length / 64) * 64; i >= 64; i -= 64)
                    sb.Insert(i, Environment.NewLine);

                var buffer = Encoding.ASCII.GetBytes(sb.ToString());
                fs.Write(buffer,0, buffer.Length);
                fs.Flush();
            }

            Console.WriteLine("Done");
        }
    }
}
