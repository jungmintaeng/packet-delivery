using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Net.Sockets;
using Packet_Delivery;
using System.Threading;


namespace Client
{
    public partial class ClientForm : Form
    {
        byte[] SendBuffer = new byte[1024 * 4]; //서버에 보낼 때 쓰는 버퍼
        byte[] ReadBuffer = new byte[1024 * 4]; //서버에서 받아올 때 쓰는 버퍼
        bool ConnectedToServer = false;         //현재 서버에 연결된 상태인지 나타내는 bool형 변수
        private NetworkStream NetStream;        //네트워크 스트림
        private TcpClient Cli;                  //클라이언트측이므로 클라이언트 생성
        private FilePacket Fp;                  //
        private FileList Fl;                    //
        private Request Rq;                     //Fp, Fl, Rq 타입에 따라서 Serializable class 선언
        private FileStream f;                   //파일을 읽어올 (Receive) 파일스트림
        private FileStream buttonstream;        //Send 버튼을 눌렀을 때 사용할 파일스트림
        private Thread threader;                //Receive에 사용하는 쓰레드
        private Thread thrsender;               //Send에 사용하는 쓰레드

        public ClientForm()
        {
            InitializeComponent();
        }

        private void ClientForm_Load(object sender, EventArgs e) //ClientForm이 로드될 때
        {
            try
            {
                thrsender = new Thread(new ThreadStart(Send)); //Send를 한번도 실행하지 않아도 
                //ListView 더블클릭 동작에서 thrsender가 널이 되지 않기 위해 선언하였다.(개체 인스턴스 예외 방지)
                folderBrowserDialog.SelectedPath = @"C:\\"; //다운로드 경로의 Default를 설정해 준다
                PathTextBox.Text = folderBrowserDialog.SelectedPath; //TextBox에 Default값 출력
                for (int i = 0; i < SendBuffer.Length; i++)
                    SendBuffer[i] = 0;                          //SendBuffer 초기화
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FilePathBtn_Click(object sender, EventArgs e)  //File Path 버튼을 클릭했을 때의 동작
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)     //folderBrowserDialog를 열어 Path를 제대로 받아왔다면
                PathTextBox.Text = folderBrowserDialog.SelectedPath;    //Path TextBox에 Path를 출력한다
        }

        private void SelectFileBtn_Click(object sender, EventArgs e) //Select file 버튼을 클릭했을 때의 동작
        {
            try
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK) //openFileDialog로 파일을 얻어왔다면
                {
                    FileTextBox.Text = openFileDialog.FileName; //텍스트박스에 파일의 경로를 출력하고
                    buttonstream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                    //openfileDialog로 선택한 파일을 미리 오픈한다.
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);    //오류가 발생할 경우 메시지를 출력한다.
            }
        }

        private void Send() //Send 버튼을 눌렀을 때 thrsend 쓰레드 안에서 돌아갈 함수
        {
            try
            {
                while (ConnectedToServer)   //서버에 연결되어 있을 때
                {
                    buttonstream.Position = 0; //파일의 인덱스를 맨앞으로 이동시키고

                    FileInfo fi = new FileInfo(openFileDialog.FileName);

                    Fp = new FilePacket();

                    Fp.FileName = fi.Name;  //파일의 경로를 생각한 짧은 경로를 얻어오기위해 FileInfo 사용
                    Fp.FileSize = fi.Length; //파일의 길이를 FileSize로 입력

                    Invoke(new MethodInvoker(delegate () //Control을 건드리는 부분 --> Invoke 사용
                    {
                        ProgressBar.Minimum = 0; // 초기값 0
                        ProgressBar.Maximum = (int)(buttonstream.Length / Fp.FileData.Length) + 1; //보내야 하는 횟수
                        ProgressBar.Value = 0; // 현재 값 0
                    }
));
                    for (int i = 0; i < (int)(buttonstream.Length / Fp.FileData.Length) + 1; i++)
                    {//파일의 크기를 버퍼의 크기로 나누고 나머지부분 전송을 위해 + 1번만큼 더 전송함
                        buttonstream.Read(Fp.FileData, 0, Fp.FileData.Length);  //파일을 버퍼에 읽어와서
                        Fp.Type = 0; //보통의 파일 전송 타입
                        Packet.Serialize(Fp).CopyTo(this.SendBuffer, 0); //SendBuffer로 Serialize
                        NetStream.Write(SendBuffer, 0, SendBuffer.Length); //네트워크 스트림에 쓴다
                        NetStream.Flush(); //보낸다

                        ClearBuffer(SendBuffer);    //SendBuffer의 모든 index를 0으로 초기화

                        Invoke(new MethodInvoker(delegate () //Control을 건드리는 부분
                        {
                            ProgressBar.Value++;    //Value를 증가시킴(Progressbar 수치 증가)
                        }
    ));
                    }
                    //파일 전송 끝

                    buttonstream.Dispose();
                    buttonstream.Close();       //
                    FileTextBox.Clear();        //파일스트림을 닫아주고, 파일경로 텍스트박스 초기화

                    Invoke(new MethodInvoker(delegate ()//컨트롤을 건드리는 부분
                    {
                        ListViewItem item = new ListViewItem(new string[] { fi.Name, fi.Length.ToString() });
                        listView.Items.Add(item);           //전송을 완료하면 서버의 경로에 이 파일도 저장되므로
                    }
                        ));

                    threader = new Thread(new ThreadStart(Receive)); //Abort되었던 쓰레드를 다시 생성해줌
                    threader.Start();   //시작
                    thrsender.Abort();  //전송 쓰레드 Abort
                }
            }
            catch
            {//쓰레드가 중단되었습니다. 같은 무시할 수 있는 오류를 깔끔히 처리하기 위해서 비워둠
            }
        }
        private void SendBtn_Click(object sender, EventArgs e) //Send 버튼을 눌렀을 때의 행동
        {
            if (!ConnectedToServer) //서버에 연결되지 않은 상태로 Send버튼을 눌렀다면
            {
                MessageBox.Show("Please connect to server first."); //메시지를 출력하고
                return; //리턴한다
            }
            else
            {
                threader.Abort();   //혹시라도 처리안되게 더블클릭시 Receive되는 것을 막기위해
                thrsender = new Thread(new ThreadStart(Send)); //전송 쓰레드 생성
                thrsender.Start();  //시작
            }
        }

        private void ClearBuffer(byte[] array)  //버퍼의 값을 모두 0으로 초기화
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = 0;
        }

        private void Receive()
        {
            try
            {
                int count = 1;  //현재 파일의 인덱스
                int nRead = 0;  //총 전송 횟수

                while (ConnectedToServer)
                {  
                    this.NetStream.Read(this.ReadBuffer, 0, this.ReadBuffer.Length); //읽어오고
                    Packet p = (Packet)Packet.Deserialize(this.ReadBuffer); //해독해서

                    switch (p.Type) //타입에 따라 나눈다
                    {
                        case 0: //보통의 파일 전송일 때
                            {
                                Fp = (FilePacket)p; //파일을 보내기 위한 클래스로 다운캐스팅
                                nRead = (int)Fp.FileSize / Fp.FileData.Length + 1; //파일을 받아야 하는 횟수
                                int fileIndex = 1;  //파일 중복 시 앞에 번호를 붙여주기 위한 인덱스
                                string fileName = PathTextBox.Text + "\\" + Fp.FileName;

                                if (count == 1) //파일의 첫부분을 받았다면
                                {
                                    while (File.Exists(fileName)) //파일이 있는지 확인하고
                                    {
                                        fileName =  PathTextBox.Text + "\\" + "(" + fileIndex.ToString() + ")" + Fp.FileName;
                                        fileIndex++; //앞에 번호를 붙여 파일 이름이 겹치지 않도록 해준다
                                    }
                                    f = new FileStream(fileName , FileMode.Create, FileAccess.Write);
                                    //중복되지 않는 파일 이름을 얻은 후 파일스트림을 생성한다(생성으로)
                                }

                               
                                f.Write(Fp.FileData, 0, Fp.FileData.Length);

                                this.Invoke(new MethodInvoker(delegate ()//컨트롤 건드리는 부분
                                {
                                    this.ProgressBar.Maximum = nRead;   //최대전송횟수로 초기화
                                    this.ProgressBar.Minimum = 0;       //최소는 0
                                    this.ProgressBar.Value = count;     //count는 현재 몇번 받았는지 나타냄
                                    count++;
                                }
                                ));

                                if (count > nRead)  //count==nRead일 때 파일을 모두 받음
                                                    //따라서 count++을 한 직후 count>nRead 라면 파일을 다 받았으므로
                                {
                                    count = 1;      //count를 다시 1로 초기화하고
                                    f.Dispose();
                                    f.Close();      //파일의 인덱스는 파일이 닫지 않으면 유지되기 때문에
                                                    //파일의 전송을 모두 끝낸 시점에서 닫아준다.
                                }
                                break;
                            }
                        case 1: //리스트뷰에 파일을 띄우기 위한 Type일 때
                            {
                                Fl = (FileList)p;   //리스트 뷰에 파일을 띄우기 위한 클래스로 다운캐스팅
                                this.Invoke(new MethodInvoker(delegate ()//컨트롤을 건드리는 부분
                                {
                                    ListViewItem item = new ListViewItem(new string[] { Fl.FileName, Fl.FileSize.ToString()});
                                    listView.Items.Add(item);   //아이템을 파일이름, 사이즈로 생성 후 Add
                                }));
                                break;
                            }
                        case 2: //파일을 달라는 request일 때-->클라이언트에서는 받을 일 없음
                            {
                                break;
                            }
                    }
                }
            }
            /*
            catch (SocketException)                 //처음에 서버 연결이 끊겼을 때 자동으로 Disconnect 버튼이
            {                                       //Connect 버튼으로 바뀌도록 하기 위해 만든 코드였는데,
                Cli = null;                         //서버와 연결이 끊길 때 발생하는 Exception이
                NetStream = null;                   //SocketException 아니라 밑의 IOException이라는 것을 알고 주석처리하였다.
                if (f != null)
                    f.Close();
                ConnectedToServer = false;

                Invoke(new MethodInvoker(delegate ()
                {
                    ConnectBtn.Text = "Connect";
                    ConnectBtn.ForeColor = Color.Black;
                    listView.Items.Clear();
                }));

                threader.Abort();
            }*/
            catch(System.IO.IOException)        //서버와의 연결이 끊겼을 경우 or IO 예외가 발생했을 경우
            {
                Cli = null;
                NetStream = null;               //넷스트림과 클라이언트를 널로만듬
                if (f != null)
                    f.Close();                  //파일스트림이 널이 아니라면 닫고

                ConnectedToServer = false;      //서버와 연결상태를 거짓으로 설정

                Invoke(new MethodInvoker(delegate ()//컨트롤을 건드리는 부분
                {
                    ConnectBtn.Text = "Connect";    //버튼의 텍스트를 Connect로
                    ConnectBtn.ForeColor = Color.Black; //색깔을 검정색으로
                    listView.Items.Clear();         //리스트뷰를 비움
                }));

                threader.Abort();                 //리시브 쓰레드 종료
                thrsender.Abort();                  //보내는 쓰레드 종료
            }
            catch
            {//쓰레드가 중단되었습니다. 같은 무시할 수 있는 오류를 깔끔히 처리하기 위해서 비워둠
            }
        }

        private void ConnectBtn_Click(object sender, EventArgs e)   //Connect 버튼을 눌렀을 때
        {
            try
            {
                if (folderBrowserDialog.SelectedPath == @"C:\\")    //다운로드 경로가 C:\\라면 액세스 거부 당할 수 있으므로
                                                                    //경로를 바꾸라는 메시지를 띄운다
                {
                    MessageBox.Show("No permission to access C:\\\r\nPlease Change the path before Connection");
                    return;
                }
                if (!ConnectedToServer)                             //서버에 연결된 상태가 아닐 때
                {
                    Cli = new TcpClient();                          //새로운 클라이언트를 생성하고

                    try
                    {
                        this.Cli.Connect(this.IPTextBox.Text, int.Parse(this.PortTextBox.Text));
                                                                    //서버에 연결한다
                    }
                    catch (Exception ex)    //예외가 발생하면
                    {
                        MessageBox.Show("Connection ERROR" + ex.Message); //어떤 예외인지 출력하고
                        return; //리턴
                    }
                    NetStream = Cli.GetStream();        //네트워크 스트림 연결

                    this.ConnectedToServer = true;      //서버에 연결되었다고 bool형으로 표시
                    ConnectBtn.Text = "Disconnect";     //버튼을 Disconnect버튼으로 바꾸고
                    ConnectBtn.ForeColor = Color.Red;   //글씨색을 빨간색으로 바꿈

                    threader = new Thread(new ThreadStart(Receive));    //받는 쓰레드 실행
                    threader.Start();   //시작 --> 전송되기를 기다림
                }
                else  //반대(Disconnect버튼을 눌렀을 때)
                {
                    Cli = null;         
                    NetStream = null;   //클라이언트와 넷스트림 null로 만들고
                    if (f != null)
                        f.Close();      //파일스트림을 닫고
                    ConnectedToServer = false;      //서버에 연결되지 않았다고 표시
                    ConnectBtn.Text = "Connect";    //버튼을 Connect로 바꾸고
                    ConnectBtn.ForeColor = Color.Black; //글자색을 검정색으로 바꾼다
                    listView.Items.Clear();         //리스트뷰를 비움
                    threader.Abort();               //receive 중지
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connect ERROR\r\n" + ex.Message);
            }
        }

        private void ClientForm_FormClosed(object sender, FormClosedEventArgs e)    //폼이 닫혔을 때 행동
        {
            try
            {
                if (f != null)
                    f.Close();                      //파일을 닫고
                if (threader != null)
                    threader.Abort();               //받는 쓰레드를 닫고
                if (thrsender != null)
                    thrsender.Abort();              //보내는 중에 꺼졌다면 보내는 쓰레드를 닫음
            }
            catch (NullReferenceException)          //이 예외는 그냥 무시하기 위해서 비워둠
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);        //다른 오류가 발생한다면 출력
            }
        }

        private void listView_DoubleClick(object sender, EventArgs e)   //리스트뷰 아이템을 더블클릭했을 때
        {
            if (thrsender.IsAlive)  //보내는 쓰레드가 살아있다면(보내는중이라면)
            {
                MessageBox.Show("Uploading..............."); //메시지 출력
                return;
            }

            Rq = new Request(); //요청을 보내는 클래스인 Rq 인스턴스 생성
            Rq.Type = 2;        //타입도 요청으로 바꾸고
            ListViewItem item = listView.FocusedItem;
            Rq.FileName = item.SubItems[0].Text;        //선택된 파일의 이름을 클래스에 저장
            Packet.Serialize(Rq).CopyTo(this.SendBuffer, 0);    //Serialize해서 SendBuffer에 저장
            NetStream.Write(SendBuffer, 0, SendBuffer.Length);  //네트워크 스트림에 씀
            NetStream.Flush();  //서버로 요청을 보냄
            ClearBuffer(SendBuffer);    //버퍼를 초기화시킴
        }
    }
}
