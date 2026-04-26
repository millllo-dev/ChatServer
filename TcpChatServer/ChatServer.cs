using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace TcpChatServer;

public class ChatServer
{
    // TCP 연결을 기다리는(수신하는) 리스너
    private TcpListener? _listener;
    
    // 서버가 실행 중인지 여부 (Whether the server is running)
    private bool _isRunning;
    
    // 서버 포트 (Server port)
    private readonly int _port;
    
    // 연결된 클라이언트 목록을 저장하기 위한 컬렉션
    // - Thread safe한 Dictionary
    private readonly ConcurrentDictionary<string, ConnectedClient> _clients;
    
    // 생성자
    public ChatServer(int port)
    {
        _port = port;
        // TCP Listener를 가동시킨 후 true로 변경 예정
        _isRunning = false;
        _clients = new ConcurrentDictionary<string, ConnectedClient>();
    }
    
    // 서버 시작
    public void Start()
    {
        // TCP Listener가 가동중인지 먼저 체크
        if (_isRunning)
        {
            Console.WriteLine("서버가 이미 실행 중입니다.");
            return;
        }
        
        // TCP 리스너 초기화 (Init TCPListener)
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        _isRunning = true;
        
        Console.WriteLine($"서버가 포트 {_port}에서 시작되었습니다.");
        
        // 백그라운드에서 클라이언트 연결 수락 시작
        _ = Task.Run(AcceptClientAsync);
    }
    
    // 클라이언트 연결 (비동기)
    // - 비동기 메서드의 경우 접미사로 Async를 붙여주는 것이 관례
    private async Task AcceptClientAsync()
    {
        Console.WriteLine("클라이언트 연결을 기다리는 중...");
        while (_isRunning)
        {
            try
            {
                // 클라이언트 연결 수락
                // - 외부에서 접속을 시도하는 클라이언트 연결을 기다리겠다
                // - await 키워드로 Non-blocking 방식으로 접속 대기
                var client = await _listener!.AcceptTcpClientAsync();
                
                // 접속한 클라이언트를 저장
                var connectedClient = new ConnectedClient(client);
                // 클라이언트 아이디 생성
                var clientId = client.Client.RemoteEndPoint.ToString() ?? Guid.NewGuid().ToString();
                // _clients.TryAdd(clientId, connectedClient); // true/false를 반환한다
                _clients[clientId] = connectedClient;
                
                // 연결된 클라이언트 정보 출력
                var endPoint = client.Client.RemoteEndPoint;
                
                Console.WriteLine($"[정보] 현재 연결된 클라이언트 수 : {_clients.Count}");
                
                // 클라이언트로부터 메시지 수신 시작(비동기)
                _ = Task.Run(() => connectedClient.ReceiveMessageAsync());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    public void Stop()
    {
        if (!_isRunning)
        {
            Console.WriteLine("서버가 이미 종료되었습니다.");
            return;
        }

        _isRunning = false;
        _listener?.Stop();
        
        Console.WriteLine("서버가 중지되었습니다.");
    }
}