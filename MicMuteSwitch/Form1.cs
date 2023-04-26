using Microsoft.VisualBasic;
using NAudio.CoreAudioApi;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MicMuteSwitch
{
    public partial class Form1 : Form
    {
        // MMDeviceEnumerator 객체 생성
        MMDeviceEnumerator enumerator = new MMDeviceEnumerator();

        // RegisterHotKey 함수
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        // UnregisterHotKey 함수
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // 단축키 ID (Toggle Key)
        private const int HOTKEY_ID = 0;

        public void CheckMicStatus()
        {
            // 기본 입력 장치 (마이크) 가져오기
            MMDevice device = (MMDevice)enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            //생성된 스레드가 아닌 다른 스레드에서 호출될 경우 true
            var deviceName = device.DeviceFriendlyName;
            if (label2.InvokeRequired)
            {
                Console.WriteLine("out thread");
                label2.Invoke(new MethodInvoker(delegate ()
                {
                    label2.Text = deviceName;

                }));
                if (device.AudioEndpointVolume.Mute)
                {
                    label4.Invoke((MethodInvoker)(() => label4.Text = "Off"));
                    button1.Invoke((MethodInvoker)(() => button1.Text = "마이크 켜기(&X)"));
                    button1.Invoke((MethodInvoker)(() => notifyIcon1.BalloonTipText = "마이크 꺼짐"));

                }
                else
                {
                    label4.Invoke((MethodInvoker)(() => label4.Text = "On"));
                    button1.Invoke((MethodInvoker)(() => button1.Text = "마이크 끄기(&X)"));
                    button1.Invoke((MethodInvoker)(() => notifyIcon1.BalloonTipText = "마이크 켜짐"));
                }
            }
            else
            {
                Console.WriteLine("in thread");
                label2.Text = device.DeviceFriendlyName;
                if (device.AudioEndpointVolume.Mute)
                {
                    label4.Text = "Off";
                    button1.Text = "마이크 켜기(&X)";
                    notifyIcon1.BalloonTipText = "마이크 꺼짐";
                }
                else
                {
                    label4.Text = "On";
                    button1.Text = "마이크 끄기(&X)";
                    notifyIcon1.BalloonTipText = "마이크 켜짐";
                }
            }

            notifyIcon1.ShowBalloonTip(0);

        }

        public void ToggleMicOnOff()
        {
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            // 마이크 On/Off
            device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;

            // 마이크 상태 Label 값 변경
            //CheckMicStatus();
        }

        public Form1()
        {
            InitializeComponent();
            try
            {
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
                device.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
                CheckMicStatus();
            }
            catch (Exception ex)
            {
                // 예외 처리
                if (ex is COMException)
                {
                    MessageBox.Show("연결된 기본 마이크가 없습니다.", "장치 없음", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                else
                {
                    MessageBox.Show(ex.Message, "알 수 없는 에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                // 예외 발생 시 강제 종료
                Environment.Exit(1);
            }
        }

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            Console.WriteLine("VolumeNotification: {0}", data.MasterVolume);
            // 마이크 상태 변경 시 실행할 로직 구현
            CheckMicStatus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("button click");
            ToggleMicOnOff();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == HOTKEY_ID)
            {
                // 단축키 처리 코드 작성
                ToggleMicOnOff();
                Console.WriteLine("토글 실행");
            }

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // Ctrl + Alt + Shift + M 단축키 등록
            Console.WriteLine(" 단축키 등록");
            RegisterHotKey(this.Handle, HOTKEY_ID, 0x0, (int)Keys.Scroll);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            // 등록한 단축키 해제
            Console.WriteLine(" 단축키 해제");
            UnregisterHotKey(this.Handle, HOTKEY_ID);
        }
    }
}