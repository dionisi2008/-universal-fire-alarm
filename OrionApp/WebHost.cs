using System;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;

namespace OrionApp // Note: actual namespace depends on the project name.
{
    public class WebHost
    {
        private System.Net.HttpListener ServerAPI;
        private Dictionary<string, byte[]> BaseFiles = new Dictionary<string, byte[]>();
        private Dictionary<string, SessionData> Sessions = new Dictionary<string, SessionData>();
        private List<User> Users = new List<User>();
        private byte[] OutDate;
        public WebHost(int GetPort)
        {
            ReadAllFilesWeb();
            ReadAllUsers();


            ServerAPI = new System.Net.HttpListener();
            ServerAPI.Prefixes.Add("http://*:" + GetPort + '/');
            ServerAPI.Start();
            do
            {
                EventNewResult(ServerAPI.GetContextAsync().Result);
            } while (ServerAPI.IsListening);



        }

        public void ReadAllUsers()
        {
            List<string> usersReadFile = new List<string>(File.ReadAllLines("Users.txt"));
            for (int shag = 0; shag <= usersReadFile.Count - 1; shag++)
            {
                string[] workDate = usersReadFile[shag].Split(' ');
                this.Users.Add(new User(workDate[0], workDate[1]));
            }

        }

        public int AuthUser(string GetLogin, string GetPasword)
        {
            for (int shag = 0; shag <= Users.Count - 1; shag++)
            {
                if (Users[shag].Login == GetLogin & Users[shag].Password == GetPasword)
                {
                    return shag;
                }
            }
            return -1;
        }
        public void EventNewResult(System.Net.HttpListenerContext GetContext)
        {
            if (GetContext.Request.HttpMethod == "GET")
            {

                if (GetContext.Request.RawUrl == "/")
                {
                    byte[] OutDate = this.BaseFiles[@"\Default.html"];
                    GetContext.Response.OutputStream.Write(new ReadOnlySpan<byte>(OutDate));
                    GetContext.Response.OutputStream.Close();
                }
                else
                {
                    // Console.WriteLine(GetContext.Request.RawUrl);
                    if (this.BaseFiles.ContainsKey(GetContext.Request.RawUrl.Replace('/', @"\s"[0])))
                    {
                        byte[] OutDate = this.BaseFiles[GetContext.Request.RawUrl.Replace('/', @"\s"[0])];
                        GetContext.Response.ContentLength64 = OutDate.Length;
                        GetContext.Response.OutputStream.Write(new ReadOnlySpan<byte>(OutDate));

                    }
                    else
                    {
                        // System.Console.WriteLine(string.Join('\n', BaseFiles.Keys));
                        // System.Console.WriteLine('\n' + string.Join('\n', BaseFiles.Keys));
                        // System.Console.WriteLine(GetContext.Request.RawUrl);
                        GetContext.Response.OutputStream.Close();
                    }


                }
            }
            else
            {
                if (GetContext.Request.RawUrl == "/API")
                {
                    byte[] TmpDate = new byte[1024];
                    int tmpReadByte = GetContext.Request.InputStream.Read(TmpDate, 0, 1024);
                    Array.Resize(ref TmpDate, tmpReadByte);
                    string[] LoginAndPassword = System.Text.Encoding.UTF8.GetString(TmpDate).Split(' ');
                    if (LoginAndPassword.Length > 1)
                    {
                        switch (LoginAndPassword[0])
                        {
                            case "Auth":
                                int position = AuthUser(LoginAndPassword[1], LoginAndPassword[2]);
                                if (position >= 0)
                                {
                                    byte[] TempKeySession = new byte[8];
                                    new Random(DateTime.Now.Millisecond).NextBytes(TempKeySession);
                                    Sessions.Add(BitConverter.ToString(TempKeySession), new SessionData(GetContext.Request));
                                    string TmpKey = BitConverter.ToString(TempKeySession);
                                    OutDate = System.Text.Encoding.UTF8.GetBytes("Auth " + TmpKey);
                                    GetContext.Response.ContentLength64 = OutDate.Length;
                                }
                                else
                                {
                                    OutDate = System.Text.Encoding.UTF8.GetBytes("Auth ERROR");
                                    GetContext.Response.ContentLength64 = OutDate.Length;
                                }
                                break;
                            case "Session":
                                OutDate = System.Text.Encoding.UTF8.GetBytes("Session ERROR");
                                GetContext.Response.ContentLength64 = OutDate.Length;
                                if (Sessions.ContainsKey(LoginAndPassword[1]))
                                {
                                    if (Sessions[LoginAndPassword[1]].CreateTime.Day == DateTime.Now.Day &
                                     Sessions[LoginAndPassword[1]].CreateTime.Month == DateTime.Now.Month &
                                      Sessions[LoginAndPassword[1]].CreateTime.Year == DateTime.Now.Year &
                                       Sessions[LoginAndPassword[1]].CreateTime.Hour == DateTime.Now.Hour &
                                        Sessions[LoginAndPassword[1]].InfoUser.UserHostAddress == GetContext.Request.UserHostAddress)
                                    {
                                        Sessions[LoginAndPassword[1]].CreateTime = DateTime.Now;
                                        OutDate = System.Text.Encoding.UTF8.GetBytes("Session Ok");
                                        GetContext.Response.ContentLength64 = OutDate.Length;
                                    }
                                }
                                break;
                        }
                        GetContext.Response.OutputStream.Write(new ReadOnlySpan<byte>(OutDate));
                        GetContext.Response.Close();
                    }
                }
            }
        }
        public void ReadAllFilesWeb()
        {
            List<string> DirecectoryRead = new List<string>();
            DirecectoryRead.Add("Site");
            DirecectoryRead.AddRange(Directory.GetDirectories("Site"));
            for (int shag = 0; shag <= DirecectoryRead.Count - 1; shag++)
            {
                string[] ListFilesRootDir = Directory.GetFiles(DirecectoryRead[shag]);
                for (int shag2 = 0; shag2 <= ListFilesRootDir.Length - 1; shag2++)
                {
                    BaseFiles.Add(ListFilesRootDir[shag2].Replace("Site", ""), File.ReadAllBytes(ListFilesRootDir[shag2]));
                }
            }

        }

        public class SessionData
        {
            public DateTime CreateTime;
            public System.Net.HttpListenerRequest InfoUser;
            public SessionData(System.Net.HttpListenerRequest GetContext)
            {
                CreateTime = DateTime.Now;
                InfoUser = GetContext;

            }
        }
        public class User
        {
            public string Login, Password, Session;
            public DateTime DateActive;

            public User(string UserLoginGet, string UserPasswordGet)
            {
                this.Login = UserLoginGet;
                this.Password = UserPasswordGet;
            }
        }
    }
}
