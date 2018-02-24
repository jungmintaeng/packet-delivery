using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Packet_Delivery;

namespace SoftwareHW4
{
    public partial class ServerForm : Form
    {
        private NetworkStream NetStream; //네트워크 스트림 선언
        private byte[] ReadBuffer = new byte[1024 * 4]; //Receive에 사용할 버퍼
        private byte[] SendBuffer = new byte[1024 * 4]; //Send에 사용할 버퍼
        private TcpListener server; //Server측이기 때문에 클라이언트를 기다리는 listener 생성

        private Thread thr;         //ServerStart(Listen)하는 쓰레드
        private Thread threader;    //Receive하는 쓰레드

        private FileStream f;       //파일스트림
        private Socket Cli;         //클라이언트 연결 소켓

        private bool ServerRunning = false; //서버가 켜져있는지
        private bool CliConnected = false;  //클라이언트가 연결되었는지

        private FilePacket Fp;
        private FileList Fl;
        private Request Rq;         //타입에 맞게 사용하기 위해 Packet의 자식 클래스들 선언

        public ServerForm()
        {
            InitializeComponent();
        }

        private void ServerForm_Load(object sender, EventArgs e)    //ServerForm이 Load되었을 때
        {
            try
            {
                this.thr = new Thread(new ThreadStart(ServerStart));    //thr을 NULL로 만들지 않기 위해(예외방지)
                folderBrowserDialog.SelectedPath = @"C:\\";         //Defalt 경로를 C:\\로 설정하였다.
                PathTextBox.Text = folderBrowserDialog.SelectedPath;//PathTextBox에 경로를 출력한다.
            }
            catch (Exception ex)             //예외가 발생할 경우 에러메시지를 출력한다
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PathBtn_Click(object sender, EventArgs e)  //Path 버튼 클릭했을 때
        {
            if (!ServerRunning) //서버가 닫혀있을 때에만 버튼이 동작하도록 한다.
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)        //folderBrowserDialog를 열어 Path를 선택하게 하고
                    this.PathTextBox.Text = folderBrowserDialog.SelectedPath;   //선택된 Path를 TextBox에 출력한다.
            }
            else
                MessageBox.Show("You can't change Path while server is running !"); //서버가 구동중이면 수정할 수 없다

        }

        private void StartBtn_Click(object sender, EventArgs e) //Start버튼을 눌렀을 때
        {
            if (PortTextBox.Text == "") //포트 주소가 비어있다면
            {
                MessageBox.Show("Input PORT Number");   //메시지 출력후
                return; //리턴
            }
            if (folderBrowserDialog.SelectedPath == @"C:\\")    //C:\\는 경로 액세스 거부가 나올 수 있기 때문에
            {
                MessageBox.Show("No permission to access C:\\\r\nPlease Change the path before Starting"); //메시지 출력 후
                return; //리턴
            }


            if (!ServerRunning) //서버가 구동중이 아닐 때
            {
                thr = new Thread(new ThreadStart(ServerStart)); //서버를 시작하는 함수 쓰레드 할당
                thr.Start();    //시작
            }
            else
            {
                this.StartBtn.Text = "Start"; //Stop버튼을 다시 Start버튼으로 돌려놓음

                this.StartBtn.ForeColor = Color.Black;   //Start버튼의 폰트색깔을 검정색으로 변경

                this.ProceedTextBox.Text = this.ProceedTextBox.Text + "\r\nServer : Stop\r\n"; //진행 상태 출력

                this.ProceedTextBox.Select(ProceedTextBox.Text.Length, 0);//
                this.ProceedTextBox.ScrollToCaret();                      //텍스트박스의 스크롤을 맨 밑으로 내려주는 부분


                ClearTextBox();     //Ip와 Port란을 비워주는 함수

                ServerStop();       //서버를 멈추는 함수
            }
        }

        private void ServerStart()  //서버를 시작하여 Listener를 열고 클라이언트를 기다리는 함수
        {
            try
            {
                int PortNum = int.Parse(PortTextBox.Text);  //예외 발생할 수 있는 부분(Parsing오류)
                server = new TcpListener(PortNum);  //Port란에 써있는 숫자로 server 열기
                server.Start(); //서버 시작

                IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName()); //HostName을 통해 자신의 IP 알아냄

                this.Invoke(new MethodInvoker(delegate () //쓰레드 내에서 컨트롤을 건드리는 부분
                {
                    this.StartBtn.Text = "Stop";             //Start 버튼을 Stop버튼으로 변경

                    this.StartBtn.ForeColor = Color.Red;     //폰트 색상 변경

                    this.IPTextBox.Text = IPHost.AddressList[0].ToString(); //알아낸 IP Address를 문자열로 바꾸어 IPTextBox에 띄움

                    this.ProceedTextBox.Text = this.ProceedTextBox.Text + "Server : Start" + "\r\nStorage Path : "
                            + this.PathTextBox.Text + "\r\nwaiting for client access...."; //텍스트박스에 텍스트 출력

                    this.ProceedTextBox.Select(ProceedTextBox.Text.Length, 0);
                    this.ProceedTextBox.ScrollToCaret();    //텍스트박스의 스크롤을 끝까지 내려주는 부분
                }));

                this.ServerRunning = true;              //서버가 켜졌다는 플래그 참

                while (ServerRunning)               //서버가 Running되는 동안
                {
                    Cli = server.AcceptSocket();    //클라이언트를 기다려서 연결한다

                    if (Cli.Connected)       //클라이언트 연결됨
                    {
                        CliConnected = true;    //클라이언트가 연결되었다는 플래그를 참으로 만들고

                        this.Invoke(new MethodInvoker(delegate ()//컨트롤을 건드리는 부분
                        {
                            this.ProceedTextBox.Text = this.ProceedTextBox.Text + "\r\nClient accessed !\r\n";

                            this.ProceedTextBox.Select(ProceedTextBox.Text.Length, 0);
                            this.ProceedTextBox.ScrollToCaret();//클라이언트가 접속했다고 알리고 스크롤
                        }));

                        this.NetStream = new NetworkStream(Cli);    //넷스트림 연결

                        System.Threading.Thread.Sleep(1000);    //서버가 파일의 리스트를 연결하자마자 보냈더니
                                                                //클라이언트의 리시브 쓰레드가 실행되기도 전에
                                                                //보내지는 것이 있어서 리스트뷰에 표시되지 않음
                                                                //따라서 서버에서 1초를 sleep으로 기다리다가 보냄

                        SendFileList();                         //현재 경로 밑의 파일 리스트를 보내어
                                                                //클라이언트 리스트뷰에 추가 할수 있도록 해주는 함수

                        threader = new Thread(new ThreadStart(Receive));    //받는 쓰레드 생성
                        threader.Start();   //시작
                    }
                    else
                    {
                        CliConnected = false;
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            catch (OverflowException ex)    //위의 3개 Exception은 Port number parsing
                                            //과정에서 나올 수 있는 예외이다
            {
                MessageBox.Show(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void SendFileList()     //파일의 이름과 사이즈만 보내어 리스트 뷰의 아이템을 만들 수 있도록 한다
        {
            foreach (string s in Directory.GetFiles(folderBrowserDialog.SelectedPath))
            {//디렉토리에 있는 파일들을 가져온 후 첫 파일부터 마지막 파일까지
                Fl = new FileList();
                FileInfo fi = new FileInfo(s);  //파일 정보를 얻기 위해
                Fl.FileName = fi.Name;  //파일이름
                Fl.FileSize = fi.Length;    //파일 사이즈
                Fl.Type = 1;    //보내기 타입 = 리스트뷰 아이템
                Packet.Serialize(Fl).CopyTo(this.SendBuffer, 0);    //Serialize하여 SendBuffer에 저장
                NetStream.Write(SendBuffer, 0, SendBuffer.Length);  //네트워크 스트림에 쓰고
                NetStream.Flush();  //보낸다
                ClearBuffer(SendBuffer);    //SendBuffer를 비운다
            }
        }

        private void Receive()  //전송이 오면 받는 함수-> 쓰레드로 돌아감
        {
            try
            {
                int count = 1;  //현재 파일의 받고 있는 부분
                int nRead = 0;  //파일을 몇번 전송해야 하는가

                while (CliConnected)    //클라이언트가 서버에 연결되어 있는 동안
                {
                    this.NetStream.Read(this.ReadBuffer, 0, this.ReadBuffer.Length);    //전송이 온 경우 네트워크 스트림을 읽고
                    Packet p = (Packet)Packet.Deserialize(this.ReadBuffer);     //해독한 후
                    
                    switch (p.Type) //타입에 따라 행동한다
                    {
                        case 0: //보통의 파일 전송타입일 때
                            {
                                Fp = (FilePacket)p; //파일 전송을 위한 클래스로 다운캐스팅
                                nRead = (int)Fp.FileSize / Fp.FileData.Length + 1;  //몇 번 보내야 하는가
                                int fileIndex = 1;  //파일 이름의 중복을 처리하기 위한 변수
                                string fileName = PathTextBox.Text + "\\" + Fp.FileName;    //파일 이름을 저장하고

                                if (count == 1) //파일의 첫부분을 받았을 때
                                {
                                    while (File.Exists(fileName))   //파일의 이름이 존재한다면
                                    {
                                        fileName = PathTextBox.Text + "\\" + "(" + fileIndex.ToString() + ")" + Fp.FileName;
                                        fileIndex++;//파일 이름이 중복될 경우 앞에 번호를 붙이는 식으로 중복을 피함
                                    }
                                    f = new FileStream(fileName, FileMode.Create, FileAccess.Write); //중복되지 않는 파일 이름으로 파일을 생성
                                }

                                f.Write(Fp.FileData, 0, Fp.FileData.Length);    //파일에 데이터를 쓴다
                                                                                //이 때 파일 안에서의 position은 파일을 닫지 않으면 계속 이어서 쓸 수 있기 때문에
                                                                                //파일의 끝부분을 받았을 때 파일스트림을 닫아준다

                                this.Invoke(new MethodInvoker(delegate ()//컨트롤을 건드리는 부분
                                {
                                    this.ProceedTextBox.Text = this.ProceedTextBox.Text + "\r\nFile Receive....  " + count + "/" + nRead + "  Done\r\n";
                                    this.ProceedTextBox.Select(ProceedTextBox.Text.Length, 0);
                                    this.ProceedTextBox.ScrollToCaret();// 현재 전송된 부분 / 전체 전송 부분 을 출력하고, 텍스트박스의 가장 밑으로 스크롤한다
                                    count++;
                                }));

                                if (count > nRead)  //count = nRead일 때가 마지막 전송이므로, count가 nRead보다 커진다면
                                {
                                    count = 1;      //다른 파일을 받을 수 있도록 count = 1로 초기화 해주고
                                    f.Dispose();
                                    f.Close();      //파일을 다 썼으므로 파일을 닫는다.
                                }
                                break;
                            }
                        case 1: //파일의 리스트 Type일 때-->서버가 받을 일 없음
                            {
                                break;
                            }
                        case 2: //파일을 달라는 request일 때
                            {
                                try
                                {
                                    Rq = (Request)p;    //요청 클래스를 받아서 다운캐스팅
                                    f = new FileStream(PathTextBox.Text + "\\" + Rq.FileName, FileMode.Open, FileAccess.Read);
                                    //파일 이름을 받았고, 경로도 알고 있기 때문에 파일스트림 생성 가능
                                    Fp = new FilePacket();  //파일 전송을 위한 클래스 인스턴스 생성

                                    Fp.Type = 0;        //타입은 보통의 파일 전송
                                    Fp.FileName = Rq.FileName;  //파일 이름
                                    Fp.FileSize = f.Length; //파일 크기
                                    for (int i = 0; i < (int)(f.Length / Fp.FileData.Length) + 1; i++)
                                    {//파일 크기를 나누고 버퍼의 크기로 나누고, 나머지부분을 한번 더 전송하기 위해 +1을 함
                                        f.Read(Fp.FileData, 0, Fp.FileData.Length); //파일의 데이터를 버퍼의 크기만큼 읽어와 버퍼에 넣어줌
                                        Packet.Serialize(Fp).CopyTo(this.SendBuffer, 0);    //Serialize해서 SendBuffer에 넣어줌
                                        NetStream.Write(SendBuffer, 0, SendBuffer.Length);  //네트워크 스트림에 쓰고
                                        NetStream.Flush();  //보냄

                                        ClearBuffer(SendBuffer);    //버퍼를 초기화 한다
                                    }
                                    f.Dispose();
                                    f.Close();      //파일 읽기가 끝났으므로 파일을 닫아준다.
                                }
                                catch (Exception ex)
                                {
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {//예상되는 무시해도 되는 오류들을 깔끔히 처리하기 위해 catch문을 비워두었다.ex)쓰레드가 중단되었습니다

            }
        }

        private void ServerStop()
        {
            try
            {
                ServerRunning = false;              //서버의 상태를 꺼짐으로 바꿈
                CliConnected = false;               //클라이언트 연결 플래그를 거짓으로 바꿈
                if (server != null)
                    server.Stop();                  //서버를 중지
                if (thr.IsAlive)
                    thr.Abort();                    //Listen하는 쓰레드 중지
                if (f != null)
                    f.Close();                      //파일 닫기
                if (threader.IsAlive)
                    threader.Abort();               //Receive하는 쓰레드 중지
                if (Cli != null)
                    Cli.Close();                    //클라이언트 소켓 닫음
            }
            catch
            {
                //개체 인스턴스로 설정되지 않았다는 예외 처리-->아무행동도 하지 않음
            }
        }

        private void ClearTextBox()            //서버가 닫힐 때 IP와 Port란을 비워주는 함수
        {
            this.IPTextBox.Text = "";
            this.PortTextBox.Text = "";
        }
        private void ClearBuffer(byte[] array)  //버퍼를 초기화해주는 함수
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = 0;
        }

        private void ServerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ServerStop();   //서버 폼이 닫히면 닫히지 않은 것들 닫아줌
        }
    }
}
