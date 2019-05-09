using System;
using System.Net.Sockets;

namespace ff
{
    class FFAcceptor
    {
        protected Socket              m_oSocket;
        protected ISocketCtrl         m_oSocketCtrl;
        public FFAcceptor(ISocketCtrl ctrl)
        {
            m_oSocket   = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_oSocketCtrl = ctrl;
        }
        public bool Listen(string ip, int port){
            try{
                if (ip == "*" || ip == ""){
                    m_oSocket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, port));
                }
                else{
                    m_oSocket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), port));
                }
                m_oSocket.Listen(2);
                m_oSocket.BeginAccept(new AsyncCallback(HandleAccepted), m_oSocket);
            }
            catch (Exception ex)
            {
                FFLog.Trace("scoket: listen Error " + ex.Message);
                return false;
            }
            return true;
        }
        public void HandleAccepted(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;

                //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.endreceive.aspx
                if (socket != null)
                {
                    var client = socket.EndAccept(ar);
                    IFFSocket ffsocket = new FFScoketAsync(m_oSocketCtrl.ForkSelf(), client);
                    ffsocket.AsyncRecv();
                    m_oSocket.BeginAccept(new AsyncCallback(HandleAccepted), m_oSocket);
                    FFLog.Trace(string.Format("scoket: handleAccepted ip:{0}", ffsocket.GetIP()));
                }
            }
            catch (Exception ex)
            {
                FFLog.Trace("scoket: handleAccepted Error " + ex.Message);
            }

        }
        public void Close(){
            try {
                m_oSocket.Close();
            }
            catch (Exception ex)
            {
                FFLog.Trace("scoket: close Error " + ex.Message);
            }
        }
    }
}
