using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Awesomium.Core;
using SecondScreen.Utilities;

namespace SecondScreen
{
    public partial class SecondScreenForm : Form
    {
        private WebSession _webSession;
        private Awesomium.Windows.Forms.WebControl _webControl;
        private bool _isFirstPageLoad = true;

        public SecondScreenForm()
        {
            InitializeComponent();
            InitWebCore();
            MoveMainWindowToSecondScreen();
        }

        void MoveMainWindowToSecondScreen()
        {
            var secondaryScreen = Screen.AllScreens.FirstOrDefault(s => !s.Primary);
            if (secondaryScreen == null)
            {
                MessageBox.Show(@"Failed to detect your second PC monitor", @"Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            else
            {
                Left = secondaryScreen.WorkingArea.Left;
                Top = secondaryScreen.WorkingArea.Top;
                Width = secondaryScreen.WorkingArea.Width;
                Height = secondaryScreen.Bounds.Height;
            }
        }

        private void OnPageLoad(object sender, FrameEventArgs e)
        {
            if (_isFirstPageLoad)
            {
                _isFirstPageLoad = false;

                CommonUtil.Run(() =>
                {
                    _webControl.Reload(false);
                }, 5 * 60 * 1000);
            }
        }
        
        void InitWebCore()
        {
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "secondscreen2");

            WebCore.Initialize(new WebConfig()
            {
                LogLevel = Awesomium.Core.LogLevel.None,
                LogPath = Path.Combine(appData, "log.txt"),

                AdditionalOptions = new string[] { "--enable-video-fullscreen=false" },

                PluginsPath = "plugins",

                UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36"
            });


            _webSession = WebCore.CreateWebSession(appData, new WebPreferences()
            {
                // Устанавливаем AcceptLanguage
                AcceptLanguage = "ru-ru,ru;q=0.8,en-us;q=0.6,en;q=0.4",

                // Отключаем плагины (Flash, Silverlight)
                Plugins = false,

                // Запрещаем открывать окна с помощью JavaScript
                CanScriptsOpenWindows = false,

                // Запрещаем закрывать окна с помощью JavaScript
                CanScriptsCloseWindows = false,

                // Отключаем звуки в браузере
                WebAudio = false,

                // Отключаем защиту для возможности взаимодействия с фреймами внутри страницы
                WebSecurity = false,

                // Код JavaScript, который будет выполняться каждый раз перед загрузкой страниц в браузере
                // В данном примере подменяем Referrer и запрещаем открытие встроенных браузерных окон (alert, confirm, prompt)
                UserScript = "delete window.document.referrer; window.document.__defineGetter__('referrer', function () {return 'http://info-less.ru';}); window.alert = function(){}; window.confirm = function(){return false;}; window.prompt = function(){return NULL;};"
            });

            // Создаем контрол
            _webControl = new Awesomium.Windows.Forms.WebControl(this.components);
            _webControl.Dock = DockStyle.Fill;
            this.Controls.Add(_webControl);
            _webControl.WebSession = _webSession;

            // Очистка кук (перед этим должна быть установлена сессия)
            _webSession.ClearCookies();

            // Очистка кук через контрол браузера
            _webControl.WebSession.ClearCookies();

            // Очистка кеша браузера (перед этим должна быть установлена сессия)
            _webSession.ClearCache();

            // Очистка кеша через контрол браузера
            _webControl.WebSession.ClearCache();

            // Считываем из файла адрес, по которому надо загрузить страницу
            var url = File.ReadLines("url.txt").First();
            _webControl.Source = new Uri(url);

            // Задаем событие при перезагрузке страницы
            _webControl.LoadingFrameComplete += OnPageLoad;
        }
    }
}
