using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static WpfApp5.MainWindow;

namespace WpfApp5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public class GameSchedule
        {
            /// <summary>
            /// 게임 상태
            /// </summary>
            public enum GameState
            {
                None = 0,
                InitGame,               // 게임 초기 세팅
                StartBetting,           // 베팅 시작
                StartBettingSchedule,   // 베팅 시작 스케줄러
                PassTheCard,            // 카드 받기
                PassTheCardSchedule,    // 카드 받기 스케줄러
                PlayerTime,             // 플레이어 타임
                PlayerTimeSchedule,     // 플레이어 타임 스케줄러
                DealerTime,             // 딜러 타임
                DealerTimeSchedule,     // 딜러 타임 스케줄러
                Result,                 // 결과

            
            
            }

            private GameState _prevGameState = GameState.None;  // 이전 게임 상태
            private GameState _currentGameState;                // 현재 게임 상태
            public GameState CurrentGameState 
            { 
                get => _currentGameState;
                set
                {
                    _prevGameState = _currentGameState;
                    _currentGameState = value;
                }
            }

            private Dictionary<GameState, Action> stateHandleList { get; init; } // 상태 핸들 함수 배열

            private bool _isRunning = true;                                      // 쓰레드 bool
            private const float _tick = 0.5f;                                    // 쓰레드 틱


            public GameSchedule()
            {
                CurrentGameState = GameState.None;


                stateHandleList = new Dictionary<GameState, Action>()
                {
                    {GameState.InitGame, InitGame},
                    {GameState.StartBetting, StartBetting},
                    {GameState.StartBettingSchedule, StartBettingSchedule},
                    {GameState.PassTheCard, PassTheCard},
                    {GameState.PassTheCardSchedule, PassTheCardSchedule},
                    {GameState.PlayerTime, PlayerTime},
                    {GameState.PlayerTimeSchedule, PlayerTimeSchedule},
                    {GameState.DealerTime, DealerTime},
                    {GameState.DealerTimeSchedule, DealerTimeSchedule},
                    {GameState.Result, Result},
                };
            }

            /// <summary>
            /// 쓰레드 시작
            /// </summary>
            public async void Run()
            {
                while (_isRunning)
                {
                    if(_prevGameState != _currentGameState)
                        Log($"GameState: {CurrentGameState.ToString()}", Brushes.LightBlue);
                    try { 
                        if(stateHandleList.ContainsKey(CurrentGameState)) 
                            stateHandleList[CurrentGameState](); 
                    } catch(Exception e) { 
                        MessageBox.Show(e.Message); 
                    }
                    
                    await Task.Delay( (int)(_tick * 1000) );
                }
            }

            /// <summary>
            /// 플레이 시작
            /// </summary>
            public virtual void StartPlay()
            {
                _isRunning = true;
                CurrentGameState = GameState.InitGame;

                Task.Run(Run);
            }


            /// <summary>
            /// 플레이 종료
            /// </summary>
            public virtual void StopPlay()
            {
                _isRunning = false;
                CurrentGameState = GameState.None;
            }

            public virtual void InitGame()
            {

                CurrentGameState = GameState.StartBetting;
            }
            public virtual void StartBetting()
            {
                CurrentGameState = GameState.StartBettingSchedule;
            }
            public virtual void StartBettingSchedule()
            {

            }
            public virtual void PassTheCard()
            {
                CurrentGameState = GameState.PassTheCardSchedule;
            }
            public virtual void PassTheCardSchedule()
            {

            }
            public virtual void PlayerTime()
            {
                CurrentGameState = GameState.PlayerTimeSchedule;
            }
            public virtual void PlayerTimeSchedule()
            {

            }
            public virtual void DealerTime()
            {
                CurrentGameState = GameState.DealerTimeSchedule;
            }
            public virtual void DealerTimeSchedule()
            {

            }
            public virtual void Result()
            {

            }

            
            
        }


        public class Card
        {
            public enum SymbolType
            { 
                Clover = 0,
                Heart,
                Diamond,
                Spade
            }

            public SymbolType symbolType { get; init; }
            public int cardNum { get; init; }

            public Card(SymbolType symbolType, int cardNum)
                => (this.symbolType, this.cardNum) = (symbolType, cardNum);
        }



        public class CardGetter
        {
            /// <summary>
            /// 받는자 타입
            /// </summary>
            public enum GetterType
            { 
                Empty = 0,
                Dealer,
                Player
            }

            public GetterType getterType { get; set; }
            public List<Card> cards { get; set; } = new List<Card>();

            /// <summary>
            /// 스코어가 17보다 낮으면 True 리턴
            /// </summary>
            /// <param name="score"></param>
            /// <returns></returns>
            public bool JudgeByAI(int score)
            {
                return score < 17;
            }

            public CardGetter(GetterType getterType)
                => (this.getterType) = (getterType);
        }

        public class Dealer : CardGetter
        {
            public Queue<Card> shuffledDeck { get; set; } = new Queue<Card>();

            public Dealer() : base(GetterType.Dealer)
            {
                ShuffleDeck();
            }

            /// <summary>
            /// 덱 섞기 
            /// </summary>
            public void ShuffleDeck()
            {
                shuffledDeck.Clear();
                CreateDeck().OrderBy(a => Guid.NewGuid()).ToList().ForEach(i => shuffledDeck.Enqueue(i));
            }

            /// <summary>
            /// 카드 덱 생성
            /// </summary>
            /// <returns></returns>
            public List<Card> CreateDeck()
            {
                List<Card> cardList = new List<Card>();
                const int maxNumber = 13;

                foreach(var cardType in Enum.GetValues(typeof(Card.SymbolType)))
                {
                    for(int i=1; i<=maxNumber; i++)
                    {
                        cardList.Add(new Card(symbolType: (Card.SymbolType)cardType, i));
                    }
                }

                return cardList;
            }
        }

        public class Player : CardGetter
        {
            /// <summary>
            /// 관찰 타입
            /// </summary>
            public enum ObserveType
            { 
                None = 0,
                Ready,
                InGame,
                Observe,
            }
            public ObserveType observeType { get; set; } = ObserveType.None;

            /// <summary>
            /// 플레이어 타입
            /// </summary>
            public enum PlayerType
            { 
                Empty = 0,
                Me,
                Others
            }
            public PlayerType playerType { get; set; } = PlayerType.Empty;


            public PlayerGestureManager playerGestureManager { get; set; } = new PlayerGestureManager();

            public int orderNum { get; init; }        // 자리 순서
            public int currentBetting { get; set; }   // 현재 베팅
            public int currentChip { get; set; }      // 보유 칩

            public Player(PlayerType playerType, int orderNum) : base(GetterType.Player)
                => (this.playerType, this.orderNum) = (playerType, orderNum);

        }

        public class PlayerGestureManager
        {
            /// <summary>
            /// 플레이어 제스처 타입
            /// </summary>
            public enum GestureType
            {
                Stand,
                Hit,
                Split,
                DoubleDown
            }
            public Dictionary<GestureType, Button> gestureButtons { get; set; } = new Dictionary<GestureType, Button>();

            /// <summary>
            /// 버튼 활성화
            /// </summary>
            public void EnableButtons() => gestureButtons.Values.ToList().ForEach(btn => btn.IsEnabled = true);

            /// <summary>
            /// 버튼 비활성화
            /// </summary>
            public void DisableButtons() => gestureButtons.Values.ToList().ForEach(btn => btn.IsEnabled = false);
        }


        public class OrderManager : GameSchedule
        {
            public const int MIN_COST = 100;    // 최소 베팅
            public const int MAX_COST = 10000;  // 최대 베팅

            public Dealer dealer { get; set; } = new Dealer();                  // 딜러
            public List<Player> players { get; set; } = new List<Player>();     // 플레이어들
            public List<Player> gamePlayers { get; set; } = new List<Player>(); // 게임에 참여한 플레이어들

            public OrderManager() : base()
            {
                
            }


            /// <summary>
            /// 카드 스코어 계산
            /// </summary>
            /// <param name="cards"></param>
            /// <returns></returns>
            public int ComputeCardScore(List<Card> cards)
            {
                if(cards.Count <= 0)
                    return 0;

                int sum = 0;
                int aceCount = cards.Count - cards.Where(card => card.cardNum != 1).ToList().Count;
                
                cards.Select((card, index) => (card, index))
                 .OrderByDescending(item => item.card.cardNum)
                 .ToList()
                 .ForEach(item =>
                 {
                     int cardNum = item.card.cardNum;
                     int index = item.index;
                     sum += sum + (aceCount - index) * 11 <= 21 && cardNum == 1 ? 11 : cardNum >= 10 ? 10 : cardNum == 1 ? 1 : cardNum;
                 });
                return sum;
            }

            /// <summary>
            /// 게임 시작
            /// </summary>
            public override void StartPlay()
            {
                base.StartPlay();
            }


            /// <summary>
            /// 게임 종료
            /// </summary>
            public override void StopPlay()
            {
                base.StopPlay();
            }


            /// <summary>
            ///  상태 : 게임초기화
            /// </summary>
            /// <exception cref="Exception"></exception>
            public override void InitGame()
            {
                gamePlayers.Clear(); // 참여 플레이어 초기화

                // 참여할 플레이어가 없을 경우
                if (players.Count == 0)
                {
                    StopPlay();
                    throw new Exception("플레이어 없음");
                }

                gamePlayers = players.Where(p => p.currentChip > MIN_COST && p.orderNum > 0).ToList(); // 게임에 참여할 플레이어 추가
                gamePlayers.ForEach(p => { p.observeType = Player.ObserveType.Ready; });               // 참여한 플레이어들 레디로 상태 변경
                
                base.InitGame();
            }

            /// <summary>
            /// 상태 : 베팅 시작
            /// </summary>
            /// <exception cref="Exception"></exception>
            public override void StartBetting()
            {
                // 준비된 플레이어가 없을 경우
                if(gamePlayers.Count != 0 && !gamePlayers.Any(p=>p.observeType == Player.ObserveType.Ready))
                {
                    StopPlay();
                    throw new Exception("준비된 플레이어 없음");
                }


                
                gamePlayers.ForEach((p) => { p.currentBetting = MIN_COST; }); // 모든 유저 최소베팅 (임시코드)


                base.StartBetting();
            }

            /// <summary>
            /// 상태 : 베팅 시작 스케줄러
            /// </summary>
            public override void StartBettingSchedule()
            {
                // 모든 참여플레이어가 베팅을 했을 경우 카드 받기 상태로 
                if(gamePlayers.All(p => p.currentBetting != 0))
                {
                    CurrentGameState = GameState.PassTheCard;
                }
                base.StartBettingSchedule();
            }

            /// <summary>
            /// 상태 : 카드 받기
            /// </summary>
            public override void PassTheCard()
            {
                // 카드 두장 받기
                for(int i=0; i<2; i++)
                {
                    gamePlayers.OrderBy(p => p.orderNum).ToList().ForEach(p => { 
                        p.cards.Add(dealer.shuffledDeck.Dequeue());
                        Log($"Player{p.orderNum}: {ComputeCardScore(p.cards)}, CardCount: {p.cards.Count}");
                    });
                    dealer.cards.Add(dealer.shuffledDeck.Dequeue());
                }
                base.PassTheCard();
            }

            /// <summary>
            /// 상태 : 카드 받기 스케줄러
            /// </summary>
            public override void PassTheCardSchedule()
            {
                // 모든 참여 플레이어와 딜러가 카드 두장씩 받았을 경우 플레이어 타임으로 상태 변경
                if(gamePlayers.All(p => p.cards.Count == 2) && dealer.cards.Count == 2)
                {
                    CurrentGameState = GameState.PlayerTime;
                }
                base.PassTheCardSchedule();
            }

            /// <summary>
            /// 상태 : 플레이어 타임
            /// </summary>
            public override void PlayerTime()
            {
                base.PlayerTime();
            }

            /// <summary>
            /// 상태 : 플레이어 타임 스케줄러
            /// </summary>
            public override void PlayerTimeSchedule()
            {


                // 다른 플레이어 자동 카드 받기 
                gamePlayers.Where(p => p.playerType == Player.PlayerType.Others).ToList().ForEach(p => { 
                    if(p.JudgeByAI(ComputeCardScore(p.cards)))
                    {
                        p.cards.Add(dealer.shuffledDeck.Dequeue());
                        Log($"Player{p.orderNum}: {ComputeCardScore(p.cards)}, CardCount: {p.cards.Count}");
                    }

                });

                // 모든 플레이어가 Stand 상태일 경우 처리 
                if (gamePlayers.All(p => !p.JudgeByAI(ComputeCardScore(p.cards))))
                    CurrentGameState = GameState.DealerTime;

                base.PlayerTimeSchedule();
            }

            /// <summary>
            /// 상태 : 딜러 타임
            /// </summary>
            public override void DealerTime()
            {
                base.DealerTime();
            }

            /// <summary>
            /// 상태 : 딜러 타임 스케줄러
            /// </summary>
            public override void DealerTimeSchedule()
            {
                // 17 넘지 않을 경우 카드 추가
                if (dealer.JudgeByAI(ComputeCardScore(dealer.cards)))
                    dealer.cards.Add(dealer.shuffledDeck.Dequeue());

                // 17 넘었을 경우 결과로 상태 변경
                else
                    CurrentGameState = GameState.Result;


                Log($"Dealer: {ComputeCardScore(dealer.cards)}, CardCount: {dealer.cards.Count}");

                base.DealerTimeSchedule();
            }

            /// <summary>
            /// 상태 : 결과
            /// </summary>
            public override void Result()
            {
                Log("=================Result=================", Brushes.Red);
                int dealerScore = ComputeCardScore(dealer.cards);

                gamePlayers.ForEach(p =>
                {
                    int playerScore = ComputeCardScore(p.cards);
                    string result = (dealerScore < playerScore && playerScore <= 21) || (dealerScore > 21 && playerScore <= 21) ? "Win" : "Lose";
                    Log($"Player{p.orderNum}: {playerScore}, CardCount: {p.cards.Count}, Result: {result}");
                });

                Log($"Dealer: {dealerScore}, CardCount: {dealer.cards.Count}");
                
                StopPlay();

                base.Result();
            }


        }
        public OrderManager orderManager = new OrderManager();

        private readonly HttpClient _client;

        public MainWindow()
        {
            InitializeComponent();

            orderManager.players.Add(new Player(playerType: Player.PlayerType.Me, orderNum: 1) { currentChip = 30000 });
            orderManager.players.Add(new Player(playerType: Player.PlayerType.Others, orderNum: 2) { currentChip = 30000 });
            orderManager.players.Add(new Player(playerType: Player.PlayerType.Others, orderNum: 3) { currentChip = 30000 });

            Player? me = orderManager.players.Where(p => p.playerType == Player.PlayerType.Me).FirstOrDefault();
            if(me is not null)
            {
                me.playerGestureManager.gestureButtons = new Dictionary<PlayerGestureManager.GestureType, Button>()
                {
                    { PlayerGestureManager.GestureType.Hit, HitButton },
                    { PlayerGestureManager.GestureType.Stand, StandButton },
                    { PlayerGestureManager.GestureType.Split, SplitButton },
                    { PlayerGestureManager.GestureType.DoubleDown, DoubleDownButton },
                };

                orderManager.StartPlay();
            }

            


        }


        private async Task<string> GetHttpResponseAsync(string url)
        {
            HttpResponseMessage response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }


        private static string prevLogMsg = string.Empty;
        private static void Log(string message, Brush color = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string msg = message + "\r\n";
                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                TextRange tr = new TextRange(mainWindow.LogBox.Document.ContentEnd, mainWindow.LogBox.Document.ContentEnd);
                if(!msg.Equals(prevLogMsg))
                    tr.Text = msg;

                // 중복 체크
                if (msg.Contains("GameState"))
                    prevLogMsg = msg;


                // Set the color of the text
                if (color != null)
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            });
        }
    }
}
