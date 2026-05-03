namespace TcpChatClient;

class Program
{
    static async Task Main(string[] args)
    {
        // Windows 콘솔에서 UTF-8 인코딩 설정
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;
            
        // 서버 IP 및 포트 번호 설정
        const string SERVER_IP = "127.0.0.1";
        const int SERVER_PORT = 7777;
        
        Console.WriteLine("===== TCP 채팅 클라이언트 =====");
        
        // ChatClient 인스턴스 생성
        // using 키워드
        // - 코드 블럭 안에서 사용하는 using은 제공된 리소스를 사용해서
        // 로직 처리를 하고, 처리를 다 하고 해당 블록을 빠져나가면,
        // 자동적으로 release된다
        using var client = new ChatClient(SERVER_IP, SERVER_PORT);

        try
        {
            // 서버 연결
            await client.ConnectAsync();
            
            // 메세지 수신 시작 (백그라운드)
            _ = Task.Run(() => client.ReceiveMessageAsync());

            // client.ReceiveMessageAsync();
            
            // 사용자 입력 루프
            Console.WriteLine("메세지를 입력하세요. (종료: exit)");
            Console.WriteLine("=============================");

            while (client.IsConnected)
            {
                // 사용자 입력 받기
                string? input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input)) continue;

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("채팅을 종료합니다...");
                    break;
                }
                
                // 서버에 메세지 전송
                await client.SendMessageAsync(input);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine("[오류]" + e.Message);
        }
    }
}