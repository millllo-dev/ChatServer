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
    
    // 스트림 - 읽기 전용 (수신)
    private readonly StreamReader _reader;
    
    // 스트림 - 쓰기 전용 (송신)
    private readonly StreamWriter _writer;
    
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
        
        // 스트림 초기화 (UTF-8 인코딩)
        // - 데이터를 주고 받을 때 UTF-8 형식으로 주고받는다는 의미
        // reader stream을 생성하면 내부적으로 스트림 파이프라인
        _reader = new StreamReader(_stream, System.Text.Encoding.UTF8);
        // AutoFlush - Writer Stream에 데이터를 전달하면 바로 전달한다
        _writer = new StreamWriter(_stream, System.Text.Encoding.UTF8) { AutoFlush = true};
        
        _clientId = _client.Client.RemoteEndPoint?.ToString() ?? Guid.NewGuid().ToString();
        _isDisposed = false;
        
        Console.WriteLine($"[연결] 클라이언트가 연결되었습니다 : {_clientId}");
    }
    
    // 비동기 메시지 수신
    public async Task ReceiveMessageAsync()
    {
        try
        {
            while (!_isDisposed && IsConnected)
            {
                // StreamReader에 넘어오는 데이터가 NULL이면 클라이언트가 끊겼다는 의미
                // 읽기 파이프라인에서 한 줄씩 읽는다(비동기)
                string? message = await _reader.ReadLineAsync();
                
                // 연결이 끊어지면 NULL 반환
                if (message == null)
                {
                    Console.WriteLine($"[연결 종료] {_clientId}");
                    break;
                }

                Console.WriteLine($"[수신] {_clientId} : {message}");
            }
        }
        catch (Exception e)
        {
            // 연결이 끊기지 않았을 때만 에러 메세지 출력
            if (!_isDisposed)
            {
                Console.WriteLine(e);
            }
        }
        finally
        {
            Dispose();
        }
    }

    // Dispose 메서드
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