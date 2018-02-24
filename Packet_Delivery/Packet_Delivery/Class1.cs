using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Packet_Delivery
{
    [Serializable]
    public class Packet
    {
        public int Length;
        public int Type;

        public Packet()
        {
            this.Length = 0;
            this.Type = 0;
        }

        public static byte[] Serialize(Object o)
        {
            MemoryStream ms = new MemoryStream(1024 * 4);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, o);
            return ms.ToArray();
        }

        public static Object Deserialize(byte[] bt)
        {
            MemoryStream ms = new MemoryStream(1024 * 2);
            foreach (byte b in bt)
            {
                ms.WriteByte(b);
            }

            ms.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            Object obj = bf.Deserialize(ms);
            ms.Close();
            return obj;
        }
    }

    [Serializable]
    public class FilePacket : Packet //패킷타입 0 -> 보통 파일 전송
    {
        public long FileSize;   //파일의 크기
        public string FileName; //파일의 이름
        public byte[] FileData = new byte[1024 * 2];    //파일 데이터를 담을 버퍼

        public FilePacket()    //클래스의 타입을 초기화 해주는 생성자
        {
            Type = 0;
        }
    }

    [Serializable]
    public class FileList : Packet //패킷타입 1 -> 리스트뷰에 파일 목록 전송할 때
    {
        public long FileSize;   //파일의 크기
        public string FileName; //파일의 이름      이 두 개로 리스트뷰 추가 가능

        public FileList()  //클래스의 타입을 초기화 해주는 생성자
        {
            Type = 1;
        }
    }

    [Serializable]
    public class Request : Packet //패킷타입 2 -> 리스트뷰를 더블클릭 했을 때 파일을 요청함
    {
        public string FileName; //파일 이름

        public Request()   //클래스의 타입을 초기화 해주는 생성자
        {
            Type = 2;
        }
    }
}
