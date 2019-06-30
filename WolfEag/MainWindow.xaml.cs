using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WolfEag
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private double speedSpawnEgg = 0.65; // Скорость появления яйца;
        private double speedMoveEgg = 1.15; // Скорость анимации "переката" яйца по платформе;
        private double speedDownEgg = 0.75; // Скорость анимации "падения" яйца;

        private int points = 0; // Количество очков у игрока;
        private int numberPlatformBasket = 4; // Номер платформы над корзиной;
        private Key[] rememberKey = new Key[2]; // Запоминаем клавиши который были нажаты для перемещения корзины;

        private Random rand = new Random(); // РАНДОММММ ДЖЭННКИИИННССС;

        private List<Egg> eggs = new List<Egg>(); // Список наших яиц на форме;

        private Thread threadCheckHitOnBasket; // Поток для проверки попадания яйца в корзину.

        /// <summary>
        /// Загрузка окна
        /// </summary>        /// <param name="sender">Окно приложения (Window)</param>
        /// <param name="e">Загрузка окна (Loaded)</param>
        private void LoadWindow(object sender, RoutedEventArgs e)
        {
            rememberKey[0] = Key.A; // Запоминаем стартовую комбинацию клавиш;
            rememberKey[1] = Key.S; 

            Basket.Fill = new ImageBrush(new BitmapImage(new Uri("Basket.png", UriKind.Relative))); // Установка внешнего вида корзины;
            player.Fill = new ImageBrush(new BitmapImage(new Uri("positionPlayer4.png", UriKind.Relative))); // Установка внешнего вида игрока;
            
            Grid.SetZIndex(Basket, 2); // Устанавливаем Z - координат для корзины;

            threadCheckHitOnBasket = new Thread(new ThreadStart(CheckHitEggOnBasket)); // Устанавливаем свойства потока для проверки попадания яйца в корзину;
            threadCheckHitOnBasket.Start(); // Запускаем поток;

            Application app = Application.Current; // Создаем объект нашего приложения, для установки к нему делегата закрытие приложения;
            app.Exit += ExitApplication; // Делегат с закрытием приложения
                

            StartGame();
        }

        /// <summary>
        /// Проверяет на попадания в корзину яйца, которые находятся в состоянии "падения"
        /// </summary>
        private void CheckHitEggOnBasket()
        {
            while(true)
            {
                 Dispatcher.BeginInvoke(new ThreadStart(() =>
                 {
                    for (int i = 0; i < eggs.Count; i++)
                    {               
                        // Если есть яйцо на форме, то сравнимаем координаты яйца и корзины;
                        if (GamePanel.Children.Contains(eggs[i].egg))
                        {
                            //Если яйцо попало в корзину;
                            if (CheckHit(eggs[i].egg))
                            {
                                GamePanel.Children.Remove(eggs[i].egg); // Удаление из игры объект "яйцо";
                                points += eggs[i].valueCoin; // Увеличиваем игрока очки в зависимости от количество очков за яйцо;
                                Points.Content = "Очки : " + points; // Отображаем игрока очки в лейбле;
                            }
                        }                 
                    }
                }));
                Thread.Sleep(10); // Усыпляем поток.
            }

        }
       
        /// <summary>
        /// Сравнивает координаты яйца и корзины
        /// </summary>
        /// <param name="egg">Яйцо</param>
        /// <returns>Попало ли яйцо в корзину или нет</returns>
        private bool CheckHit(Ellipse egg)
        {
            Rect coorEgg = new Rect(egg.Margin.Left, egg.Margin.Top,egg.ActualWidth,5); // Установка координат яйца;
            Rect coorBasket = new Rect(Basket.Margin.Left + (Basket.ActualWidth / 2 - 10), Basket.Margin.Top, Basket.ActualWidth / 3 , Basket.ActualHeight); // Установка координат корзины;

            bool hit = coorBasket.IntersectsWith(coorEgg); // Сравнение 2-х координат.

            return hit;
        }

        /// <summary>
        /// Метод начала игры
        /// </summary>
        private void StartGame()
        {
            DispatcherTimer timerSpawnEgg = new DispatcherTimer { Interval = TimeSpan.FromSeconds(speedSpawnEgg) }; // Устанавливаем таймер появления яйца с интервалом speedSpawnEgg;

            //Действие таймера который он будет выполнять с интервалом speedSpawnEgg;
            timerSpawnEgg.Tick += (s, e) =>
                {
                    Egg egg = RandomEgg(); // Получаем случайное яйцо с рандомным цветом;
                    Storyboard storyAnimation = new Storyboard();
                    storyAnimation.Children.Add(CreateAnimationAngleEgg(egg));
                    storyAnimation.Children.Add(CreateAnimationMoveEgg(egg));
                    
                    storyAnimation.Begin();
                    
                   // egg.egg.BeginAnimation(MarginProperty, CreateAnimationMoveEgg(egg)); // Добавление анимации "переката" яйца по платформе;
                    GamePanel.Children.Add(egg.egg); // Добавление яйца на форму;
                };
            timerSpawnEgg.Start(); // Запуск цеха по созданию яиц.
        }

        private DoubleAnimation CreateAnimationAngleEgg(Egg egg)
        {
            DoubleAnimation angle = new DoubleAnimation 
            { 
                Duration = TimeSpan.FromSeconds(speedMoveEgg), 
                To = 360, 
                From = 0 
            };

            Storyboard.SetTarget(angle, Basket);
            Storyboard.SetTargetProperty(angle, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

            
            return angle;
        }


        /// <summary>
        /// Устанавливает свойства анимации "переката"
        /// </summary>        
        /// <param name="egg">Яйцо</param>
        /// <returns>Анимация "переката" яйца по платформе</returns>
        private ThicknessAnimation CreateAnimationMoveEgg(Egg egg)
        {
            ThicknessAnimation anime = new ThicknessAnimation { Duration = TimeSpan.FromSeconds(speedMoveEgg) }; // Анимация "переката" с длиной выполнения speedMoveEgg;

            Thickness[] startAndEndPointMoveEgg = RandomPlatform(rand.Next(1, 5)); // Рандомим платформу по которой будет катиться яйцо;

            anime.From = startAndEndPointMoveEgg[0]; // Устанавливаем начальную точку пути яйца;
            anime.To = startAndEndPointMoveEgg[1]; // Устанавливаем конечную точку пути яйца;

            Storyboard.SetTarget(anime, egg.egg);
            Storyboard.SetTargetProperty(anime, new PropertyPath("Margin"));

            // Завершение анимации "переката" и переход на новую анимацию "падения";
            anime.Completed += (s, e) =>
                {
                    egg.egg.BeginAnimation(MarginProperty, CreateAnimationDownEgg(egg)); // Установка новой анимации "падения";

                    eggs.Add(egg); // Добавление в список.
                };

            return anime;
        }


        /// <summary>
        /// Устанавливает свойства анимаии "падения"
        /// </summary>
        /// <param name="egg">Яйцо</param>
        /// <returns>Анимация "падения" яйца</returns>
        private ThicknessAnimation CreateAnimationDownEgg(Egg egg)
        {
            ThicknessAnimation anime = new ThicknessAnimation // Анимация "падения" с длиной выполнения speedDownEgg;
            {
                Duration = TimeSpan.FromSeconds(speedDownEgg),

                From = egg.egg.Margin, // Установка начальной точки пути яйца;
                To = new Thickness(egg.egg.Margin.Left, this.ActualHeight + egg.egg.ActualHeight, 0, 0) // Установка конечной точки пути яйца;
            };
          
            // Завершение анимации "падения";
            anime.Completed += (s, e) =>
                {
                    eggs.Remove(egg); // Удаляем из списка яйцо, которое было поймано, или упало на землю;

                    // Если яйцо надено на форме после завершения анимации "падения";
                    if(GamePanel.Children.Contains(egg.egg))
                    {
                        MessageBox.Show(Points.Content.ToString()); // Выводим кол-во очков игрока;
                        Close(); // Закрываем приложение;
                    }

                    GamePanel.Children.Remove(egg.egg); // Удаление яйца из формы после завершение анимации "падения".
                };
           
            return anime;
        }

        /// <summary>
        /// Рандомим платформу по которой будет катиться яйцо
        /// </summary>
        /// <param name="platforma">Номер платформы</param>
        /// <returns>Возрат стартовой точки пути яйца и конечной точки</returns>
        private Thickness[] RandomPlatform(int platforma)
        {
            Thickness[] startAndEndPointMoveEgg = new Thickness[2];

            switch(platforma)
            {
                case 1:
                    startAndEndPointMoveEgg[0] = PlatformStart_1.Margin; // Координаты (старта) первой платформы; 
                    startAndEndPointMoveEgg[1] = PlatformEnd_1.Margin; // Координаты (финиша) первой платформы.
                    return startAndEndPointMoveEgg;

                case 2:
                    startAndEndPointMoveEgg[0] = PlatformStart_2.Margin; // Координаты (старта) второй платформы; 
                    startAndEndPointMoveEgg[1] = PlatformEnd_2.Margin; // Координаты (финиша) второй платформы.
                    return startAndEndPointMoveEgg;

                case 3:
                    startAndEndPointMoveEgg[0] = PlatformStart_3.Margin; // Координаты (старта) третьей платформы; 
                    startAndEndPointMoveEgg[1] = PlatformEnd_3.Margin; // Координаты (финиша) третьей платформы.
                    return startAndEndPointMoveEgg;

                case 4:
                    startAndEndPointMoveEgg[0] = PlatformStart_4.Margin; // Координаты (старта) четвертой платформы; 
                    startAndEndPointMoveEgg[1] = PlatformEnd_4.Margin; // Координаты (финиша) четвертой платформы.
                    return startAndEndPointMoveEgg;

                default :
                    return null;
            }
        }

        /// <summary>
        /// Рандомим яйцо
        /// </summary>
        /// <returns>Яйцо</returns>
        private Egg RandomEgg()
        {
            int temp = rand.Next(1, 101); // Получаем случайно сгенирированное число в диапозоне 1-100, для индефикации яйца.

            if (temp < 50)
                return new Egg(1,Brushes.ForestGreen); 
            else if (temp >= 50 && temp < 75)
                return new Egg(3, Brushes.Yellow);
            else if (temp >= 75 && temp < 95)
                return new Egg(5, Brushes.Purple);
            else
                return new Egg(10, new ImageBrush(new BitmapImage(new Uri("Putin_3.png" , UriKind.RelativeOrAbsolute))));

        }

        
        /// <summary>
        /// Передвижение корзины игрока 
        /// </summary>
        /// <param name="sender">Окно приложения (Window)</param>
        /// <param name="e">Нажатие на клавишу (KeyDown)</param>
        private void MoveBasket(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.W:
                    case Key.Up:
                        if (!rememberKey[1].Equals(Key.W))
                        {
                            SettingsBasket(ref Basket, ref numberPlatformBasket, -1);
                            rememberKey[1] = Key.W;
                        }
                        break;

                case Key.A:
                    case Key.Left:
                        if (!rememberKey[0].Equals(Key.A))
                        {
                            SettingsBasket(ref Basket, ref numberPlatformBasket, 2);
                            rememberKey[0] = Key.A;
                        }
                        break;

                 case Key.D:
                    case Key.Right:
                        if (!rememberKey[0].Equals(Key.D))
                        {
                            SettingsBasket(ref Basket, ref numberPlatformBasket, -2);
                            rememberKey[0] = Key.D;
                        }
                        break;

                case Key.S:
                    case Key.Down:
                        if (!rememberKey[1].Equals(Key.S))
                        {
                            SettingsBasket(ref Basket, ref numberPlatformBasket, 1);
                            rememberKey[1] = Key.S;
                        }
                        break;
            }

        }

        /// <summary>
        /// Установка свойства корзины
        /// </summary>
        /// <param name="Basket">Корзина игрока</param>
        /// <param name="numberPlatformBasket">Номер платформы над корзиной</param>
        /// <param name="direction">Определяем направление</param>
        private void SettingsBasket(ref Rectangle Basket, ref int numberPlatformBasket , int direction)
        {
            numberPlatformBasket += direction;
            
            switch(numberPlatformBasket)
            {
                case 1:
                 
                    player.Fill = new ImageBrush(new BitmapImage(new Uri("positionPlayer1.png", UriKind.Relative))); // Установка внешнего вида человечка;
                    player.Margin = new Thickness(183,160,0,0); // Координаты человечка.

                    Basket.Margin = new Thickness(PlatformEnd_1.Margin.Left + 10, PlatformEnd_1.Margin.Top + (PlatformEnd_1.ActualHeight + 15),0,0);
                    Basket.Margin = new Thickness(Basket.Margin.Left - Basket.ActualWidth / 2, Basket.Margin.Top, 0, 0);
                    break;

                case 2:
                 
                    player.Fill = new ImageBrush(new BitmapImage(new Uri("positionPlayer2.png", UriKind.Relative))); // Установка внешнего вида человечка;
                    player.Margin = new Thickness(198,160,0,0); // Координаты человечка.

                    Basket.Margin = new Thickness(PlatformEnd_2.Margin.Left + 10, PlatformEnd_2.Margin.Top + (PlatformEnd_2.ActualHeight + 15),0,0);
                    Basket.Margin = new Thickness(Basket.Margin.Left - Basket.ActualWidth / 2, Basket.Margin.Top, 0, 0);
                    break;

                case 3:
                 
                    player.Fill = new ImageBrush(new BitmapImage(new Uri("positionPlayer3.png", UriKind.Relative))); // Установка внешнего вида человечка;
                    player.Margin = new Thickness(129,160,0,0); // Координаты человечка.

                    Basket.Margin = new Thickness(PlatformEnd_3.Margin.Left - 25, PlatformEnd_3.Margin.Top + (PlatformEnd_3.ActualHeight + 15),0,0);
                    
                    break;

                case 4:
              
                    player.Fill = new ImageBrush(new BitmapImage(new Uri("positionPlayer4.png", UriKind.Relative))); // Установка внешнего вида человечка;
                    player.Margin = new Thickness(103, 161, 0, 0); // Координаты человечка.

                    Basket.Margin = new Thickness(PlatformEnd_4.Margin.Left - 25, PlatformEnd_4.Margin.Top + (PlatformEnd_4.ActualHeight + 15),0,0);
                    break;
            }
        }

        /// <summary>
        /// Закртытие приложения
        /// </summary>
        /// <param name="sender">Приложение (Application)</param>
        /// <param name="e">Закрытие (Exit)</param>
        private void ExitApplication(object sender, ExitEventArgs e)
        {
            threadCheckHitOnBasket.Abort(); // Останавливаем поток по проверки на попадание яйца в корзину.
        }

    }


    /// <summary>
    /// Класс яйца
    /// </summary>
    class Egg
    {

        public Ellipse egg { get; set; } // Яйцо;

        public int valueCoin { get; set; } // Количество очков которые будут получены за пойманое яйцо;     
        public Brush brush { get; set; } // Цвет яйца.

        public Egg(int valueCoin, Brush brush)
        {
            this.valueCoin = valueCoin;
            this.brush = brush;

            egg = CreateEgg(valueCoin, brush);
        }

        /// <summary>
        /// Создание яйца по полученным аргументам
        /// </summary>
        /// <param name="coin">Количество очков которые будут получены за пойманое яйцо</param>
        /// <param name="brush">Цвет яйца</param>
        /// <returns>Яйцо</returns>
        private Ellipse CreateEgg(int coin, Brush brush)
        {
            Ellipse egg = new Ellipse //Устанавливаем свойста яйца.
            {
                Fill = brush, 
                Width = 15, 
                Height = 15,
                Tag = coin,

                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            }; 

            return egg;
        }
    }
}
