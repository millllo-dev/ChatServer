using System.Net.Sockets;

namespace TcpChatServer;

// IDisposable
// - 관리되지 않는 리소스를 명시적으로 해제시키기 위한 인터페이스
// - C#의 GC는 메모리만 자동적으로 해제 -> 그 외 리소스(File Read/Write, 네트워크 연결 정보 등)은 GC가 해제해주지 않는다
// - 위와 같은 이유로 개발자가 직접 해제해야 하는데, 그 작업을 명시적으로 처리하기 위한 것이 IDisposable 인터페이스.
public class ConnectedClient : IDisposable
{
    // TCP 클라이언트
    private readonly TcpClient _client;
    
    // 네트워크 스트림
    // -  각 노드 간(클라이언트-서버) 통신할 수 있는 하나의 통로
    private readonly NetworkStream _stream;
    
    // 클라이언트 ID
    private readonly string _clientId;
    
    // 연결 종료 ID
    private bool _isDisposed;
    
    // 클라이언트 ID 프로퍼티
    public string ClientId => _clientId;
    // 클라이언트 연결 여부 프로퍼티
    public bool IsConnected => !_isDisposed && _client.Connected;
    
    // 생성자
    public ConnectedClient(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        // remote endpoint IP + port number로 clientID 설정 (null일 경우 난수로 설정)
        _clientId = _client.Client.RemoteEndPoint?.ToString() ?? Guid.NewGuid().ToString();
        _isDisposed = false;
        
        Console.WriteLine($"[연결] 클라이언트가 연결되었습니다 : {_clientId}");
    }

    // 해제할 리소스를 명시
    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        
        // Close() 메서드 내부적으로 dispose 메서드를 자동으로 호출
        // _stream.Close();
        _stream.Dispose();
        _client.Dispose();
    }
}